using UnityEngine;

namespace MeteorStorm
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("--- Fontes de Audio ---")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("--- Clipes de Efeitos Sonoros ---")]
        [SerializeField] private AudioClip shootClip;
        [SerializeField] private AudioClip damageClip;
        [SerializeField] private AudioClip explosionSmallClip;
        [SerializeField] private AudioClip explosionLargeClip;
        [SerializeField] private AudioClip gameOverClip;

        [Header("--- Trilha Sonora ---")]
        [SerializeField] private AudioClip bgmClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioSources();
            EnsureClips();
        }

        private void EnsureAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = 0.55f;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = 0.85f;
            }
        }

        private void EnsureClips()
        {
            if (shootClip == null) shootClip = AssetLoader.LoadAudioClip("sfx_shoot.wav");
            if (damageClip == null) damageClip = AssetLoader.LoadAudioClip("sfx_damage.wav");
            if (explosionSmallClip == null) explosionSmallClip = AssetLoader.LoadAudioClip("sfx_explosion_small.wav");
            if (explosionLargeClip == null) explosionLargeClip = AssetLoader.LoadAudioClip("sfx_explosion_large.wav");
            if (gameOverClip == null) gameOverClip = AssetLoader.LoadAudioClip("sfx_gameover.wav");
            if (bgmClip == null) bgmClip = AssetLoader.LoadAudioClip("music_arcade_theme.wav");
        }

        public void PlayShoot()
        {
            PlaySFX(shootClip, 0.7f);
        }

        public void PlayDamage()
        {
            PlaySFX(damageClip, 0.9f);
        }

        public void PlayExplosionSmall()
        {
            PlaySFX(explosionSmallClip, 0.75f);
        }

        public void PlayExplosionLarge()
        {
            PlaySFX(explosionLargeClip, 1.0f);
        }

        public void PlayGameOver()
        {
            if (musicSource != null)
                musicSource.Stop();

            PlaySFX(gameOverClip, 1.0f);
        }

        public void PlayMusic()
        {
            EnsureAudioSources();
            EnsureClips();

            if (musicSource != null && bgmClip != null)
            {
                if (!musicSource.isPlaying || musicSource.clip != bgmClip)
                {
                    musicSource.clip = bgmClip;
                    musicSource.Play();
                }
            }
        }

        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        private void PlaySFX(AudioClip clip, float volume = 1.0f)
        {
            EnsureAudioSources();
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void SetClips(AudioClip shoot, AudioClip damage, AudioClip expS, AudioClip expL, AudioClip over, AudioClip bgm)
        {
            shootClip = shoot;
            damageClip = damage;
            explosionSmallClip = expS;
            explosionLargeClip = expL;
            gameOverClip = over;
            bgmClip = bgm;
        }
    }
}
