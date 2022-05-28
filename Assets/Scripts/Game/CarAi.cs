using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

using MatrixMath;

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

    private Car car;

    public string saveFile = "test1";
    // Start is called before the first frame update
    void Start() {
        //GetRandom();
        car = GetComponent<Car>();
        //car.SetColor(color);
        LoadFromFile();

        var fakeInput = Matrix.Rand(1, layers[0], 0, 20);
        print(fakeInput);
        print(Compute(fakeInput));

        Save();
    }

    void GetRandom() {
        this.color = new Color(Random.value, Random.value, Random.value);
        this.weights = new Matrix[layers.Length - 1];
        for (int i = 0; i < layers.Length - 1; i++) {
            this.weights[i] = Matrix.Rand(layers[i], layers[i + 1], -1, 1);
        }
        car.SetColor(color);
    }

    Matrix Compute(Matrix input) {
        var current = (input * weights[0]).ApplyFunc(GetFunc(activations[0]));
        for (int i = 1; i < weights.Length; i++) {
            current *= weights[i];
            current = current.ApplyFunc(GetFunc(activations[i]));
        }
        return current;
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
            Debug.LogError("There is no save data!");
            GetRandom();
        }
    }

    void LoadFromData(CarAIData data) {
        this.color = new Color(data.color[0], data.color[1], data.color[2]);
        this.layers = data.layers;
        this.activations = data.activations;
        this.weights = new Matrix[data.weights.Length];
        for (int i = 0; i < weights.Length; i++) {
            this.weights[i] = new Matrix(data.weights[i]);
        }
        car.SetColor(color);
    }

    void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath
                     + "/" + saveFile + ".dat");
        CarAIData data = new CarAIData();
        data.weights = new double[weights.Length][,];
        for (int i = 0; i < weights.Length; i++) {
            data.weights[i] = weights[i].GetMatrix();
        }
        data.layers = layers;
        data.color = new float[] { color.r, color.g, color.b };
        data.activations = activations;
        // data.savedInt = intToSave;
        // data.savedFloat = floatToSave;
        // data.savedBool = boolToSave;
        bf.Serialize(file, data);
        file.Close();
        Debug.Log("Model saved!");
    }

    public static System.Func<double, double> GetFunc(ActivationFunctions fname) {
        if (fname == ActivationFunctions.Relu) {
            return (double a) => { return a > 0 ? a : 0; };
        } else if (fname == ActivationFunctions.Tanh) {
            return System.Math.Tanh;
        }
        print("??");
        return (double a) => { return a; };
    }
}

