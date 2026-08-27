using UnityEngine;

namespace FoodSurvivors.Data
{
    [CreateAssetMenu(fileName = "NewChefData", menuName = "FoodSurvivors/Chef Data")]
    public class ChefData : ScriptableObject
    {
        public string chefName;
        [TextArea] public string description;
        public Color chefColor = Color.white;
        public float maxHealth = 100f;
        public float moveSpeed = 5f;
        public float armor = 0f;
        public WeaponData startingWeapon;
        public PassiveData startingPassive;
    }
}
