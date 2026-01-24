using RoR2;
using System;
using UnityEngine;

namespace AssortedExperiments.Items
{
    public class CombatDirectorAnchor : MonoBehaviour
    {
        [SerializeField]
        private GameObject? combatDirectorObject;

        [SerializeField]
        private Action<TeamComponent, TeamIndex>? joinTeamListener;

        public CombatDirector? CombatDirector => this.combatDirectorObject.Then(x => x.GetComponent<CombatDirector>());

        public CombatDirector Init(GameObject prefab)
        {
            this.combatDirectorObject = Instantiate(prefab, this.transform);
            return this.CombatDirector!;
        }

        public void SetJoinTeamListener(Action<TeamComponent, TeamIndex> listener)
        {
            this.joinTeamListener = listener;
            TeamComponent.onJoinTeamGlobal += listener;
        }

        private void OnDestroy()
        {
            if (this.joinTeamListener != null)
            {
                TeamComponent.onJoinTeamGlobal -= this.joinTeamListener;
            }
        }
    }
}