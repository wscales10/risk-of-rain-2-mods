using HG;
using PactOfPunishment.Conditions;
using PactOfPunishment.Waves.Common;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite1;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite2;
using PactOfPunishment.Waves.Stage1.Halcyonites.Halcyonite3;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage1.Halcyonites
{
    public class Stage1HalcyoniteUpgrade : UpgradeEncounterStrategy
    {
        private readonly AssetPromise<CharacterSpawnCard> halcyoniteSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC2/Halcyonite/cscHalcyonite.asset");

        public override WaveUpgradeFilter WaveUpgradeFilter => WaveUpgradeFilter.MainBoss;

        public override void PostInitialise(EncounterContext ctx)
        {
            var bossFightBehavior = ctx.GameObject.GetComponent<Stage1HalcyoniteBossFightBehavior>();
            bossFightBehavior.MainBossSpawnedServer += mainBossBody => this.BossFightBehavior_MainBossSpawnedServer(bossFightBehavior, mainBossBody);
        }

        private void BossFightBehavior_MainBossSpawnedServer(Stage1HalcyoniteBossFightBehavior bossFightBehavior, CharacterBody mainBossBody)
        {
            mainBossBody.EnsureComponent<UndeployMinionsOnDeathBehavior>();
            DirectorCore.GetMonsterSpawnDistance(DirectorCore.MonsterSpawnDistance.Standard, out var minimumDistance, out var maximumDistance);
            var ghostBodies = new List<CharacterBody>();

            mainBossBody.master.onBodyDeath.AddListener(() =>
            {
                for (int i = ghostBodies.Count - 1; i >= 0; i--)
                {
                    var ghostBody = ghostBodies[i];

                    if (ghostBody)
                    {
                        ghostBody.master?.TrueKill(bossFightBehavior.gameObject, bossFightBehavior.gameObject, DamageType.VoidDeath);
                    }

                    ghostBodies.RemoveAt(i);
                }
            });

            switch (mainBossBody.GetComponent<Stage1HalcyoniteBodyBehavior>())
            {
                case Halcyonite1BodyBehavior _:
                    SpawnGhost(SetupHalcyonite2);
                    SpawnGhost(SetupHalcyonite3);
                    break;

                case Halcyonite2BodyBehavior _:
                    SpawnGhost(SetupHalcyonite1);
                    SpawnGhost(SetupHalcyonite3);
                    break;

                case Halcyonite3BodyBehavior _:
                    SpawnGhost(SetupHalcyonite1);
                    SpawnGhost(SetupHalcyonite2);
                    break;
            }

            void SetupHalcyonite1(CharacterBody ghostBody)
            {
                ghostBody.gameObject.AddComponent<Halcyonite1BodyBehavior>().BossStateMachine.SetState(new Halcyonite1States.Support());
            }

            void SetupHalcyonite2(CharacterBody ghostBody)
            {
                ghostBody.gameObject.AddComponent<Halcyonite2BodyBehavior>().BossStateMachine.SetState(new Halcyonite2States.Support());
            }

            void SetupHalcyonite3(CharacterBody ghostBody)
            {
                var halcyoniteBodyBehavior = ghostBody.gameObject.AddComponent<Halcyonite3BodyBehavior>();
                halcyoniteBodyBehavior.rng = new Xoroshiro128Plus(bossFightBehavior.CombatDirector.rng.nextUlong);
                halcyoniteBodyBehavior.BossStateMachine.SetState(new Halcyonite3States.Support());
            }

            void SpawnGhost(Action<CharacterBody> variantSpecificSetup)
            {
                var spawnedInstance = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(this.halcyoniteSpawnCard.Value, new DirectorPlacementRule
                {
                    minDistance = minimumDistance,
                    maxDistance = maximumDistance,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    preventOverhead = false,
                    rotation = Quaternion.identity,
                    spawnOnTarget = mainBossBody.transform
                }, bossFightBehavior.CombatDirector.rng)
                {
                    teamIndexOverride = mainBossBody.teamComponent.teamIndex, // Do not set summonerBodyObject as we don't want the ghosts in the combat squad
                    ignoreTeamMemberLimit = true,
                });

                if (Utils.TryGetCharacterBody(spawnedInstance, out var spawnedBody))
                {
                    spawnedBody.MakeGhost();
                    spawnedBody.inventory.GiveItemPermanent(DLC1Content.Items.HalfSpeedDoubleHealth);
                    spawnedBody.ScaleCooldowns(this, 2);
                    ghostBodies.Add(spawnedBody);
                    variantSpecificSetup(spawnedBody);
                }
            }
        }
    }
}