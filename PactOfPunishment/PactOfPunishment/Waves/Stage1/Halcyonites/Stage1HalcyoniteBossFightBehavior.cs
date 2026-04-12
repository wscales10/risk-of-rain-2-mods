using PactOfPunishment.Waves.Common;
using RoR2;
using System;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    public abstract class Stage1HalcyoniteBossFightBehavior : BossFightBehavior
    {
        public event Action<CharacterBody>? MainBossSpawnedServer;

        private BossGroupWrapper? bossGroup;

        protected sealed override void OnBossSpawnedServer(CharacterBody body)
        {
            if (body.Is(DLC2Content.BodyPrefabs.HalcyoniteBody))
            {
                this.AddBossToGroup(ref this.bossGroup, body);
                this.OnMainBossSpawnedServer(body);
                this.MainBossSpawnedServer?.Invoke(body);
            }
            else
            {
                this.OnAddSpawnedServer(body);
            }
        }

        protected abstract void OnMainBossSpawnedServer(CharacterBody body);

        protected virtual void OnAddSpawnedServer(CharacterBody body)
        {
        }
    }
}