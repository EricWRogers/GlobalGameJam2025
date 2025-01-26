using UnityEngine;
using OmnicatLabs.Input;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.Timeline;
using System.Linq;

public class BubbleController : PlayerControllerBase, Unity.Cinemachine.IInputAxisOwner {
    public override IState[] States => new IState[] { new PlayerIdle(this), new PlayerMovement(this), };
    [Header("Input Axes")]
    [Tooltip("X Axis movement.  Value is -1..1.  Controls the sideways movement")]
    public InputAxis MoveX = InputAxis.DefaultMomentary;

    [Tooltip("Z Axis movement.  Value is -1..1. Controls the forward movement")]
    public InputAxis MoveZ = InputAxis.DefaultMomentary;

    [Tooltip("Jump movement.  Value is 0 or 1. Controls the vertical movement")]
    public InputAxis Jump = InputAxis.DefaultMomentary;

    [Tooltip("Sprint movement.  Value is 0 or 1. If 1, then is sprinting")]
    public InputAxis Sprint = InputAxis.DefaultMomentary;

    public float force = 1000f;
    public float maxSpeed = 50f;
    private List<GameObject> players = new List<GameObject>();
    private GameObject closestPlayer;


    void IInputAxisOwner.GetInputAxes(List<IInputAxisOwner.AxisDescriptor> axes) {
        axes.Add(new() {
            DrivenAxis = () => ref MoveX,
            Name = "Move X",
            Hint = IInputAxisOwner.AxisDescriptor.Hints.X
        });
        axes.Add(new() {
            DrivenAxis = () => ref MoveZ,
            Name = "Move Z",
            Hint = IInputAxisOwner.AxisDescriptor.Hints.Y
        });
        axes.Add(new() {
            DrivenAxis = () => ref Jump,
            Name = "Jump"
        });
        axes.Add(new() {
            DrivenAxis = () => ref Sprint,
            Name = "Sprint"
        });
    }

    protected override void Start() {
        base.Start();

        players = GameObject.FindGameObjectsWithTag("Player").ToList();
        players.Remove(gameObject);
        Debug.Log(players.DebugFormat());
    }

    public override void Update() {
        base.Update();

        if (IsMoving() && currentState is not PlayerMovement) {
            ChangeState<PlayerMovement>();
        }

        if (transform.position.y <= GameRules.Instance.killLevel) {
            GameRules.Instance.KillPlayer(this);
        }

        TrackPkayers();
        Debug.Log(closestPlayer.name);
    }

    private void TrackPkayers() {
        float shortestDistance = float.MaxValue;
        foreach (var player in players)
        {
            Vector3 directionToPlayer = player.transform.position - transform.position;
            float distanceToPlayer = directionToPlayer.magnitude;

            if (Physics.Linecast(transform.position, player.transform.position)) {
                if (distanceToPlayer < shortestDistance) {
                    shortestDistance = distanceToPlayer;
                    closestPlayer = player;
                }
            }

            //if (Physics.Linecast(transform.position, player.transform.position, out RaycastHit hit)) {
            //    if (hit.collider.CompareTag("Player")) {
            //        if (Vector3.Distance(transform.position, hit.transform.position) < shortestDistance) {
                        
            //            closestPlayer = player;
            //            Debug.Log("Updated closest");
            //        }
            //    }
            //}
        }
    }

    private bool IsMoving() {
        return new Vector3(MoveX.Value, 0, MoveZ.Value).magnitude > 0.1f;
    }

    public override void FixedUpdate() {
        base.FixedUpdate();
    }
}
