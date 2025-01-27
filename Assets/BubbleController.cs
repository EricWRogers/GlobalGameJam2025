using UnityEngine;
using OmnicatLabs.Input;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine.Timeline;
using System.Linq;
using Unity.Netcode;
using UnityEngine.UI;

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

    public InputAxis Aim = InputAxis.DefaultMomentary;
    public InputAxis Fire = InputAxis.DefaultMomentary;

    public float force = 1000f;
    public float oppositeForce = 10.0f;
    public float maxSpeed = 50f;
    private List<GameObject> players = new List<GameObject>();
    [HideInInspector]
    public GameObject closestPlayer;
    public float trackingForce = 50f;
    public float trackingRange = 5f;
    public float minClosenessToTrack = 1f;
    public GameObject cameraRig;
    private GameObject rig;
    public float distaceToCollide = 0.2f;
    public float knockbackMultiplier = 1.2f;
    [HideInInspector]
    public Vector3 lastVelocity;
    public float distanceToDash = 10f;
    public float abilityCooldown = 2f;
    public float dashForce = 10f;
    [HideInInspector]
    public AbilityButtonControl btnCtrl;
    public ReticleController reticleController;

    private bool isBelowKillLevel = false;

    private float lastLifeChangeTime = 0f;
    public float lifeChangeCooldown = .5f;
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
        axes.Add(new() {
            DrivenAxis = () => ref Aim,
            Name = "Aim"
        });
        axes.Add(new() {
            DrivenAxis = () => ref Fire,
            Name = "Fire"
        });
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        if (IsLocalPlayer) {
            rig = Instantiate(cameraRig);
            var cc = rig.GetComponentInChildren<CinemachineCamera>();
            cc.Follow = transform;
        }
    }

    private void Awake() {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (!IsServer) {
            return; 
        }

        ulong clientId = transform.GetComponent<NetworkObject>().OwnerClientId;
        
        var id = GetComponent<NetworkObject>().OwnerClientId;
        NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client);
            
        rig = Instantiate(cameraRig);
        rig.GetComponentInChildren<CinemachineCamera>().Follow = client.PlayerObject.transform;
        
    }

    protected override void Start() {
        base.Start();

        players = GameObject.FindGameObjectsWithTag("Player").ToList();
        players.Remove(gameObject);
        btnCtrl = GetComponentInChildren<AbilityButtonControl>();
        reticleController = GetComponentInChildren<ReticleController>();
    }

    public override void Update() {
        base.Update();

        //ulong clientId = transform.GetComponent<NetworkObject>().OwnerClientId;

        //bool isLocal = true;

        //if (NetworkManager.Singleton)
        //    isLocal = NetworkManager.Singleton.LocalClientId == clientId;


        //if (isLocal) {
        //    rig.GetComponentInChildren<CinemachineCamera>().Follow = transform;
        //}

        if (IsMoving() && currentState is not PlayerMovement) {
            ChangeState<PlayerMovement>();
        }

        ulong clientId = gameObject.GetComponent<NetworkObject>().OwnerClientId;

        

        bool isLocal = true;

        if (NetworkManager.Singleton)
            isLocal = NetworkManager.Singleton.LocalClientId == clientId;

        if (isLocal)
        {

            if (Time.time - lastLifeChangeTime >= lifeChangeCooldown)
            {
                lastLifeChangeTime = Time.time;


                if (transform.position.y <= GameRules.Instance.killLevel && isBelowKillLevel == false)
                {
                    isBelowKillLevel = true;
                    var id = GetComponent<NetworkObject>().OwnerClientId;


                    GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                    GameRules.Instance.KillPlayerServerRPC(id);
                }
                else
                {
                    isBelowKillLevel = false;
                }
            }
        }
        TrackPkayers();
    }

    public void ShowReticle() {
        reticleController.GetComponent<Image>().enabled = true;
    }

    public void HideReticle() {
        reticleController.GetComponent<Image>().enabled = false;
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
