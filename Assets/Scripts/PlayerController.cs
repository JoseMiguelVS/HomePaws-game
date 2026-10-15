using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private Animator animator;

    private float lastDirection = 0f;
    private bool isTurning = false;

    [Header("Animaciones")]
    public string idleAnimation = "Sol_idle";
    public string runAnimation = "Sol_run";
    public string jumpAnimation = "Sol_jump";
    public string fallAnimation = "Sol_fall";
    public string turnAnimation = "Sol_turn";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float moveInput = Input.GetAxis("Horizontal");

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );
        }

        // Detectar cambio de direcci�n
        if (moveInput != 0 && lastDirection != 0)
        {
            if (Mathf.Sign(moveInput) != Mathf.Sign(lastDirection))
            {
                animator.Play(turnAnimation);
                isTurning = true;
            }
        }

        // Guardar �ltima direcci�n
        if (moveInput != 0)
        {
            lastDirection = moveInput;
        }

        // Girar personaje
        if (moveInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        SetAnimation(moveInput);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void SetAnimation(float moveInput)
    {
        if (isTurning)
        {
            return;
        }

        if (isGrounded)
        {
            if (moveInput == 0)
            {
                animator.Play(idleAnimation);
            }
            else
            {
                animator.Play(runAnimation);
            }
        }
        else
        {
            if (rb.linearVelocity.y > 0)
            {
                animator.Play(jumpAnimation);
            }
            else
            {
                animator.Play(fallAnimation);
            }
        }
    }

    public void FinishTurn()
    {
        isTurning = false;
    }
}