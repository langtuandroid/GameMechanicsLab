using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenObject : MonoBehaviour
{
    private void DestoyObject()
    {
        //Random delete children objects, when there is no children, delete parent
        if (transform.childCount > 0)
        {
            int randomChild = Random.Range(0, transform.childCount);
            Destroy(transform.GetChild(randomChild).gameObject);
            Invoke("DestoyObject", 0.1f);
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Destroy the object after 5 seconds.
        Invoke("DestoyObject", 5f);
    }

}
