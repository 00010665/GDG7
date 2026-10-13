using System.Collections.Generic;
using UnityEngine;

namespace FoodSurvivors.Data
{
    [CreateAssetMenu(menuName = "FoodSurvivors/Level Up Content Catalog", fileName = "LevelUpContentCatalog")]
    public class LevelUpContentCatalog : ScriptableObject
    {
        public List<WeaponData> weapons = new List<WeaponData>();
        public List<PassiveData> passives = new List<PassiveData>();
    }
}
