using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private Vector2 inputVector;
    private Vector3 velocity;
    private bool isSprinting = false;

    // By using [SerializeField], these stay private but show up in the Inspector
    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;

    [Header("Physics")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -15f; // Buffed gravity for better feel

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            isSprinting = true;
        else if (context.canceled)
            isSprinting = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            // The math for a perfect jump arc
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void Update()
    {
        MovePlayer();
        ApplyGravity();
    }

    private void MovePlayer()
    {
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = new Vector3(inputVector.x, 0, inputVector.y);
        move = transform.TransformDirection(move);
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}