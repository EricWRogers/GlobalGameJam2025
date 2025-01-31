using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerPhysicsMovement : NetworkBehaviour
{
    [SerializeField] private float movementForce = 10f;
    [SerializeField] private float predictionThreshold = 0.5f;

    private Rigidbody rb;
    private Vector3 serverPosition;
    private Vector3 serverVelocity;
    private Queue<PlayerInput> inputQueue = new Queue<PlayerInput>();

    private struct PlayerInput
    {
        public Vector2 movement;
        public ulong tick;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        PlayerInput playerInput = new PlayerInput { movement = input, tick = (ulong)NetworkManager.Singleton.LocalTime.Tick };
        inputQueue.Enqueue(playerInput);

        ApplyInput(playerInput);

        SendInputToServerRpc(input, (ulong)NetworkManager.Singleton.LocalTime.Tick);
    }

    [ServerRpc]
    private void SendInputToServerRpc(Vector2 input, ulong tick)
    {
        Vector3 force = new Vector3(input.x, 0, input.y) * movementForce;
        rb.AddForce(force);

        UpdateClientStateClientRpc(rb.position, rb.linearVelocity, tick);
    }

    [ClientRpc]
    private void UpdateClientStateClientRpc(Vector3 position, Vector3 velocity, ulong tick)
    {
        if (!IsOwner)
        {
            rb.position = position;
            rb.linearVelocity = velocity;
        }
        else
        {
            serverPosition = position;
            serverVelocity = velocity;
            ReconcilePrediction(tick);
        }
    }

    private void ApplyInput(PlayerInput input)
    {
        Vector3 force = new Vector3(input.movement.x, 0, input.movement.y) * movementForce;
        rb.AddForce(force);
    }

    private void ReconcilePrediction(ulong serverTick)
    {
        while (inputQueue.Count > 0 && inputQueue.Peek().tick <= serverTick)
        {
            inputQueue.Dequeue();
        }

        if (Vector3.Distance(rb.position, serverPosition) > predictionThreshold)
        {
            rb.position = serverPosition;
            rb.linearVelocity = serverVelocity;
           
            foreach (var input in inputQueue)
            {
                ApplyInput(input);
            }
        }
    }
}
