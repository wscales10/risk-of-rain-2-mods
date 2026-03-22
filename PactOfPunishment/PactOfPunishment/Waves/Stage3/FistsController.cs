using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace PactOfPunishment.Waves.Stage3
{
    public class FistsController : NetworkBehaviour
    {
        public static GameObject zoneProjectilePrefab;

        public IFistsStrategy strategy;

        private readonly List<CharacterMaster> halcyonites = new List<CharacterMaster>();

        private int halcyoniteIndex;

        private float timer;

        private float cooldown = 6;

        public static FistsController? Instance { get; private set; }

        public void AddHalcyonite(CharacterMaster halcyoniteMaster)
        {
            if (!this.halcyonites.Contains(halcyoniteMaster))
            {
                this.halcyonites.Add(halcyoniteMaster);
            }
        }

        public void Awake()
        {
            var rng = this.GetComponent<CombatDirector>().rng;
            this.strategy = rng.NextElementUniform(new Func<IFistsStrategy>[]
            {
                Create<RingAroundTargetFistsStrategy>,
                Create<ChaseTargetFistsStrategy>,
                Create<LineInFacingDirectionFistsStrategy>
            })();

            Instance = this;
            this.GetComponent<CombatSquad>().onMemberLost += this.CombatSquad_onMemberLost;

            static T Create<T>()
                where T : new() => new T();
        }

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void FixedUpdate()
        {
            this.ManagedFixedUpdate(Time.fixedDeltaTime);
        }

        private (CharacterBody?, GameObject?) GetHalcyoniteAndTarget()
        {
            CharacterBody? fallback = null;

            foreach (var halcyonite in this.halcyonites.Skip(this.halcyoniteIndex).Concat(this.halcyonites.Take(this.halcyoniteIndex)).ToArray())
            {
                this.halcyoniteIndex++;
                this.FixHalcyoniteIndex();

                var body = halcyonite.GetBody();
                fallback ??= body;

                foreach (var ai in halcyonite.AiComponents)
                {
                    var target = ai.currentEnemy.gameObject;

                    if (target)
                    {
                        return (body, target);
                    }
                }
            }

            return (fallback, null);
        }

        private void CombatSquad_onMemberLost(CharacterMaster obj) // TODO: it's weird that this is here but not the code for adding
        {
            this.halcyonites.Remove(obj);
            this.FixHalcyoniteIndex();
        }

        private void FixHalcyoniteIndex()
        {
            this.halcyoniteIndex = this.halcyonites.Count == 0 ? 0 : this.halcyoniteIndex % this.halcyonites.Count;
        }

        private void ManagedFixedUpdate(float deltaTime)
        {
            if (this.halcyonites.Count == 0)
            {
                return;
            }

            this.timer -= deltaTime;

            if (this.timer < 0)
            {
                this.timer = this.cooldown;

                if (this.hasAuthority)
                {
                    var (halcyonite, targetGameObject) = this.GetHalcyoniteAndTarget();

                    if (!halcyonite)
                    {
                        return;
                    }

                    if (!targetGameObject)
                    {
                        targetGameObject = this.gameObject;
                    }

                    this.StartCoroutine(this.strategy.PlaceFists(new PlaceFistsArgs(halcyonite!, targetGameObject!.transform, targetGameObject.GetComponent<CharacterBody>())));
                }
            }
        }
    }
}