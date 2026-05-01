using RoR2;
using System.Linq;
using UnityEngine;
using static PactOfPunishment.RebirthPlus.RebirthPlus;

namespace PactOfPunishment.RebirthPlus
{
    public class RebirthPlusBehavior : MonoBehaviour
    {
        private readonly ILevelInfo[] levels = GetLevels();

        internal static IItemChoiceStrategy itemChoiceStrategy = new RandomItemChoiceStrategy();

        public void Awake()
        {
            Stage.onStageStartGlobal += this.OnStageStart;
        }

        internal static ILevelInfo[] GetLevels()
        {
            return new LevelInfo[]
            {
                new LevelInfo((DLC3Content.Items.CritAtLowerElevation, 3), (RoR2Content.Items.Crowbar, 1)),
                new LevelInfo((RoR2Content.Items.TPHealingNova, 1), RoR2Content.Items.HealOnCrit),
                new LevelInfo((DLC1Content.Items.HealingPotion, 3), (DLC2Content.Items.TeleportOnLowHealth, 2)),
                new LevelInfo(RoR2Content.Items.Feather, DLC3Content.Equipment.Parry),
                new LevelInfo(RoR2Content.Items.ArmorReductionOnHit, (DLC1Content.Items.SlowOnHitVoid, 1)),
                new LevelInfo((RoR2Content.Items.SecondarySkillMagazine, 2), RoR2Content.Items.AutoCastEquipment),
                new LevelInfo(DLC1Content.Equipment.BossHunter, DLC2Content.Items.ExtraStatsOnLevelUp),
                new LevelInfo((RoR2Content.Items.FlatHealth, 4), DLC3Content.Items.UltimateMeal),
                new LevelInfo((RoR2Content.Items.DeathMark, 2), DLC2Content.Items.BoostAllStats),
                new LevelInfo(DLC3Content.Items.Duplicator, DLC2Content.Items.ItemDropChanceOnKill),
                new LevelInfo((DLC1Content.Items.RegeneratingScrap, 5), DLC1Content.Items.CloverVoid),
                new LevelInfo(DLC2Content.Items.LowerPricedChests, RoR2Content.Equipment.Recycle)
            }.Select((level, i) =>
            {
                level.index = i;
                return level;
            }).ToArray();
        }

        private void OnStageStart(Stage obj)
        {
            Stage.onStageStartGlobal -= this.OnStageStart;

            foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
            {
                this.GiveStartingItems(player, GetLevelCount?.Invoke(levels.Length) ?? levels.Length);
            }
        }

        private void GiveStartingItems(PlayerCharacterMasterController player, int count)
        {
            Inventory inventory = player.master.inventory;

            if (levels.Length < count)
            {
                Debug.LogWarning($"Number of levels {levels.Length} is less than count {count}");
                count = levels.Length;
            }

            for (int i = 0; i < count; i++)
            {
                var level = levels[i];
                var pickup = itemChoiceStrategy.ChoosePickup(level);
                if (pickup == null)
                {
                    Debug.LogWarning($"Failed to get pickup for level {i}.");
                }
                else
                {
                    Debug.Log($"Granting {pickup} to player.");
                    pickup.GiveTo(inventory);
                }
            }
        }
    }
}