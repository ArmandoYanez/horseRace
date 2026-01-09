using Managers;
using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class TurnUI : MonoBehaviour
{
    public GameManager gameManager;

    [Header("Feedbacks")]
    public MMF_Player showFeedback; // ENTRADA
    public MMF_Player hideFeedback; // SALIDA

    public CanvasGroup canvasGroup;
    
    public void Show()
    {
        gameObject.SetActive(true);

        // habilita interacción
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        showFeedback.PlayFeedbacks();
    }

    public void Hide()
    {
        // corta input inmediatamente
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        hideFeedback.PlayFeedbacks();
        //Invoke(nameof(DisableUI), 0.2f); // ajusta al largo del feedback
    }

    void DisableUI()
    {
        gameObject.SetActive(false);
    }

    // botones
    public void PickLow()
    {
        EventSystem.current.SetSelectedGameObject(null);
        gameManager.PlayerChooseShot(ShotType.Low);
    }

    public void PickMedium()
    {
        EventSystem.current.SetSelectedGameObject(null);
        gameManager.PlayerChooseShot(ShotType.Medium);
    }

    public void PickHigh()
    {
        EventSystem.current.SetSelectedGameObject(null);
        gameManager.PlayerChooseShot(ShotType.High);
    }
}