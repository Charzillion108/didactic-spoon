using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 3f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float sensitivity = 0.1f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Feel Settings (Tilt)")]
    [SerializeField] private float tiltAmount = 3f; // Reduced slightly for smoother look
    [SerializeField] private float tiltSpeed = 10f; // Adjusted for better reaction

    [Header("Bobbing Settings")]
    [SerializeField] private float walkingBobAmount = 0.05f;
    [SerializeField] private float sprintingBobAmount = 0.1f;
    [SerializeField] private float crouchingBobAmount = 0.02f;
    [SerializeField] private float bobFrequency = 10f;

    [Header("Audio")]
    [SerializeField] private AudioSource footstepSource;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    
    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool isMovementDisabled = false;
    private bool isDashing = false;
    private float nextDashTime = 0f;

    private float defaultYPos = 0f;
    private float timer = 0f;
    
    // --- NEW SMOOTHING VARIABLES ---
    private float currentTilt = 0f;
    // -------------------------------

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        if (cameraTransform != null) defaultYPos = cameraTransform.localPosition.y;
    }

    private void Update()
    {
        if (isMovementDisabled) 
        {
            if (footstepSource != null && footstepSource.isPlaying) footstepSource.Stop();
            return;
        }

        HandleRotation();
        
        if (!isDashing)
        {
            HandleMovement();
            HandleHeadBob();
        }
        
        HandleTilt();
    }

    // --- INPUT EVENTS ---
    public void OnMove(InputAction.CallbackContext context) => moveInput = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (isMovementDisabled || isDashing) return;
        if (context.started && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    public void OnSprint(InputAction.CallbackContext context) => isSprinting = context.ReadValueAsButton();

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isCrouching = true;
            controller.height = 1f;
        }
        else if (context.canceled)
        {
            isCrouching = false;
            controller.height = 2f;
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (isMovementDisabled || !context.started || isDashing || Time.time < nextDashTime) return;
        StartCoroutine(DashRoutine());
    }

    // --- LOGIC ---

    private void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (cameraTransform == null) return;
        verticalRotation -= lookInput.y * sensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        
        // This only handles looking UP/DOWN
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x * sensitivity);
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;
        float startTime = Time.time;

        Vector3 dashDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (dashDir == Vector3.zero) dashDir = transform.forward;

        while (Time.time < startTime + dashDuration)
        {
            float lerpVal = (Time.time - startTime) / dashDuration;
            float currentDashSpeed = Mathf.Lerp(dashSpeed, dashSpeed * 0.2f, lerpVal);
            
            controller.Move(dashDir * currentDashSpeed * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
    }

    private void HandleHeadBob()
    {
        if (cameraTransform == null || !controller.isGrounded) return;

        if (Mathf.Abs(moveInput.x) > 0.1f || Mathf.Abs(moveInput.y) > 0.1f)
        {
            float currentBobAmount = walkingBobAmount;
            if (isCrouching) currentBobAmount = crouchingBobAmount;
            else if (isSprinting) currentBobAmount = sprintingBobAmount;

            timer += Time.deltaTime * bobFrequency;
            cameraTransform.localPosition = new Vector3(
                cameraTransform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * currentBobAmount,
                cameraTransform.localPosition.z
            );
        }
        else
        {
            timer = 0f;
            cameraTransform.localPosition = new Vector3(
                cameraTransform.localPosition.x,
                Mathf.Lerp(cameraTransform.localPosition.y, defaultYPos, Time.deltaTime * bobFrequency),
                cameraTransform.localPosition.z
            );
        }
    }

    // --- REFINED TILT LOGIC ---
    private void HandleTilt()
    {
        if (cameraTransform == null) return;
        
        // Determine target tilt based on horizontal input (A/D keys)
        float targetTilt = -moveInput.x * tiltAmount;
        
        // Smoothly move the current tilt towards the target tilt
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
        
        // Apply the tilt to the camera's rotation without affecting look up/down
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, currentTilt);
    }
    // --------------------------

    public void DisableMovement()
    {
        isMovementDisabled = true;
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        if (footstepSource != null) footstepSource.Stop();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}