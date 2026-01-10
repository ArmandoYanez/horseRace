using UnityEngine;
using MoreMountains.Feedbacks;

public class HUDController : MonoBehaviour
{
    public MMF_Player enterFeedback;
    public MMF_Player exitFeedback;

    public GameFlowManager flowManager;

    public void Show()
    {
        enterFeedback.PlayFeedbacks();
    }

    public void Hide()
    {
        exitFeedback.PlayFeedbacks();
    }

    // BOTONES ----------------

    public void OnRacePressed()
    {
        Hide();
        flowManager.StartRace();
    }

    public void OnTalkPressed()
    {
        Hide();
        flowManager.StartTalk();
    }

    public void OnShopPressed()
    {
        Hide();
        flowManager.StartShop();
    }
}