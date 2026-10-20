using UnityEngine;

public class FootSensor : MonoBehaviour
{
    private PlayerCheckpoint playerCheckpoint;

    void Start()
    {
        playerCheckpoint = GetComponentInParent<PlayerCheckpoint>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && !collision.isTrigger)
        {
            if (playerCheckpoint != null)
            {
                playerCheckpoint.lastSafePosition = playerCheckpoint.transform.position;
            }
        }
    } 
}