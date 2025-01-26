using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    public Transform cameraController;

    public float moveSpeed = 10f;

    private float xInput;
    private float zInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        xInput = Input.GetAxis("Horizontal");
        zInput = Input.GetAxis("Vertical");
    }

    void FixedUpdate()
    {

        var moveDirection = cameraController.forward * zInput + cameraController.right * xInput;
        rb.AddForce(moveDirection * moveSpeed);

    }
}
