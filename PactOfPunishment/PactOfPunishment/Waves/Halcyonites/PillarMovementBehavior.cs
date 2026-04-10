using HG;
using RoR2.Projectile;
using UnityEngine;

namespace PactOfPunishment.Waves.Halcyonites
{
    public class PillarMovementBehavior : MonoBehaviour // TODO: if above ground, move down to ground
    {
        private ProjectileController? projectileController;

        public void Awake()
        {
            this.projectileController = this.GetComponent<ProjectileController>();
            var steer = this.EnsureComponent<ProjectileSteerTowardTarget>();
            steer.yAxisOnly = true;
            steer.rotationSpeed = 90;
            this.EnsureComponent<Rigidbody>();
            var projectile = this.EnsureComponent<ProjectileSimple>();
            projectile.lifetime = 45;
            projectile.desiredForwardSpeed = 6;
            projectile.updateAfterFiring = true;
            this.EnsureComponent<ProjectileTargetComponent>();
            var targetFinder = this.EnsureComponent<ProjectileDirectionalTargetFinder>();
            targetFinder.lookRange = 120;
            targetFinder.lookCone = 180;
            targetFinder.targetSearchInterval = 1;
            targetFinder.testLoS = false;
        }

        public void Update()
        {
            // TODO: review whether this is enough for multiplayer
            if (!this.projectileController?.owner)
            {
                Destroy(this.gameObject);
            }
        }
    }
}