using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Horse Selection")]
    public HorseSO selectedHorseSO;

    [Header("Scene Horses")]
    public HorseView[] horseViews;

    private HorseRuntime activeHorse;
    private HorseView activeHorseView;

    private bool gameStarted = false;

    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        // Buscar el HorseView que corresponde al SO seleccionado
        foreach (var view in horseViews)
        {
            if (view.horseData == selectedHorseSO)
            {
                activeHorse = new HorseRuntime(selectedHorseSO);
                activeHorseView = view;

                activeHorseView.Initialize(activeHorse);

                gameStarted = true;

                Debug.Log($"🐎 Playing with {selectedHorseSO.horseName}");
                return;
            }
        }

        Debug.LogError("Selected horse not found in scene");
    }

    void Update()
    {
        if (!gameStarted) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            PlayTurn(ShotType.Low);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            PlayTurn(ShotType.Medium);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            PlayTurn(ShotType.High);
    }

    void PlayTurn(ShotType shot)
    {
        int gained = ShotResolver.ResolvePureLuck(shot);
        activeHorse.currentPoints += gained;

        Debug.Log(
            gained > 0
                ? $"{shot} → +{gained} (Total: {activeHorse.currentPoints})"
                : $"{shot} → FAIL"
        );

        activeHorseView.UpdatePositionSmooth();

        if (activeHorse.currentPoints >= activeHorse.baseData.pointsToWin)
        {
            Debug.Log("🏁 FINISH LINE REACHED");
            gameStarted = false;
        }
    }
}