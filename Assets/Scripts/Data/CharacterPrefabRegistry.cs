using System;
using UnityEngine;
using FoodSurvivors.Boss;

namespace FoodSurvivors.Data
{
    [CreateAssetMenu(fileName = "CharacterPrefabRegistry", menuName = "FoodSurvivors/Character Prefab Registry")]
    public class CharacterPrefabRegistry : ScriptableObject
    {
        [Serializable]
        public struct EnemyPrefabEntry
        {
            public string enemyName;
            public GameObject prefab;
        }

        public GameObject playerPrefab;
        public GameObject[] enemyPrefabs = Array.Empty<GameObject>();
        public GameObject levelBossPrefab;
        public GameObject foodCartBossPrefab;
        public GameObject foodTruckBossPrefab;
        public GameObject tripleDeliveryBossPrefab;
        private readonly System.Collections.Generic.Dictionary<string, int> nextVariantByFamily = new System.Collections.Generic.Dictionary<string, int>();

        public GameObject GetEnemyPrefab(EnemyData enemyData)
        {
            if (enemyData == null) return GetEnemyPrefab(string.Empty);
            string key = string.IsNullOrWhiteSpace(enemyData.prefabFamily) ? enemyData.name : enemyData.prefabFamily;
            return GetEnemyPrefab(key);
        }

        public GameObject GetEnemyPrefab(string enemyName)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return null;

            string key = enemyName ?? string.Empty;
            string group = key.IndexOf("blogger", StringComparison.OrdinalIgnoreCase) >= 0 ? "FoodBlogger" :
                key.IndexOf("hipster", StringComparison.OrdinalIgnoreCase) >= 0 ? "Hipster" :
                key.IndexOf("vip", StringComparison.OrdinalIgnoreCase) >= 0 ? "VIPGuest" : null;

            if (group != null)
            {
                var matches = new System.Collections.Generic.List<GameObject>();
                for (int i = 0; i < enemyPrefabs.Length; i++)
                {
                    GameObject candidate = enemyPrefabs[i];
                    if (candidate != null && candidate.name.StartsWith(group, StringComparison.OrdinalIgnoreCase)) matches.Add(candidate);
                }
                if (matches.Count > 0)
                {
                    nextVariantByFamily.TryGetValue(group, out int nextVariant);
                    nextVariantByFamily[group] = nextVariant + 1;
                    return matches[nextVariant % matches.Count];
                }
            }

            // Non-special enemy data also cycles through the complete configured roster.
            const string allEnemies = "__all_enemies__";
            nextVariantByFamily.TryGetValue(allEnemies, out int index);
            nextVariantByFamily[allEnemies] = index + 1;
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                GameObject candidate = enemyPrefabs[(index + i) % enemyPrefabs.Length];
                if (candidate != null) return candidate;
            }

            return null;
        }

        public GameObject GetBossPrefab(BossArchetype archetype)
        {
            switch (archetype)
            {
                case BossArchetype.FoodCart: return foodCartBossPrefab;
                case BossArchetype.FoodTruck: return foodTruckBossPrefab;
                case BossArchetype.TripleDelivery: return tripleDeliveryBossPrefab;
                default: return null;
            }
        }
    }
}
