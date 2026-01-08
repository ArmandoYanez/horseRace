using UnityEngine;

public class TurnUI : MonoBehaviour
{
    public GameManager gameManager;

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void PickLow()    => gameManager.PlayerChooseShot(ShotType.Low);
    public void PickMedium() => gameManager.PlayerChooseShot(ShotType.Medium);
    public void PickHigh()   => gameManager.PlayerChooseShot(ShotType.High);
}