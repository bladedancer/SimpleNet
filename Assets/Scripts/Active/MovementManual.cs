using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementManual : MonoBehaviour {
    
    private MovementController moveCtl;

    private void Awake()
    {
        moveCtl = GetComponent<MovementController>();
    }

    // Update is called once per frame
    void Update()
    {
        moveCtl.SetMovement(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"));
    }
}
