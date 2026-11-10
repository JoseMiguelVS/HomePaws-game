using UnityEngine;

public class EstrellaFollower : MonoBehaviour
{
    [Header("Referencias")]
    public Transform targetToFollow; 

    [Header("Configuración")]
    public float followSpeed = 4f;
    public float stoppingDistance = 1.5f; 
    public float maxDistanceBeforeTeleport = 7f; 

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isFollowing = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Intentar buscar a Sol automáticamente por etiqueta si no está asignado
        if (targetToFollow == null)
        {
            GameObject playerSol = GameObject.FindGameObjectWithTag("Player");
            if (playerSol != null)
            {
                targetToFollow = playerSol.transform;
            }
        }
    }

    void Update()
    {
        // 1. Verificar si tiene permiso para seguir
        if (!isFollowing) 
        {
            return; // Aún no se activa
        }

        // 2. Verificar si encuentra a Sol
        if (targetToFollow == null)
        {
            Debug.LogError("¡ERROR: Estrella no encuentra a Sol (targetToFollow es null)! Asigna el objeto PlayerSol en el Inspector o revisa su etiqueta.");
            return;
        }

        float distance = Vector2.Distance(transform.position, targetToFollow.position);

        // 🚨 3. TELETRANSPORTE DE SEGURIDAD (Si se atora o se aleja demasiado)
        if (distance > maxDistanceBeforeTeleport || Mathf.Abs(transform.position.x - targetToFollow.position.x) > maxDistanceBeforeTeleport)
        {
            transform.position = new Vector2(targetToFollow.position.x - 1.5f, targetToFollow.position.y);
            if (anim != null) anim.SetBool("isRunning", false);
            Debug.Log("¡Estrella se atascó y se teletransportó junto a Sol!");
            return;
        }

        // 4. SEGUIMIENTO NORMAL
        if (distance > stoppingDistance)
        {
            Vector2 targetPos = new Vector2(targetToFollow.position.x, targetToFollow.position.y);
            transform.position = Vector2.MoveTowards(transform.position, targetPos, followSpeed * Time.deltaTime);

            // Control de dirección del Sprite
            if (targetToFollow.position.x > transform.position.x)
            {
                spriteRenderer.flipX = false; 
            }
            else if (targetToFollow.position.x < transform.position.x)
            {
                spriteRenderer.flipX = true; 
            }

            if (anim != null) anim.SetBool("isRunning", true);
        }
        else
        {
            if (anim != null) anim.SetBool("isRunning", false);
        }
    }

    public void StartFollowing()
    {
        isFollowing = true;
        Debug.Log("¡El método StartFollowing() se ejecutó correctamente en Estrella!");
    }
}