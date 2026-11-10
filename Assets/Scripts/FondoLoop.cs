using UnityEngine;

public class FondoLoop : MonoBehaviour
{
    private Transform transformCamara;
    [SerializeField] private float tamanoSprite = 18.5f; 

    private void Start()
    {
        transformCamara = Camera.main.transform;
    }

    private void Update()
    {
        if (transformCamara.position.x - transform.position.x >= tamanoSprite)
        {
            transform.position += new Vector3(tamanoSprite * 2f, 0, 0);
        }
        else if (transform.position.x - transform.position.x < -tamanoSprite)
        {
            transform.position -= new Vector3(tamanoSprite * 2f, 0, 0);
        }
    }
}