using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpeedToggleButton : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI label;

    [Header("Speed Steps")]
    public float[] speeds = { 1f, 2f, 3f, 4f, 5f };

    private int currentIndex = 0;

    void Start()
    {
        ApplySpeed();
    }

    public void ToggleSpeed()
    {
        currentIndex++;

        if (currentIndex >= speeds.Length)
            currentIndex = 0;

        ApplySpeed();
    }

    void ApplySpeed()
    {
        Time.timeScale = speeds[currentIndex];

        if (label != null)
            label.text = $"x{speeds[currentIndex]}";
    }
}