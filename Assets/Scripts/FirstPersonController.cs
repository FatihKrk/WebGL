using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public GameObject firstPersonTutorialPanel;

    public void StartFirstPersonMode()
    {
       
        firstPersonTutorialPanel.SetActive(false);

       
        Debug.Log("First Person Mode Activated!");
       
    }
}
