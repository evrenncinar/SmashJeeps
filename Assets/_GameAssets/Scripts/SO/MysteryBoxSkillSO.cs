using UnityEngine;

[CreateAssetMenu(fileName = "MysteryBoxSkill", menuName = "ScriptableObjects/MysteryBoxSkill")]
public class MysteryBoxSkillSO : ScriptableObject
{
    [SerializeField] private string _skillName;
    [SerializeField] private Sprite _skillIcon;
    [SerializeField] private SkillType _skillType;
    [SerializeField] private SkillUsageType _skillUsageType;
    [SerializeField] private SkillDataSO _skillData;

    public string SkillName => _skillName;
    public Sprite SkillIcon => _skillIcon;
    public SkillType SkillType => _skillType;
    public SkillUsageType SkillUsageType => _skillUsageType;
    public SkillDataSO SkillData => _skillData;
}
