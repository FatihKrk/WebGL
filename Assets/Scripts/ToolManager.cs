using UnityEngine;

public class ToolManager : MonoBehaviour
{
  
    private ToolSelectButton activeButton;

    
    public void SetActiveButton(ToolSelectButton newButton)
    {
        if (activeButton != null && activeButton != newButton)
        {
            activeButton.SetSelected(false);
        }
      
        activeButton = newButton;
        activeButton.SetSelected(true);
    }
}
