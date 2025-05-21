using UnityEngine;

public class showtutorial : MonoBehaviour
{
    public GameObject firstPersonTutorialPanel;

    public void ShowFirstPersonTutorial()
    {
        firstPersonTutorialPanel.SetActive(true);
    }
}
