﻿using Spotify.Commands;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utils;

namespace Spotify
{
	public class SpotifyPlaybackClient : NiceSpotifyClient
	{
		private PlayerState cachedState = PlayerState.Stopped;
		private string deviceId;

		public SpotifyPlaybackClient(Logger logger) : base(logger) { }

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

		protected override async Task<bool> Handle(Command command)
		{
			switch (command)
			{
				case PauseCommand _:
					return await PauseInner();
				case StopCommand _:
					if (await GetState() == PlayerState.Playing)
					{
						if (!await PauseInner())
						{
							return false;
						}
					}

					if (await GetState() != PlayerState.Stopped)
					{
						if (!await SeekToInner(0))
						{
							return false;
						}
					}

					SetState(PlayerState.Stopped);
					return true;
				case PlayCommand playCommand:
					return await PlayInner(playCommand.Item, playCommand.Milliseconds);
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
					if (await GetState() == PlayerState.Paused)
					{
						if (await Client.Player.ResumePlayback())
						{
							SetState(PlayerState.Playing);
							return true;
						}

						return false;
					}

					return true;
				case TransferCommand transferCommand:
					var stopwatch = new Stopwatch();
					stopwatch.Start();
					var info = await GetCurrentlyPlaying();
					stopwatch.Stop();
					var track = info?.Item as FullTrack;
					int predictedtime = info?.ProgressMs is int time2 ? time2 + ((int)stopwatch.ElapsedMilliseconds / 2) : 0;
					return await PlayInner(transferCommand.Item, transferCommand.FromTrackId.IsMatch(track?.Id) ? transferCommand.Map(predictedtime) : 0);
				case SetPlaybackOptionsCommand optionsCommand:
					bool success = true;
					var playback = await Client.Player.GetCurrentPlayback();

					if (optionsCommand.RepeatMode is RepeatMode repeatMode)
					{
						if (!string.Equals(playback.RepeatState, repeatMode.ToString(), StringComparison.InvariantCultureIgnoreCase))
						{
							success = success && await Client.Player.SetRepeat(new PlayerSetRepeatRequest(repeatMode.AsEnum<PlayerSetRepeatRequest.State>()));
						}
					}

					if (optionsCommand.Shuffle is bool shuffle)
					{
						if (playback.ShuffleState != shuffle)
						{
							success = success && await Client.Player.SetShuffle(new PlayerShuffleRequest(shuffle));
						}
					}

					if (optionsCommand.VolumePercent is int volumePercent)
					{
						if (playback.Device.VolumePercent != volumePercent)
						{
							success = success && await Client.Player.SetVolume(new PlayerVolumeRequest(volumePercent));
						}
					}

					return success;
			}

			return false;

			async Task<bool> PauseInner()
			{
				if (await GetState() == PlayerState.Playing)
				{
					if (await Client.Player.PausePlayback())
					{
						SetState(PlayerState.Paused);
						return true;
					}

					return false;
				}

				return true;
			}

			async Task<bool> SeekToInner(uint ms)
			{
				var request = new PlayerSeekToRequest(ms);
				return await Client.Player.SeekTo(request);
			}

			async Task<bool> PlayInner(SpotifyItem? item, int milliseconds)
			{
				if (item is null)
				{
					throw new ArgumentNullException(nameof(item));
				}

				var s = item?.GetUri().ToString();
				var positionMs = await GetPosition(milliseconds);
				var request = item?.Type == SpotifyItemType.Track ? new PlayerResumePlaybackRequest { Uris = new string[] { s }, PositionMs = positionMs } : new PlayerResumePlaybackRequest { ContextUri = s, PositionMs = positionMs };

				if (await Client.Player.ResumePlayback(request))
				{
					SetState(PlayerState.Playing);
					return true;
				}

				return false;
			}
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

		private async Task<bool> PickDevice()
		{
			var response = await Client.Player.GetAvailableDevices();
			var devices = response?.Devices;

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

		protected override async Task InitialiseAsync()
		{
			await base.InitialiseAsync();
			await PickDevice();
		}

		private void SetState(PlayerState value)
		{
			cachedState = value;
		}

		protected override async Task<bool> ExecuteInner(CommandListAsync wrapper, List<Type> exceptionTypes = null)
		{
			try
			{
				return await base.ExecuteInner(wrapper, exceptionTypes);
			}
			catch (APIException e)
			{
				if (!exceptionTypes.Contains(e.GetType()))
				{
					await PickDevice();
					exceptionTypes.Add(e.GetType());
					return await ExecuteInner(wrapper, exceptionTypes);
				}

				Throw(e);
			}

			return false;
		}
	}
}
