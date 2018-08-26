using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour {
    [SerializeField]
    public float motion;

    [SerializeField]
    public float rotation;

    private Rigidbody rb;
    private Stats stats;

    public void SetMovement(float motion, float rotation)
    {
        this.motion = motion;
        this.rotation = rotation;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stats = GetComponent<Stats>();
        rotation = 0;
        motion = 0;
    }

    private void Start()
    {
        motion = 0;
        rotation = 0;
    }

    private void FixedUpdate()
    {
        Vector3 targetRotation = new Vector3(
            0,
            rb.rotation.eulerAngles.y + (stats.RotationSpeed * rotation * Time.fixedDeltaTime),
            0
        );
        rb.rotation = Quaternion.Euler(targetRotation);
        rb.velocity = transform.forward * motion * stats.MovementSpeed * Time.fixedDeltaTime;
    }
}
