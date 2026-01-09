using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class ResultPopupFeedback : MonoBehaviour
{
    public TextMeshPro text;
    public CanvasGroup canvasGroup;
    public MMF_Player popFeedback;

    [Tooltip("Debe coincidir con la duración total del MMF_Player")]
    public float lifetime = 2f;

    void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    public void Play(int gained)
    {
        // Texto
        text.text = gained > 0 ? $"+{gained}" : "FAIL";
        //text.color = gained > 0 ? Color.green : Color.red;

        // Reset visual por si se reutiliza
        canvasGroup.alpha = 0f;
        transform.localScale = Vector3.one;

        popFeedback.PlayFeedbacks();

        // Se destruye solo
        Destroy(gameObject, lifetime);
    }
}