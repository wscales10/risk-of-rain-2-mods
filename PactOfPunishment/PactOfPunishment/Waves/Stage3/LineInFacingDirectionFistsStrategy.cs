using System.Collections;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class LineInFacingDirectionFistsStrategy : IFistsStrategy
    {
        public IEnumerator PlaceFists(PlaceFistsArgs args)
        {
            Vector3 direction;

            if (args.TargetBody)
            {
                direction = args.TargetBody!.GetHorizontalFacingDirection();
            }
            else
            {
                direction = args.Halcyonite.GetHorizontalFacingDirection();
            }

            for (int i = -2; i < 2; i++)
            {
                args.PlaceFist(args.Target.position + i * 14 * direction, 1, 1);
            }

            yield break;
        }
    }
}