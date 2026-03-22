using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PactOfPunishment
{
    public static partial class Utils
    {
        public class SpawnCards
        {
            public SpawnCards(IEnumerable<SpawnCard> source)
            {
                this.GetSpawnCards = () => source;
            }

            public SpawnCards(params IAssetPromise<SpawnCard>[] array)
            {
                this.GetSpawnCards = () => array.Select(x => x.Value);
            }

            public Func<IEnumerable<SpawnCard>> GetSpawnCards { get; }

            public static implicit operator SpawnCards(IAssetPromise<SpawnCard>[] array) => new SpawnCards(array);

            public static implicit operator SpawnCards(SpawnCard[] source) => new SpawnCards(source);
        }
    }
}