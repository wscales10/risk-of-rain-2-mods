using RoR2;
using System;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public class FallRiskMitigator : MonoBehaviour
    {
        public Mode CurrentMode;

        public enum Mode // this is bad OOP but I don't care right now
        {
            Halcyonite,

            Mithrix
        }

        public bool? IsAboveGround { get; private set; }

        public static bool IsAboveGroundInternal(Transform transform, Mode mode)
        {
            switch (mode)
            {
                case Mode.Halcyonite:
                    return Physics.Raycast(transform.position, Vector3.down, 35, LayerIndex.world.mask, QueryTriggerInteraction.Ignore);

                case Mode.Mithrix:
                    float desiredHeightAboveSeaLevel = Physics.Raycast(transform.position, Vector3.down, 1000, LayerIndex.world.mask, QueryTriggerInteraction.Ignore) ? -15 : -1;
                    if (Run.instance is InfiniteTowerRun run && run.waveController is InfiniteTowerWaveController waveController && waveController.spawnTarget is GameObject spawnTarget)
                    {
                        return (transform.position - spawnTarget.transform.position).y > desiredHeightAboveSeaLevel;
                    }

                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unsupported mode");
            }
        }

        public static bool IsInDangerOfFalling(CharacterBody body) // TODO: look at references to this and don't apply effects without FallRiskMitigator component?
        {
            bool? isAboveGround;

            if (!body)
            {
                isAboveGround = null;
            }
            else if (body.TryGetComponent<FallRiskMitigator>(out var behavior))
            {
                isAboveGround = behavior.IsAboveGround;
            }
            else
            {
                isAboveGround = IsAboveGroundInternal(body.transform, Mode.Halcyonite);
            }

            return isAboveGround == false;
        }

        public void DoUpdate(Transform? transform)
        {
            if (transform == null)
            {
                this.IsAboveGround = null;
            }
            else
            {
                this.IsAboveGround = IsAboveGroundInternal(transform, this.CurrentMode);
            }
        }
    }
}