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
    
    // States
    private bool isSprinting = false;
    private bool isCrouching = false;
    private bool isDashing = false;
    private float xRotation = 0f;
    private bool wasGrounded;

    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 20f;
    [SerializeField] private Transform cameraTransform;

    [Header("Crouch & Slam")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float slamForce = -20f; // Velocity when crouching in air

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 30f;
    [SerializeField] private float dashDuration = 0.2f;

    [Header("Physics")]
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float jumpHeight = 2f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (cameraTransform == null)
            cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    // --- INPUT EVENTS (For Invoke Unity Events) ---
    
    public void OnMove(InputAction.CallbackContext context) => inputVector = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => lookInput = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, crouchHeight - 0.5f, cameraTransform.localPosition.z);
        }
        else if (context.canceled)
        {
            isCrouching = false;
            controller.height = standingHeight;
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, standingHeight - 0.5f, cameraTransform.localPosition.z);
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing && inputVector.magnitude > 0.1f)
        {
            StartCoroutine(DashRoutine());
        }
    }

    // --- UPDATES & LOGIC ---
    private void Update()
    {
        HandleLook();
        
        if (!isDashing) 
        {
            HandleMovement();
            ApplyGravity();
        }

        wasGrounded = controller.isGrounded;
    }

    private void HandleLook()
    {
        if (cameraTransform == null) return;
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 move = transform.right * inputVector.x + transform.forward * inputVector.y;
        controller.Move(move * speed * Time.deltaTime);
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        
        Vector3 dashDirection = (transform.right * inputVector.x + transform.forward * inputVector.y).normalized;
        if (dashDirection == Vector3.zero) dashDirection = transform.forward;

        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashDirection * dashForce * Time.deltaTime);
            yield return null;
        }

        isDashing = false;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}