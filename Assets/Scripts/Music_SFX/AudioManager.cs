using System.Collections;
using UnityEngine;
using System.Collections.Generic;

    public class AudioManager : MonoBehaviour
    {
        private Dictionary<string, AudioSource> activeSources = new Dictionary<string, AudioSource>();
        
        #region Singleton
        public static AudioManager Instance;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            CreatePool();
        }
        #endregion
        
        #region Pool Fields
        [Header("Pool Settings")]
        public int poolSize = 10;
        private List<AudioSource> pool;
        #endregion
        
        #region Pool Creation
        void CreatePool()
        {
            pool = new List<AudioSource>();

            for (int i = 0; i < poolSize; i++)
            {
                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                pool.Add(src);
            }
        }
        #endregion
        
        #region Pool Logic
        AudioSource GetAvailableSource()
        {
            foreach (var src in pool)
            {
                if (!src.isPlaying)
                    return src;
            }

            // Si no hay disponible, puedes expandir
            AudioSource extra = gameObject.AddComponent<AudioSource>();
            pool.Add(extra);
            return extra;
        }
        #endregion

        #region Play By Name/Index
        public void Play(SoundLibrary lib, string clipName)
        {
            if (!lib.TryGetIndex(clipName, out int index))
            {
                Debug.LogWarning($"[AudioManager] Sonido '{clipName}' no encontrado en la librería '{lib.name}'");
                return;
            }

            Play(lib, index);
        }
        public void Play(SoundLibrary lib, int index)
        {
            if (index < 0 || index >= lib.sounds.Length)
                return;

            NamedClip data = lib.sounds[index];

            AudioSource src = GetAvailableSource();

            src.clip = data.clip;

            // Configuración general de la librería
            src.outputAudioMixerGroup = lib.output;
            src.loop = lib.loop;
            src.spatialBlend = lib.spatial ? 1f : 0f;

            // Configuración específica del clip
            src.volume = data.volume;
            src.pitch = data.pitch;

            src.Play();

            // Registrar este AudioSource bajo su nombre
            string clipKey = lib.sounds[index].name;

            if (activeSources.ContainsKey(clipKey))
                activeSources[clipKey] = src;
            else
                activeSources.Add(clipKey, src);
        }
        #endregion
        
        // Como usar sistema de audio
        // AudioManager.Instance.Play(SoundLibrary librería, string nombreDelClip -> Se puede usar ConstantManager para registrar los sonidos);
        // Ejemplo: AudioManager.Instance.Play(sfx,ConstantManager.Library.Earth.Dialogue_01);

        #region Control Functions

        public void SetVolume(string clipName, float volume)
        {
            if (activeSources.TryGetValue(clipName, out AudioSource src))
            {
                src.volume = volume;
            }
        }
        public void SetPitch(string clipName, float pitch)
        {
            if (activeSources.TryGetValue(clipName, out AudioSource src))
            {
                src.pitch = pitch;
            }
        }
        public void FadeOut(string clipName, float duration)
        {
            if (activeSources.TryGetValue(clipName, out AudioSource src))
                StartCoroutine(FadeOutRoutine(src, duration));
        }
        public void FadeIn(string clipName, float duration, float targetVolume)
        {
            if (activeSources.TryGetValue(clipName, out AudioSource src))
            {
                src.volume = 0f;
                src.Play();
                StartCoroutine(FadeInRoutine(src, duration, targetVolume));
            }
        }
        private IEnumerator FadeOutRoutine(AudioSource src, float time)
        {
            float start = src.volume;
            float t = 0;

            while (t < time)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(start, 0f, t / time);
                yield return null;
            }

            src.Stop();
        }
        private IEnumerator FadeInRoutine(AudioSource src, float time, float target)
        {
            float t = 0;

            while (t < time)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(0f, target, t / time);
                yield return null;
            }
        }

        #endregion
    }