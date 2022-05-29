using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public static CameraFollow instace;
    [SerializeField] Transform cam;
    public Transform followTarget;
    public Vector3 PosOffset;
    public float speed = 10f;
    public float rotationSpeed = 10f;
    private Vector3 lastPos;
    // Start is called before the first frame update
    void Awake() {
        instace = this;
        if (followTarget != null) {
            lastPos = followTarget.position;
        }
    }

    void OnValidate() {
        if (followTarget != null) {
            cam.localPosition = PosOffset;
            transform.position = followTarget.position;
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (followTarget != null) {
            transform.position = Vector3.Lerp(transform.position, followTarget.position, speed * Time.fixedDeltaTime);
            var rot = Quaternion.Euler(0, followTarget.eulerAngles.y, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
