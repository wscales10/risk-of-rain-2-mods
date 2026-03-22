using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace PactOfPunishment.Waves.Stage3
{
    public class PlaceFistsArgs
    {
        private readonly bool crit;

        public PlaceFistsArgs(CharacterBody halcyonite, Transform target, CharacterBody? targetBody)
        {
            this.crit = halcyonite.RollCrit();
            this.Halcyonite = halcyonite;
            this.Target = target;
            this.TargetBody = targetBody;
        }

        public CharacterBody Halcyonite { get; }

        public Transform Target { get; }

        public CharacterBody? TargetBody { get; }

        public void PlaceFist(Vector3 centre, float damageCoefficient, float fuse)
        {
            if (!MoveTargetToGround(centre + Vector3.up, out var position))
            {
                return;
            }

            var fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = FistsController.zoneProjectilePrefab,
                position = position,
                rotation = Quaternion.identity,
                owner = this.Halcyonite.gameObject,
                damage = this.Halcyonite.damage * damageCoefficient,
                crit = this.crit,
                fuseOverride = fuse,
            };

            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        private static bool MoveTargetToGround(Vector3 target, out Vector3 result)
        {
            if (Physics.Raycast(target, Vector3.down, out var hitInfo, 1000f, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
            {
                result = hitInfo.point;
                return true;
            }

            result = target;
            return false;
        }
    }
}