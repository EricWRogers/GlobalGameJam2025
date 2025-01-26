using UnityEngine;
using OmnicatLabs.Input;
using Unity.Cinemachine;
using static Unity.Cinemachine.Samples.SimplePlayerController;

public class PlayerMovement : IState {
    private InputAxis moveX, moveZ;
    private BubbleController bubbleController;
    Vector3 m_LastRawInput;
    private Vector3 m_LastInput;
    private bool m_InTopHemisphere;
    private float m_TimeInHemisphere;
    private Quaternion m_Upsidedown;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 lastMagnitude;

    public PlayerMovement(BubbleController _controller) : base(_controller) {
        bubbleController = _controller;
        rb = bubbleController.GetComponentInChildren<Rigidbody>();
    }

    public override void Initialize() {
        bubbleController = controller as BubbleController;
        rb = bubbleController.GetComponentInChildren<Rigidbody>();
        
        moveX = bubbleController.MoveX;
        moveZ = bubbleController.MoveZ;
    }

    public override void Enter() {
        
    }

    public override void Exit() {
        
    }

    public override void FixedUpdate() {
        
        rb.AddForce(moveDirection * Time.deltaTime * bubbleController.force);

        if (rb.linearVelocity.magnitude > bubbleController.maxSpeed) {
            rb.linearVelocity = rb.linearVelocity.normalized * bubbleController.maxSpeed;
        }

        if (bubbleController.closestPlayer != null) {
            SoftTrack();
        }


    }

    private void SoftTrack() {
        Vector3 directionToTarget = (bubbleController.closestPlayer.transform.position - bubbleController.transform.position).normalized;
        float distanceToTarget = Vector3.Distance(bubbleController.transform.position, bubbleController.closestPlayer.transform.position);

        float dotProduct = Vector3.Dot(rb.linearVelocity.normalized, directionToTarget);

        if (dotProduct < bubbleController.minClosenessToTrack)
        {
            Debug.DrawLine(bubbleController.transform.position, bubbleController.closestPlayer.transform.position, Color.red); // Debug: Not influenced

            return;
        }

        float strength = Mathf.Lerp(0, bubbleController.trackingForce, Mathf.Clamp01((bubbleController.trackingRange - distanceToTarget) / bubbleController.trackingRange));
        Vector3 force = directionToTarget * strength;
        rb.AddForce(force, ForceMode.Force);
        Debug.DrawLine(bubbleController.transform.position, bubbleController.closestPlayer.transform.position, Color.green);
    }

    public override void Update() {
        ProcessInput();
    }

    private void ProcessInput() {
        //var rawInput = new Vector3(bubbleController.MoveX.Value, 0, bubbleController.MoveZ.Value);

        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

        if (bubbleController.Aim.Value >= 1) {
            Debug.Log("Aiming");
        }

        moveDirection = cameraForward * bubbleController.MoveZ.Value + cameraRight * bubbleController.MoveX.Value;

        //var rawInput = new Vector3(moveX.Value, 0, moveZ.Value);
        //var inputFrame = GetInputFrame(Vector3.Dot(rawInput, m_LastRawInput) < 0.8f);
        //m_LastRawInput = rawInput;

        //m_LastInput = inputFrame * rawInput;
        //if (m_LastInput.sqrMagnitude > 1)
        //    m_LastInput.Normalize();


    }

    //Quaternion GetInputFrame(bool inputDirectionChanged) {
    //    var frame = Camera.main.transform.rotation;

    //    var playerUp = controllerObject.transform.up;
    //    var up = frame * Vector3.up;

    //    const float BlendTime = 2f;
    //    m_TimeInHemisphere += Time.deltaTime;
    //    bool inTopHemisphere = Vector3.Dot(up, playerUp) >= 0;
    //    if (inTopHemisphere != m_InTopHemisphere) {
    //        m_InTopHemisphere = inTopHemisphere;
    //        m_TimeInHemisphere = Mathf.Max(0, BlendTime - m_TimeInHemisphere);
    //    }

    //    var axis = Vector3.Cross(up, playerUp);
    //    if (axis.sqrMagnitude < 0.001f && inTopHemisphere)
    //        return frame;

    //    var angle = UnityVectorExtensions.SignedAngle(up, playerUp, axis);
    //    var frameA = Quaternion.AngleAxis(angle, axis) * frame;

    //    Quaternion frameB = frameA;
    //    if (!inTopHemisphere || m_TimeInHemisphere < BlendTime) {
    //        // Compute an alternative reference frame for the bottom hemisphere.
    //        // The two reference frames are incompatible where they meet, especially
    //        // when player up is pointing along the X axis of camera frame. 
    //        // There is no one reference frame that works for all player directions.
    //        frameB = frame * m_Upsidedown;
    //        var axisB = Vector3.Cross(frameB * Vector3.up, playerUp);
    //        if (axisB.sqrMagnitude > 0.001f)
    //            frameB = Quaternion.AngleAxis(180f - angle, axisB) * frameB;
    //    }

    //    if (inputDirectionChanged)
    //        m_TimeInHemisphere = BlendTime;

    //    if (m_TimeInHemisphere >= BlendTime)
    //        return inTopHemisphere ? frameA : frameB;

    //    // Because frameA and frameB do not join seamlessly when player Up is along X axis,
    //    // we blend them over a time in order to avoid degenerate spinning.
    //    // This will produce weird movements occasionally, but it's the lesser of the evils.
    //    if (inTopHemisphere)
    //        return Quaternion.Slerp(frameB, frameA, m_TimeInHemisphere / BlendTime);
    //    return Quaternion.Slerp(frameA, frameB, m_TimeInHemisphere / BlendTime);
    //}
}
