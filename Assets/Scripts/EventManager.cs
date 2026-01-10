using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public List<EventSO> allEvents;

    public EventSO GetRandomEvent(int currentRound)
    {
        List<EventSO> validEvents = allEvents.FindAll(e =>
            currentRound >= e.minRound &&
            currentRound <= e.maxRound
        );

        if (validEvents.Count == 0)
            return null;

        return validEvents[Random.Range(0, validEvents.Count)];
    }

    public void AcceptEvent(EventSO evt, EventContext context)
    {
        if (evt.effect != null)
        {
            evt.effect.Apply(context);
        }
    }
}