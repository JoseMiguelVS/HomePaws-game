using UnityEngine;

public class PlayerCheckpoint : MonoBehaviour
{
    public Vector3 lastSafePosition;

    void Start()
    {
        // Guardamos la posición inicial al arrancar
        lastSafePosition = transform.position;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Si tocamos cualquier cosa que sea una plataforma o suelo (puedes verificar por capa o tag)
        // Guardamos esta posición como un punto totalmente seguro
        if (!collision.gameObject.CompareTag("Player"))
        {
            lastSafePosition = transform.position;
        }
    }
}