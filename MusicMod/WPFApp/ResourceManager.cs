using Spotify;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utils;
using Utils.Async;

namespace WPFApp
{
    public static class ResourceManagers
    {
        public static SpotifyInfoResourceManager SpotifyInfo { get; } = new();
    }

    public abstract class ResourceManager<TKey, TResource>
    {
        public abstract Task<TResource> RequestResource(TKey resourceKey, CancellationToken cancellationToken = default);
    }

    public class SpotifyInfoResourceManager : ResourceManager<SpotifyItem, MusicItemInfo>
    {
        private static readonly SeniorTaskMachine taskMachine = new();

        public delegate Task<ConditionalValue<MusicItemInfo>> MusicItemInfoRequestHandler(SpotifyItem item, CancellationToken cancellationToken);

        internal static event MusicItemInfoRequestHandler MusicItemInfoRequested;

        public static AsyncCache<SpotifyItem, MusicItemInfo> MusicItemDictionary { get; } = new((si, token) => MusicItemInfoRequested?.Invoke(si, token));

        public override async Task<MusicItemInfo> RequestResource(SpotifyItem resourceKey, CancellationToken cancellationToken = default)
        {
            if (resourceKey is SpotifyItem si)
            {
                MusicItemInfo output = null;
                var result = taskMachine.TryIngest(token => MusicItemDictionary.GetValueAsync(si, info => output = info, token), cancellationToken);
                if (!result.HasValue)
                {
                    throw new InvalidOperationException();
                }
                await result.Value.Awaitable;
                return output;
            }
            else
            {
                return null;
            }
        }
    }
}