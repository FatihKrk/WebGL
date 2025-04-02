using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderSize : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(this.transform.CompareTag("Hierarchy"))
        {
            gameObject.GetComponent<BoxCollider>().size = new Vector3 (
            transform.GetComponent<RectTransform>().rect.width,
            transform.parent.GetComponent<RectTransform>().rect.height,1f
            );
        }
        else
        {
            gameObject.GetComponent<BoxCollider>().size = new Vector3 (
            transform.GetComponent<RectTransform>().rect.width,
            transform.GetComponent<RectTransform>().rect.height,1f
            );
        }
    }
}
