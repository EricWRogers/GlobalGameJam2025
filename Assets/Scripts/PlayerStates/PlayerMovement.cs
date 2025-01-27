using UnityEngine;
using OmnicatLabs.Input;
using Unity.Cinemachine;
using OmnicatLabs.Timers;

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
    private bool dashed = false;


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


        if (rb.linearVelocity.magnitude < bubbleController.lastVelocity.magnitude) {
            rb.linearVelocity = rb.linearVelocity.normalized * rb.linearVelocity.magnitude;
        }

        // 0 pointing the same way 1 pointing the opposite
        float oppositeAmount = Mathf.Clamp(Vector3.Dot(rb.linearVelocity.normalized, moveDirection.normalized) * -1, 0.0f, 1.0f);
        float speedPercentage = rb.linearVelocity.magnitude/bubbleController.maxSpeed;
        float breakingForce = speedPercentage * oppositeAmount * bubbleController.oppositeForce;

        rb.AddForce(moveDirection * (bubbleController.force + breakingForce));


        if (rb.linearVelocity.magnitude > bubbleController.maxSpeed) {
            rb.linearVelocity = rb.linearVelocity.normalized * bubbleController.maxSpeed;
        }

        if (bubbleController.closestPlayer != null) {
            SoftTrack();
        }

        bubbleController.lastVelocity = rb.linearVelocity;
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

    private Vector3 DirectionOfClosestPlayer() => (bubbleController.closestPlayer.transform.position - bubbleController.transform.position).normalized;

    private float DistanceToClosestPlayer() => Vector3.Distance(bubbleController.transform.position, bubbleController.closestPlayer.transform.position);

    public override void Update() {
        ProcessInput();




        if (bubbleController.closestPlayer)
        {
        if (DistanceToClosestPlayer() <= bubbleController.distanceToDash) {
            bubbleController.ShowReticle();
            if (bubbleController.Fire.Value >= 1 && !dashed) {
                dashed = true;
                TimerManager.Instance.CreateTimer(bubbleController.abilityCooldown, () => { dashed = false; bubbleController.btnCtrl.EndCooldown(); }, out Timer timer);
                bubbleController.btnCtrl.StartCoolDown(timer);
                Vector3 desiredVelocity = (bubbleController.closestPlayer.transform.position - bubbleController.transform.position) / 0.3f;
                rb.linearVelocity = desiredVelocity;
                //rb.AddForce(DirectionOfClosestPlayer() * bubbleController.dashForce, ForceMode.Impulse);
            }
        } else {
            bubbleController.HideReticle();
        }
        }
        else
        {
            bubbleController.HideReticle();
        }

        //if (Vector3.Distance(bubbleController.transform.position, bubbleController.closestPlayer.transform.position) < bubbleController.distaceToCollide) {
        //    if (rb.linearVelocity.magnitude > bubbleController.closestPlayer.GetComponent<Rigidbody>().linearVelocity.magnitude) {
        //        DoKnockServerRpc();
        //    }
        //}
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void DoKnockServerRpc() {
    //    var dir = (bubbleController.closestPlayer.transform.position - bubbleController.transform.position).normalized;
    //    Debug.Log("doing knock");
    //    bubbleController.closestPlayer.GetComponent<Rigidbody>().linearVelocity = dir * rb.linearVelocity.magnitude * 10000f;
    //}

    private void ProcessInput() {
        //var rawInput = new Vector3(bubbleController.MoveX.Value, 0, bubbleController.MoveZ.Value);

        Vector3 cameraForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 cameraRight = Vector3.Scale(Camera.main.transform.right, new Vector3(1, 0, 1)).normalized;

        if (bubbleController.Aim.Value >= 1) {
            Debug.Log("Aiming");
        }

        moveDirection = cameraForward * bubbleController.MoveZ.Value + cameraRight * bubbleController.MoveX.Value;
    }
}
