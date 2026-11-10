using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Deteccion del suelo")]
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

    [Header("Control")]
    [Tooltip("Lo activa y desactiva ControlDuo. Si esta apagado, el personaje se queda quieto en Idle.")]
    public bool controlActivo = true;

    /// <summary>Atun recogido en toda la partida (lo muestra el HUD).</summary>
    public static int AtunTotal { get; private set; }

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    private bool isTurning;
    private bool jumpRequested;
    private bool estabaEnSuelo = true;

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

        // Actualizar la deteccion del suelo antes de leer el salto.
        CheckGrounded();

        if (!controlActivo)
        {
            // El personaje inactivo del duo se queda quieto en Idle.
            moveInput = 0f;
            jumpRequested = false;
            UpdateAnimation();
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        // Registrar el salto solamente estando en el suelo.
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jumpRequested = true;

        // Girar el personaje y detectar cambios de direccion.
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

        // Sonido al tomar contacto con el suelo tras una caida o un salto.
        if (isGrounded && !estabaEnSuelo)
            GestorAudio.Sonar(Sonido.Aterrizaje, 0.7f);

        estabaEnSuelo = isGrounded;

        // Movimiento horizontal.
        rb.linearVelocity = new Vector2(
            controlActivo ? moveInput * moveSpeed : 0f,
            rb.linearVelocity.y
        );

        // Ejecutar y consumir una unica peticion de salto.
        if (jumpRequested)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                isGrounded = false;
                estabaEnSuelo = false;

                GestorAudio.Sonar(Sonido.Salto, 0.8f);
            }

            jumpRequested = false;
        }
    }

    /// <summary>
    /// Activa o desactiva las entradas de este personaje. Lo usa ControlDuo
    /// para que solo el personaje activo se mueva.
    /// </summary>
    public void EstablecerControl(bool activo)
    {
        controlActivo = activo;

        if (activo)
            return;

        moveInput = 0f;
        jumpRequested = false;

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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
                PlayAnimation(jumpAnimation);
            else
                PlayAnimation(fallAnimation);
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
        if (string.IsNullOrEmpty(animationName))
            return;

        int hash = Animator.StringToHash(animationName);

        // Evitar reiniciar la misma animacion en cada fotograma.
        if (currentAnimationHash == hash)
            return;

        if (!animator.HasState(0, hash))
        {
            // Estrella solo tiene Idle: si falta un estado, se usa Idle
            // en vez de llenar la consola de avisos.
            if (animationName != idleAnimation && !string.IsNullOrEmpty(idleAnimation))
                PlayAnimation(idleAnimation);

            return;
        }

        animator.Play(hash);
        currentAnimationHash = hash;
    }

    // Anade un Animation Event al final de Sol_turn
    // que llame a este metodo.
    public void FinishTurn()
    {
        isTurning = false;
        currentAnimationHash = 0;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!controlActivo)
            return;

        if (collision.CompareTag("atun"))
        {
            Destroy(collision.gameObject);

            AtunTotal++;
            atun = AtunTotal;

            GestorAudio.Sonar(Sonido.Moneda);

            if (textAtun != null)
                textAtun.text = "Coleccionable: " + atun;
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
