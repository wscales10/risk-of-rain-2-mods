using RoR2;
using UnityEngine;

namespace PactOfPunishment.Waves.Common
{
    public class UndeployMinionsOnDeathBehavior : MonoBehaviour
    {
        public void Awake()
        {
            if (this.GetCharacterMaster(out var master))
            {
                (master.onBodyDeath ??= new UnityEngine.Events.UnityEvent()).AddListener(this.OnBodyDeath);
            }
        }

        private void OnBodyDeath()
        {
            if (this.GetCharacterMaster(out var master))
            {
                var list = master.deployablesList;

                for (int num = list.Count - 1; num >= 0; num--)
                {
                    Deployable deployable = list[num].deployable;
                    list.RemoveAt(num);
                    deployable.ownerMaster = null;
                    deployable.onUndeploy.Invoke();
                }
            }
        }

        private bool GetCharacterMaster(out CharacterMaster master)
        {
            if (this.TryGetComponent(out master))
            {
                return true;
            }

            if (this.TryGetComponent(out CharacterBody body))
            {
                master = body.master;
                return true;
            }

            return false;
        }
    }
}