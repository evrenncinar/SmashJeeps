using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerSkillController : NetworkBehaviour
{
    public static event Action<ulong> OnTimerFinished;
    [Header("References")]
    [SerializeField] private Transform _rocketTransform;
    [SerializeField] private Transform _rocketLauncherPoint;

    [Header("Settings")]
    [SerializeField] private bool _hasSkillAlready;
    [SerializeField] private float _resetDelay = 2f;
    private MysteryBoxSkillSO _currentSkill;
    private bool _isSkillUsed;
    private bool _hasTimerStarted;
    private float _timer;
    private float _timerMax;
    private int _mineAmountCounter;

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
                OnTimerFinished?.Invoke(OwnerClientId);
                SkillsUI.Instance.SetSkillToNone();
                _hasTimerStarted = false;
                _hasSkillAlready = false;
            }
        }
    }

    public void SetUpSkill(MysteryBoxSkillSO skill)
    {
        _currentSkill = skill;

        if (_currentSkill.SkillType == SkillType.Rocket)
        {
            RocketSkillRpc(true);
        }
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

        if (_currentSkill.SkillType == SkillType.Rocket)
        {
            StartCoroutine(ResetRocketLauncher());
        }

        SkillManager.Instance.ActiveSkill(_currentSkill.SkillType, transform, OwnerClientId);
        SetSkillToNone();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void RocketSkillRpc(bool active)
    {
        _rocketTransform.gameObject.SetActive(active);
    }


    private IEnumerator ResetRocketLauncher()
    {
        yield return new WaitForSeconds(_resetDelay);
        RocketSkillRpc(false);
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
        if (_currentSkill.SkillUsageType == SkillUsageType.Amount)
        {
            _mineAmountCounter = _currentSkill.SkillData.SpawnAmountOrTimer;
            SkillManager.Instance.OnMineCountChanged += SkillManager_OnMineCountChanged;
        }
    }

    private void SkillManager_OnMineCountChanged()
    {
        _mineAmountCounter--;
        SkillsUI.Instance.SetTimerCounterText(_mineAmountCounter);

        if (_mineAmountCounter <= 0)
        {
            _hasSkillAlready = false;
            SkillsUI.Instance.SetSkillToNone();
            SkillManager.Instance.OnMineCountChanged -= SkillManager_OnMineCountChanged;
        }
    }

    public Vector3 GetRocketLauncherPoint()
    {
        return _rocketLauncherPoint.position;
    }
}
