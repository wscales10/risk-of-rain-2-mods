using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using Spotify.Commands;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private static readonly ReadOnlyDictionary<SpotifyItemType, Type> enumToTypeDictionary = new Dictionary<SpotifyItemType, Type>
        {
            [SpotifyItemType.Album] = typeof(FullAlbum),
            [SpotifyItemType.Artist] = typeof(FullArtist),
            [SpotifyItemType.Playlist] = typeof(FullPlaylist),
            [SpotifyItemType.Track] = typeof(FullTrack),
            [SpotifyItemType.User] = typeof(PublicUser)
        }.ToReadOnly();

        private readonly BasicTimer mainPlaylistTimer;

        private readonly BasicTimer secondaryPlaylistTimer;

        private readonly BasicTimer[] timers;

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
            timers = new[] { mainPlaylistTimer, secondaryPlaylistTimer };
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

                        var positionMs = await GetPositionAsync(seekToCommand.Milliseconds);
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
                        return await PlayItemAsync(transferCommand.Item, mapped);
                    }
                    else
                    {
                        return await PlayItemAsync(transferCommand.Item, 0);
                    }

                case SetPlaybackOptionsCommand optionsCommand:
                    {
                        bool success = true;
                        var playback = await Client.Player.GetCurrentPlayback();

                        if (optionsCommand.RepeatMode is RepeatMode repeatMode
                            && !string.Equals(playback?.RepeatState, repeatMode.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            success &= await Client.Player.SetRepeat(new PlayerSetRepeatRequest(repeatMode.AsEnum<PlayerSetRepeatRequest.State>()));
                        }

                        if (optionsCommand.Shuffle is bool shuffle
                            && playback?.ShuffleState != shuffle)
                        {
                            success &= await Client.Player.SetShuffle(new PlayerShuffleRequest(shuffle));
                        }

                        return success;
                    }
                case SetVolumeCommand volumeCommand:
                    {
                        bool success = true;
                        var playback = await Client.Player.GetCurrentPlayback();

                        if (playback?.Device.VolumePercent != volumeCommand.VolumePercent)
                        {
                            success &= await Client.Player.SetVolume(new PlayerVolumeRequest(volumeCommand.VolumePercent));
                        }

                        return success;
                    }
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

            timers.ForEach(t => t.Resume());

            Log($"Exiting method {nameof(ResumeInner)}");
            return true;
        }

        private void ClearCurrentPlaylist()
        {
            timers.ForEach(t => t.Stop());
            currentPlaylist = null;
            playlistIndex = -1;
        }

        private async Task<bool> PlayInner(ISpotifyItem item, int milliseconds, IOffset offset)
        {
            ClearCurrentPlaylist();
            switch (item)
            {
                case SpotifyItem spotifyItem:
                    return await PlayItemAsync(spotifyItem, milliseconds, offset);

                case PlaylistRef pRef:
                    return await PlayPlaylist(playlists.FirstOrDefault(p => p.Name == pRef.Name), milliseconds, offset);

                default:
                    throw new ArgumentOutOfRangeException(nameof(item));
            }
        }

        private async Task<bool> StopInner()
        {
            timers.ForEach(t => t.Stop());
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
            timers.ForEach(t => t.Pause());

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

        private async Task<(object, bool)> TryGetTrackAsync(SpotifyItem item, IOffset offset)
        {
            try
            {
                return (await GetTrackAsync(item, offset), true);
            }
            catch
            {
                return (null, false);
            }
        }

        private async Task<bool> GetShuffleStateAsync()
        {
            return (await Client.Player.GetCurrentPlayback()).ShuffleState;
        }

        private Task<object> GetTrackAsync(SpotifyItem item, IOffset offset)
        {
            if (item.Type == SpotifyItemType.Track && !(offset is null))
            {
                throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset cannot be used with tracks");
            }

            return getTrackAsync(item, offset);
        }

        private async Task<object> getTrackAsync(SpotifyItem item, IOffset offset)
        {
            if (item.Type == SpotifyItemType.Track)
            {
                return await GetSpotifyItemInfoAsync<FullTrack>(item);
            }

            if (await GetShuffleStateAsync())
            {
                throw new InvalidOperationException();
            }

            switch (item.Type)
            {
                case SpotifyItemType.Album:

                    switch (offset)
                    {
                        case null:
                            return (await GetSpotifyItemInfoAsync<FullAlbum>(item)).Tracks.Items[0];

                        case ItemOffset itemOffset:
                            return await GetSpotifyItemInfoAsync<FullTrack>(itemOffset.Item);

                        case IndexOffset indexOffset:
                            return (await GetSpotifyItemInfoAsync<FullAlbum>(item)).Tracks.Items[indexOffset.Position];

                        default:
                            throw new NotSupportedException();
                    }

                case SpotifyItemType.Playlist:

                    switch (offset)
                    {
                        case null:
                            return (await GetSpotifyItemInfoAsync<FullPlaylist>(item)).Tracks.Items[0];

                        case ItemOffset itemOffset:
                            return await GetSpotifyItemInfoAsync<FullTrack>(itemOffset.Item);

                        case IndexOffset indexOffset:
                            return (await GetSpotifyItemInfoAsync<FullPlaylist>(item)).Tracks.Items[indexOffset.Position];

                        default:
                            throw new NotSupportedException();
                    }
            }

            throw new NotSupportedException();
        }

        private Task<bool> PlayItemAsync(SpotifyItem item, int milliseconds, IOffset offset = null)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            return playItemAsync(item, milliseconds, offset);
        }

        private async Task<bool> playItemAsync(SpotifyItem item, int milliseconds, IOffset offset = null)
        {
            var s = item?.GetUri().ToString();
            var positionMs = await GetPositionAsync(milliseconds, item, offset);
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
                this.Log(ex);
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
                bool result = await PlayItemAsync(currentPlaylist[playlistIndex], milliseconds);
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

        private Task<T> GetSpotifyItemInfoAsync<T>(SpotifyItem item)
            where T : class
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (enumToTypeDictionary[item.Type] != typeof(T))
            {
                throw new ArgumentOutOfRangeException(nameof(item), item, $"type mismatch with {typeof(T).Name}");
            }

            return getSpotifyItemInfoAsync<T>(item);
        }

        private async Task<T> getSpotifyItemInfoAsync<T>(SpotifyItem item)
            where T : class
        {
            switch (item.Type)
            {
                case SpotifyItemType.Track:
                    return await Client.Tracks.Get(item.Id) as T;

                case SpotifyItemType.Playlist:
                    return await Client.Playlists.Get(item.Id) as T;

                case SpotifyItemType.Album:
                    return await Client.Albums.Get(item.Id) as T;

                case SpotifyItemType.Artist:
                    return await Client.Artists.Get(item.Id) as T;

                case SpotifyItemType.User:
                    return await Client.UserProfile.Get(item.Id) as T;

                default:
                    throw new NotSupportedException($"Item type {item.Type} not supported");
            }
        }

        private async Task<int> GetPositionAsync(int milliseconds, SpotifyItem spotifyItem, IOffset offset)
        {
            if (spotifyItem is null)
            {
                return await GetPositionAsync(milliseconds);
            }
            else
            {
                return await GetPositionAsync(milliseconds, async () =>
                {
                    var (track, success) = await TryGetTrackAsync(spotifyItem, offset);

                    if (success)
                    {
                        return GetDuration(track);
                    }
                    else
                    {
                        return null;
                    }
                });
            }
        }

        private async Task<int> GetPositionAsync(int milliseconds, Func<Task<int?>> getTrackDurationMs = null)
        {
            if (milliseconds < 0)
            {
                int? trackDurationMs;
                if (getTrackDurationMs is null || (trackDurationMs = await getTrackDurationMs()) is null)
                {
                    trackDurationMs = ((FullTrack)(await GetCurrentlyPlaying()).Item).DurationMs;
                }

                return Math.Max(0, trackDurationMs.Value + milliseconds);
            }
            else
            {
                return milliseconds;
            }
        }

        private int GetDuration(object track)
        {
            switch (track)
            {
                case SimpleTrack simpleTrack:
                    return simpleTrack.DurationMs;

                case FullTrack fullTrack:
                    return fullTrack.DurationMs;

                default:
                    throw new ArgumentOutOfRangeException(nameof(track), track, $"Type {track.GetType()} not supported");
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

                if (!(defaultDevice?.Id is null) && devices.Exists(d => d.Id == defaultDevice.Id))
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

            if (!(deviceId is null) && devices.Exists(d => d.Id == deviceId))
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
                        if (devices.Exists(d => d.Id == s))
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