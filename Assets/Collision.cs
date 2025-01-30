using Unity.Netcode;
using UnityEngine;

public class Collision : NetworkBehaviour
{
    public float knockbackMultiplier = 50f;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {


            if (other.CompareTag("MapMechanic"))
            {
                Debug.Log("Entering");
                GetComponent<BubbleController>().lastVelocity = Vector3.zero;
            }


            if (other.CompareTag("Player"))
            {
                Debug.Log("Player Found");
                var otherRb = other.GetComponentInParent<Rigidbody>();

                if (otherRb != null)
                {
                    float myVelocity = rb.linearVelocity.magnitude;
                    float otherVelocity = otherRb.linearVelocity.magnitude;
                    Debug.Log("knock");
                    if (myVelocity > otherVelocity)
                    {
                        Vector3 dir = (transform.position - other.transform.position).normalized;
                        otherRb.linearVelocity = dir * myVelocity * knockbackMultiplier;
                    }
                    else
                     if (myVelocity < otherVelocity)
                    {
                        Vector3 knockbackDirection = (other.transform.position - transform.position).normalized;
                        rb.linearVelocity = knockbackDirection * otherVelocity * knockbackMultiplier;
                    }
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MapMechanic"))
        {
            Debug.Log("Staying");
            GetComponent<BubbleController>().lastVelocity = Vector3.zero;
        }
    }
}
