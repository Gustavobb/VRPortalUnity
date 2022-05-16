using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityBody : MonoBehaviour
{
    public float gravity = -10f, speed = 5f, jumpHeight = 5f;
    
    [SerializeField]
    protected bool hasGravity = true, canJump = true;
    protected bool isGrounded = false;

    [SerializeField]
    protected LayerMask ground;
    
    [HideInInspector]
    public Vector3 velocity;
    
    [HideInInspector]
    public Rigidbody rb;

    [HideInInspector]
    public float distanceToTheGround;

    protected Vector3 direction;
    protected Vector3 targetVelocity, smoothVelocity;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        distanceToTheGround = GetComponent<Collider>().bounds.extents.y;
    }

    protected virtual void ApplyGravity() 
    {
        if (hasGravity) 
        {
            rb.AddForce(Vector3.up * gravity);
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, 1.5f * speed);
        }
    }

}
