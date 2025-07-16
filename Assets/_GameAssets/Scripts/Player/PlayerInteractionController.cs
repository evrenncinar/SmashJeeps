using Unity.Netcode;
using UnityEngine;

public class PlayerInteractionController : NetworkBehaviour
{
    private PlayerSkillController _playerSkillController;
    private PlayerVehicleController _playerVehicleController;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        _playerSkillController = GetComponent<PlayerSkillController>();
        _playerVehicleController = GetComponent<PlayerVehicleController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) { return; }

        if (other.gameObject.TryGetComponent(out ICollectible collectible))
        {
            collectible.Collect(_playerSkillController);
        }

        if(other.gameObject.TryGetComponent(out IDamageable damageable))
        {
            damageable.Damage(_playerVehicleController);
        }
    }
}
