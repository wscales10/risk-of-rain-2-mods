using JetBrains.Annotations;
using RoR2;
using System.Linq;

namespace PactOfPunishment.BugFixes
{
    public class GlassArtifact : Module
    {
        public override void Init()
        {
            RunArtifactManager.onArtifactEnabledGlobal += this.RunArtifactManager_onArtifactToggledGlobal;
            RunArtifactManager.onArtifactDisabledGlobal += this.RunArtifactManager_onArtifactToggledGlobal;
        }

        private void RunArtifactManager_onArtifactToggledGlobal([NotNull] RunArtifactManager runArtifactManager, [NotNull] ArtifactDef artifactDef)
        {
            if (artifactDef != RoR2Content.Artifacts.glassArtifactDef)
            {
                return;
            }

            foreach (var member in TeamComponent.GetTeamMembers(TeamIndex.Player).ToArray())
            {
                if (member && member.body)
                {
                    member.body.MarkAllStatsDirty();
                }
            }
        }
    }
}