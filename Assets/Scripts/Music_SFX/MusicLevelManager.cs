using Managers;
using UnityEngine;

public class MusicLevelManager : MonoBehaviour
{
    [SerializeField]
    private SoundLibrary currentSceneAudioLibrary;
 
    void Start()
    {
        AudioManager.Instance.Play(currentSceneAudioLibrary, ConstantManager.Music.Race.InRace);
        AudioManager.Instance.SetVolume(ConstantManager.Music.Race.InRace,0.7f);
    }
}
