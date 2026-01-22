using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public MMF_Player TextUpdate_feel;
    private int stepIndex = 0;
    
    [Header("Input Control")]
    [SerializeField] private float inputCooldown = 1.0f;
    private bool canAdvance = true;
    
    [Header("Tutorial Steps")]
    [SerializeField] private List<MMF_Player> tutorialFeedbacks;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Texts")]
    [TextArea(2, 5)]
    [SerializeField] private List<string> tutorialTexts;

    [Header("State")]
    [SerializeField] private int textIndex = 0;
    
    [Header("Typewriter")]
    [SerializeField] private float letterDelay = 0.04f;

    private Coroutine typingCoroutine;
    
    public void Start()
    {
        PlayNextStep();
        //tutorial_part1.PlayFeedbacks();
    }
    
    // Llama esto cuando quieras mostrar el texto actual
    public void ShowCurrentText()
    {
        if (tutorialTexts == null || tutorialTexts.Count == 0)
        {
            Debug.LogWarning("No hay textos de tutorial.");
            return;
        }

        if (textIndex < 0 || textIndex >= tutorialTexts.Count)
        {
            Debug.LogWarning("textIndex fuera de rango.");
            return;
        }

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(tutorialTexts[textIndex]));
    }
    
    IEnumerator TypeText(string fullText)
    {
        tutorialText.text = "";

        foreach (char c in fullText)
        {
            tutorialText.text += c;

            // 🔊 Aquí puedes disparar FEEL por letra si quieres
            // tutorial_part1?.PlayFeedbacks();
            TextUpdate_feel.PlayFeedbacks();

            yield return new WaitForSeconds(letterDelay);
        }
    }

    // Avanza al siguiente texto y lo muestra
    public void NextText()
    {
        textIndex++;

        if (textIndex >= tutorialTexts.Count)
            textIndex = tutorialTexts.Count - 1;

        ShowCurrentText();
    }

    // Útil si controlas el orden desde fuera
    public void SetTextIndex(int index)
    {
        textIndex = index;
        ShowCurrentText();
    }

    // Opcional
    public int GetCurrentIndex()
    {
        return textIndex;
    }
    
    public void ClearText()
    {
        // Detener escritura si está activa
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // Limpiar texto visual
        if (tutorialText != null)
            tutorialText.text = "";
    }
    
    void Update()
    {
        if (!canAdvance)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            PlayNextStep();
            StartCoroutine(InputDelay());
        }
    }
    
    IEnumerator InputDelay()
    {
        canAdvance = false;
        yield return new WaitForSeconds(inputCooldown);
        canAdvance = true;
    }
    
    public void PlayNextStep()
    {
        // Validaciones
        if (stepIndex >= tutorialTexts.Count ||
            stepIndex >= tutorialFeedbacks.Count)
        {
            Debug.Log("Tutorial terminado.");
            return;
        }

        // Limpiar texto anterior
        ClearText();

        // Reproducir feedback
        if (tutorialFeedbacks[stepIndex] != null)
            tutorialFeedbacks[stepIndex].PlayFeedbacks();

        // Mostrar texto letra por letra
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(
            TypeText(tutorialTexts[stepIndex])
        );

        // Avanzar índice
        stepIndex++;
    }
}
