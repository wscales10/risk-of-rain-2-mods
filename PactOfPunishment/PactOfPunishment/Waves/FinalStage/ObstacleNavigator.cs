using RoR2;
using RoR2.CharacterAI;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.FinalStage
{
    [RequireComponent(typeof(CharacterBody))]
    public class ObstacleNavigator : MonoBehaviour
    {
        private float frustration;

        private CharacterBody body;

        private EntityStateMachine bodyStateMachine;

        private bool isCharging = true;

        public bool IsInNavigateAroundObstacleMode => !this.isCharging;

        private float dischargeTimer;

        private bool IsSprintingAfterTarget => (this.bodyStateMachine?.state is EntityStates.FalseSonBoss.FalseSonBossGenericStateWithSwing) &&
            this.body && this.body.master && this.body.master.AiComponents?.Any(x => x.selectedSkilldriverName == "Sprint After Target") == true;

        public void Awake()
        {
            this.body = this.GetComponent<CharacterBody>();
            this.bodyStateMachine = EntityStateMachine.FindByCustomName(this.body.gameObject, "Body");
        }

        public void FixedUpdate()
        {
            this.ManagedUpdate(Time.fixedDeltaTime);
        }

        private void ManagedUpdate(float deltaTime)
        {
            float actualSpeed = this.body.characterMotor.velocity.magnitude;
            float expectedSpeed = this.body.inputBank.moveVector.magnitude * this.body.moveSpeed;

            float increaseRate = Mathf.Approximately(expectedSpeed, 0) ? 0 : Mathf.Clamp01(1 - actualSpeed / expectedSpeed);

            if (Mathf.Approximately(increaseRate, 0))
            {
                this.frustration = Mathf.Max(0, this.frustration - deltaTime * 2);
            }
            else if (this.IsSprintingAfterTarget)
            {
                this.frustration += deltaTime * increaseRate;
            }

            if (this.isCharging)
            {
                if (this.frustration > 1)
                {
                    this.dischargeTimer = 4;
                    this.isCharging = false;
                    this.SetIgnoreNodeGraph(false);
                }
            }
            else
            {
                if (this.frustration < 0.5f && this.IsSprintingAfterTarget)
                {
                    this.dischargeTimer -= deltaTime;
                }

                if (this.dischargeTimer < 0)
                {
                    this.isCharging = true;
                    this.SetIgnoreNodeGraph(true);
                }
            }
        }

        private void SetIgnoreNodeGraph(bool ignoreNodeGraph)
        {
            foreach (var skillDriver in this.body?.master?.GetSkillDrivers("Sprint After Target") ?? Enumerable.Empty<AISkillDriver>())
            {
                skillDriver.ignoreNodeGraph = ignoreNodeGraph;
            }
        }
    }
}