using RoR2;

namespace PactOfPunishment
{
    public class Logging : Module
    {
        private bool logStateChanges;

        private Logging()
        {
        }

        public static Logging Instance { get; } = new Logging();

        public bool LogStateChanges
        {
            get => this.logStateChanges;

            set
            {
                this.logStateChanges = value;

                if (this.logStateChanges)
                {
                    On.RoR2.EntityStateMachine.SetState += this.EntityStateMachine_SetState;
                }
                else
                {
                    On.RoR2.EntityStateMachine.SetState -= this.EntityStateMachine_SetState;
                }
            }
        }

        public override void Init()
        {
            this.LogStateChanges = false;
        }

        private void EntityStateMachine_SetState(On.RoR2.EntityStateMachine.orig_SetState orig, EntityStateMachine self, EntityStates.EntityState newState)
        {
            this.Logger.LogDebug($"{self.gameObject} {self.customName} state changing from '{self.state?.GetType().Name}' to '{newState?.GetType().Name}'");
            orig(self, newState);
        }
    }
}