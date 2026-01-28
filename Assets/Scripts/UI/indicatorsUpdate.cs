using TMPro;
using UnityEngine;

public class indicatorsUpdate : MonoBehaviour
{
    public TextMeshProUGUI high;
    public TextMeshProUGUI mid;
    public TextMeshProUGUI low;

    public GameManager gamemanager;

    // Valores base
    const int HIGH_BASE = 3;
    const int MID_BASE = 2;
    const int LOW_BASE = 1;

    public void UpdateText()
    {
        UpdateIndicator(high, gamemanager.highShotModifier, HIGH_BASE);
        UpdateIndicator(low, gamemanager.lowShotModifier, LOW_BASE);

        // MID tiene comportamiento especial por swap
        if (gamemanager.swapMidActive)
        {
            mid.text = "-" + gamemanager.midShotModifier;
            mid.color = Color.red;
        }
        else
        {
            UpdateIndicator(mid, gamemanager.midShotModifier, MID_BASE);
        }
    }

    void UpdateIndicator(TextMeshProUGUI txt, int modifier, int baseValue)
    {
        int finalValue = baseValue + modifier;

        txt.text = (finalValue >= 0 ? "+" : "") + finalValue;

        if (finalValue > baseValue)
            txt.color = Color.blue;     // buff
        else if (finalValue < baseValue)
            txt.color = Color.red;      // nerf
        else
            txt.color = Color.black;    // neutral
    }
}

