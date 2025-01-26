using Unity.VisualScripting;
using UnityEngine;

public class Collision : MonoBehaviour
{
    public float knockbackMultiplier = 50f;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Debug.Log("Player Found");
            var otherRb = other.GetComponentInParent<Rigidbody>();

            if (otherRb != null) {
                float myVelocity = rb.linearVelocity.magnitude;
                float otherVelocity = otherRb.linearVelocity.magnitude;
                Debug.Log("knock");
                if (myVelocity > otherVelocity) {
                    Vector3 dir = (other.transform.position - transform.position).normalized;
                    otherRb.linearVelocity = dir * myVelocity * knockbackMultiplier;
                } else if (myVelocity < otherVelocity) {
                    Vector3 knockbackDirection = (transform.position - other.transform.position).normalized;
                    rb.linearVelocity = knockbackDirection * otherVelocity * knockbackMultiplier;
                }
            }
        }
    }
}
