using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Detección del suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Animaciones")]
    public string idleAnimation = "Sol_idle";
    public string runAnimation = "Sol_run";
    public string jumpAnimation = "Sol_jump";
    public string fallAnimation = "Sol_fall";
    public string turnAnimation = "Sol_turn";

    [Header("Coleccionables")]
    public TMP_Text textAtun;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    private bool isTurning;
    private bool jumpRequested;

    private float lastDirection;
    private float moveInput;
    private int atun;

    private int currentAnimationHash;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null)
            Debug.LogError("Falta Rigidbody2D en el personaje.");

        if (animator == null)
            Debug.LogError("Falta Animator en el personaje.");
    }

    private void Update()
    {
        if (rb == null || animator == null)
            return;

        // Actualizar la detección del suelo antes de leer el salto.
        CheckGrounded();

        moveInput = Input.GetAxisRaw("Horizontal");

        // Registrar el salto solamente estando en el suelo.
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpRequested = true;
        }

        // Girar el personaje y detectar cambios de dirección.
        if (moveInput != 0)
        {
            if (lastDirection != 0 &&
                Mathf.Sign(moveInput) != Mathf.Sign(lastDirection))
            {
                isTurning = true;
                PlayAnimation(turnAnimation);
            }

            lastDirection = moveInput;

            transform.localScale = new Vector3(
                moveInput > 0 ? 1f : -1f,
                1f,
                1f
            );
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // Comprobar de nuevo el suelo antes de ejecutar el salto.
        CheckGrounded();

        // Movimiento horizontal.
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );

        // Ejecutar y consumir una única petición de salto.
        if (jumpRequested)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                isGrounded = false;
            }

            jumpRequested = false;
        }
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void UpdateAnimation()
    {
        if (isTurning)
            return;

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.05f)
            {
                PlayAnimation(jumpAnimation);
            }
            else
            {
                PlayAnimation(fallAnimation);
            }
        }
        else
        {
            if (Mathf.Abs(moveInput) > 0.01f)
                PlayAnimation(runAnimation);
            else
                PlayAnimation(idleAnimation);
        }
    }

    private void PlayAnimation(string animationName)
    {
        int hash = Animator.StringToHash(animationName);

        // Evitar reiniciar la misma animación en cada fotograma.
        if (currentAnimationHash == hash)
            return;

        if (!animator.HasState(0, hash))
        {
            Debug.LogWarning(
                "No se encuentra el estado de animación: " +
                animationName +
                ". Revisa su nombre en el Animator."
            );
            return;
        }

        animator.Play(hash);
        currentAnimationHash = hash;
    }

    // Añade un Animation Event al final de Sol_turn
    // que llame a este método.
    public void FinishTurn()
    {
        isTurning = false;
        currentAnimationHash = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("atun"))
        {
            Destroy(collision.gameObject);
            atun++;

            if (textAtun != null)
                textAtun.text = atun.ToString();
        }
    }

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