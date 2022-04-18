namespace RoR2Mod1
{
	// RoR2.Items.MinionLeashBodyBehavior
	using RoR2;
	using RoR2.Items;
	using RoR2.Navigation;
	using System.Linq;
	using UnityEngine;

	public class MinionLeash2BodyBehavior : BaseItemBodyBehavior
	{
		public const float leashDistSq = 160000f;

		public const float teleportDelayTime = 10f;

		public const float minTeleportDistance = 10f;

		public const float maxTeleportDistance = 40f;

		private GameObject helperPrefab;

		private RigidbodyMotor rigidbodyMotor;

		private float teleportAttemptTimer = 10f;

		[ItemDefAssociation(useOnServer = true, useOnClient = true)]
		private static ItemDef GetItemDef()
		{
			return Items.MinionLeash2;
		}

		public void Start()
		{
			if (base.body.hasEffectiveAuthority)
			{
				helperPrefab = LegacyResourcesAPI.Load<GameObject>("SpawnCards/HelperPrefab");
				rigidbodyMotor = GetComponent<RigidbodyMotor>();
			}
		}

		private void FixedUpdate()
		{
			if (!base.body.hasEffectiveAuthority)
			{
				return;
			}
			CharacterMaster master = base.body.master;
			CharacterMaster characterMaster = (master ? master.minionOwnership.ownerMaster : null);
			CharacterBody characterBody = (characterMaster ? characterMaster.GetBody() : null);
			if (!characterBody)
			{
				return;
			}
			Vector3 corePosition = characterBody.corePosition;
			Vector3 corePosition2 = base.body.corePosition;

			if (Utils.WantsToStayInZone(master))
			{
				return;
			}

			if (((!base.body.characterMotor || !(base.body.characterMotor.walkSpeed > 0f)) && (!rigidbodyMotor || !(base.body.moveSpeed > 0f))) || !((corePosition2 - corePosition).sqrMagnitude > 160000f))
			{
				return;
			}
			teleportAttemptTimer -= Time.fixedDeltaTime;
			if (!(teleportAttemptTimer <= 0f))
			{
				return;
			}
			teleportAttemptTimer = 10f;
			SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
			spawnCard.hullSize = base.body.hullClassification;
			spawnCard.nodeGraphType = (base.body.isFlying ? MapNodeGroup.GraphType.Air : MapNodeGroup.GraphType.Ground);
			spawnCard.prefab = helperPrefab;
			GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Approximate,
				position = corePosition,
				minDistance = 10f,
				maxDistance = 40f
			}, RoR2Application.rng));
			if ((bool)gameObject)
			{
				Vector3 position = gameObject.transform.position;
				if ((position - corePosition).sqrMagnitude < 160000f)
				{
					Debug.Log("MinionLeash teleport for " + base.body.name);
					TeleportHelper.TeleportBody(base.body, position);
					GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(base.body.gameObject);
					if ((bool)teleportEffectPrefab)
					{
						EffectManager.SimpleEffect(teleportEffectPrefab, position, Quaternion.identity, transmit: true);
					}
					Object.Destroy(gameObject);
				}
			}
			Object.Destroy(spawnCard);
		}
	}

}
