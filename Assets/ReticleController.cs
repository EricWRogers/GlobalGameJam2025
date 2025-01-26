using UnityEngine;

public class ReticleController : MonoBehaviour
{
    private void Update() {
        transform.LookAt(Camera.main.transform);

        // To make it perfectly flat, you can also rotate it around the X-axis to avoid flipping:
        Vector3 eulerRotation = transform.eulerAngles;
        eulerRotation.x = 0; // Keep the image upright
        transform.eulerAngles = eulerRotation;
    }
}
