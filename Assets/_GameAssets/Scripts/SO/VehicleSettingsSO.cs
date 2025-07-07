using UnityEngine;

[CreateAssetMenu(fileName = "VehicleSettings", menuName = "ScriptableObjects/VehicleSettings")]
public class VehicleSettingsSO : ScriptableObject
{
    [Header("Vehicle Paddings")]
    [SerializeField] private float _wheelsPaddingX;
    [SerializeField] private float _wheelsPaddingZ;

    [Header("Suspension Settings")]
    [SerializeField] private float _springRestLength;
    [SerializeField] private float _springStrength;
    [SerializeField] private float _springDamper;

    [Header("Handling Settings")]
    [SerializeField] private float _steerAngle;
    [SerializeField] private float _frontWheelGripFactor;
    [SerializeField] private float _rearWheelGripFactor;

    [Header("Body Settings")]
    [SerializeField] private float _tireMass;

    [Header("Power Settings")]
    [SerializeField] private float _acceleratePower;
    [SerializeField] private float _maxSpeed;
    [SerializeField] private float _maxReverseSpeed;
    [SerializeField] private float _brakesPower;

    [Header("Air Resistance")]
    [SerializeField] private float _airResistance;


    public float WheelsPaddingX => _wheelsPaddingX;
    public float WheelsPaddingZ => _wheelsPaddingZ;
    public float SpringRestLength => _springRestLength;
    public float SpringStrength => _springStrength;
    public float SpringDamper => _springDamper;
    public float SteerAngle => _steerAngle;
    public float FrontWheelGripFactor => _frontWheelGripFactor;
    public float RearWheelGripFactor => _rearWheelGripFactor;
    public float TireMass => _tireMass;
    public float AcceleratePower => _acceleratePower;
    public float MaxSpeed => _maxSpeed;
    public float MaxReverseSpeed => _maxReverseSpeed;
    public float BrakesPower => _brakesPower;
    public float AirResistance => _airResistance;
}
