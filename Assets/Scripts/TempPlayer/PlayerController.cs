using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    public Transform cameraController;
    public Vector3 lastVelocity;

    public float moveSpeed = 10f;
    public float maxSpeed = 20.0f;

    private float xInput;
    private float zInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        lastVelocity = rb.linearVelocity;
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
        
        if (rb.linearVelocity.magnitude < lastVelocity.magnitude) {
            rb.linearVelocity = rb.linearVelocity.normalized * rb.linearVelocity.magnitude;
        }

        rb.AddForce(moveDirection * moveSpeed);


        if (rb.linearVelocity.magnitude > maxSpeed) {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        lastVelocity = rb.linearVelocity;

    }
}
