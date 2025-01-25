using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.transform.position;
        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));
    }
}
