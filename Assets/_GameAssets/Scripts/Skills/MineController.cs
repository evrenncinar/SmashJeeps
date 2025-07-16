using Unity.Netcode;
using UnityEngine;

public class MineController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Collider _mineCollider;

    [Header("Settings")]
    [SerializeField] private float _fallspeed;
    [SerializeField] private float _raycastDistance;
    [SerializeField] private LayerMask _groundLayer;

    private bool _hasLanded;
    private Vector3 _lastSentPosition;

    private void Update()
    {
        if (!IsServer || _hasLanded) { return; }

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
        {
            _hasLanded = true;
            transform.position = hit.point;

            if (_lastSentPosition != transform.position)
            {
                SyncPositionRpc(transform.position);
                _lastSentPosition = transform.position;
            }
        }
        else
        {
            transform.position += Vector3.down * _fallspeed * Time.deltaTime;
            
            if (_lastSentPosition != transform.position)
            {
                SyncPositionRpc(transform.position);
                _lastSentPosition = transform.position;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SyncPositionRpc(Vector3 position)
    {
        if(IsServer) { return; }
        transform.position = position;
    }

    override public void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SetOwnerVisualRpc();
        }
    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualRpc()
    {
        _mineCollider.enabled = false;
    }
}
