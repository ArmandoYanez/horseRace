using UnityEngine;
using System.Collections;
using Managers;

public class GameManager : MonoBehaviour
{
    [Header("Player Selection")]
    public HorseSO selectedHorseSO;

    [Header("Scene Horses (TOP → BOTTOM order)")]
    public HorseView[] horseViews;

    [Header("UI")]
    public TurnUI turnUI;
    
    public SoundLibrary uiSfx;

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
            Debug.LogError(" Selected horse not found in HorseViews");
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

            // MOSTRAR RESULTADO PRIMERO
            HorseResultSpawner spawner =
                horseViews[i].GetComponent<HorseResultSpawner>();
                AudioManager.Instance.Play(uiSfx, ConstantManager.Sfx.Race.PopScore);

            if (spawner != null)
            {
                spawner.ShowResult(gained);
            }

            // ESPERAR A QUE SE LEA EL RESULTADO
            yield return new WaitForSeconds(1f);

            // AHORA MOVER EL CABALLO
            horseViews[i].UpdatePositionSmooth();

            // PEQUEÑO RESPIRO ANTES DEL SIGUIENTE
            yield return new WaitForSeconds(0.3f);
        }

        // 🏁 CHECAR GANADOR
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
