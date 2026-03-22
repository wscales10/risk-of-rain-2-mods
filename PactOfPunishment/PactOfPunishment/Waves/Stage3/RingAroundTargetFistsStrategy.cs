using System.Collections;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class RingAroundTargetFistsStrategy : IFistsStrategy
    {
        public IEnumerator PlaceFists(PlaceFistsArgs args)
        {
            var targetPosition = args.Target.position;

            Vector3 offset = new Vector3(14, 0f, 0f); // TODO: use target horizontal facing direction instead?
            Quaternion step = Quaternion.Euler(0f, 60f, 0f); // 60° rotation

            for (int i = 0; i < 6; i++)
            {
                args.PlaceFist(targetPosition + offset, 1, 1);
                offset = step * offset;
                yield return new WaitForSeconds(1 / 30f);
            }
        }
    }

    public class ChaseTargetFistsStrategy : IFistsStrategy
    {
        public IEnumerator PlaceFists(PlaceFistsArgs args)
        {
            for (int i = 0; i < 17; i++)
            {
                args.PlaceFist(args.Target.position, 1, 1);
                yield return new WaitForSeconds(0.125f);
            }
        }
    }
}