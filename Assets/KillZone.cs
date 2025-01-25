using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<BubbleController>()) {
            
        }
    }
}
