using EntityStates.MeridianEvent;
using HG;
using RoR2;
using UnityEngine;

namespace PactOfPunishment.Conditions
{
    public abstract class EncounterContext
    {
        protected EncounterContext(GameObject gameObject)
        {
            this.GameObject = gameObject;
            this.GameObject.EnsureComponent<EncounterContextHolder>().encounterContext = this;
        }

        public GameObject GameObject { get; }

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

        public InfiniteTowerWaveContext(InfiniteTowerWaveController wave) : base(wave.gameObject)
        {
            this.wave = wave;
        }

        public override GameObject SpawnTarget => this.wave.spawnTarget;

        public override CombatSquad CombatSquad => this.wave.combatSquad;

        public override CombatDirector CombatDirector => this.wave.combatDirector;

        public override MonoBehaviour Controller => this.wave;
    }

    public class FalseSonMinionContext : EncounterContext
    {
        private readonly MeridianEventTriggerInteraction meridianEventTriggerInteraction;

        public FalseSonMinionContext(MeridianEventTriggerInteraction meridianEventTriggerInteraction) : base(meridianEventTriggerInteraction.phase2CombatDirector)
        {
            this.meridianEventTriggerInteraction = meridianEventTriggerInteraction;
        }

        public override GameObject SpawnTarget => this.meridianEventTriggerInteraction.arenaCenter.gameObject;

        public override CombatSquad CombatSquad => this.CombatDirector.combatSquad;

        public override CombatDirector CombatDirector => this.GameObject.GetComponent<CombatDirector>();

        public override MonoBehaviour Controller => this.meridianEventTriggerInteraction;
    }

    public class FalseSonBossFightContext : EncounterContext
    {
        public FalseSonBossFightContext(FSBFPhaseBaseState phaseState) : base(phaseState.outer.gameObject)
        {
            this.PhaseState = phaseState;
        }

        public FSBFPhaseBaseState PhaseState { get; }

        public override GameObject SpawnTarget => this.PhaseState.meridianEventTriggerInteraction.arenaCenter.gameObject;

        public override CombatSquad CombatSquad => this.PhaseState.phaseScriptedCombatEncounter.combatSquad;

        public override CombatDirector CombatDirector => this.PhaseState.meridianEventTriggerInteraction.combatDirector;

        public override MonoBehaviour Controller => this.PhaseState.outer;
    }
}