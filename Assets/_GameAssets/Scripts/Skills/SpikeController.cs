using System;
using Unity.Netcode;
using UnityEngine;

public class SpikeController : NetworkBehaviour
{
    [SerializeField] private Collider _spikeCollider;

    public override void OnNetworkSpawn()
    {
        PlayerSkillController.OnTimerFinished += OnTimerFinished;
        SetOwnerVisualRpc();
    }

    private void OnTimerFinished()
    {
        DestroyRpc();
    }

    public override void OnNetworkDespawn()
    {
        PlayerSkillController.OnTimerFinished -= OnTimerFinished;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }

    }

    [Rpc(SendTo.Owner)]
    private void SetOwnerVisualRpc()
    {
        if (IsOwner)
        {
            _spikeCollider.enabled = false;
        }
    }
}
