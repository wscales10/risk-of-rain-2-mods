using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PactOfPunishment
{
    public class DisplayFullKillerNameInRunReport : Module
    {
        private readonly Dictionary<CharacterMaster, string> characterMasterToKillerBodyName = new Dictionary<CharacterMaster, string>();

        private delegate void ResolveKillerBodyNameDelegate(RunReport.PlayerInfo playerInfo, ref string killerBodyName);

        public override void Init()
        {
            IL.RoR2.CharacterMaster.OnBodyStart += Utils.HookIL(this.CharacterMaster_OnBodyStart);
            GlobalEventManager.onCharacterDeathGlobal += this.GlobalEventManager_onCharacterDeathGlobal; // This is easier than trying to add IL hook to anonymous method
            IL.RoR2.UI.GameEndReportPanelController.SetPlayerInfo += Utils.HookIL(this.GameEndReportPanelController_SetPlayerInfo);
            // On.RoR2.UI.GameEndReportPanelController.SetPlayerInfo += this.GameEndReportPanelController_SetPlayerInfo;
        }

        private void GameEndReportPanelController_SetPlayerInfo(On.RoR2.UI.GameEndReportPanelController.orig_SetPlayerInfo orig, GameEndReportPanelController self, RunReport.PlayerInfo playerInfo, int playerIndex)
        {
            orig(self, playerInfo, playerIndex);
            this.FixSpacing(self);
        }

        private void FixSpacing(GameEndReportPanelController panelController)
        {
            panelController.StartCoroutine(this.Foo(panelController));

            var killerArea = panelController.killerPanelObject;
            var verticalLayoutGroupController = killerArea.transform.parent.gameObject;

            var newChild1 = new GameObject("PlayerAndKillerInfoContainer", typeof(RectTransform));
            newChild1.transform.SetParent(verticalLayoutGroupController.transform, false);
            newChild1.transform.SetAsFirstSibling();
            newChild1.AddComponent<HorizontalLayoutGroup>();
            newChild1.EnsureComponent<LayoutElement>();
            var playerPanelObject = panelController.playerBodyLabel.transform.parent.gameObject;

            var newChild2 = new GameObject("LabelsContainer", typeof(RectTransform));
            newChild2.AddComponent<VerticalLayoutGroup>();
            newChild2.EnsureComponent<LayoutElement>();
            newChild2.transform.SetParent(newChild1.transform);

            panelController.playerBodyLabel.transform.SetParent(newChild2.transform, false);
            panelController.killerBodyLabel.transform.SetParent(newChild2.transform, false);

            panelController.killerPanelObject.transform.SetParent(newChild1.transform, false);
            playerPanelObject.transform.SetParent(newChild1.transform, false);
        }

        private IEnumerator Foo(GameEndReportPanelController panelController)
        {
            while (Run.instance)
            {
                yield return new WaitForSecondsRealtime(5);
                this.Logger.LogDebug(panelController.killerBodyLabel);
            }
        }

        private void GameEndReportPanelController_SetPlayerInfo(ILCursor c)
        {
            int killerBodyNameVariableNumber = -1;

            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<GameEndReportPanelController>(nameof(GameEndReportPanelController.killerBodyLabel)),
                x => x.MatchLdloc(out _),
                x => x.MatchLdloc(out killerBodyNameVariableNumber),
                x => x.MatchCall<string>(nameof(string.Format)),
                x => x.MatchCallvirt<TMP_Text>($"set_{nameof(TMP_Text.text)}")
            );
            c.GotoPrev(MoveType.After,
                x => x.MatchLdloc(19),
                x => x.MatchCallvirt<GameObject>(nameof(GameObject.GetComponent)),
                x => x.MatchLdfld<CharacterBody>(nameof(CharacterBody.baseNameToken)),
                x => x.MatchCall<Language>(nameof(Language.GetString)),
                x => x.MatchStloc(killerBodyNameVariableNumber));
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldloca_S, (byte)killerBodyNameVariableNumber);
            c.EmitDelegate<ResolveKillerBodyNameDelegate>((RunReport.PlayerInfo playerInfo, ref string killerBodyName) =>
            {
                if (this.characterMasterToKillerBodyName.TryGetValue(playerInfo.master, out string foundKillerBodyName))
                {
                    killerBodyName = foundKillerBodyName;
                    this.Logger.LogDebug($"Found killer body name for player '{playerInfo.name}': '{foundKillerBodyName}'");
                }
                else
                {
                    this.Logger.LogWarning($"Unable to find killer body name for player '{playerInfo.name}'");
                }
            });
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            CharacterMaster victimMaster = damageReport.victimMaster;

            if (victimMaster && victimMaster.playerCharacterMasterController && !victimMaster.GetInRemoteOp())
            {
                this.characterMasterToKillerBodyName[victimMaster] = Util.GetBestBodyName(damageReport.damageInfo.attacker);
            }
        }

        private void CharacterMaster_OnBodyStart(ILCursor c)
        {
            c.GotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdcI4(-1),
                x => x.MatchStfld<CharacterMaster>(nameof(CharacterMaster.killerBodyIndex)));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<CharacterMaster>>(self => this.characterMasterToKillerBodyName.Remove(self));
        }
    }
}