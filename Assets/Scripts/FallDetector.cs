using UnityEngine;

public class FallDetector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name.Contains("PlayerSol") || collision.name.Contains("PlayerEstrella"))
        {
            PlayerCheckpoint checkpoint = collision.GetComponent<PlayerCheckpoint>();
            if (checkpoint == null)
            {
                checkpoint = collision.GetComponentInParent<PlayerCheckpoint>();
            }

            if (checkpoint != null)
            {
                Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                // Teletransportamos al último punto seguro registrado
                collision.transform.position = checkpoint.lastSafePosition;
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );
            }
        }
    }
}