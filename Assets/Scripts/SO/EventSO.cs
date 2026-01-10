using UnityEngine;

public enum EventType
{
    ContextOnly,
    ImmediateEffect,
    DelayedEffect
}

[CreateAssetMenu(menuName = "Carnival/Event")]
public class EventSO : ScriptableObject
{
    [Header("Narrative")]
    [TextArea(3, 6)]
    public string dialogueTemplate;
    [TextArea(3, 6)]
    public string dialogueTemplate_Spanish;

    [Header("Event Info")]
    public EventType eventType;

    [Header("Conditions")]
    public int minRound = 1;
    public int maxRound = 15;

    [Header("Effect")]
    public EventEffectSO effect;
}