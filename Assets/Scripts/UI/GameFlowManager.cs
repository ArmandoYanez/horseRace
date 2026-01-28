using UnityEngine;
using UnityEngine.Playables;

public class GameFlowManager : MonoBehaviour
{
    public GameManager gameManager;
    public EventManager eventManager;

    public HUDController hud;
    public EventUI eventUI;
    public ShopUI shopUI;

    public bool bossRound = false;
    
    [Header("Boss Intro")]
    [SerializeField] private PlayableDirector bossIntroDirector;
    
    // ---------------- FLOWS ----------------

    void Start()
    {
        //hud.Show();
    }
    
    public void StartRace()
    {
        
        // Si es boss round, NO iniciar carrera directo
        if (gameManager.currentRound >= gameManager.bossRound)
        {
            hud.Hide();

            // Preparar escena
            

            // Reproducir cinemática
            bossIntroDirector.stopped -= OnBossIntroFinished;
            bossIntroDirector.stopped += OnBossIntroFinished;
            bossIntroDirector.Play();
            return;
        }
        
        gameManager.StartRaceFlow();
        hud.Hide();
    }
    
    private void OnBossIntroFinished(PlayableDirector d)
    {
        bossIntroDirector.stopped -= OnBossIntroFinished;
        gameManager.ActivateBossOnlyRace();
        gameManager.StartRaceFlow();
    }

    public void StartTalk()
    {
        if (gameManager.currentRound == gameManager.bossRound)
            return;
        
        hud.Hide();
        
        Debug.Log($"eventUI null? {eventUI == null}");
        Debug.Log($"eventManager null? {eventManager == null}");
        Debug.Log($"gameManager null? {gameManager == null}");
        
        if (eventManager == null || gameManager == null || eventUI == null || hud == null)
        {
            Debug.LogError("GameFlowManager: Missing reference");
            return;
        }

        EventSO evt = eventManager.GetRandomEvent(gameManager.currentRound);

        if (evt == null)
        {
            hud.Show();
            return;
        }
        HorseRuntime playerHorse = gameManager.GetPlayerHorse();
        
        EventContext context = new EventContext
        {
            currentRound = gameManager.currentRound,
            playerHorse = playerHorse,
            gameManager = gameManager
        };

        eventUI.Show(
            evt,
            () =>
            {
                eventManager.AcceptEvent(evt, context);
                gameManager.ConsumeRound();
                hud.Show();
            },
            () =>
            {
                gameManager.ConsumeRound();
                hud.Show();
            }
        );
    }
    
    
    public void ReturnToMainMenu()
    {
        hud.Show();
    }
    
    public void StartShop()
    {
        if (gameManager.currentRound == gameManager.bossRound)
            return;
        
        hud.Hide();

        shopUI.Show(() =>
        {
            gameManager.ConsumeRound();
            hud.Show();
        });
    }

    public void howHud()
    {
        hud.Show();
    }

}