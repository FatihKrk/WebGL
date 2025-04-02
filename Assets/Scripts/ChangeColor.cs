using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    private MeshRenderer mesh;
    // Start is called before the first frame update
    void Start()
    {
        mesh = this.GetComponent<MeshRenderer>();
    }

    void OnMouseEnter()
    {
        mesh.material.color = Color.blue;
    }
    
    void OnMouseExit()
    {
        mesh.material.color = Color.white;
    }
}
