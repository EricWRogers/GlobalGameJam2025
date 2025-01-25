using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    public float speedingTicket = 20f;

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            col.GetComponent<Rigidbody>().AddForce(transform.forward * (col.GetComponent<Rigidbody>().linearVelocity.magnitude + speedingTicket), ForceMode.Impulse);
        }
    }
}
