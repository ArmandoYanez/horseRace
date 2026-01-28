using Managers;
using UnityEngine;

public class MusicLevelManager : MonoBehaviour
{
    [SerializeField]
    private SoundLibrary currentSceneAudioLibrary;

    public bool tutorial = false;
    
    void Start()
    {
        if (tutorial)
        {
            AudioManager.Instance.Play(currentSceneAudioLibrary, ConstantManager.Music.Race.Intro);
            AudioManager.Instance.SetVolume(ConstantManager.Music.Race.Intro,0.5f);
        }
        else
        {
          
            AudioManager.Instance.Play(currentSceneAudioLibrary, ConstantManager.Music.Race.InRace);
            AudioManager.Instance.SetVolume(ConstantManager.Music.Race.InRace,0.5f);
        }
        
    }
    
    public void changeToRaceSong()
    {
        AudioManager.Instance.FadeOut(ConstantManager.Music.Race.Intro, 0.5f);
        AudioManager.Instance.Play(currentSceneAudioLibrary, ConstantManager.Music.Race.InRace);
        AudioManager.Instance.SetVolume(ConstantManager.Music.Race.InRace, 0);
        AudioManager.Instance.FadeIn(ConstantManager.Music.Race.InRace, 0.5f, 0.5f);
    }
    
    public void bossFase1()
    {
        AudioManager.Instance.SetVolume(ConstantManager.Music.Race.InRace, 0);
        AudioManager.Instance.SetVolume(ConstantManager.Music.Race.Intro, 0);
        AudioManager.Instance.Play(currentSceneAudioLibrary, ConstantManager.Music.Race.boss);
        AudioManager.Instance.FadeIn(ConstantManager.Music.Race.boss, 0.5f, 0.5f);
    }
}
