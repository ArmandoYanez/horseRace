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
    public void ShowBonusResult(int gained)
    {
        ResultPopupFeedback popup =
            Instantiate(resultPopupPrefab, spawnPoint.position, Quaternion.identity);
        
        popup.PlayColor(gained,Color.yellow);
    }
    
    public void ShowCustomText(string text, Color color)
    {
        ResultPopupFeedback popup =
            Instantiate(resultPopupPrefab, spawnPoint.position, Quaternion.identity);
        
        popup.PlayColor_text(text,color);
    }
}