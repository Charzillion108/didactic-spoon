using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 inputVector;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isSprinting = false;
    private float xRotation = 0f;
    private int jumpCount = 0;
    private bool isDashing = false;
    private bool isCrouching = false;

    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Look & Tilt")]
    [SerializeField] private float mouseSensitivity = 20f;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float tiltAmount = 2.5f; 
    [SerializeField] private float tiltSpeed = 6f;

    [Header("Physics")]
    [SerializeField] private float jumpHeight = 2.5f;
    [SerializeField] private float gravity = -28f;
    [SerializeField] private int maxJumps = 2;

    [Header("Crouch & Slam")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float slamForce = -35f;

    [Header("Dash & Effects")]
    [SerializeField] private float dashForce = 40f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float baseFOV = 60f;
    [SerializeField] private float dashFOV = 80f;
    [SerializeField] private float fovSpeed = 10f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        
        // Lock cursor for single-player warehouse exploration
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mainCamera == null) mainCamera = GetComponentInChildren<Camera>();
        if (mainCamera != null) mainCamera.fieldOfView = baseFOV;
    }

    public void OnMove(InputAction.CallbackContext context) => inputVector = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && (controller.isGrounded || jumpCount < maxJumps))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpCount++;
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (!controller.isGrounded) velocity.y = slamForce; 
            isCrouching = true;
            controller.height = crouchHeight;
        }
        else if (context.canceled)
        {
            isCrouching = false;
            controller.height = standingHeight;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing) StartCoroutine(Dash());
    }

    private void Update()
    {
        HandleLook();
        
        if (!isDashing) 
        {
            if (mainCamera != null) 
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, baseFOV, Time.deltaTime * fovSpeed);

            HandleMovement();
            ApplyGravity();
        }
    }

    private void HandleLook()
    {
        if (cameraTransform == null) return;
        
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        float targetTilt = -inputVector.x * tiltAmount;
        float currentTilt = Mathf.LerpAngle(cameraTransform.localEulerAngles.z, targetTilt, Time.deltaTime * tiltSpeed);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, currentTilt);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 move = transform.right * inputVector.x + transform.forward * inputVector.y;
        controller.Move(move * speed * Time.deltaTime);
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        if (dashTrail != null) dashTrail.emitting = true;

        float startTime = Time.time;
        Vector3 dashDir = transform.right * inputVector.x + transform.forward * inputVector.y;
        if (dashDir == Vector3.zero) dashDir = transform.forward; 

        while (Time.time < startTime + dashDuration)
        {
            if (mainCamera != null) 
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, dashFOV, Time.deltaTime * fovSpeed);

            controller.Move(dashDir * dashForce * Time.deltaTime);
            yield return null;
        }

        if (dashTrail != null) dashTrail.emitting = false;
        isDashing = false;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}