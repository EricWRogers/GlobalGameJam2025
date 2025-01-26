using UnityEngine;

public class ReticleController : MonoBehaviour
{
    public float offsetDistance = 1.5f;
    private BubbleController player;
    private void Start() {
        player = GetComponentInParent<BubbleController>();
    }

    private void Update() {

        Vector3 directionToCamera = (Camera.main.transform.position - player.transform.position).normalized;
        transform.position = player.transform.position + directionToCamera * offsetDistance;

        transform.LookAt(Camera.main.transform);

        transform.Rotate(0, 180, 0);
    }
}
