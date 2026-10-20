using UnityEngine;

public class EstrellaFollower : MonoBehaviour
{
    public Transform targetToFollow; 
    public float followSpeed = 4f;
    public float stoppingDistance = 1.5f; 
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private bool isFollowing = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (targetToFollow == null)
        {
            GameObject playerSol = GameObject.FindGameObjectWithTag("Player");
            if (playerSol != null && playerSol != this.gameObject)
            {
                targetToFollow = playerSol.transform;
            }
        }
    }

    void Update()
    {
        if (!isFollowing || targetToFollow == null) return;

        float distance = Vector2.Distance(transform.position, targetToFollow.position);

        if (distance > stoppingDistance)
        {
            Vector2 targetPosition = new Vector2(targetToFollow.position.y, transform.position.y); 
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(targetToFollow.position.x, transform.position.y), followSpeed * Time.deltaTime);

            if (targetToFollow.position.x > transform.position.x)
            {
                spriteRenderer.flipX = false; 
            }
            else
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
    }
}