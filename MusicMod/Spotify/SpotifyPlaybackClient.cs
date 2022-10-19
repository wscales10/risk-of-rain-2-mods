using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using Spotify.Commands;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace Spotify
{
	public class SpotifyPlaybackClient : NiceSpotifyClient
	{
		private readonly BasicTimer mainPlaylistTimer;

		private readonly BasicTimer secondaryPlaylistTimer;

		private readonly IEnumerable<Playlist> playlists;

		private readonly int transferFix;

		private Playlist currentPlaylist;

		private int playlistIndex;

		private PlayerState cachedState = PlayerState.Stopped;

		private string deviceId;

		private JoinableTask<FullTrack> getTrackInfoTask;

		public SpotifyPlaybackClient(IEnumerable<Playlist> playlists, Logger logger, IPreferences preferences, int transferFix = 0) : base(logger, preferences)
		{
			mainPlaylistTimer = new BasicTimer(() => _ = NextPlaylistItem());
			secondaryPlaylistTimer = new BasicTimer(() => _ = PrepareNextTime());
			this.playlists = playlists;
			this.transferFix = transferFix;
			preferences.PropertyChanged += (s) =>
			{
				if (s == nameof(Preferences.DefaultDevice))
				{
					_ = ActivateDeviceAsync();
				}
			};
		}

		private enum PlayerState
		{
			Stopped,

			Paused,

			Playing
		}

		public async Task Pause()
		{
			await Do(new PauseCommand());
		}

		public async Task Resume()
		{
			await Do(new ResumeCommand());
		}

		public async Task Stop()
		{
			await Do(new StopCommand());
		}

		protected override async Task<bool> HandleAsync(Command command, CancellationToken cancellationToken)
		{
			switch (command)
			{
				case PauseCommand _:
					return await PauseInner();

				case StopCommand _:
					return await StopInner();

				case SkipCommand _:
					return await Client.Player.SkipNext();

				case PlayCommand playCommand:
					return await PlayInner(playCommand.Item, playCommand.Milliseconds, playCommand.Offset);

				case PlayOnceCommand playOnceCommand:
					return await PlayInner(playOnceCommand.Item, playOnceCommand.Milliseconds, playOnceCommand.Offset) && await HandleAsync(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Off }, cancellationToken);

				case LoopCommand loopCommand:
					return await PlayInner(loopCommand.Item, loopCommand.Milliseconds, loopCommand.Offset) && await HandleAsync(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Context }, cancellationToken);

				case SeekToCommand seekToCommand:
					{
						if (await GetState() == PlayerState.Stopped)
						{
							return false;
						}

						var positionMs = await GetPosition(seekToCommand.Milliseconds);
						Log(positionMs);
						var request = new PlayerSeekToRequest(positionMs);
						return await Client.Player.SeekTo(request);
					}
				case ResumeCommand _:
					return await ResumeInner();

				case TransferCommand transferCommand:

					// var stopwatch = new Stopwatch(); stopwatch.Start();
					var info = await GetCurrentlyPlaying();

					// stopwatch.Stop();
					var track = info?.Item as FullTrack;
#warning not sure whether this would be helpful or not, difficult to figure out transfercommand - test with transferring to same track

					//int predictedtime = info?.ProgressMs is int time2 ? time2 + ((int)stopwatch.ElapsedMilliseconds / 2) : 0;

					if (transferCommand.FromTrackId.IsMatch(track?.Id))
					{
						int progressMs = info.ProgressMs ?? 0;
						int mapped = Math.Max(0, transferCommand.Map(progressMs) + transferFix);
						return await PlayItem(transferCommand.Item, mapped);
					}
					else
					{
						return await PlayItem(transferCommand.Item, 0);
					}

				case SetPlaybackOptionsCommand optionsCommand:
					bool success = true;
					var playback = await Client.Player.GetCurrentPlayback();

					if (optionsCommand.RepeatMode is RepeatMode repeatMode
						&& !string.Equals(playback?.RepeatState, repeatMode.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						success = success && await Client.Player.SetRepeat(new PlayerSetRepeatRequest(repeatMode.AsEnum<PlayerSetRepeatRequest.State>()));
					}

					if (optionsCommand.Shuffle is bool shuffle
						&& playback?.ShuffleState != shuffle)
					{
						success = success && await Client.Player.SetShuffle(new PlayerShuffleRequest(shuffle));
					}

					if (optionsCommand.VolumePercent is int volumePercent
						&& playback?.Device.VolumePercent != volumePercent)
					{
						success = success && await Client.Player.SetVolume(new PlayerVolumeRequest(volumePercent));
					}

					return success;
			}

			return false;
		}

		protected override async Task InitialiseAsync()
		{
			await base.InitialiseAsync();
			await ActivateDeviceAsync();
		}

		protected override async Task<bool> HandleErrorAsync(Exception e, ICommandList commands, CancellationToken cancellationToken, List<SpotifyError> errors = null)
		{
			var error = ClassifyException(e);
			switch (e)
			{
				case APIException _:
					if (!errors.Contains(error))
					{
						switch (error.Subtype)
						{
							case ErrorType.BadGateway:
								errors.Add(error);
								return await ExecuteAsync(commands, cancellationToken, errors);

							case ErrorType.RestrictionViolated:
								Log(e);
								System.Diagnostics.Debugger.Break();
								return true;

							case ErrorType.NoActiveDevice:
								await ActivateDeviceAsync();
								errors.Add(error);
								return await ExecuteAsync(commands, cancellationToken, errors);
						}
					}

					break;
			}

			return await base.HandleErrorAsync(e, commands, cancellationToken, errors);
		}

		protected override SpotifyError ClassifyException(Exception e)
		{
			var message = e.Message?.ToLower();

			var output = new SpotifyError(e.GetType());

			switch (e)
			{
				case APIException _:
					if (!(message is null))
					{
						if (message.Contains("active device"))
						{
							return output.With(ErrorType.NoActiveDevice);
						}
						else if (message.Contains("restriction violated"))
						{
							return output.With(ErrorType.RestrictionViolated);
						}
						else if (message.Contains("bad gateway"))
						{
							return output.With(ErrorType.BadGateway);
						}
					}

					break;
			}

			return base.ClassifyException(e);
		}

		private void Timers(Action<BasicTimer> action)
		{
			action(mainPlaylistTimer);
			action(secondaryPlaylistTimer);
		}

		private async Task<bool> ResumeInner()
		{
			Log($"Entering method {nameof(ResumeInner)}");
			if (await GetState() == PlayerState.Paused)
			{
				Log($"State is PlayerState.Paused, attempting resume");
				if (!await Client.Player.ResumePlayback())
				{
					Log("Resume unsuccessful");
					return false;
				}

				Log($"Resume successful");

				SetState(PlayerState.Playing);
			}

			Timers(t => t.Resume());

			Log($"Exiting method {nameof(ResumeInner)}");
			return true;
		}

		private void ClearCurrentPlaylist()
		{
			Timers(t => t.Stop());
			currentPlaylist = null;
			playlistIndex = -1;
		}

		private async Task<bool> PlayInner(ISpotifyItem item, int milliseconds, IOffset offset)
		{
			ClearCurrentPlaylist();
			switch (item)
			{
				case SpotifyItem spotifyItem:
					return await PlayItem(spotifyItem, milliseconds, offset);

				case PlaylistRef pRef:
					return await PlayPlaylist(playlists.FirstOrDefault(p => p.Name == pRef.Name), milliseconds, offset);

				default:
					throw new ArgumentOutOfRangeException(nameof(item));
			}
		}

		private async Task<bool> StopInner()
		{
			Timers(t => t.Stop());
			if (await GetState() == PlayerState.Playing && !await PauseInner())
			{
				return false;
			}

			if (await GetState() != PlayerState.Stopped && !await SeekToInner(0))
			{
				return false;
			}

			SetState(PlayerState.Stopped);
			return true;
		}

		private async Task<bool> PlayPlaylist(Playlist playlist, int milliseconds, IOffset offset)
		{
			currentPlaylist = playlist;

			switch (offset)
			{
				case null:
					playlistIndex = -1;
					break;

				case IndexOffset indexOffset:
					playlistIndex = indexOffset.Position - 1;
					break;

				case ItemOffset itemOffset:
					playlistIndex = playlist.IndexOf(itemOffset.Item) - 1;
					break;

				default:
					throw new NotImplementedException();
			}

			return await NextPlaylistItem(milliseconds);
		}

		private async Task<bool> SeekToInner(uint ms)
		{
			// TODO: handle case where this would mean track ending before secondaryPlaylistTimer finishes

			if (mainPlaylistTimer.Remaining > TimeSpan.Zero)
			{
				mainPlaylistTimer.SkipTo(mainPlaylistTimer.Time - TimeSpan.FromMilliseconds(ms));
			}

			var request = new PlayerSeekToRequest(ms);
			return await Client.Player.SeekTo(request);
		}

		private async Task<bool> PauseInner()
		{
			Log($"Entered method {nameof(PauseInner)}");
			Timers(t => t.Pause());

			Log("await pause playback");
			if (await Client.Player.PausePlayback())
			{
				Log("Playback paused");
				SetState(PlayerState.Paused);
				return true;
			}
			else
			{
				Log("Playback not paused");
			}

			Log("Get state (verify pause successful)");
			var state = await GetState();
			Log($"State got: {state}");
			return state == PlayerState.Paused;
		}

		private async Task<bool> PlayItem(SpotifyItem item, int milliseconds, IOffset offset = null)
		{
			if (item is null)
			{
				throw new ArgumentNullException(nameof(item));
			}

			var s = item?.GetUri().ToString();
			var positionMs = await GetPosition(milliseconds);
			PlayerResumePlaybackRequest request;
			if (item?.Type == SpotifyItemType.Track)
			{
				if (!(offset is null))
				{
					throw new InvalidOperationException("Offset cannot be used with tracks");
				}

				request = new PlayerResumePlaybackRequest { Uris = new string[] { s }, PositionMs = positionMs };
			}
			else
			{
				PlayerResumePlaybackRequest.Offset convertedOffset;

				switch (offset)
				{
					case null:
						convertedOffset = null;
						break;

					case ItemOffset itemOffset:
						convertedOffset = new PlayerResumePlaybackRequest.Offset { Uri = itemOffset.Item.GetUri().ToString() };
						break;

					case IndexOffset indexOffset:
						convertedOffset = new PlayerResumePlaybackRequest.Offset { Position = indexOffset.Position };
						break;

					default:
						throw new FormatException();
				}

				request = new PlayerResumePlaybackRequest { ContextUri = s, PositionMs = positionMs, OffsetParam = convertedOffset };
			}

			if (await Client.Player.ResumePlayback(request))
			{
				SetState(PlayerState.Playing);
				return true;
			}

			return false;
		}

		private async Task<bool> SetPlaylistIndex()
		{
			CurrentlyPlayingContext playbackState;
			try
			{
				playbackState = await Client.Player.GetCurrentPlayback();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debugger.Break();
				throw;
			}

			if (playbackState is null)
			{
				return false;
			}

			var repeatMode = playbackState.RepeatState.AsEnum<RepeatMode>(true);

			if (repeatMode == RepeatMode.Track)
			{
				playlistIndex--;
			}

			playlistIndex++;

			if (playlistIndex == currentPlaylist.Count)
			{
				switch (repeatMode)
				{
					case RepeatMode.Off:
						playlistIndex = -1;
						break;

					case RepeatMode.Context:
						playlistIndex = 0;
						break;
				}
			}

			Log(playlistIndex);

			return true;
		}

		private async Task<bool> NextPlaylistItem(int milliseconds = 0)
		{
			if (!await SetPlaylistIndex())
			{
				return false;
			}

			if (playlistIndex != -1)
			{
				bool result = await PlayItem(currentPlaylist[playlistIndex], milliseconds);
				getTrackInfoTask = Async.Manager.RunSafely(() => Client.Tracks.Get(currentPlaylist[playlistIndex].Id));
				Log(DateTime.UtcNow);
				secondaryPlaylistTimer.Start(TimeSpan.FromSeconds(3));
				return result;
			}
			else
			{
				Log("playlist end");
				currentPlaylist = null;
				return await StopInner();
			}
		}

		private async Task<bool> PrepareNextTime()
		{
			Log($"{playlistIndex} / {DateTime.UtcNow}");
			var currentlyPlaying = await GetCurrentlyPlaying();

			if ((currentlyPlaying.Item as FullTrack)?.Id != currentPlaylist[playlistIndex].Id)
			{
				System.Diagnostics.Debugger.Break();
			}

			var desiredTrack = await getTrackInfoTask;

			int durationMs;
			switch (currentlyPlaying.Item)
			{
				case FullTrack t:
					Log($"Currently playing: {t.Name} at {TimeSpan.FromMilliseconds(currentlyPlaying.ProgressMs ?? 0)}");
					durationMs = desiredTrack.DurationMs - (currentlyPlaying.ProgressMs ?? 0);
					break;

				case FullEpisode e:
					Log($"Currently playing: {e.Name} at {TimeSpan.FromMilliseconds(currentlyPlaying.ProgressMs ?? 0)}");
					durationMs = e.DurationMs - (currentlyPlaying.ProgressMs ?? 0);
					break;

				default:
					return false;
			}

			mainPlaylistTimer.Start(TimeSpan.FromMilliseconds(durationMs));
			Log(durationMs);
			return true;
		}

		private async Task<List<Device>> GetDevicesAsync()
		{
			try
			{
				if (!(Client is null))
				{
					var response = await Client.Player.GetAvailableDevices();

					if (!(response is null))
					{
						return response.Devices;
					}
				}
			}
			catch (HttpRequestException ex)
			{
				Log(ex);
			}

			return new List<Device>();
		}

		private async Task<CurrentlyPlaying> GetCurrentlyPlaying()
		{
			return await Client.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
		}

		private async Task<int> GetPosition(int milliseconds)
		{
			if (milliseconds < 0)
			{
				var track = (await GetCurrentlyPlaying()).Item as FullTrack;
				return Math.Max(0, track.DurationMs + milliseconds);
			}
			else
			{
				return milliseconds;
			}
		}

		private async Task<PlayerState> GetState()
		{
			var playback = await Client.Player.GetCurrentPlayback();

			if (playback is null)
			{
				return cachedState;
			}
			else if (playback.IsPlaying)
			{
				return PlayerState.Playing;
			}
			else if (playback.ProgressMs > 0)
			{
				return PlayerState.Paused;
			}
			else
			{
				return PlayerState.Stopped;
			}
		}

		private async Task ActivateDeviceAsync()
		{
			if (await PickDeviceAsync())
			{
				await Client.Player.TransferPlayback(new PlayerTransferPlaybackRequest(new[] { deviceId }));
			}
		}

		private async Task<bool> PickDeviceAsync()
		{
			var str = preferences.DefaultDeviceString;

			var devices = await GetDevicesAsync();

			if (!string.IsNullOrWhiteSpace(str))
			{
				DeviceInfo defaultDevice = null;

				try
				{
					defaultDevice = JsonConvert.DeserializeObject<DeviceInfo>(str);
				}
				catch (Exception ex)
				{
					Log(ex);
				}

				if (!(defaultDevice?.Id is null) && devices.Any(d => d.Id == defaultDevice.Id))
				{
					SaveId(defaultDevice.Id);
					return true;
				}
			}

			var activeDevice = Client is null ? null : (await Client.Player.GetCurrentPlayback())?.Device;

			if (!(activeDevice is null))
			{
				SaveId(activeDevice.Id);
				return true;
			}

			if (!(deviceId is null) && devices.Any(d => d.Id == deviceId))
			{
				SaveId();
				return true;
			}

			switch (devices.Count)
			{
				case 0:
					break;

				case 1:
					SaveId(devices[0].Id);
					return true;

				default:
					try
					{
						var s = File.ReadAllText(Path.Combine(Paths.AssemblyDirectory, "deviceId.txt"));
						if (devices.Any(d => d.Id == s))
						{
							SaveId(s);
							return true;
						}
					}
					catch
					{
						SaveId(devices[0].Id);
						return true;
					}
					break;
			}

			SaveId("");
			return false;

			void SaveId(string id = null)
			{
				if (!(id is null))
				{
					deviceId = id;
				}

				File.WriteAllText(Path.Combine(Paths.AssemblyDirectory, "deviceId.txt"), deviceId);
			}
		}

		private void SetState(PlayerState value)
		{
			cachedState = value;
		}
	}
}