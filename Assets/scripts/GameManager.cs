using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Player Selection")]
    public HorseSO selectedHorseSO;

    [Header("Scene Horses (TOP → BOTTOM order)")]
    public HorseView[] horseViews;

    [Header("UI")]
    public TurnUI turnUI;

    private HorseRuntime[] horses;
    private int playerIndex = -1;
    private bool resolvingTurn = false;

    void Start()
    {
        StartRace();
    }
    
    void StartRace()
    {
        horses = new HorseRuntime[horseViews.Length];

        for (int i = 0; i < horseViews.Length; i++)
        {
            horses[i] = new HorseRuntime(horseViews[i].horseData);
            horseViews[i].Initialize(horses[i]);

            if (horseViews[i].horseData == selectedHorseSO)
                playerIndex = i;
        }

        if (playerIndex == -1)
        {
            Debug.LogError("❌ Selected horse not found in HorseViews");
            return;
        }

        Debug.Log($"🐎 Player horse: {selectedHorseSO.horseName}");
        turnUI.Show();
    }
    
    public void PlayerChooseShot(ShotType shot)
    {
        if (resolvingTurn) return;

        turnUI.Hide();
        StartCoroutine(ResolveTurn(shot));
    }
    
    IEnumerator ResolveTurn(ShotType playerShot)
    {
        resolvingTurn = true;

        for (int i = 0; i < horses.Length; i++)
        {
            ShotType shotToUse =
                (i == playerIndex)
                    ? playerShot
                    : AIShotChooser.ChooseShot();

            int gained = ShotResolver.ResolvePureLuck(shotToUse);
            horses[i].currentPoints += gained;

            Debug.Log(
                gained > 0
                    ? $"{horseViews[i].horseData.horseName} → +{gained}"
                    : $"{horseViews[i].horseData.horseName} → FAIL"
            );

            horseViews[i].UpdatePositionSmooth();

            // Delay para ver el avance uno por uno
            yield return new WaitForSeconds(0.45f);
        }
        
        for (int i = 0; i < horses.Length; i++)
        {
            if (horses[i].currentPoints >= horses[i].baseData.pointsToWin)
            {
                Debug.Log($"🏁 {horses[i].baseData.horseName} WINS");
                yield return new WaitForSeconds(1f);
                ResetRace();
                yield break;
            }
        }

        resolvingTurn = false;
        turnUI.Show();
    }


    void ResetRace()
    {
        resolvingTurn = false;

        for (int i = 0; i < horses.Length; i++)
        {
            horses[i].currentPoints = horses[i].baseData.startingPoints;
            horseViews[i].UpdatePositionSmooth();
        }

        turnUI.Show();
    }
}
