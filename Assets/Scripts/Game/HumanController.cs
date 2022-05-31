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
        var participant = GetComponent<RaceParticipant>();
        CameraFollow.instace.followTarget = transform;
        car.SetColor(color);

        // in essense we want this to be managed by some other entity.
        participant.ResetParticipant();
    }

    // Update is called once per frame
    void Update() {
        car.SetPower(Input.GetAxis("Vertical"));
        car.SetSteer(Input.GetAxis("Horizontal"));
    }
}
