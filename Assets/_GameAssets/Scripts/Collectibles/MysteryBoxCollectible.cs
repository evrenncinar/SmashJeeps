using Unity.Netcode;
using UnityEngine;

public class MysteryBoxCollectible : NetworkBehaviour, ICollectible
{
    [Header("References")]
    [SerializeField] private Animator _boxAnimator;
    [SerializeField] private Collider _boxCollider;
    [Header("Settings")]
    [SerializeField] private float _respawnTimer;

    public void Collect()
    {
        Debug.Log("Mystery Box Collected!");
        CollectRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CollectRpc()
    {
        AnimateCollection();
        Invoke(nameof(Respawn), _respawnTimer);
    }

    private void AnimateCollection()
    {
        _boxCollider.enabled = false;
        _boxAnimator.SetTrigger(Consts.BoxAnimations.IS_COLLECTED);
    }

    private void Respawn()
    {
        _boxAnimator.SetTrigger(Consts.BoxAnimations.IS_RESPAWNED);
        _boxCollider.enabled = true;
    }
}
