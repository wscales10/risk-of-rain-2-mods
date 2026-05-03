using HG;
using RoR2;
using RoR2.UI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using static RoR2.TimerStringFormatter;

namespace PactOfPunishment.Conditions
{
    public sealed class LimitRunTime : ConditionDef
    {
        private const int numberOfEarlyStages = 3;

        private const int minutesPerStageAtRank1 = 11;

        private const int minutesPerStageReductionPerRank = 2;

        private static float damagePerSecond = 10;

        public override int MaxRank => 3;

        public override string Description => string.Format(base.Description, numberOfEarlyStages, minutesPerStageAtRank1, minutesPerStageReductionPerRank);

        public static void PauseTimer()
        {
            var run = Run.instance;

            if (run && run.TryGetComponent<LimitRunTimeBehavior>(out var behavior))
            {
                behavior.Pause();
            }
        }

        public static void ResumeTimer()
        {
            var run = Run.instance;

            if (run && run.TryGetComponent<LimitRunTimeBehavior>(out var behavior))
            {
                behavior.Resume();
            }
        }

        public override int GetHeatForRank(int rank) => rank;

        public override void Init()
        {
            // Create LimitRunTimeBehavior
            On.RoR2.InfiniteTowerRun.Start += this.InfiniteTowerRun_Start;

            // Add time
            On.RoR2.InfiniteTowerRun.OnServerStageBegin += this.InfiniteTowerRun_OnServerStageBegin;

            // Pause timer on exit scene
            On.RoR2.SceneExitController.SetState += this.SceneExitController_SetState;

            // Pause timer when final boss defeated
            On.EntityStates.MeridianEvent.Phase3.OnBossGroupDefeated += this.Phase3_OnBossGroupDefeated;

            // Initialise GUI
            On.RoR2.UI.HUD.Awake += this.HUD_Awake;

            // Enable interaction with safe ward when wave cleared
            On.RoR2.InfiniteTowerWaveController.StartTimer += this.InfiniteTowerWaveController_StartTimer;

            // Disable interaction with safe ward when wave starts
            On.RoR2.InfiniteTowerWaveController.OnTimerExpire += this.InfiniteTowerWaveController_OnTimerExpire; // This is ignored for boss waves, but they change the state of the ward which should disable the interaction anyway.

            // Allow players to spawn next wave immediately
            On.RoR2.InfiniteTowerSafeWardController.Activate += this.InfiniteTowerSafeWardController_Activate;

            On.RoR2.InfiniteTowerRun.InitializeWaveController += this.InfiniteTowerRun_InitializeWaveController;

            On.RoR2.MeridianEventLightningTrigger.PopulateSceneWithMonsters += this.MeridianEventLightningTrigger_PopulateSceneWithMonsters;
        }

        private void MeridianEventLightningTrigger_PopulateSceneWithMonsters(On.RoR2.MeridianEventLightningTrigger.orig_PopulateSceneWithMonsters orig, MeridianEventLightningTrigger self)
        {
            ResumeTimer();
            orig(self);
        }

        private void InfiniteTowerRun_InitializeWaveController(On.RoR2.InfiniteTowerRun.orig_InitializeWaveController orig, InfiniteTowerRun self)
        {
            orig(self);
            ResumeTimer();
        }

        private void SceneExitController_SetState(On.RoR2.SceneExitController.orig_SetState orig, SceneExitController self, SceneExitController.ExitState newState)
        {
            if (newState == SceneExitController.ExitState.TeleportOut)
            {
                PauseTimer();
            }

            orig(self, newState);
        }

        private void Phase3_OnBossGroupDefeated(On.EntityStates.MeridianEvent.Phase3.orig_OnBossGroupDefeated orig, EntityStates.MeridianEvent.Phase3 self, BossGroup bossGroup)
        {
            PauseTimer();
            orig(self, bossGroup);
        }

        private void HUD_Awake(On.RoR2.UI.HUD.orig_Awake orig, HUD self)
        {
            orig(self);

            if (this.IsEnabled(self))
            {
                Transform rightInfoBar = self.gameModeUiRoot.GetChild(0).Find("RightInfoBar");

                var timerGameObject = new GameObject(nameof(LimitRunTimeUI), typeof(RectTransform));
                timerGameObject.transform.SetParent(rightInfoBar, false);

                // timerGameObject.transform.SetAsLastSibling();
                timerGameObject.AddComponent<LimitRunTimeUI>().LimitRunTimeBehavior = Run.instance?.GetComponent<LimitRunTimeBehavior>();
            }
        }

