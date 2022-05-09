using Newtonsoft.Json;
using Spotify.Commands;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Spotify
{
    public class SpotifyPlaybackClient : NiceSpotifyClient
    {
        private Playlist currentPlaylist;

        private int playlistIndex;

        private PlayerState cachedState = PlayerState.Stopped;

        private string deviceId;

        public SpotifyPlaybackClient(Logger logger) : base(logger)
        {
            Preferences.PropertyChanged += (s) =>
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

        protected override async Task<bool> Handle(Command command)
        {
            switch (command)
            {
                case PauseCommand _:
                    return await PauseInner();

                case StopCommand _:
                    return await StopInner();

                case PlayCommand playCommand:
                    return await PlayInner(playCommand.Item, playCommand.Milliseconds);

                case PlayOnceCommand playOnceCommand:
                    return await PlayInner(playOnceCommand.Item, playOnceCommand.Milliseconds) && await Handle(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Off });

                case LoopCommand loopCommand:
                    return await PlayInner(loopCommand.Item, loopCommand.Milliseconds) && await Handle(new SetPlaybackOptionsCommand { RepeatMode = RepeatMode.Context });

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
#warning not sure whether this would be helpful or not, difficult to figure out transfercommand - test with transferring to same track

                    //int predictedtime = info?.ProgressMs is int time2 ? time2 + ((int)stopwatch.ElapsedMilliseconds / 2) : 0;
                    return await PlayItem(transferCommand.Item, transferCommand.FromTrackId.IsMatch(track?.Id) ? transferCommand.Map(info.ProgressMs ?? 0) : 0);

                case SetPlaybackOptionsCommand optionsCommand:
                    bool success = true;
                    var playback = await Client.Player.GetCurrentPlayback();

                    if (optionsCommand.RepeatMode is RepeatMode repeatMode)
                    {
                        if (!string.Equals(playback.RepeatState, repeatMode.ToString(), StringComparison.OrdinalIgnoreCase))
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

            async Task<bool> PlayItem(SpotifyItem? item, int milliseconds)
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

            async Task<bool> PlayPlaylist(Playlist playlist, int milliseconds)
            {
                playlistIndex = -1;
                currentPlaylist = playlist;
                return await NextPlaylistItem(milliseconds);
            }

            async Task SetPlaylistIndex()
            {
                var playbackState = (await Client.Player.GetCurrentPlayback());
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
            }

            async Task<bool> NextPlaylistItem(int milliseconds = 0)
            {
                await SetPlaylistIndex();

                if (playlistIndex != -1)
                {
                    bool result = await PlayItem(currentPlaylist[playlistIndex], milliseconds);
                    var info = (await GetCurrentlyPlaying()).Item;

                    int durationMs;
                    switch (info)
                    {
                        case FullTrack t:
                            durationMs = t.DurationMs;
                            break;

                        case FullEpisode e:
                            durationMs = e.DurationMs;
                            break;

                        default:
                            return false;
                    }

                    _ = Task.Delay(durationMs).ContinueWith(_ => NextPlaylistItem(), TaskScheduler.Default);

                    return result;
                }
                else
                {
                    currentPlaylist = null;
                    return await StopInner();
                }
            }

            async Task<bool> StopInner()
            {
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
            }

            async Task<bool> PlayInner(ISpotifyItem item, int milliseconds)
            {
                switch (item)
                {
                    case SpotifyItem spotifyItem:
                        return await PlayItem(spotifyItem, milliseconds);

                    case Playlist playlist:
                        return await PlayPlaylist(playlist, milliseconds);

                    default:
                        throw new ArgumentOutOfRangeException(nameof(item));
                }
            }
        }

        protected override async Task InitialiseAsync()
        {
            await base.InitialiseAsync();
            await ActivateDeviceAsync();
        }

        protected override async Task<bool> HandleErrorAsync(Exception e, CommandListAsync wrapper, List<Type> exceptionTypes = null)
        {
            switch (e)
            {
                case APIException _:
                    if (!exceptionTypes.Contains(e.GetType()))
                    {
                        await ActivateDeviceAsync();
                        exceptionTypes.Add(e.GetType());
                        return await ExecuteInner(wrapper, exceptionTypes);
                    }
                    break;
            }

            return await base.HandleErrorAsync(e, wrapper, exceptionTypes);
        }

        private async Task<List<Device>> GetDevicesAsync()
        {
            var response = await Client.Player.GetAvailableDevices();
            return response?.Devices;
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
            var defaultDeviceByteArray = Preferences.DefaultDevice;

            var devices = await GetDevicesAsync();

            if (defaultDeviceByteArray?.Length > 0)
            {
                var str = Encoding.UTF8.GetString(defaultDeviceByteArray);
                DeviceInfo defaultDevice = null;

                try
                {
                    defaultDevice = JsonConvert.DeserializeObject<DeviceInfo>(str);
                }
                catch
                {
                }

                if (!(defaultDevice?.Id is null) && devices.Any(d => d.Id == defaultDevice.Id))
                {
                    SaveId(defaultDevice.Id);
                    return true;
                }
            }

            var activeDevice = (await Client.Player.GetCurrentPlayback())?.Device;

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