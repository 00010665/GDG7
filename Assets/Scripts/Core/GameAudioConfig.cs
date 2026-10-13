using UnityEngine;

namespace FoodSurvivors.Core
{
    [System.Serializable]
    public class AudioTrackSettings
    {
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.5f;
        [Range(0.1f, 2f)] public float playbackSpeed = 1f;
    }

    [System.Serializable]
    public class AudioEffectSettings : AudioTrackSettings
    {
        [Min(0.1f)] public float intervalSeconds = 18f;
    }

    [CreateAssetMenu(menuName = "FoodSurvivors/Audio Configuration", fileName = "GameAudioConfig")]
    public class GameAudioConfig : ScriptableObject
    {
        [Header("Looping Music")]
        public AudioTrackSettings mainMenu = new AudioTrackSettings();
        public AudioTrackSettings inGame = new AudioTrackSettings();
        public AudioTrackSettings bossCourier = new AudioTrackSettings();
        [Min(0f)] public float bossCourierFadeOutSeconds = 8f;

        [Header("Periodic Boss Effects (seconds between plays)")]
        public AudioEffectSettings bossFoodTruck = new AudioEffectSettings();
        public AudioEffectSettings bossCart = new AudioEffectSettings();
    }
}
