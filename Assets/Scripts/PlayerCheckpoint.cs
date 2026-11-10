using UnityEngine;

public class PlayerCheckpoint : MonoBehaviour
{
    public Vector3 lastSafePosition;

    void Start()
    {
        lastSafePosition = transform.position;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            lastSafePosition = transform.position;
        }
    }
}