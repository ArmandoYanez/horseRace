using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public GameManager gameManager;
    public EventManager eventManager;

    public HUDController hud;
    public EventUI eventUI;
    public ShopUI shopUI;

    // ---------------- FLOWS ----------------

    void Start()
    {
        hud.Show();
    }
    
    public void StartRace()
    {
        gameManager.StartRaceFlow();
        hud.Hide();
    }

    public void StartTalk()
    {
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
        hud.Hide();

        shopUI.Show(() =>
        {
            gameManager.ConsumeRound();
            hud.Show();
        });
    }

}