        private void InfiniteTowerWaveController_OnTimerExpire(On.RoR2.InfiniteTowerWaveController.orig_OnTimerExpire orig, InfiniteTowerWaveController self)
        {
            try
            {
                if (Utils.GetSafeWardState() is EntityStates.InfiniteTowerSafeWard.Active activeState && activeState.purchaseInteraction)
                {
                    activeState.purchaseInteraction.SetAvailable(false);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex);
            }

            orig(self);
        }

        private void InfiniteTowerWaveController_StartTimer(On.RoR2.InfiniteTowerWaveController.orig_StartTimer orig, InfiniteTowerWaveController self)
        {
            orig(self);

            try
            {
                var safeWardState = Utils.GetSafeWardState();

                switch (safeWardState)
                {
                    case EntityStates.InfiniteTowerSafeWard.Active activeState:
                        if (activeState.purchaseInteraction)
                        {
                            activeState.purchaseInteraction.SetAvailable(true);
                        }
                        break;

                    case EntityStates.InfiniteTowerSafeWard.Unburrow unburrowState:
                        if (unburrowState.purchaseInteraction)
                        {
                            unburrowState.purchaseInteraction.SetAvailable(true);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex);
            }
        }

        private void InfiniteTowerSafeWardController_Activate(On.RoR2.InfiniteTowerSafeWardController.orig_Activate orig, InfiniteTowerSafeWardController self)
        {
            if (!(Utils.GetSafeWardState() is EntityStates.InfiniteTowerSafeWard.AwaitingActivation)) // TODO: check exact states
            {
                var waveController = ((InfiniteTowerRun)Run.instance).waveController;

                if (waveController && waveController.isTimerActive)
                {
                    waveController.secondsAfterWave = 0;
                    return;
                }
            }

            orig(self);
        }

        private void InfiniteTowerRun_Start(On.RoR2.InfiniteTowerRun.orig_Start orig, InfiniteTowerRun self)
        {
            if (this.IsEnabled(self))
            {
                this.SetupBehavior(self.EnsureComponent<LimitRunTimeBehavior>());
            }

            orig(self);
        }

        private void InfiniteTowerRun_OnServerStageBegin(On.RoR2.InfiniteTowerRun.orig_OnServerStageBegin orig, InfiniteTowerRun self, Stage stage)
        {
            orig(self, stage);

            if (self.TryGetComponent<LimitRunTimeBehavior>(out var behavior)) // TODO: wait until players spawned in?
            {
                if (self.stageClearCount <= numberOfEarlyStages)
                {
                    behavior.AddTime(this.GetTimeLimitPerRegion(self));
                }
            }
        }

        private void SetupBehavior(LimitRunTimeBehavior behavior)
        {
            behavior.getDamagePerSecond = (body) => damagePerSecond * ((body.maxHealth + body.maxShield) / 600f + (5 / 6f)) + 1 + 0.2f * (body.level - 1);
        }

        private TimeSpan GetTimeLimitPerRegion(UnityEngine.Object context) => TimeSpan.FromMinutes(minutesPerStageAtRank1 - minutesPerStageReductionPerRank * (this.GetRank(context) - 1));

        public class LimitRunTimeBehavior : MonoBehaviour
        {
            public TimeSpan timerTotal;

            public Func<CharacterBody, float>? getDamagePerSecond;

            private bool isTimerPaused = true;

            private float elapsedTime;

            /// <summary>
            /// Increments to 1 when the timer reaches -0.5s, 2 at -1.5s etc.
            /// </summary>
            private int lastProcessedSecond;

            public float SecondsSinceLastTick { get; private set; }

            public float TimeRemaining
            {
                get => (float)this.timerTotal.TotalSeconds - this.elapsedTime;
                internal set => this.timerTotal = TimeSpan.FromSeconds(this.elapsedTime + value);
            }

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    this.ServerFixedUpdate(Time.fixedDeltaTime);
                }
            }

            public void Pause()
            {
                this.isTimerPaused = true;
            }

            public void Resume()
            {
                this.isTimerPaused = false;
            }

            internal void AddTime(TimeSpan timeSpan)
            {
                this.timerTotal = timeSpan + TimeSpan.FromSeconds(Mathf.Max(this.TimeRemaining, 0));
                this.elapsedTime = 0;
            }

