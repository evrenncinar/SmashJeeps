using System;
using Unity.Netcode;
using UnityEngine;

public class RocketController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider _rocketCollider;
    [Header("Settings")]
    [SerializeField] private float _rocketSpeed = 20f;
    [SerializeField] private float _rotationSpeed = 20f;
    [SerializeField] private bool _isMoving = false;


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetOwnerVisualRpc();
            RequestStartMovementFromServerRpc();
        }
    }

    private void Update()
    {
        if(IsServer && _isMoving)
        {
            MoveRocket();
        }
    }

    private void MoveRocket()
    {
        transform.position += _rocketSpeed * Time.deltaTime * transform.forward;
        transform.Rotate(Vector3.forward, _rotationSpeed * Time.deltaTime, Space.Self);
    }

    [Rpc(SendTo.Server)]
    private void RequestStartMovementFromServerRpc()
    {
        _isMoving = true;
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualRpc()
    {
        _rocketCollider.enabled = false;
    }
}
