using UnityEngine;

public class HorseResultSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    public ResultPopupFeedback resultPopupPrefab;

    public void ShowResult(int gained)
    {
        ResultPopupFeedback popup =
            Instantiate(resultPopupPrefab, spawnPoint.position, Quaternion.identity);

        popup.Play(gained);
    }
}