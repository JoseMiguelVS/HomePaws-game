using UnityEngine;

public class OnTriggerFin : MonoBehaviour
{
    [SerializeField] private GameObject m_gameObject;
    [SerializeField] private GameObject m_parent;
    [SerializeField] private GameObject m_target;
    [SerializeField] private GameObject m_source;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag ("trash"))
        {
            m_gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag ("trash"))
        {
            m_gameObject.SetActive(false);
        }
    }
}
