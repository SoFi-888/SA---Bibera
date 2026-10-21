using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    public float rotationSpeed = 12f;

    [Header("X - Rotate")]
    public float spinSpeed = 180f;

    [Header("Y - Scale")]
    public float scaleRate = 1f;
    public float minScale = 0.5f;
    public float maxScale = 3f;

    [Header("K - Speed Boost")]
    public float boostMultiplier = 2f;

    private CharacterController controller;
    private Animator animator;
    private Transform cam;
    private Vector3 velocity;
    private Vector3 startScale;
    private float scaleFactor = 1f;
    private bool boosted;

    public bool IsBoosted => boosted;
    public float SpeedMultiplier => boosted ? boostMultiplier : 1f;

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (Camera.main != null) cam = Camera.main.transform;
        startScale = transform.localScale;
    }

    void Update()
    {
        HandleSpecialKeys();
        HandleMovement();
    }

    void HandleSpecialKeys()
    {
        // X: hold to spin the character around the Y (up) axis
        if (IsHeld(KeyName.X))
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);

        // Y: hold to grow, hold Y + Left Shift to shrink (real time)
        if (IsHeld(KeyName.Y))
        {
            float dir = IsHeld(KeyName.Shift) ? -1f : 1f;
            scaleFactor = Mathf.Clamp(scaleFactor + dir * scaleRate * Time.deltaTime, minScale, maxScale);
            transform.localScale = startScale * scaleFactor;
        }

        // K: toggle speed boost on/off
        if (WasPressed(KeyName.K))
            boosted = !boosted;
    }

    void HandleMovement()
    {
        Vector2 input = ReadMoveInput();
        bool jumpPressed = WasPressed(KeyName.Space);

        float yaw = cam != null ? cam.eulerAngles.y : 0f;
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 forward = yawRot * Vector3.forward;
        Vector3 right = yawRot * Vector3.right;

        Vector3 moveDir = forward * input.y + right * input.x;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        bool isMoving = moveDir.sqrMagnitude > 0.001f;

        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving && controller.isGrounded);
            animator.speed = boosted ? boostMultiplier : 1f; // walk animation speeds up with boost
        }

        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (jumpPressed && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;

        float currentSpeed = boosted ? moveSpeed * boostMultiplier : moveSpeed;
        Vector3 finalMove = moveDir * currentSpeed + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }

    enum KeyName { X, Y, K, Space, Shift }

    Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return Vector2.zero;
        float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        return new Vector2(x, y);
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    bool IsHeld(KeyName key)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return false;
        switch (key)
        {
            case KeyName.X: return kb.xKey.isPressed;
            case KeyName.Y: return kb.yKey.isPressed;
            case KeyName.K: return kb.kKey.isPressed;
            case KeyName.Space: return kb.spaceKey.isPressed;
            case KeyName.Shift: return kb.leftShiftKey.isPressed;
        }
        return false;
#else
        switch (key)
        {
            case KeyName.X: return Input.GetKey(KeyCode.X);
            case KeyName.Y: return Input.GetKey(KeyCode.Y);
            case KeyName.K: return Input.GetKey(KeyCode.K);
            case KeyName.Space: return Input.GetKey(KeyCode.Space);
            case KeyName.Shift: return Input.GetKey(KeyCode.LeftShift);
        }
        return false;
#endif
    }

    bool WasPressed(KeyName key)
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return false;
        switch (key)
        {
            case KeyName.X: return kb.xKey.wasPressedThisFrame;
            case KeyName.Y: return kb.yKey.wasPressedThisFrame;
            case KeyName.K: return kb.kKey.wasPressedThisFrame;
            case KeyName.Space: return kb.spaceKey.wasPressedThisFrame;
            case KeyName.Shift: return kb.leftShiftKey.wasPressedThisFrame;
        }
        return false;
#else
        switch (key)
        {
            case KeyName.X: return Input.GetKeyDown(KeyCode.X);
            case KeyName.Y: return Input.GetKeyDown(KeyCode.Y);
            case KeyName.K: return Input.GetKeyDown(KeyCode.K);
            case KeyName.Space: return Input.GetKeyDown(KeyCode.Space);
            case KeyName.Shift: return Input.GetKeyDown(KeyCode.LeftShift);
        }
        return false;
#endif
    }
}