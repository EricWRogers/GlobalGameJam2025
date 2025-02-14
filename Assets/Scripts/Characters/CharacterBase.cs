using UnityEngine;
using UnityEngine.InputSystem;

public interface ICharacter {
    public void OnBasicAttack(InputValue input);
    public void OnAbility(InputValue input);
    public void OnUltimate(InputValue input);
    public void OnUseItem(InputValue input);
}


public abstract class CharacterBase : MonoBehaviour, ICharacter {
    [Header("Movement Settings")]
    public float acceleration = 15f;
    public float maxSpeed = 20f;

    [Header("Rotation & Torque Settings")]
    [Tooltip("Adjust to control how much torque is applied for visual roll")]
    public float rollTorque = 10f;

    [Header("Drift Settings")]
    [Range(0f, 1f)]
    public float driftFactor = 0.9f;

    private Rigidbody rb;
    private Transform cameraTransform;
    private float horizontal;
    private float vertical;

    [Header("Character")]
    public PassiveAbility passive;
    [Tooltip("In seconds")]
    public float abilityCooldown = 10f;
    [Tooltip("In seconds")]
    public float ultimateCooldown = 15f;
    [Tooltip("Max energy capacity and requirement for ultimate")]
    public float utlimateMaxEnergy = 100f;

    // The input callbacks are passed directly through these to the inheritor
    public abstract void OnAbility(InputValue input);
    public abstract void OnBasicAttack(InputValue input);
    public abstract void OnUltimate(InputValue input);
    public abstract void OnUseItem(InputValue input);

    protected virtual void Start() {
        passive.Activate(this);
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
    }

    protected virtual void Update() {
        if (passive.IsEnabled)
            passive.Tick();
    }

    protected virtual void FixedUpdate() {
        HandleMovement();

        if (passive.IsEnabled)
            passive.FixedTick();
    }

    public virtual void OnMove(InputValue input) {
        var value = input.Get<Vector2>();
        horizontal = value.x; vertical = value.y;
    }

    private void HandleMovement() {
        // use the camera's forward/right vectors to allow directional movement independent of ball's rotation.
        Vector3 camForward = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight = cameraTransform.right;
        Vector3 moveDirection = (camForward * vertical + camRight * horizontal).normalized;

        if (moveDirection != Vector3.zero) {
            rb.AddForce(moveDirection * acceleration, ForceMode.Acceleration);
        }

        // Cap the maximum speed (only horizontal speed)
        Vector3 horizontalVelocity = Vector3.Scale(rb.linearVelocity, new Vector3(1, 0, 1));
        if (horizontalVelocity.magnitude > maxSpeed) {
            horizontalVelocity = horizontalVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        // Apply torque so the ball visually rolls in the direction of movement.
        if (moveDirection != Vector3.zero) {
            Vector3 torqueVector = Vector3.Cross(Vector3.up, moveDirection);
            rb.AddTorque(torqueVector * rollTorque, ForceMode.Acceleration);
        }

        // reduce lateral velocity. Simulates a loss of grip when turning.
        Vector3 forwardVel = Vector3.Project(rb.linearVelocity, moveDirection);
        Vector3 lateralVel = rb.linearVelocity - forwardVel;
        rb.linearVelocity = forwardVel + lateralVel * driftFactor;
    }
}
