using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [SerializeField] private int targetFPS = 120;

    void Awake()
    {
        Application.targetFrameRate = targetFPS;
    }
}