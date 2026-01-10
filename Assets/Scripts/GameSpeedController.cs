using UnityEngine;

public class GameSpeedController : MonoBehaviour
{
    public float normalSpeed = 1f;
    public float fastSpeed = 2f;

    private bool isFast = false;

    public void ToggleSpeed()
    {
        isFast = !isFast;
        Time.timeScale = isFast ? fastSpeed : normalSpeed;
        Time.fixedDeltaTime = 0.03f * Time.timeScale;
    }

    void OnDisable()
    {
        Time.timeScale = normalSpeed;
        Time.fixedDeltaTime = 0.03f;
    }
}