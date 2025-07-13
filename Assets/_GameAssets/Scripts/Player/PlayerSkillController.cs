using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerSkillController : NetworkBehaviour
{
    public static event Action OnTimerFinished;

    [SerializeField] private bool _hasSkillAlready;
    private MysteryBoxSkillSO _currentSkill;
    private bool _isSkillUsed;
    private bool _hasTimerStarted;
    private float _timer;
    private float _timerMax;

    private void Update()
    {
        if (!IsOwner) { return; }

        if (Input.GetKeyDown(KeyCode.Space) && !_isSkillUsed)
        {
            ActivateSkill();
            _isSkillUsed = true;
        }

        if (_hasTimerStarted)
        {
            _timer -= Time.deltaTime;
            SkillsUI.Instance.SetTimerCounterText((int)_timer);
            if (_timer <= 0f)
            {
                OnTimerFinished?.Invoke();
                SkillsUI.Instance.SetSkillToNone();
                _hasTimerStarted = false;
                _hasSkillAlready = false;
            }
        }
    }

    public void SetUpSkill(MysteryBoxSkillSO skill)
    {
        _currentSkill = skill;
        _hasSkillAlready = true;
        _isSkillUsed = false;

    }

    public bool HasSkillAlready()
    {
        return _hasSkillAlready;
    }

    public void ActivateSkill()
    {
        if (_currentSkill == null) return;
        SkillManager.Instance.ActiveSkill(_currentSkill.SkillType, transform, OwnerClientId);
        SetSkillToNone();
    }

    private void SetSkillToNone()
    {
        if (_currentSkill.SkillUsageType == SkillUsageType.None)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
        }
        if (_currentSkill.SkillUsageType == SkillUsageType.Timer)
        {
            _hasTimerStarted = true;
            _timerMax = _currentSkill.SkillData.SpawnAmountOrTimer;
            _timer = _timerMax;
        }
    }
}
