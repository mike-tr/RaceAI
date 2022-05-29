using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour {
    // Start is called before the first frame update
    [SerializeField] int id = 0;
    public int ID { get { return id; } }
    public float scoreFactor = 1f;
    void OnValidate() {
        id = 0;
        foreach (var c in name) {
            if (char.IsDigit(c)) {
                id = id * 10 + (int)char.GetNumericValue(c);
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        //var car = GetComponent<
    }
}
