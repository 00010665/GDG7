using UnityEngine;

namespace FoodSurvivors.Data
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "FoodSurvivors/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public Color enemyColor = Color.red;
        public float maxHealth = 20f;
        public float moveSpeed = 3f;
        public float damage = 10f;
        public int xpValue = 1;
        public float armor = 0f;
        public Vector3 scale = Vector3.one;
        public bool isBoss = false;
    }
}
