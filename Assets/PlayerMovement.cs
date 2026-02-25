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

    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 20f;
    [SerializeField] private Transform cameraTransform;

    [Header("Physics")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private int maxJumps = 2;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    private bool isCrouching = false;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing = false;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- INPUT EVENTS ---
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
        if (context.started) StartCrouch();
        else if (context.canceled) StopCrouch();
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && !isDashing) StartCoroutine(Dash());
    }

    // --- MOVEMENT LOGIC ---
    private void Update()
    {
        HandleLook();
        if (!isDashing) 
        {
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
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
        Vector3 move = transform.right * inputVector.x + transform.forward * inputVector.y;
        controller.Move(move * speed * Time.deltaTime);
    }

    private void StartCrouch()
    {
        isCrouching = true;
        controller.height = crouchHeight;
    }

    private void StopCrouch()
    {
        isCrouching = false;
        controller.height = standingHeight;
    }

    private IEnumerator Dash()
    {
        isDashing = true;
        float startTime = Time.time;
        
        // Dash in the direction of WASD; if no keys held, dash forward
        Vector3 dashDir = transform.right * inputVector.x + transform.forward * inputVector.y;
        if (dashDir == Vector3.zero) dashDir = transform.forward; 

        while (Time.time < startTime + dashDuration)
        {
            controller.Move(dashDir * dashForce * Time.deltaTime);
            yield return null;
        }

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