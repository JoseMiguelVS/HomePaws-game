using UnityEngine;

public class SpawnEstrellaTrigger : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject estrellaObject;

    private bool yaAparecio = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!yaAparecio && collision.CompareTag("Player"))
        {
            yaAparecio = true;

            if (estrellaObject != null)
            {
                estrellaObject.transform.position = collision.transform.position + new Vector3(1.5f, 0f, 0f);

                estrellaObject.SetActive(true);
                
                Debug.Log("¡Estrella apareció junto a Sol con éxito!");
            }
        }
    }
}