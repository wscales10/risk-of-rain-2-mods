using RoR2;
using RoR2.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage2.Summoner
{
    public class DefeatSupportsObjectiveController : MonoBehaviour
    {
        private readonly Dictionary<string, SupportGroup> dictionary = new Dictionary<string, SupportGroup>();

        public void OnEnable()
        {
            ObjectivePanelController.collectObjectiveSources += this.ObjectivePanelController_collectObjectiveSources;
        }

        public void OnDisable()
        {
            ObjectivePanelController.collectObjectiveSources -= this.ObjectivePanelController_collectObjectiveSources;
        }

        public void AddSupport(CharacterBody body)
        {
            string bodyNameToken = body.baseNameToken;
            if (!this.dictionary.TryGetValue(bodyNameToken, out var supportGroup))
            {
                supportGroup = new SupportGroup(bodyNameToken);
                this.dictionary.Add(bodyNameToken, supportGroup);
            }

            supportGroup.AddSupport(body);
        }

        public void OnSupportDefeated(CharacterBody body)
        {
            if (this.dictionary.TryGetValue(body.baseNameToken, out var supportGroup))
            {
                supportGroup.RemoveSupport(body);

                if (supportGroup.Defeated >= supportGroup.Total)
                {
                    this.dictionary.Remove(body.baseNameToken);
                }
            }
        }

        private void ObjectivePanelController_collectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
        {
            objectiveSourcesList.AddRange(this.dictionary.Select(kvp => new ObjectivePanelController.ObjectiveSourceDescriptor
            {
                master = master,
                objectiveType = typeof(DefeatSupportsObjectiveTracker),
                source = kvp.Value,
            }));
        }

        public class DefeatSupportsObjectiveTracker : ObjectivePanelController.ObjectiveTracker
        {

            public DefeatSupportsObjectiveTracker()
            {
                this.baseToken = "OBJECTIVE_GOLDSHORES_DEFEAT_BOSS";
            }

            private SupportGroup Source => (SupportGroup)this.sourceDescriptor.source;

            public override string GenerateString()
            {
                this.Source.IsDirty = false;
                var supportGroup = this.Source;

                // TODO: use GetStringFormatted?
                return string.Format(Regex.Replace(Language.GetString(this.baseToken), ",(?! )", ", "), Language.GetString(supportGroup.bodyNameToken) + "s", $"{supportGroup.Defeated}/{supportGroup.Total}");
            }

            public override bool IsDirty()
            {
                return base.IsDirty() || this.Source.IsDirty;
            }
        }

        private sealed class SupportGroup : UnityEngine.Object
        {
            internal bool IsDirty = true;

            public readonly string bodyNameToken;

            private readonly List<CharacterBody> bodies = new List<CharacterBody>();

            public SupportGroup(string bodyNameToken)
            {
                this.bodyNameToken = bodyNameToken;
            }

            public int Total { get; private set; }

            public int Defeated { get; private set; }

            public void AddSupport(CharacterBody body)
            {
                this.bodies.Add(body);
                this.Total++;
                this.IsDirty = true;
            }

            public void RemoveSupport(CharacterBody body)
            {
                if (this.bodies.Remove(body))
                {
                    this.Defeated++;
                }

                this.IsDirty = true;
            }
        }
    }
}