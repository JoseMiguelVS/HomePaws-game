using UnityEngine; 

public class PlatformCheckpoint : MonoBehaviour
{ 
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.name.Contains("PlayerSol") || collision.gameObject.name.Contains("PlayerEstrella"))
        {
            PlayerCheckpoint checkpoint = collision.gameObject.GetComponent<PlayerCheckpoint>();
            if (checkpoint != null)
            {
                checkpoint.lastSafePosition = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
            }
        }
    }
} 