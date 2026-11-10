using UnityEngine;

public class ItemEnergia : MonoBehaviour
{
    public float recargaEnergia = 30f; 
    public int puntosQueOtorga = 1;   

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("¡Algo tocó al coleccionable: " + collision.gameObject.name + " con Tag: " + collision.tag);

        if (collision.CompareTag("Player"))
        {
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.RecargarEnergia(recargaEnergia);
                ScoreManager.instance.AgregarPuntos(puntosQueOtorga);
            }

            Destroy(gameObject);
        }
    }
}