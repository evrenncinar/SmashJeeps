using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/SkillData")]
public class SkillDataSO : ScriptableObject
{
    [SerializeField] private Transform skillPrefab;
    [SerializeField] private Vector3 _skillOffset;
    [SerializeField] private int _spawnAmountOrTimer;
    [SerializeField] private bool _shouldBeAttachedToParent;
    [SerializeField] private int _respawnTimer;
    [SerializeField] private int _damageAmount;

    public Transform SkillPrefab => skillPrefab;
    public Vector3 SkillOffset => _skillOffset;
    public int SpawnAmountOrTimer => _spawnAmountOrTimer;
    public bool ShouldBeAttachedToParent => _shouldBeAttachedToParent;
    public int RespawnTimer => _respawnTimer;
    public int DamageAmount => _damageAmount;
}
