using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Car))]
public class HumanController : MonoBehaviour {
    private Car car;
    public Color color = Color.red;
    // Start is called before the first frame update
    void Start() {
        car = GetComponent<Car>();
        CameraFollow.instace.followTarget = transform;
        car.SetColor(color);
    }

    // Update is called once per frame
    void Update() {
        car.SetPower(Input.GetAxis("Vertical"));
        car.SetSteer(Input.GetAxis("Horizontal"));
    }
}
