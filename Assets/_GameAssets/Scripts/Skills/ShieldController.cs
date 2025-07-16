using System;
using Unity.Netcode;
using UnityEngine;

public class ShieldController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        PlayerSkillController.OnTimerFinished += OnTimerFinished;
    }

    public override void OnNetworkDespawn()
    {
        PlayerSkillController.OnTimerFinished -= OnTimerFinished;
    }

    private void OnTimerFinished(ulong _clientId)
    {
        if(_clientId != OwnerClientId) return;
        DestroyRpc();
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void DestroyRpc()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }

    }
}
