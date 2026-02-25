using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 inputVector;
    private Vector2 lookInput;
    private Vector3 velocity;
    private bool isSprinting = false;
    private float xRotation = 0f;

    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 20f;
    [SerializeField] private Transform cameraTransform;

    [Header("Physics")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -15f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // --- INPUT EVENTS (Link these in the Player Input component) ---
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
        if (context.started || context.performed) isSprinting = true;
        else if (context.canceled) isSprinting = false;
    }

    // --- THE ONLY UPDATE FUNCTION ---
    private void Update()
    {
        HandleLook();
        HandleMovement();
        ApplyGravity();
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
        float speed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * inputVector.x + transform.forward * inputVector.y;
        controller.Move(move * speed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;

        // Safety against NaN errors
        if (float.IsNaN(velocity.y)) velocity.y = 0;

        controller.Move(velocity * Time.deltaTime);
    }
}