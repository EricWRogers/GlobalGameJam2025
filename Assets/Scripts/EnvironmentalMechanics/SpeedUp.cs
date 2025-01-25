using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    public float speedingTicket = 20f;

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            col.GetComponentInParent<Rigidbody>().AddForce(transform.forward * (col.GetComponentInParent<Rigidbody>().linearVelocity.magnitude + speedingTicket), ForceMode.Impulse);
        }
    }
}
