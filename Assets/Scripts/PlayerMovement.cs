using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Basic Jumping
    private float horizontal;
    private float vertical;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpingPower = 16f;
    private bool isFacingRight = true;

    //Double Jumping
    [SerializeField] private int maxJumps = 1;
    private int jumpsRemaining;

    //Dashing
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.3f;
    private bool isDashing;
    private bool airDashAvailable = true;
    private float dashTimeRemaining;
    private float dashCooldownRemaining;
    private float originalGravityScale;
    private Vector2 dashDirection;

    //Wall Jumping
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.15f;
    [SerializeField] private float wallJumpHorizontalForce = 8f;
    [SerializeField] private float wallJumpVerticalForce = 8f;
    [SerializeField] private float wallJumpDuration = 0.2f;
    private bool isWallJumping;
    private float wallJumpTimer;
    private int wallJumpDirection;

    //Ground Detection
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    //Animations
    private Animator animator;

    private bool jumpRequested;

    



    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current == null) return;
        

        horizontal = 0f;

        

        if (Keyboard.current.aKey.isPressed)
        {
            horizontal = -1f;
            if (IsGrounded())
                animator.SetBool("Run", true);
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            horizontal = 1f;
            if (IsGrounded())
                animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Run", false);
        }

        bool jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame;

        if (jumpPressed)
        {
            if (IsTouchingWall(out int direction) && !IsGrounded())
            {
                StartWallJump(direction);
            }
            else if (jumpsRemaining > 0)
            {
                jumpRequested = true;
                jumpsRemaining--;
                /*if(rb.linearVelocity.y > 0f)
                    animator.SetBool("Jump", true);
                else if(rb.linearVelocity.y < 0f)
                    animator.SetBool("Fall", true);
                else if(IsGrounded())
                {
                    animator.SetBool("Jump", false);
                    animator.SetBool("Fall", false);
                }*/
            }
        }

        if (IsGrounded())
        {
            jumpsRemaining = maxJumps;
        }

        if ((Keyboard.current.spaceKey.wasReleasedThisFrame || Keyboard.current.wKey.wasReleasedThisFrame) && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * 0.5f
            );
        }


        if (dashCooldownRemaining > 0f)
        {
            dashCooldownRemaining -= Time.deltaTime;
        }

        if (IsGrounded() && rb.linearVelocity.y <= 0.01f)
        {
            airDashAvailable = true;
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame)
        {
            bool canGroundDash = IsGrounded();
            bool canAirDash = !IsGrounded() && airDashAvailable;

            if (!isDashing && dashCooldownRemaining <= 0f && (canGroundDash || canAirDash))
            {
            StartDash();
            }
        }

        Flip();
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (isDashing)
        {
            dashTimeRemaining -= Time.fixedDeltaTime;
            rb.linearVelocity = dashDirection * dashSpeed;

            if (dashTimeRemaining <= 0f)
                StopDash();

            return;
        }

        if (isWallJumping)
        {
            wallJumpTimer -= Time.fixedDeltaTime;

            rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpHorizontalForce, wallJumpVerticalForce);
            
            if (wallJumpTimer <= 0f)
            {
                isWallJumping = false;
            }
            return;
        }

        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);

        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            jumpRequested = false;
        }
    }

    private void Awake()
    {
        jumpsRemaining = maxJumps;
        originalGravityScale = rb.gravityScale;

        animator = GetComponent<Animator>();
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(
            groundCheck.position, 
            0.2f,  
            groundLayer
        ) != null;
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimeRemaining = dashDuration;
        dashCooldownRemaining = dashCooldown;

        dashDirection = isFacingRight ? Vector2.right : Vector2.left;

        if (!IsGrounded())
            airDashAvailable = false;

        rb.gravityScale = 0f;
        rb.linearVelocity = dashDirection * dashSpeed;
    }

    private void StopDash()
    {
        isDashing = false;
        rb.gravityScale = originalGravityScale;

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }


    private bool IsTouchingWall(out int jumpDirection)
    {
        jumpDirection = 0;

        bool touchingLeft = wallCheckLeft != null &&
            Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer) != null;

        bool touchingRight = wallCheckRight != null && 
            Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer) != null;

        if (touchingLeft)
        {
            if (!isFacingRight)
                jumpDirection = 1;
            else if (isFacingRight)
                jumpDirection = -1;
            return true;
        }

        if (touchingRight)
        {
            if (!isFacingRight)
                jumpDirection = 1;
            else if (isFacingRight)
                jumpDirection = -1;
            return true;
        }

        return false;
    }


    private void StartWallJump(int direction)
    {
        if (isDashing) return;

        isWallJumping = true;
        wallJumpTimer = wallJumpDuration;
        wallJumpDirection = direction;
        jumpsRemaining = maxJumps;
        airDashAvailable = true;

        rb.linearVelocity = new Vector2(
            direction * wallJumpHorizontalForce,
            wallJumpVerticalForce
        );

    
        if ((direction > 0 && !isFacingRight) ||
            (direction < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;

            Vector3 scale = transform.localScale;
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }



    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
