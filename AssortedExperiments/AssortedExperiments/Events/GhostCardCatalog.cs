using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AssortedExperiments.Events
{
    public static class GhostCardCatalog
    {
        private static SummonGhostCard[] catalog = Array.Empty<SummonGhostCard>();

        public static IReadOnlyList<SummonGhostCard> Options => catalog;

        public static void Init()
        {
            static DirectorPlacementRule PlaceNearOwner(CharacterBody ownerBody, SpawnCard spawnCard)
            {
                var ghostRadius = HullDef.Find(spawnCard.hullSize).radius;
                var minDistance = HullDef.Find(ownerBody.hullClassification).radius + ghostRadius;

                return new DirectorPlacementRule()
                {
                    position = ownerBody.transform.position,
                    placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                    minDistance = minDistance,
                    maxDistance = Mathf.Max(minDistance, TeleporterInteraction.instance.holdoutZoneController.currentRadius - ghostRadius)
                };
            }

            static PlacementRuleGetter PlaceNearPlayer(float maxDistance, float minDistance = 0)
            {
                return (ownerBody, spawnCard) => PlaceNearPlayerInternal(ownerBody, spawnCard, maxDistance, minDistance);
            }

            static DirectorPlacementRule PlaceNearPlayerInternal(CharacterBody ownerBody, SpawnCard spawnCard, float maxDistance, float minDistance)
            {
                var target = PickPlayerAsSpawnTarget(RoR2Application.rng);

                if (target)
                {
                    var velocity = target!.rigidbody.velocity;

                    if (Mathf.Approximately(velocity.magnitude, 0) && target.characterDirection)
                    {
                        velocity = target.characterDirection.forward * Mathf.Max(1, target.baseMoveSpeed);
                    }

                    if (velocity.magnitude < 7)
                    {
                        velocity = velocity.normalized * 7;
                    }

                    return new DirectorPlacementRule()
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                        position = target!.transform.position + velocity * 3,
                        minDistance = minDistance,
                        maxDistance = maxDistance
                    };
                }
                else
                {
                    // Fallback
                    return PlaceNearOwner(ownerBody, spawnCard);
                }
            }

            var allCharacterSpawnCards = Resources.FindObjectsOfTypeAll<CharacterSpawnCard>();

            catalog = new SummonGhostCard[]
            {
                new SummonGhostCard("TitanBlackBeach", PlaceNearOwner),
                new SummonGhostCard("RoboBallBoss", PlaceNearOwner, x => x.utility)
                {
                    Lifespan = 5, // Is this different lifespan actually necessary?
                    StartingHealthFraction = 0.24f
                },
                new SummonGhostCard("ClayBoss", PlaceNearOwner),
                new SummonGhostCard("ImpBoss", PlaceNearPlayer(15), x => x.primary, x => x.utility)
                {
                    Lifespan = 6, // Is this different lifespan actually necessary?
                },
                new SummonGhostCard("MegaConstruct", PlaceNearPlayer(110, 50), x => x.special),
                new SummonGhostCard("Gravekeeper", PlaceNearPlayer(75, 50), x => x.secondary), // TODO: pretty difficult to dodge, not sure about this one
                new SummonGhostCard("Grandparent", PlaceNearPlayer(250, 50))
                {
                    Lifespan = 29,
                    StartingHealthFraction = 0.49f,
                    CanDoFriendlyFire = true,
                },
                new SummonGhostCard("VoidMegaCrab", PlaceNearPlayer(100, 50))
                {
                    CanDoFriendlyFire = true,
                },
                new SummonGhostCard("Child", PlaceNearPlayer(37, 27)),
                new SummonGhostCard("ClayGrenadier", PlaceNearPlayer(78, 50))
                {
                    StartingHealthFraction = 0.49f,
                    Lifespan = 10
                },
                new SummonGhostCard("GupBody", PlaceNearPlayer(30))
                {
                    Lifespan = 5,
                },
                new SummonGhostCard("Jellyfish", PlaceNearPlayer(38, 8)),
                new SummonGhostCard("Parent", PlaceNearPlayer(18)),
                new SummonGhostCard("Scorchling", PlaceNearPlayer(27, 25), x => x.secondary),
                new SummonGhostCard("DefectiveUnit", PlaceNearPlayer(38)),
                new SummonGhostCard("Nullifier", PlaceNearPlayer(6))
                {
                    Lifespan = 1,
                    CanDoFriendlyFire = true,
                },
                new SummonGhostCard("Halcyonite", PlaceNearPlayer(78, 51), x => x.utility)
            }.Where(card =>
            {
                var spawnCard = allCharacterSpawnCards.FirstOrDefault(csc => csc.name == $"csc{card.SpawnCardName}");

                if (!spawnCard)
                {
                    Debug.LogWarning($"Could not find spawn card for ghost summon '{card.SpawnCardName}'");
                    return false;
                }

                card.SpawnCard = spawnCard;
                return true;
            }).ToArray();

            static CharacterBody? PickPlayerAsSpawnTarget(Xoroshiro128Plus rng)
            {
                var instances = PlayerCharacterMasterController.instances;
                List<PlayerCharacterMasterController> list = new List<PlayerCharacterMasterController>();
                foreach (PlayerCharacterMasterController item in instances)
                {
                    if (item.master.hasBody)
                    {
                        list.Add(item);
                    }
                }
                if (list.Count > 0)
                {
                    return rng.NextElementUniform(list).master.GetBody();
                }

                return null;
            }
        }
    }
}