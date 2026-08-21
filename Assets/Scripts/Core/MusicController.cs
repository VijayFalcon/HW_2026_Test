// Plays the background soundtrack, looping, once a run actually begins
// (GameState.Playing). Loads the clip from Resources so nothing needs to
// be wired up manually in the Editor; a missing clip is handled gracefully
// -- the game just runs silently instead of throwing.

using UnityEngine;

namespace DoofusDiaries.Core
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicController : MonoBehaviour
    {
        private const string ClipResourcePath = "Audio/Soundtrack";

        private AudioSource _audioSource;
        private GameManager _gameManager;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.volume = 0.6f;

            AudioClip clip = Resources.Load<AudioClip>(ClipResourcePath);
            if (clip == null)
            {
                Debug.LogWarning($"[MusicController] No audio clip found at 'Resources/{ClipResourcePath}'. The game will run without music until one is placed there.");
            }
            _audioSource.clip = clip;
        }

        public void Bind(GameManager gameManager)
        {
            _gameManager = gameManager;
            _gameManager.OnStateChanged += HandleStateChanged;
        }

        private void OnDestroy()
        {
            if (_gameManager != null) _gameManager.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state != GameState.Playing) return;
            if (_audioSource.clip == null) return;

            _audioSource.Stop();
            _audioSource.Play();
        }
    }
}
