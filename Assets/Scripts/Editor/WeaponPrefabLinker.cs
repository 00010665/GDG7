using UnityEditor;
using UnityEngine;
using FoodSurvivors.Data;

namespace FoodSurvivors.EditorTools
{
    public static class WeaponPrefabLinker
    {
        [MenuItem("FoodSurvivors/Auto-Link Weapon Prefabs (Привязать 3D Префабы по именам)")]
        public static void LinkAllPrefabsToAssets()
        {
            string soFolder = "Assets/ScriptableObjects/Weapons";
            string prefabFolder = "Assets/Prefabs/Weapons";

            string[] soGuids = AssetDatabase.FindAssets("t:WeaponData", new[] { soFolder });
            string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabFolder });

            if (soGuids.Length == 0)
            {
                Debug.LogError($"[WeaponPrefabLinker] Не найдены ScriptableObjects в папке {soFolder}!");
                return;
            }

            int linkedCount = 0;

            foreach (var soGuid in soGuids)
            {
                string soPath = AssetDatabase.GUIDToAssetPath(soGuid);
                WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(soPath);
                if (weaponData == null) continue;

                string cleanSoName = weaponData.name.Replace("Weapon_", "").Replace("_Data", "").ToLower();

                GameObject matchedPrefab = null;

                // Ищем подходящий префаб по имени
                foreach (var pGuid in prefabGuids)
                {
                    string pPath = AssetDatabase.GUIDToAssetPath(pGuid);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pPath);
                    if (prefab == null) continue;

                    string cleanPrefabName = prefab.name.ToLower();

                    if (cleanPrefabName.Contains(cleanSoName) || cleanSoName.Contains(cleanPrefabName))
                    {
                        matchedPrefab = prefab;
                        break;
                    }
                }

                if (matchedPrefab != null)
                {
                    weaponData.weaponPrefab = matchedPrefab;
                    EditorUtility.SetDirty(weaponData);
                    linkedCount++;
                    Debug.Log($"[WeaponPrefabLinker] 🔗 УСПЕХ: '{weaponData.name}' -> привязан 3D-префаб '{matchedPrefab.name}'");
                }
                else
                {
                    Debug.LogWarning($"[WeaponPrefabLinker] ⚠️ Для оружия '{weaponData.name}' не найден префаб в {prefabFolder}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[WeaponPrefabLinker] ✅ Готово! Связано префабов: {linkedCount} из {soGuids.Length}.");
        }
    }
}