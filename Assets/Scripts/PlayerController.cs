
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Detección del suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    private bool isTurning = false;
    private bool jumpRequested = false;

    private float lastDirection = 0f;
    private float moveInput;

    [Header("Animaciones")]
    public string idleAnimation = "Sol_idle";
    public string runAnimation = "Sol_run";
    public string jumpAnimation = "Sol_jump";
    public string fallAnimation = "Sol_fall";
    public string turnAnimation = "Sol_turn";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Registrar la pulsación para procesar el salto en FixedUpdate.
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpRequested = true;
        }

        // Detectar cambio de dirección.
        if (moveInput != 0 && lastDirection != 0)
        {
            if (Mathf.Sign(moveInput) != Mathf.Sign(lastDirection))
            {
                animator.Play(turnAnimation);
                isTurning = true;
            }
        }

        if (moveInput != 0)
        {
            lastDirection = moveInput;
        }

        // Girar el personaje.
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
        // Comprobar si el personaje está en el suelo.
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Movimiento horizontal.
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        // Ejecutar el salto solo si sigue en el suelo.
        if (jumpRequested)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );
            }

            // Consumir la petición para evitar saltos repetidos.
            jumpRequested = false;
        }
    }

    private void SetAnimation(float input)
    {
        if (isTurning)
            return;

        if (isGrounded)
        {
            if (input == 0)
                animator.Play(idleAnimation);
            else
                animator.Play(runAnimation);
        }
        else
        {
            if (rb.linearVelocity.y > 0)
                animator.Play(jumpAnimation);
            else
                animator.Play(fallAnimation);
        }
    }

    public void FinishTurn()
    {
        isTurning = false;
    }

    // Mostrar el área de detección del suelo en el editor.
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
