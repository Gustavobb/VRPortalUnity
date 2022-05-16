using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : GravityBody
{
    [SerializeField]
    float mouseSensitivity = 5f, rotationSmoothTime = 0.05f, smoothMoveTime = .1f;

    [SerializeField]
    bool paused = false;
    float mouseX, mouseY, rotationX, smoothPitch, smoothYaw, yawSmoothV, pitchSmoothV;
    Camera cam;

    protected override void Start()
    {
        base.Start();
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandlePauseGame();
        if (paused) return;
        
        HandlePlayerMovement();
        if (canJump) HandleJump();

        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }

    void FixedUpdate() 
    {
        if (paused) return;
        
        ApplyGravity();
        HandleLookMovement();

        rb.MovePosition(transform.position + velocity * Time.deltaTime);
        cam.transform.localRotation = Quaternion.Euler(smoothPitch, 0, 0);
        transform.rotation *= Quaternion.Euler(0, smoothYaw, 0);
    }

    void HandleJump()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, distanceToTheGround + 0.01f, ground);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            rb.AddForce(new Vector3(0, jumpHeight, 0));
    }

    void HandleLookMovement()
    {
        mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -75f, 75f);

        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, rotationX, ref pitchSmoothV, rotationSmoothTime);
        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, mouseX, ref yawSmoothV, rotationSmoothTime);
    }

    void HandlePlayerMovement()
    { 
        direction = (transform.right * Input.GetAxisRaw("Horizontal") + transform.forward * Input.GetAxisRaw("Vertical")).normalized;
        targetVelocity = direction * speed;
        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref smoothVelocity, smoothMoveTime);
    }

    public void HandlePauseGame()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        paused = !paused;
        Cursor.lockState = CursorLockMode.Locked;
        if (paused) Cursor.lockState = CursorLockMode.None;        
        Cursor.visible = paused;
        Time.timeScale = paused ? 0 : 1;
    }
}