            private void ServerFixedUpdate(float deltaTime)
            {
                this.SecondsSinceLastTick += deltaTime;

                if (this.isTimerPaused)
                {
                    return;
                }

                this.elapsedTime += deltaTime;

                float timeRemaining = this.TimeRemaining;
                var secondBeingProcessed = Mathf.FloorToInt(0.5f - timeRemaining);

                if (this.lastProcessedSecond > secondBeingProcessed)
                {
                    this.lastProcessedSecond = secondBeingProcessed;
                    return;
                }

                while (this.lastProcessedSecond < secondBeingProcessed)
                {
                    this.lastProcessedSecond++;
                    this.Tick();
                }
            }

            private void Tick()
            {
                this.SecondsSinceLastTick = 0;
                if (this.lastProcessedSecond > 0)
                {
                    foreach (var body in TeamComponent.GetTeamMembers(TeamIndex.Player).Select(x => x.body))
                    {
                        if (!body || !body.healthComponent)
                        {
                            continue;
                        }

                        float damage = this.getDamagePerSecond!(body);

                        if (damage > 0f)
                        {
                            DamageTypeCombo damageType = new DamageTypeCombo(DamageType.BypassArmor | DamageType.BypassBlock, DamageTypeExtended.DamageField, DamageSource.Hazard); // TODO: evaluate these
                            body.healthComponent.TakeDamage(new DamageInfo
                            {
                                damage = damage,
                                position = body.corePosition,
                                damageType = damageType,
                                damageColorIndex = DamageColorIndex.Bleed
                            });
                        }
                    }
                }
            }
        }

        public class LimitRunTimeUI : MonoBehaviour
        {
            private static Color defaultColor = new Color(1, 0.4f, 0.4f);

            private static float timeNearlyUpMaxTextScale = 1.4f;

            private static float timeUpMaxTextScale = 1.15f;

            private static float timeNearlyUpSecondsThreshold = 30;

            private TimerText? timerText;

            public LimitRunTimeBehavior? LimitRunTimeBehavior { get; set; }

            public void Awake()
            {
                var rectTransform = this.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.up;
                rectTransform.anchorMax = Vector2.up;
                rectTransform.pivot = new Vector2(0, 0.5f);

                // rectTransform.sizeDelta =

                var label = this.gameObject.AddComponent<TextMeshProUGUI>();
                label.alignment = TextAlignmentOptions.Left;
                label.fontSize = 16;
                label.enableAutoSizing = false;
                label.text = "";

                var timerStringFormatter = ScriptableObject.CreateInstance<TimerStringFormatter>(); // TODO: controls format string, check this is correct
                timerStringFormatter.format = new Format
                {
                    prefix = "<mspace=0.5em>",
                    suffix = "</mspace>",
                    units = new Format.Unit[]
                    {
                        new Format.Unit
                        {
                            name = "minutes",
                            conversionRate = 60.0,
                            maxDigits = uint.MaxValue,
                            minDigits = 2u,
                            prefix = string.Empty,
                            suffix = string.Empty
                        },
                        new Format.Unit
                        {
                            name = "seconds",
                            conversionRate = 1.0,
                            maxDigits = 2u,
                            minDigits = 2u,
                            prefix = ":",
                            suffix = string.Empty
                        },
                        new Format.Unit
                        {
                            name = "centiseconds",
                            conversionRate = 0.01,
                            maxDigits = 2u,
                            minDigits = 2u,
                            prefix = ".",
                            suffix = string.Empty
                        }
                    }
                };

                this.timerText = this.gameObject.AddComponent<TimerText>();
                this.timerText.targetLabel = label;
                this.timerText.format = timerStringFormatter;
            }

            public void Update()
            {
                if (this.timerText)
                {
                    float secondsRemaining = this.LimitRunTimeBehavior?.TimeRemaining ?? 0f;
                    this.timerText!.seconds = Mathf.Max(0, secondsRemaining);

                    float intensity;
                    if (secondsRemaining > timeNearlyUpSecondsThreshold)
                    {
                        intensity = 0;
                    }
                    else
                    {
                        intensity = 1 - 2 * this.LimitRunTimeBehavior?.SecondsSinceLastTick ?? 0; // No need to clamp as lerp does the clamping
                    }

                    this.timerText.targetLabel.color = Color.Lerp(defaultColor, secondsRemaining > 0 ? Color.white : Color.red, intensity);
                    this.transform.localScale = Vector3.one * Mathf.Lerp(1, secondsRemaining > 0 ? timeNearlyUpMaxTextScale : timeUpMaxTextScale, intensity);
                }
            }
        }
    }
}