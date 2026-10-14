using UnityEngine;

public class Collectible : MonoBehaviour
{
    void OnTriggerEnter2D(Collider other)
    {
        Debug.Log("El player esta dentro del disparador");
    }

    void OnTriggerStay2D(Collider other)
    {
        Debug.Log("El player esta dentro del disparador");
    }

    void OnTriggerExit2D(Collider other)
    {
        Debug.Log("El player esta dentro del disparador");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
