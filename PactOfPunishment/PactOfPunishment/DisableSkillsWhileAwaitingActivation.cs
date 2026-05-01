using EntityStates.InfiniteTowerSafeWard;
using RoR2;

namespace PactOfPunishment
{
    public class DisableSkillsWhileAwaitingActivation : Module
    {
        public override void Init()
        {
            On.EntityStates.InfiniteTowerSafeWard.AwaitingActivation.OnEnter += this.AwaitingActivation_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Active.OnEnter += this.Active_OnEnter;
            RoR2.TeamComponent.onJoinTeamGlobal += this.TeamComponent_onJoinTeamGlobal;
            RoR2.TeamComponent.onLeaveTeamGlobal += this.TeamComponent_onLeaveTeamGlobal;

            // TODO: also disable skils while waiting at start of Prime Meridian stage? or not?
        }

        private static void SetSkillsEnabled(TeamComponent member, bool enabled)
        {
            if (member && member.body)
            {
                if (enabled)
                {
                    member.body.RemoveBuff(DLC2Content.Buffs.DisableAllSkills);
                }
                else
                {
                    member.body.AddBuff(DLC2Content.Buffs.DisableAllSkills);
                }
            }
        }

        private void TeamComponent_onLeaveTeamGlobal(TeamComponent member, TeamIndex oldTeamIndex)
        {
            if (oldTeamIndex == TeamIndex.Player && Utils.GetSafeWardState() is AwaitingActivation)
            {
                if (member && member.body)
                {
                    SetSkillsEnabled(member, true);
                }
            }
        }

        private void TeamComponent_onJoinTeamGlobal(TeamComponent member, TeamIndex newTeamIndex)
        {
            if (newTeamIndex == TeamIndex.Player && Utils.GetSafeWardState() is AwaitingActivation)
            {
                if (member && member.body)
                {
                    SetSkillsEnabled(member, false);
                }
            }
        }

        private void Active_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Active.orig_OnEnter orig, Active self)
        {
            orig(self);
            foreach (var member in TeamComponent.GetTeamMembers(TeamIndex.Player))
            {
                if (member && member.body)
                {
                    SetSkillsEnabled(member, true);
                }
            }
        }

        private void AwaitingActivation_OnEnter(On.EntityStates.InfiniteTowerSafeWard.AwaitingActivation.orig_OnEnter orig, AwaitingActivation self)
        {
            orig(self);
            foreach (var member in TeamComponent.GetTeamMembers(TeamIndex.Player))
            {
                SetSkillsEnabled(member, false);
            }
        }
    }
}