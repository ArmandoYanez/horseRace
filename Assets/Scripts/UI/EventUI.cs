using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class EventUI : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public MMF_Player enterFeedback;
    public MMF_Player exitFeedback;

    private System.Action onAccept;
    private System.Action onReject;

    public void Show(EventSO evt, System.Action accept, System.Action reject)
    {
        gameObject.SetActive(true);
        dialogueText.text = evt.dialogueTemplate;

        onAccept = accept;
        onReject = reject;

        enterFeedback.PlayFeedbacks();
    }

    public void Accept()
    {
        exitFeedback.PlayFeedbacks();
        onAccept?.Invoke();
        gameObject.SetActive(false);
    }

    public void Reject()
    {
        exitFeedback.PlayFeedbacks();
        onReject?.Invoke();
        gameObject.SetActive(false);
    }
}