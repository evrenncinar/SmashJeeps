using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerVehicleController : NetworkBehaviour
{
    public class SpringData
    {
        public float _currentLength;
        public float _currentVelocity;
    }

    private static readonly WheelType[] _wheels = new WheelType[]
    {
        WheelType.FrontLeft, WheelType.FrontRight, WheelType.BackLeft, WheelType.BackRight
    };

    private static readonly WheelType[] _backWheels = new WheelType[]
    {
        WheelType.BackLeft, WheelType.BackRight
    };

    [Header("References")]
    [SerializeField] private VehicleSettingsSO _vehicleSettings;
    [SerializeField] private Rigidbody _vehicleRigidbody;
    [SerializeField] private BoxCollider _vehicleCollider;
    private Dictionary<WheelType, SpringData> _springDatas;
    private float _steerInput;
    private float _accelerationInput;
    public Vector3 Velocity => _vehicleRigidbody.linearVelocity;
    public Vector3 Forward => transform.forward;
    public VehicleSettingsSO Settings => _vehicleSettings;

    private void Awake()
    {
        _springDatas = new Dictionary<WheelType, SpringData>();
        foreach (WheelType wheelType in _wheels)
        {
            _springDatas.Add(wheelType, new());
        }
    }

    public override void OnNetworkSpawn()
    {
        _vehicleRigidbody.isKinematic = true;
        SetOwnerRigidbodyKinematicAsync();
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        SetSteerInput(Input.GetAxis("Horizontal"));
        SetAccelerationInput(Input.GetAxis("Vertical"));
    }

    private void FixedUpdate()
    {
        if (!IsOwner) { return; }

        UpdateSuspension();
        UpdateSteering();
        UpdateAcceleration();
        UpdateBrakes();
        UpdateAirResistance();
    }

    private void SetSteerInput(float steerInput)
    {
        _steerInput = Mathf.Clamp(steerInput, -1f, 1f);
    }

    private void SetAccelerationInput(float accelerationInput)
    {
        _accelerationInput = Mathf.Clamp(accelerationInput, -1f, 1f);
    }

    private void UpdateSuspension()
    {
        foreach (WheelType id in _springDatas.Keys)
        {
            CastSpring(id);
            float currentVelocity = _springDatas[id]._currentVelocity;
            float currentLength = _springDatas[id]._currentLength;

            float force = SpringMathExtensions.CalculateForceDamped(
                currentLength,
                currentVelocity,
                _vehicleSettings.SpringRestLength,
                _vehicleSettings.SpringStrength,
                _vehicleSettings.SpringDamper
            );

            _vehicleRigidbody.AddForceAtPosition(force * transform.up, GetSpringPosition(id));
        }
    }

    private void UpdateSteering()
    {
        foreach (WheelType wheel in _wheels)
        {
            if (!IsGrounded(wheel))
            {
                continue;
            }
            Vector3 springPosition = GetSpringPosition(wheel);
            Vector3 slideDirection = GetWheelSlideDirection(wheel);
            float slideVelocity = Vector3.Dot(slideDirection, _vehicleRigidbody.GetPointVelocity(springPosition));
            float desiredVelocityChange = GetWheelGripFactor(wheel) * -slideVelocity;
            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;
            Vector3 force = desiredAcceleration * slideDirection * _vehicleSettings.TireMass;
            _vehicleRigidbody.AddForceAtPosition(force, GetWheelTorquePosition(wheel));
        }
    }

    private void UpdateAcceleration()
    {
        if (Mathf.Approximately(_accelerationInput, 0f))
        {
            return;
        }

        float forwardSpeed = Vector3.Dot(transform.forward, _vehicleRigidbody.linearVelocity);
        bool movingForward = forwardSpeed >= 0f;
        float speed = Mathf.Abs(forwardSpeed);

        if (movingForward && speed > _vehicleSettings.MaxSpeed)
        {
            return;
        }
        else if (!movingForward && speed > _vehicleSettings.MaxReverseSpeed)
        {
            return;
        }

        foreach (WheelType wheel in _wheels)
        {
            if (!IsGrounded(wheel))
            {
                continue;
            }
            Vector3 position = GetWheelTorquePosition(wheel);
            Vector3 wheelForward = GetWheelRollDirection(wheel);
            _vehicleRigidbody.AddForceAtPosition(_accelerationInput * wheelForward * _vehicleSettings.AcceleratePower, position);
        }
    }

    private void UpdateBrakes()
    {
        float forwardSpeed = Vector3.Dot(transform.forward, _vehicleRigidbody.linearVelocity);
        float speed = Mathf.Abs(forwardSpeed);
        float brakeRatio;

        const float ALMOST_STOPPING_SPEED = 2f;
        bool almostStopping = speed < ALMOST_STOPPING_SPEED;
        if (almostStopping)
        {
            brakeRatio = 1f;
        }
        else
        {
            bool accelerateContrary =
                !Mathf.Approximately(_accelerationInput, 0f) &&
                Vector3.Dot(_accelerationInput * transform.forward, _vehicleRigidbody.linearVelocity) < 0f;

            if (accelerateContrary)
            {
                brakeRatio = 1f;
            }
            else if (Mathf.Approximately(_accelerationInput, 0f))
            {
                brakeRatio = 0.1f;
            }
            else
            {
                return;
            }
        }

        foreach (WheelType wheel in _backWheels)
        {
            if (!IsGrounded(wheel))
            {
                continue;
            }
            Vector3 springPosition = GetSpringPosition(wheel);
            Vector3 rollDirection = GetWheelRollDirection(wheel);
            float rollVelocity = Vector3.Dot(rollDirection, _vehicleRigidbody.GetPointVelocity(springPosition));

            float desiredVelocityChange = -rollVelocity * brakeRatio * _vehicleSettings.BrakesPower;
            float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

            Vector3 force = desiredAcceleration * _vehicleSettings.TireMass * rollDirection;
            _vehicleRigidbody.AddForceAtPosition(force, GetWheelTorquePosition(wheel));
        }
    }

    private void UpdateAirResistance()
    {
        _vehicleRigidbody.AddForce(_vehicleCollider.size.magnitude * -_vehicleRigidbody.linearVelocity * _vehicleSettings.AirResistance);
    }

    private void CastSpring(WheelType wheelType)
    {
        Vector3 position = GetSpringPosition(wheelType);

        float previousLength = _springDatas[wheelType]._currentLength;
        float currentLength;

        if (Physics.Raycast(position, -transform.up, out var hit, _vehicleSettings.SpringRestLength))
        {
            currentLength = hit.distance;
        }
        else
        {
            currentLength = _vehicleSettings.SpringRestLength;
        }

        _springDatas[wheelType]._currentVelocity = (currentLength - previousLength) / Time.fixedDeltaTime;
        _springDatas[wheelType]._currentLength = currentLength;
    }

    private Vector3 GetSpringPosition(WheelType wheelType)
    {
        return transform.localToWorldMatrix.MultiplyPoint3x4(GetSpringRelativePosition(wheelType));
    }

    private Vector3 GetSpringRelativePosition(WheelType wheelType)
    {
        Vector3 boxSize = _vehicleCollider.size;
        float boxBottom = boxSize.y * -0.5f;

        float paddingX = _vehicleSettings.WheelsPaddingX;
        float paddingZ = _vehicleSettings.WheelsPaddingZ;

        return wheelType switch
        {
            WheelType.FrontLeft => new Vector3(boxSize.x * (paddingX - 0.5f), boxBottom, boxSize.z * (0.5f - paddingZ)),
            WheelType.FrontRight => new Vector3(boxSize.x * (0.5f - paddingX), boxBottom, boxSize.z * (0.5f - paddingZ)),
            WheelType.BackLeft => new Vector3(boxSize.x * (paddingX - 0.5f), boxBottom, boxSize.z * (paddingZ - 0.5f)),
            WheelType.BackRight => new Vector3(boxSize.x * (0.5f - paddingX), boxBottom, boxSize.z * (paddingZ - 0.5f)),
            _ => default(Vector3)
        };
    }

    private Vector3 GetWheelTorquePosition(WheelType wheelType)
    {
        return transform.localToWorldMatrix.MultiplyPoint3x4(GetWheelRelativeTorquePosition(wheelType));
    }

    private Vector3 GetWheelRelativeTorquePosition(WheelType wheelType)
    {
        Vector3 boxSize = _vehicleCollider.size;

        float paddingX = _vehicleSettings.WheelsPaddingX;
        float paddingZ = _vehicleSettings.WheelsPaddingZ;

        return wheelType switch
        {
            WheelType.FrontLeft => new Vector3(boxSize.x * (paddingX - 0.5f), 0f, boxSize.z * (0.5f - paddingZ)),
            WheelType.FrontRight => new Vector3(boxSize.x * (0.5f - paddingX), 0f, boxSize.z * (0.5f - paddingZ)),
            WheelType.BackLeft => new Vector3(boxSize.x * (paddingX - 0.5f), 0f, boxSize.z * (paddingZ - 0.5f)),
            WheelType.BackRight => new Vector3(boxSize.x * (0.5f - paddingX), 0f, boxSize.z * (paddingZ - 0.5f)),
            _ => default(Vector3)
        };
    }

    private Vector3 GetWheelSlideDirection(WheelType wheelType)
    {
        Vector3 forward = GetWheelRollDirection(wheelType);
        return Vector3.Cross(transform.up, forward);
    }

    private Vector3 GetWheelRollDirection(WheelType wheelType)
    {
        bool frontWheels = wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;
        if (frontWheels)
        {
            var steerQuaternion = Quaternion.AngleAxis(_steerInput * _vehicleSettings.SteerAngle, Vector3.up);
            return steerQuaternion * transform.forward;
        }
        else
        {
            return transform.forward;
        }
    }

    private float GetWheelGripFactor(WheelType wheelType)
    {
        bool frontWheels = wheelType == WheelType.FrontLeft || wheelType == WheelType.FrontRight;
        return frontWheels ? _vehicleSettings.FrontWheelGripFactor : _vehicleSettings.RearWheelGripFactor;
    }

    private bool IsGrounded(WheelType wheelType)
    {
        return _springDatas[wheelType]._currentLength < _vehicleSettings.SpringRestLength;
    }

    public float GetSpringCurrentLength(WheelType wheelType)
    {
        return _springDatas[wheelType]._currentLength;
    }

    private async void SetOwnerRigidbodyKinematicAsync()
    {
        if (IsOwner)
        {
            await UniTask.DelayFrame(1);
            _vehicleRigidbody.isKinematic = false;
        }
    }
}


public static class SpringMathExtensions
{
    public static float CalculateForceDamped(float currentLength, float LengthVelocity, float restLength, float strength, float damper)
    {
        float lengthOffset = restLength - currentLength;
        return (lengthOffset * strength) - (LengthVelocity * damper);
    }
}