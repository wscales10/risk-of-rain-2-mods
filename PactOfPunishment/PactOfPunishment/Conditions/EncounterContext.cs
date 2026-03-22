using EntityStates.MeridianEvent;
using HG;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public abstract class EncounterContext
    {
        protected EncounterContext()
        {
            this.GameObject.EnsureComponent<EncounterContextHolder>().encounterContext = this;
        }

        public abstract GameObject GameObject { get; }

        public abstract GameObject SpawnTarget { get; }

        public abstract CombatSquad CombatSquad { get; }

        public abstract CombatDirector CombatDirector { get; }

        public abstract MonoBehaviour Controller { get; }
    }

    public class EncounterContextHolder : MonoBehaviour
    {
        public EncounterContext encounterContext;
    }

    public class InfiniteTowerWaveContext : EncounterContext
    {
        private readonly InfiniteTowerWaveController wave;

        public InfiniteTowerWaveContext(InfiniteTowerWaveController wave)
        {
            this.wave = wave;
        }

        public override GameObject GameObject => this.wave.gameObject;

        public override GameObject SpawnTarget => this.wave.spawnTarget;

        public override CombatSquad CombatSquad => this.wave.combatSquad;

        public override CombatDirector CombatDirector => this.wave.combatDirector;

        public override MonoBehaviour Controller => this.wave;
    }

    public class FalseSonMinionContext : EncounterContext
    {
        private readonly MeridianEventTriggerInteraction meridianEventTriggerInteraction;

        public FalseSonMinionContext(MeridianEventTriggerInteraction meridianEventTriggerInteraction)
        {
            this.meridianEventTriggerInteraction = meridianEventTriggerInteraction;
        }

        public override GameObject GameObject => this.meridianEventTriggerInteraction.phase2CombatDirector;

        public override GameObject SpawnTarget => this.meridianEventTriggerInteraction.arenaCenter.gameObject;

        public override CombatSquad CombatSquad => this.CombatDirector.combatSquad;

        public override CombatDirector CombatDirector => this.GameObject.GetComponent<CombatDirector>();

        public override MonoBehaviour Controller => this.meridianEventTriggerInteraction;
    }

    public class FalseSonBossFightContext : EncounterContext
    {
        public FalseSonBossFightContext(FSBFPhaseBaseState phaseState)
        {
            this.PhaseState = phaseState;
        }

        public FSBFPhaseBaseState PhaseState { get; }

        public override GameObject GameObject => this.PhaseState.outer.gameObject;

        public override GameObject SpawnTarget => this.PhaseState.meridianEventTriggerInteraction.arenaCenter.gameObject;

        public override CombatSquad CombatSquad => this.PhaseState.phaseScriptedCombatEncounter.combatSquad;

        public override CombatDirector CombatDirector => this.PhaseState.meridianEventTriggerInteraction.combatDirector;

        public override MonoBehaviour Controller => this.PhaseState.outer;
    }
}