using Unity.Netcode;
using UnityEngine;

public class MysteryBoxCollectible : NetworkBehaviour, ICollectible
{
    [Header("References")]
    [SerializeField] private MysteryBoxSkillSO[] _mysteryBoxSkills;
    [SerializeField] private Animator _boxAnimator;
    [SerializeField] private Collider _boxCollider;
    [Header("Settings")]
    [SerializeField] private float _respawnTimer;

    public void Collect(PlayerSkillController playerSkillController)
    {
        if (playerSkillController.HasSkillAlready()) { return; }

        MysteryBoxSkillSO skill = GetRandomSkill();
        SkillsUI.Instance.SetSkill(skill.SkillName, skill.SkillIcon, skill.SkillUsageType, skill.SkillData.SpawnAmountOrTimer);
        playerSkillController.SetUpSkill(skill);
        
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

    private MysteryBoxSkillSO GetRandomSkill()
    {
        int randomIndex = Random.Range(0, _mysteryBoxSkills.Length);
        return _mysteryBoxSkills[randomIndex];
    }
}
