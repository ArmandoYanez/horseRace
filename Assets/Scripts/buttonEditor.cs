using UnityEngine;
using UnityEngine.UI;

public class buttonEditor : MonoBehaviour
{
    public Button button;
    
    public void SetButtonToBlack()
    {
        if (button == null) return;

        ColorBlock cb = button.colors;

        cb.normalColor = Color.black;

        button.colors = cb;
    }
}
