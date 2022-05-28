using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour {
    [SerializeField] WheelCollider wheelCollider;
    [SerializeField] Transform wheelTransform;
    [SerializeField] bool steer;
    [SerializeField] bool inverseSteer;
    [SerializeField] bool power;

    private Quaternion originalRotation;

    public float Power { get; set; }
    public float Steer { get; set; }

    void Start() {
        originalRotation = wheelTransform.localRotation;
    }

    void Update() {
        wheelCollider.GetWorldPose(out var pos, out var rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot * originalRotation;
    }

    void FixedUpdate() {
        if (power) {
            wheelCollider.motorTorque = Power;
        }

        if (steer) {
            wheelCollider.steerAngle = Steer * (inverseSteer ? -1 : 1);
        }
    }
}
