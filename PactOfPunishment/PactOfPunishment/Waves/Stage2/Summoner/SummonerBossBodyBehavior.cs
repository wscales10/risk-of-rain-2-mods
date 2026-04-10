using R2API;
using RoR2;
using RoR2.CharacterAI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage2.Summoner
{
    public partial class SummonerBossBodyBehavior : MonoBehaviour
    {
        public readonly List<CharacterBody> ghostBodies = new List<CharacterBody>();

        public SummonerBossPowerLevel PowerLevel = SummonerBossPowerLevel.Phase1;

        public SummonerBossType BossType;

        public bool IsUpgraded;

        private CharacterBody? body;

        private float? bodyCost;

        // Custom property to allow overriding the body cost for testing purposes
        internal float BodyCost
        {
            get => this.bodyCost ??= this.Body.cost;
            set => this.bodyCost = value;
        }

        private CharacterBody Body => this.body ??= this.GetComponent<CharacterBody>();

        public void OnEnable()
        {
            this.Body.characterMotor.mass = Mathf.Max(this.Body.characterMotor.mass, 900);
            this.Body.rigidbody.mass = Mathf.Max(this.Body.rigidbody.mass, 900);
            RecalculateStats.Add(this.Body, this.OnRecalculateStats);
        }

        public void OnDisable()
        {
            RecalculateStats.Remove(this.Body, this.OnRecalculateStats);
        }

        private static void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args, SkillLocator skillLocator, CharacterDirection characterDirection, float bodyCost, SummonerBossPowerLevel powerLevel, SummonerBossType bossType, bool isUpgraded)
        {
            var primary = skillLocator.primary;
            var secondary = skillLocator.secondary;

            if (primary)
            {
                primary.cooldownOverride = 10; // TODO: maybe try coding a way to add 10s instead of replacing the original value.
            }

            if (secondary)
            {
                secondary.cooldownOverride = 10; // TODO: maybe try coding a way to add 10s instead of replacing the original value.
            }

            switch (bossType)
            {
                case SummonerBossType.Normal:
                    args.moveSpeedTotalMult /= 2f;

                    var myArgs = new Args();

                    args.damageTotalMult *= powerLevel switch
                    {
                        SummonerBossPowerLevel.Support => 0.6f,
                        _ => bodyCost < 31 ? 1 : 0.6f,
                    };

                    switch (powerLevel)
                    {
                        case SummonerBossPowerLevel.Support:
                        case SummonerBossPowerLevel.Phase1:
                            if (bodyCost < 55)
                            {
                                myArgs.AttackSpeedTotalMult = 3.5f;
                                myArgs.BonusStock = 2;
                            }

                            if (bodyCost < 31)
                            {
                                myArgs.BonusStock = 4;
                            }

                            break;

                        case SummonerBossPowerLevel.FirstInterlude:
                            if (bodyCost < 55)
                            {
                                myArgs.AttackSpeedTotalMult = 4f;
                                myArgs.BonusStock = 2;
                            }

                            if (bodyCost < 31)
                            {
                                myArgs.BonusStock = 4;
                            }

                            break;

                        case SummonerBossPowerLevel.SecondInterlude:
                            if (bodyCost < 55)
                            {
                                myArgs.AttackSpeedTotalMult = 4.8f;
                                myArgs.BonusStock = 4;
                            }

                            break;

                        case SummonerBossPowerLevel.Phase2:
                            args.specialSkill.cooldownMultiplier *= 1 - 0.3f * Mathf.Clamp01((800 - bodyCost) / 200f);

                            if (bodyCost < 500)
                            {
                                myArgs.BonusStock = 1;
                                myArgs.AttackSpeedTotalMult = 1.18f;
                            }

                            if (bodyCost < 55)
                            {
                                myArgs.AttackSpeedTotalMult = 4.6f;
                                myArgs.BonusStock = 4;
                            }

                            if (bodyCost < 31)
                            {
                                myArgs.AttackSpeedTotalMult = 15;
                                myArgs.BonusStock = 12;
                            }

                            break;

                        case SummonerBossPowerLevel.Phase3:
                            myArgs.BonusStock = 2;
                            args.specialSkill.cooldownMultiplier *= 1 - 0.5f * Mathf.Clamp01((800 - bodyCost) / 200f);

                            if (bodyCost < 500)
                            {
                                myArgs.AttackSpeedTotalMult = 1.36f;
                            }

                            if (bodyCost < 55)
                            {
                                myArgs.AttackSpeedTotalMult = 5;
                                myArgs.BonusStock = 7;
                            }

                            if (bodyCost < 31)
                            {
                                myArgs.AttackSpeedTotalMult = 15;
                                myArgs.BonusStock = 12;
                            }

                            break;
                    }

                    args.attackSpeedTotalMult *= myArgs.AttackSpeedTotalMult;
                    FrontloadPrimaryAndSecondary(myArgs.BonusStock);

                    break;

                case SummonerBossType.SlammerGhost:
                    args.damageTotalMult *= 0.5f;
                    args.attackSpeedTotalMult *= 5; // TODO: reduce for earlier phases???

                    switch (powerLevel)
                    {
                        case SummonerBossPowerLevel.Phase3:
                            FrontloadPrimary(2);
                            break;
                    }

                    switch (powerLevel)
                    {
                        case SummonerBossPowerLevel.Phase3:
                        case SummonerBossPowerLevel.SecondInterlude:
                            if (isUpgraded)
                            {
                                // TODO: undo turn speed?
                                characterDirection.turnSpeed = 720;
                                args.moveSpeedMultAdd += 0.4f;
                            }
                            break;
                    }
                    break;

                case SummonerBossType.LungerGhost:
                    args.damageTotalMult *= powerLevel switch
                    {
                        SummonerBossPowerLevel.Support => 0.5f,
                        SummonerBossPowerLevel.Phase1 => 0.5f,
                        _ => 1.6f,
                    };

                    args.attackSpeedTotalMult *= 1.36f;

                    switch (powerLevel)
                    {
                        case SummonerBossPowerLevel.Phase3:
                        case SummonerBossPowerLevel.SecondInterlude:
                            if (isUpgraded)
                            {
                                args.primarySkill.bonusStockAdd++;
                            }
                            break;
                    }
                    break;
            }

            void FrontloadPrimary(int bonusStock)
            {
                FrontloadSkill(ref args.primarySkill, primary, bonusStock);
            }

            void FrontloadPrimaryAndSecondary(int bonusStock)
            {
                FrontloadSkill(ref args.primarySkill, primary, bonusStock);
                FrontloadSkill(ref args.secondarySkill, secondary, bonusStock);
            }

            void FrontloadSkill(ref RecalculateStatsAPI.SkillSlotStatModifiers skillArgs, GenericSkill skill, int bonusStock)
            {
                if (bonusStock < 1)
                {
                    return;
                }

                skillArgs.bonusStockAdd += bonusStock;
                if (skill)
                {
                    skill.OverrideRechargeStock((_, self) => self.maxStock);
                    Utils.EnsureSafeMinimumInterruptPriority(skill);

                    foreach (var skillDriver in skill.characterBody.GetComponents<BaseAI>().SelectMany(x => x.skillDrivers).Where(x => x.skillSlot == skillLocator.FindSkillSlot(skill)))
                    {
                        skillDriver.noRepeat = false;
                    }
                }
            }
        }

        private void OnRecalculateStats(RecalculateStatsAPI.StatHookEventArgs args)
        {
            OnRecalculateStats(args, this.Body.skillLocator, this.Body.characterDirection, this.BodyCost, this.PowerLevel, this.BossType, this.IsUpgraded);
        }

        private class Args
        {
            private float attackSpeedTotalMult = 1;

            private int bonusStock;

            public int BonusStock
            {
                get => this.bonusStock;

                set
                {
                    if (value <= this.bonusStock)
                    {
                        Debug.LogError($"Bonus stock should only be set to a higher value. Current value: {this.bonusStock}, attempted new value: {value}");
                    }

                    this.bonusStock = value;
                }
            }

            public float AttackSpeedTotalMult
            {
                get => this.attackSpeedTotalMult;

                set
                {
                    if (value <= this.attackSpeedTotalMult)
                    {
                        Debug.LogError($"Attack speed total multiplier should only be set to a higher value. Current value: {this.attackSpeedTotalMult}, attempted new value: {value}");
                    }

                    this.attackSpeedTotalMult = value;
                }
            }
        }
    }
}