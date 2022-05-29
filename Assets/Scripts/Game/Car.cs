using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

    public Transform centerOfMass;

    public float motorTorque = 100f;
    public float maxSteer = 20f;
    public Material material;


    private float _powerInput = 0f;
    private float _steerInput = 0f;

    private Rigidbody _rigidbody;
    private Wheel[] wheels;

    void Start() {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = centerOfMass.localPosition;

        wheels = GetComponentsInChildren<Wheel>();
    }

    public void SetSteer(float steer) {
        _steerInput = steer;
    }

    public void SetPower(float move) {
        _powerInput = move;
    }

    public Vector3 GetVelocity() {
        return _rigidbody.velocity;
    }

    public Vector3 GetAngularVelocity() {
        return _rigidbody.angularVelocity;
    }

    public void SetColor(Color color) {
        //material.CopyPropertiesFromMaterial(material);
        //material.SetColor("_BaseColor", color);
        GetComponent<Renderer>().materials[1].SetColor("_BaseColor", color);
    }

    void Update() {
        foreach (var wheel in wheels) {
            wheel.Steer = _steerInput * maxSteer;
            wheel.Power = _powerInput * motorTorque;
        }
    }
}
