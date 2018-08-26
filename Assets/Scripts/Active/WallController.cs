using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour {

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit wall " + this.name);
        Destroy(other.gameObject);
    }
}
