using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodSurvivors.Data
{
    [Serializable]
    public struct WaveData
    {
        public EnemyData enemy;
        public float startTime;
        public float endTime;
        public float spawnInterval;
        public int spawnCountPerTick;
    }

    [CreateAssetMenu(fileName = "NewLevelData", menuName = "FoodSurvivors/Level Data")]
    public class LevelData : ScriptableObject
    {
        public string levelName;
        [TextArea] public string description;
        public float levelDuration = 300f;
        public List<WaveData> waves = new List<WaveData>();
        public EnemyData bossData;

        [Header("Media Assets")]
        public AudioClip backgroundMusic;
        public Sprite levelBackgroundSprite;
    }
}
