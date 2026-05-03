using PactOfPunishment.Navigation;
using PactOfPunishment.Waves.Common;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.AiSkillDrivers
{
    [RequireComponent(typeof(CharacterBody))]
    public class ObstacleNavigator : BossBodyBehavior
    {
        private readonly HashSet<string> applicableSkillDriverCustomNames = new HashSet<string>();

        private FrustrationMonitor frustrationMonitor;

        private EntityStateMachine? bodyStateMachine;

        public bool IsInNavigateAroundObstacleMode => !this.frustrationMonitor.IsBuildingFrustration;

        private bool IsUsingApplicableSkillDriver => this.bodyStateMachine?.IsInMainState() == true &&
            this.Body && this.Body.master && this.Body.master.AiComponents.Any(x => this.applicableSkillDriverCustomNames.Contains(x.selectedSkilldriverName));

        protected override void Awake()
        {
            this.frustrationMonitor = new FrustrationMonitor(() => this.IsUsingApplicableSkillDriver);
            this.frustrationMonitor.IsBuildingFrustrationChanged += this.FrustrationMonitor_IsBuildingFrustrationChanged;
            base.Awake();
            this.bodyStateMachine = EntityStateMachine.FindByCustomName(this.Body.gameObject, "Body");

            foreach (var skillDriver in this.Body.GetSkillDrivers())
            {
                if (skillDriver.ignoreNodeGraph && skillDriver.skillSlot == SkillSlot.None)
                {
                    this.applicableSkillDriverCustomNames.Add(skillDriver.customName);
                }
            }
        }

        protected override void ManagedFixedUpdate(float deltaTime)
        {
            base.ManagedFixedUpdate(deltaTime);

            float actualSpeed = this.Body.rigidbody.velocity.magnitude;
            float expectedSpeed = this.Body.inputBank.moveVector.magnitude * this.Body.moveSpeed;
            this.frustrationMonitor.Update(expectedSpeed, actualSpeed, deltaTime);
        }

        private void FrustrationMonitor_IsBuildingFrustrationChanged(bool value)
        {
            this.SetIgnoreNodeGraph(value);
        }

        private void SetIgnoreNodeGraph(bool ignoreNodeGraph)
        {
            foreach (var skillDriver in this.applicableSkillDriverCustomNames.SelectMany(x => this.Body?.master.GetSkillDrivers(x)))
            {
                skillDriver.ignoreNodeGraph = ignoreNodeGraph;
            }
        }
    }
}