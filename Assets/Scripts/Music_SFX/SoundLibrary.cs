using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;


    [System.Serializable]
    public struct NamedClip
    {
        public string name;         
        public AudioClip clip;      
        public float volume;        
        public float pitch;         
    }
    
    [CreateAssetMenu(menuName = "Audio/SoundLibrary")]
    public class SoundLibrary : ScriptableObject
    {
        public AudioMixerGroup output;
        public bool spatial;
        public bool loop;

        public NamedClip[] sounds;
        
        private Dictionary<string, int> lookup;   

        void OnEnable()
        {
            lookup = new Dictionary<string, int>();

            for (int i = 0; i < sounds.Length; i++)
                lookup[sounds[i].name] = i;
        }

        public bool TryGetIndex(string name, out int index)
        {
            return lookup.TryGetValue(name, out index);
        }
    }