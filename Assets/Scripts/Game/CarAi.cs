using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

using MatrixMath;
using System.Linq;

[System.Serializable]
public enum ActivationFunctions {
    Relu,
    Tanh,
    Linear
}

[RequireComponent(typeof(Car))]
public class CarAi : MonoBehaviour {

    public Matrix[] weights;
    public int[] layers;
    public ActivationFunctions[] activations;
    public Color color = Color.white;

    [SerializeField] private Car car;
    [SerializeField] private RaceParticipant participant;

    [SerializeField] LayerMask layerMask = 8;

    [Range(0f, 1f)]
    public float rayRatio = 0.5f;

    [Range(0f, 1f)]
    public float ydump = 0.5f;
    public string saveFile = "test1";

    private Vector3[] rayDirections;

    public float rayDistance = 15f;
    public bool ShowRayCast = false;
    [SerializeField] private bool enableAI;
    public bool autoLoad = false;

    private bool skip = false;

    // Start is called before the first frame update
    void Start() {
        //GetRandom();
        //car = GetComponent<Car>();
        //car.SetColor(color);
        if (!skip) {
            if (autoLoad) {
                LoadFromFile();
                Save();
            } else {
                enableAI = false;
            }
        }
    }

    private void Update() {
        if (enableAI) {
            var matrix = GetInput();
            var output = Compute(matrix);

            car.SetPower((float)output[0, 0]);
            car.SetSteer((float)output[0, 1]);
        }
    }

    private void OnValidate() {
        if (layers.Length > 0) {
            this.rayDirections = MathDirections.GetSphereDirections(layers[0] - 2, ydump);
        }
    }

    public void StartRace() {
        enableAI = true;
        participant.StartLap();
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    public RaceParticipant GetParticipationObj() {
        return participant;
    }

    public void GetRandom() {
        this.color = new Color(Random.value, Random.value, Random.value);
        this.weights = new Matrix[layers.Length - 1];
        for (int i = 0; i < layers.Length - 1; i++) {
            this.weights[i] = Matrix.Rand(layers[i], layers[i + 1], -1, 1);
        }
        car.SetColor(color);
        this.rayDirections = MathDirections.GetSphereDirections(layers[0] - 2, ydump);
        skip = true;
    }

    Matrix Compute(Matrix input) {
        var current = (input * weights[0]).ApplyFunc(GetFunc(activations[0]));
        for (int i = 1; i < weights.Length; i++) {
            current *= weights[i];
            current = current.ApplyFunc(GetFunc(activations[i]));
        }
        return current;
    }

    Matrix GetInput() {
        Matrix input = Matrix.Zeroes(1, layers[0]);
        var index = layers[0] - 6;
        for (int i = 0; i < index; i++) {
            // Bit shift the index of the layer (8) to get a bit mask

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            var dir = transform.TransformDirection(Vector3.forward * rayRatio + rayDirections[i]);
            var pos = transform.position + Vector3.up - dir;
            if (Physics.Raycast(pos, dir, out hit, rayDistance, layerMask)) {
                if (ShowRayCast) {
                    Debug.DrawRay(pos, dir * hit.distance, Color.yellow);
                }
                input[0, i] = (rayDistance - hit.distance) / rayDistance;
            } else {
                if (ShowRayCast) {
                    Debug.DrawRay(pos, dir * rayDistance, Color.white);
                }
                input[0, i] = 0;
            }
        }
        var ag = car.GetAngularVelocity() / 2;
        var vel = car.GetVelocity() / 10;
        input[0, index++] = ag.x;
        input[0, index++] = ag.y;
        input[0, index++] = ag.z;
        input[0, index++] = vel.x;
        input[0, index++] = vel.y;
        input[0, index++] = vel.z;
        return input;
    }

    void LoadFromFile() {
        // idk read file and load

        if (File.Exists(Application.persistentDataPath
               + "/" + saveFile + ".dat")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath
                            + "/" + saveFile + ".dat", FileMode.Open);
            CarAIData data = (CarAIData)bf.Deserialize(file);
            file.Close();
            LoadFromData(data);
            Debug.Log("Game data loaded!");
        } else {
            Debug.Log("There is no save data!");
            GetRandom();
        }
    }

    public void LoadFromData(CarAIData data) {
        this.color = new Color(data.color[0], data.color[1], data.color[2]);
        this.layers = data.layers;
        this.activations = data.activations;
        this.rayRatio = data.rayRatio;
        this.ydump = data.ydump;
        this.weights = new Matrix[data.weights.Length];
        for (int i = 0; i < weights.Length; i++) {
            this.weights[i] = new Matrix(data.weights[i]);
        }
        car.SetColor(color);
        this.rayDirections = MathDirections.GetSphereDirections(layers[0] - 2, ydump);
        skip = true;
    }

    public void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        var path = Application.persistentDataPath
                     + "/" + saveFile + ".dat";
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        FileStream file = File.Create(path);
        CarAIData data = GetData();
        // data.savedInt = intToSave;
        // data.savedFloat = floatToSave;
        // data.savedBool = boolToSave;
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Model saved!");
    }

    public CarAIData GetData() {
        CarAIData data = new CarAIData();
        data.weights = new double[weights.Length][,];
        for (int i = 0; i < weights.Length; i++) {
            data.weights[i] = weights[i].GetMatrix();
        }
        data.layers = layers;
        data.color = new float[] { color.r, color.g, color.b };
        data.activations = activations;
        data.rayRatio = rayRatio;
        data.ydump = ydump;
        return data;
    }

    public static System.Func<double, double> GetFunc(ActivationFunctions fname) {
        if (fname == ActivationFunctions.Relu) {
            return (double a) => { return a > 0 ? a : 0; };
        } else if (fname == ActivationFunctions.Tanh) {
            return System.Math.Tanh;
        }
        return (double a) => { return a; };
    }
}

