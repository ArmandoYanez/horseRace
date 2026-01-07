using UnityEngine;
using System.Collections;

public class HorseView : MonoBehaviour
{
    [Header("Config")]
    public HorseSO horseData;

    [Header("Track")]
    public float startX = 246.38f;
    public float finishX = 311.3f;

    [Header("Movement")]
    public float moveDuration = 0.35f;

    private HorseRuntime runtime;
    private Coroutine moveRoutine;

    public void Initialize(HorseRuntime horseRuntime)
    {
        runtime = horseRuntime;

        Vector3 pos = transform.position;
        pos.x = startX;
        transform.position = pos;
    }

    public void UpdatePositionSmooth()
    {
        if (runtime == null) return;

        float t = Mathf.Clamp01(
            (float)runtime.currentPoints / runtime.baseData.pointsToWin
        );

        float targetX = Mathf.Lerp(startX, finishX, t);

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(MoveToX(targetX));
    }

    IEnumerator MoveToX(float targetX)
    {
        float elapsed = 0f;
        float startXPos = transform.position.x;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float lerpT = elapsed / moveDuration;

            float newX = Mathf.Lerp(startXPos, targetX, lerpT);

            Vector3 pos = transform.position;
            pos.x = newX;
            transform.position = pos;

            yield return null;
        }

        // Asegura posición final exacta
        Vector3 finalPos = transform.position;
        finalPos.x = targetX;
        transform.position = finalPos;
    }
}