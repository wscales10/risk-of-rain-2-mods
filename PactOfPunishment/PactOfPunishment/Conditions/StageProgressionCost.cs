using HG;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace PactOfPunishment.Conditions
{
    public sealed class StageProgressionCost : DefaultConditionDef
    {
        private const int itemsToScrapPerRank = 2;

        private AssetPromise<InteractableSpawnCard> scrapperCard;

        public override int MaxRank => 1;

        public override int HeatPerRank => 2;

        public override string Description => string.Format(base.Description, itemsToScrapPerRank);

        public override void Init()
        {
            IL.RoR2.InfiniteTowerRun.OnWaveAllEnemiesDefeatedServer += this.InfiniteTowerRun_OnWaveAllEnemiesDefeatedServer;
            IL.RoR2.ScrapperController.BeginScrapping_UniquePickup += Utils.HookIL(BeginScrapping_UniquePickup);
            On.RoR2.InfiniteTowerRun.Start += this.InfiniteTowerRun_Start;
            On.RoR2.PickupPickerController.GetGeneratedOptionsFromInteractor += this.PickupPickerController_GetGeneratedOptionsFromInteractor;
            On.RoR2.Interactor.PerformInteraction += this.Interactor_PerformInteraction;
            Utils.RegisterChatMessageType<SpecialScrapperBehavior.ChatMessage>();
        }

        private static void BeginScrapping_UniquePickup(ILCursor c)
        {
            c.GotoNext(MoveType.After,
                x => x.MatchLdloca(out _),
                x => x.MatchLdarg(0),
                x => x.MatchCall<ScrapperController>($"get_{nameof(ScrapperController.pickupPrintQueue)}"),
                x => x.MatchCall<Inventory.ItemAndStackValues>(nameof(Inventory.ItemAndStackValues.AddAsPickupsToList)),
                x => x.MatchPop());

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<ScrapperController>>(self =>
            {
                if (self.pickupPickerController.TryGetComponent<SpecialScrapperBehavior>(out var behavior))
                {
                    behavior.OnItemScrapped(self.interactor.GetComponent<CharacterBody>());
                }
            });
        }

        private void Interactor_PerformInteraction(On.RoR2.Interactor.orig_PerformInteraction orig, Interactor self, GameObject interactableObject)
        {
            orig(self, interactableObject);

            if (interactableObject.TryGetComponent<DisabledPortalMarker>(out var marker) && marker.enabled)
            {
                foreach (var instance in InstanceTracker.GetInstancesList<SpecialScrapperBehavior>())
                {
                    instance.ExitAttempted();
                }
            }
        }

        private List<PickupPickerController.Option> PickupPickerController_GetGeneratedOptionsFromInteractor(On.RoR2.PickupPickerController.orig_GetGeneratedOptionsFromInteractor orig, PickupPickerController self, Interactor activator)
        {
            var originalResult = orig(self, activator);

            if (self.TryGetComponent<SpecialScrapperBehavior>(out var behavior))
            {
                behavior.ModifyGeneratedOptions(activator.GetComponent<CharacterBody>(), originalResult);
            }

            return originalResult;
        }

        private void InfiniteTowerRun_Start(On.RoR2.InfiniteTowerRun.orig_Start orig, InfiniteTowerRun self)
        {
            this.scrapperCard = Utils.BeginLoad<InteractableSpawnCard>("RoR2/Base/Scrapper/iscScrapper.asset", this.Logger);
            orig(self);
        }

        private void InfiniteTowerRun_OnWaveAllEnemiesDefeatedServer(ILContext il)
        {
            var c = new ILCursor(il);

            c.Index = c.Instrs.Count - 1;
            c.GotoPrev(x => x.MatchCallvirt<DirectorCore>(nameof(DirectorCore.TrySpawnObject)), x => x.MatchPop()); // TODO: more robust check for portal spawning
            c.Index++;
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<GameObject, InfiniteTowerRun>>((portalObject, self) =>
            {
                var scrapper = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(this.scrapperCard.Value, new DirectorPlacementRule
                {
                    minDistance = 0f,
                    maxDistance = self.stageTransitionPortalMaxDistance,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    position = self.safeWardController.transform.position,
                    spawnOnTarget = self.safeWardController.transform
                }, self.safeWardRng));

                this.TrySpawnGreenPortal(self);

                if (!scrapper)
                {
                    this.Logger.LogError("Failed to spawn end-of-stage scrapper.");
                    return;
                }

                if (!portalObject.GetComponent<SceneExitController>())
                {
                    throw new InvalidOperationException("Not a portal");
                }

                var behavior = scrapper.GetComponent<ScrapperController>().pickupPickerController.EnsureComponent<SpecialScrapperBehavior>();
                behavior.NumberOfSlots = itemsToScrapPerRank * 3;

                if (!this.IsEnabled(self))
                {
                    return;
                }

                // TODO: disable "activate portal" objective while portal not enabled
                var marker = portalObject.AddComponent<DisabledPortalMarker>();
                behavior.onEnoughItemsScrapped = () => marker.enabled = false;
                behavior.OpenShop(this.GetRank(self) * itemsToScrapPerRank);
            });
        }

        private void TrySpawnGreenPortal(InfiniteTowerRun run)
        {
            if (run.TryGetComponent<PrimeMeridian.GreenPortalSpawnerBehavior>(out var behavior) && behavior.GreenPortalSpawner is PortalSpawner ps) // TODO: move to other class.
            {
                ps.spawnReferenceLocationOverride = run.safeWardController.transform;

                if (!ps.AttemptSpawnPortalServer())
                {
                    this.Logger.LogWarning("Failed to spawn green portal");
                }
            }
        }

        public class DisabledPortalMarker : MonoBehaviour
        {
            public void OnEnable()
            {
                this.GetComponent<GenericInteraction>().SetInteractabilityConditionsNotMet();
            }

            public void OnDisable()
            {
                this.GetComponent<GenericInteraction>()?.SetInteractabilityAvailable();
            }
        }

        [RequireComponent(typeof(PickupPickerController))]
        public class SpecialScrapperBehavior : MonoBehaviour
        {
            public Action? onEnoughItemsScrapped;

            private readonly Dictionary<PlayerCharacterMasterController, PlayerInfo> dictionary = new Dictionary<PlayerCharacterMasterController, PlayerInfo>();

            private PickupPickerController pickupPickerController;

            public int TotalItemsToScrap { get; private set; }

            public int NumberOfSlots { get; internal set; }

            public void Awake()
            {
                this.pickupPickerController = this.GetComponent<PickupPickerController>();
            }

            public void OnEnable()
            {
                InstanceTracker.Add<SpecialScrapperBehavior>(this);
            }

            public void OnDisable()
            {
                InstanceTracker.Remove<SpecialScrapperBehavior>(this);
            }

            public void OpenShop(int numberOfItemsToScrap)
            {
                this.TotalItemsToScrap = numberOfItemsToScrap;

                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    // TODO: what about dead/disconnected players? what about players joining/leaving?
                    this.UpdateForPlayer(player);
                }
            }

            public void OnItemScrapped(CharacterBody activatorBody)
            {
                if (!this.TryGetPlayerInfo(activatorBody, out var info))
                {
                    return;
                }

                if (info!.TimesLeftToScrap > 0)
                {
                    info.TimesLeftToScrap--;

                    if (info.TimesLeftToScrap < 1)
                    {
                        info.TimesLeftToScrap = 0;
                    }
                }

                this.TryCloseShop();
            }

            internal void ModifyGeneratedOptions(CharacterBody activatorBody, List<PickupPickerController.Option> originalResult)
            {
                if (!this.TryGetPlayerInfo(activatorBody, out var info))
                {
                    return;
                }

                List<int> availableIndices = Enumerable.Range(0, originalResult.Count).ToList();

                for (int i = 0; i < info!.options.Length; i++)
                {
                    var option = info.options[i];

                    var index = option.HasValue ? originalResult.IndexOf(option.Value) : -1;

                    if (index >= 0)
                    {
                        availableIndices.Remove(index);
                    }
                    else
                    {
                        info.options[i] = null;
                    }
                }

                for (int i = 0; i < info.options.Length; i++)
                {
                    if (availableIndices.Count == 0)
                    {
                        break;
                    }

                    if (info.options[i] == null)
                    {
                        var indexOfIndex = Run.instance.treasureRng.RangeInt(0, availableIndices.Count);
                        info.options[i] = originalResult[availableIndices[indexOfIndex]];
                        availableIndices.RemoveAt(indexOfIndex);
                    }
                }

                originalResult.Clear();
                originalResult.AddRange(info.options.Where(x => x.HasValue).Select(x => x!.Value));
            }

            internal void ExitAttempted()
            {
                foreach (var kvp in this.dictionary)
                {
                    if (kvp.Value.TimesLeftToScrap > 0)
                    {
                        Chat.SendBroadcastChat(new ChatMessage
                        {
                            timesLeftToScrap = kvp.Value.TimesLeftToScrap,
                            totalTimesToScrap = kvp.Value.TotalTimesToScrap,
                            subjectAsNetworkUser = kvp.Key.networkUser,
                        });
                    }
                }
            }

            private void UpdateForPlayer(PlayerCharacterMasterController player)
            {
                if (!player)
                {
                    return;
                }

                var master = player.master;

                if (!player.master)
                {
                    return;
                }

                var bodyObject = master.GetBodyObject();

                if (!bodyObject)
                {
                    return;
                }

                this.pickupPickerController.GetGeneratedOptionsFromInteractor(bodyObject.GetComponent<Interactor>());
            }

            private void TryCloseShop()
            {
                // TODO: fix edge cases (not enough items to scrap etc)

                if (this.dictionary.Values.Any(x => x.TimesLeftToScrap > 0))
                {
                    return;
                }

                var callback = this.onEnoughItemsScrapped;
                this.onEnoughItemsScrapped = null;
                callback?.Invoke();
            }

            private bool TryGetPlayerInfo(CharacterBody activatorBody, out PlayerInfo? info)
            {
                var master = activatorBody.master;

                if (!master)
                {
                    info = null;
                    return false;
                }

                var player = master.playerCharacterMasterController;

                if (!player)
                {
                    info = null;
                    return false;
                }

                if (!this.dictionary.TryGetValue(player, out info))
                {
                    info = new PlayerInfo(this.NumberOfSlots) { TotalTimesToScrap = this.TotalItemsToScrap };
                    this.dictionary.Add(player, info);
                }

                return true;
            }

            public class ChatMessage : Chat.SubjectFormatChatMessage
            {
                public int totalTimesToScrap;

                public int timesLeftToScrap;

                public ChatMessage()
                {
                    this.baseToken = "OBJECTIVE_FRACTION_PROGRESS_FORMAT";
                }

                public override string ConstructChatString()
                {
                    try
                    {
                        return $"{this.GetSubjectName()} {Language.GetStringFormatted(this.baseToken, Language.GetString("SCRAPPER_CONTEXT"), this.totalTimesToScrap - this.timesLeftToScrap, this.totalTimesToScrap)}";
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                    }

                    return string.Empty;
                }

                public override void Serialize(NetworkWriter writer)
                {
                    base.Serialize(writer);
                    writer.Write(this.totalTimesToScrap);
                    writer.Write(this.timesLeftToScrap);
                }

                public override void Deserialize(NetworkReader reader)
                {
                    base.Deserialize(reader);
                    this.totalTimesToScrap = reader.ReadInt32();
                    this.timesLeftToScrap = reader.ReadInt32();
                }
            }

            private sealed class PlayerInfo : UnityEngine.Object
            {
                public readonly PickupPickerController.Option?[] options;

                public bool IsDirty = true;

                private int timesLeftToScrap;

                private int totalTimesToScrap;

                public PlayerInfo(int numberOfSlots)
                {
                    this.options = new PickupPickerController.Option?[numberOfSlots];
                }

                public int TotalTimesToScrap
                {
                    get => this.totalTimesToScrap;

                    set
                    {
                        if (this.totalTimesToScrap == value)
                        {
                            return;
                        }

                        this.totalTimesToScrap = value;
                        this.TimesLeftToScrap = value;
                    }
                }

                public int TimesLeftToScrap
                {
                    get => this.timesLeftToScrap;

                    set
                    {
                        this.timesLeftToScrap = value;
                        this.IsDirty = true;

                        if (value > 0)
                        {
                            ObjectivePanelController.collectObjectiveSources += this.ObjectivePanelController_collectObjectiveSources;
                        }
                        else
                        {
                            ObjectivePanelController.collectObjectiveSources -= this.ObjectivePanelController_collectObjectiveSources;
                        }
                    }
                }

                private void ObjectivePanelController_collectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> output)
                {
                    output.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
                    {
                        source = this,
                        master = master,
                        objectiveType = typeof(ObjectiveTracker)
                    });
                }
            }

            private sealed class ObjectiveTracker : ObjectivePanelController.ObjectiveTracker
            {
                public ObjectiveTracker()
                {
                    this.baseToken = "SCRAPPER_CONTEXT";
                }

                private PlayerInfo Source => (PlayerInfo)this.sourceDescriptor.source;

                public override string GenerateString()
                {
                    this.Source.IsDirty = false;
                    return Language.GetStringFormatted("OBJECTIVE_FRACTION_PROGRESS_FORMAT", base.GenerateString(), this.Source.TotalTimesToScrap - this.Source.TimesLeftToScrap, this.Source.TotalTimesToScrap);
                }

                public override bool IsDirty() => this.Source.IsDirty;
            }
        }
    }
}