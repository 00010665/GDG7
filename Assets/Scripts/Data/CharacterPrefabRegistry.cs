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
                if (matches.Count > 0) return matches[StableHash(key) % matches.Count];
            }

            int index = StableHash(key) % enemyPrefabs.Length;
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                GameObject candidate = enemyPrefabs[(index + i) % enemyPrefabs.Length];
                if (candidate != null) return candidate;
            }

            return null;
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++) hash = hash * 31 + value[i];
                return hash & 0x7fffffff;
            }
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
