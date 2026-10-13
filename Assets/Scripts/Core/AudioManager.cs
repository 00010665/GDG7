using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using FoodSurvivors.Boss;

namespace FoodSurvivors.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource bgmSource;
        public AudioSource sfxSource;
        public AudioSource transitionSource;
        public AudioSource bossEffectSource;

        [Header("All music and boss effect settings")]
        public GameAudioConfig config;

        private Coroutine musicFadeRoutine;
        private AudioEffectSettings activeBossEffect;
        private float nextBossEffectTime;
        private string lastLoadedSceneName;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfigIfNeeded();
            EnsureAudioSources();
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                Instance = null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;

            GameObject audioObj = new GameObject("[AudioManager]");
            audioObj.AddComponent<AudioManager>();
        }

        private void LoadConfigIfNeeded()
        {
            if (config == null)
                config = Resources.Load<GameAudioConfig>("GameAudioConfig");

            if (config == null)
                Debug.LogWarning("[AudioManager] GameAudioConfig was not found at Assets/Resources/GameAudioConfig.asset.");
        }

        private void EnsureAudioSources()
        {
            bgmSource = EnsureSource(bgmSource, true);
            sfxSource = EnsureSource(sfxSource, false);
            transitionSource = EnsureSource(transitionSource, true);
            bossEffectSource = EnsureSource(bossEffectSource, false);
        }

        private AudioSource EnsureSource(AudioSource source, bool loop)
        {
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            return source;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bool restartingGameScene = scene.name == "GameScene" && lastLoadedSceneName == "GameScene";
            lastLoadedSceneName = scene.name;

            activeBossEffect = null;
            if (bossEffectSource != null) bossEffectSource.Stop();

            if (scene.name == "MainMenuScene")
                PlayConfiguredTrack(config != null ? config.mainMenu : null, 0.5f);
            else if (scene.name == "GameScene")
            {
                if (restartingGameScene)
                    RestartInGameMusic();
                else
                    PlayInGameMusic(0.5f);
            }
        }

        private void RestartInGameMusic()
        {
            StopBGM();
            PlayInGameMusic();
        }

        public void PlayInGameMusic(float fadeDuration = 0f)
        {
            PlayConfiguredTrack(config != null ? config.inGame : null, fadeDuration);
        }

        public void BeginBossEncounter(BossArchetype archetype)
        {
            activeBossEffect = null;
            if (bossEffectSource != null) bossEffectSource.Stop();

            switch (archetype)
            {
                case BossArchetype.TripleDelivery:
                    PlayConfiguredTrack(config != null ? config.bossCourier : null,
                        config != null ? config.bossCourierFadeOutSeconds : 8f,
                        fadeOutBeforeStarting: true);
                    break;
                case BossArchetype.FoodTruck:
                    activeBossEffect = config != null ? config.bossFoodTruck : null;
                    ScheduleNextBossEffect();
                    break;
                case BossArchetype.FoodCart:
                    activeBossEffect = config != null ? config.bossCart : null;
                    ScheduleNextBossEffect();
                    break;
            }
        }

        private void Update()
        {
            if (activeBossEffect == null || activeBossEffect.clip == null || Time.unscaledTime < nextBossEffectTime)
                return;

            PlayBossEffect(activeBossEffect);
            ScheduleNextBossEffect();
        }

        private void ScheduleNextBossEffect()
        {
            float interval = activeBossEffect != null ? Mathf.Max(0.1f, activeBossEffect.intervalSeconds) : 0f;
            nextBossEffectTime = Time.unscaledTime + interval;
        }

        private void PlayBossEffect(AudioEffectSettings settings)
        {
            if (settings == null || settings.clip == null || bossEffectSource == null) return;

            bossEffectSource.Stop();
            bossEffectSource.clip = settings.clip;
            bossEffectSource.volume = Mathf.Clamp01(settings.volume);
            bossEffectSource.pitch = Mathf.Clamp(settings.playbackSpeed, 0.1f, 2f);
            bossEffectSource.loop = false;
            bossEffectSource.Play();
        }

        private void PlayConfiguredTrack(AudioTrackSettings settings, float fadeDuration, bool fadeOutBeforeStarting = false)
        {
            if (settings == null || settings.clip == null)
            {
                Debug.LogWarning("[AudioManager] Requested music track has no AudioClip assigned.");
                return;
            }

            PlayTrack(settings.clip, settings.volume, settings.playbackSpeed, fadeDuration, fadeOutBeforeStarting);
        }

        private void PlayTrack(AudioClip clip, float volume, float playbackSpeed, float fadeDuration, bool fadeOutBeforeStarting = false)
        {
            EnsureAudioSources();
            if (clip == null) return;
            if ((bgmSource.clip == clip && bgmSource.isPlaying) ||
                (transitionSource.clip == clip && transitionSource.isPlaying)) return;

            PrepareForTransition();

            if (!bgmSource.isPlaying || fadeDuration <= 0f)
            {
                bgmSource.Stop();
                transitionSource.Stop();
                ConfigureMusicSource(bgmSource, clip, volume, playbackSpeed, true);
                return;
            }

            if (fadeOutBeforeStarting)
            {
                musicFadeRoutine = StartCoroutine(FadeOutThenPlay(
                    bgmSource, clip, volume, playbackSpeed, fadeDuration));
            }
            else
            {
                ConfigureMusicSource(transitionSource, clip, 0f, playbackSpeed, false);
                musicFadeRoutine = StartCoroutine(CrossfadeMusic(
                    bgmSource,
                    transitionSource,
                    bgmSource.volume,
                    Mathf.Clamp01(volume),
                    fadeDuration));
            }
        }

        private void PrepareForTransition()
        {
            if (musicFadeRoutine == null) return;

            StopCoroutine(musicFadeRoutine);
            musicFadeRoutine = null;
            if (transitionSource.isPlaying && transitionSource.volume > bgmSource.volume)
            {
                AudioSource previous = bgmSource;
                bgmSource = transitionSource;
                transitionSource = previous;
            }
            transitionSource.Stop();
        }

        private static void ConfigureMusicSource(AudioSource source, AudioClip clip, float volume, float playbackSpeed, bool play)
        {
            source.Stop();
            source.clip = clip;
            source.loop = true;
            source.volume = Mathf.Clamp01(volume);
            source.pitch = Mathf.Clamp(playbackSpeed, 0.1f, 2f);
            if (play) source.Play();
        }

        private IEnumerator CrossfadeMusic(AudioSource outgoing, AudioSource incoming, float outgoingStartVolume, float incomingTargetVolume, float duration)
        {
            incoming.Play();
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float blend = Mathf.Clamp01(elapsed / duration);
                outgoing.volume = Mathf.Lerp(outgoingStartVolume, 0f, blend);
                incoming.volume = Mathf.Lerp(0f, incomingTargetVolume, blend);
                yield return null;
            }

            outgoing.Stop();
            outgoing.volume = 0f;
            incoming.volume = incomingTargetVolume;
            bgmSource = incoming;
            transitionSource = outgoing;
            musicFadeRoutine = null;
        }

        private IEnumerator FadeOutThenPlay(AudioSource outgoing, AudioClip incomingClip, float incomingVolume, float incomingSpeed, float duration)
        {
            float outgoingStartVolume = outgoing.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float blend = Mathf.Clamp01(elapsed / duration);
                outgoing.volume = Mathf.Lerp(outgoingStartVolume, 0f, blend);
                yield return null;
            }

            outgoing.Stop();
            outgoing.volume = 0f;
            transitionSource.Stop();
            ConfigureMusicSource(transitionSource, incomingClip, incomingVolume, incomingSpeed, true);
            AudioSource previous = bgmSource;
            bgmSource = transitionSource;
            transitionSource = previous;
            musicFadeRoutine = null;
        }

        // Kept for existing gameplay callers that provide a level-specific background track.
        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;
            AudioTrackSettings configured = FindTrackSettings(clip);
            PlayTrack(clip, configured != null ? configured.volume : 1f,
                configured != null ? configured.playbackSpeed : 1f, 0f);
        }

        private AudioTrackSettings FindTrackSettings(AudioClip clip)
        {
            if (config == null) return null;
            if (config.mainMenu != null && config.mainMenu.clip == clip) return config.mainMenu;
            if (config.inGame != null && config.inGame.clip == clip) return config.inGame;
            if (config.bossCourier != null && config.bossCourier.clip == clip) return config.bossCourier;
            return null;
        }

        public void StopBGM()
        {
            if (musicFadeRoutine != null)
            {
                StopCoroutine(musicFadeRoutine);
                musicFadeRoutine = null;
            }
            if (bgmSource != null) bgmSource.Stop();
            if (transitionSource != null) transitionSource.Stop();
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            EnsureAudioSources();
            if (clip == null || sfxSource == null) return;

            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }
    }
}
