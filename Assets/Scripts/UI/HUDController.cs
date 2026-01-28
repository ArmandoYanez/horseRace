using Managers;
using UnityEngine;
using MoreMountains.Feedbacks;

public class HUDController : MonoBehaviour
{
    public MMF_Player enterFeedback;
    public MMF_Player exitFeedback;
    
    public MMF_Player enterFeedbackBoss;
    public MMF_Player exitFeedbackBoss;

    public GameFlowManager flowManager;

    public void Show()
    {
        bool isBossRound =
            flowManager.gameManager.currentRound >=
            flowManager.gameManager.bossRound;

        if (isBossRound)
        {
            enterFeedbackBoss.PlayFeedbacks();
            AudioManager.Instance.FadeOut(ConstantManager.Music.Race.InRace, 1f);
        }
        else
        {
            enterFeedback.PlayFeedbacks();
        }
    }

    public void Hide()
    {
        bool isBossRound =
            flowManager.gameManager.currentRound >=
            flowManager.gameManager.bossRound;

        if (isBossRound)
        {
            exitFeedbackBoss.PlayFeedbacks();
        }
        else
        {
            exitFeedback.PlayFeedbacks();
        }
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