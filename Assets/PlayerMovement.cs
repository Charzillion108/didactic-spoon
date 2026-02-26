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
    private bool wasGrounded;

    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 20f;
    [SerializeField] private Transform cameraTransform;

    [Header("Head Bob")]
    [SerializeField] private float walkBobSpeed = 12f;
    [SerializeField] private float walkBobAmount = 0.04f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Physics")]
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float jumpHeight = 2f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform == null) cameraTransform = GetComponentInChildren<Camera>().transform;
        if (cameraTransform != null) defaultYPos = cameraTransform.localPosition.y;
    }

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

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleHeadBob();
        ApplyGravity();

        if (!wasGrounded && controller.isGrounded) StartCoroutine(LandingBounce());
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
        float speed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * inputVector.x + transform.forward * inputVector.y;
        controller.Move(move * speed * Time.deltaTime);
    }

    private void HandleHeadBob()
    {
        if (!controller.isGrounded || inputVector.magnitude < 0.1f) return;
        timer += Time.deltaTime * walkBobSpeed;
        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, defaultYPos + Mathf.Sin(timer) * walkBobAmount, cameraTransform.localPosition.z);
    }

    private IEnumerator LandingBounce()
    {
        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, defaultYPos - 0.12f, cameraTransform.localPosition.z);
        yield return new WaitForSeconds(0.05f);
        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, defaultYPos, cameraTransform.localPosition.z);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}