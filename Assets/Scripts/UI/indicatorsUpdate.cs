using System;
using TMPro;
using UnityEngine;

public class indicatorsUpdate : MonoBehaviour
{
    public TextMeshProUGUI high;
    public TextMeshProUGUI mid;
    public TextMeshProUGUI low;

    public GameManager gamemanager;
    
    public void UpdateText()
    {
        high.text = "+"+(gamemanager.highShotModifier + 3).ToString();
        low.text = "+"+(gamemanager.lowShotModifier + 1).ToString();

        if (gamemanager.swapMidActive)
        {
            mid.text = "-"+(gamemanager.midShotModifier).ToString();
            mid.color = Color.red;
        }
        else
        {
            mid.text = "+"+(gamemanager.midShotModifier + 2).ToString();
            mid.color = Color.black;
        }
    }
}
