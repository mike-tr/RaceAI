using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using MatrixMath;

public class GeneticTrainer : MonoBehaviour {
    public GeneticTrainer instance;
    public int PopulationSize = 1000;
    public float MaxGenerationTime = 150f;
    public string TrainerName = "SomeName";
    public int GenerationNumber = 0;
    public float mutationRate = 0.02f;
    public float bestScore = 0;
    public float currentBestScore = 0;

    [Range(0.1f, 0.75f)]
    public float priorityValue = 0.5f;

    private List<CarAi> population = new List<CarAi>();

    public Transform[] SpawnPoints;

    public CarAi AiPrefab;
    public int carLayer;

    private Transform genHolder;
    private float CurrentLapTime;
    private float lapTimerTimestamp = -1;
    // Start is called before the first frame update
    void Start() {
        instance = this;
        Physics.IgnoreLayerCollision(carLayer, carLayer, true);
        genHolder = new GameObject().transform;
        genHolder.name = "Gen Population";
        genHolder.parent = transform;
        LoadFromFile();

        if (PopulationSize % 2 == 1) {
            PopulationSize++;
        }
    }

    void StartRun() {
        foreach (var car in population) {
            var pos = SpawnPoints[Random.Range(0, SpawnPoints.Length)].position;
            car.transform.position = pos;
            car.transform.rotation = Quaternion.identity;
            car.StartRace();
        }
        lapTimerTimestamp = Time.time;
    }

    void NewPopulation() {
        // Create brand new generation.
        GenerationNumber = 0;

        population = new List<CarAi>();
        for (int i = 0; i < PopulationSize; i++) {
            var car = Instantiate(AiPrefab, genHolder);
            car.GetRandom();
            population.Add(car);
            car.GetParticipationObj().OnScoreChange += TryUpdateScore;
        }

        StartRun();
    }

    void NextGen() {
        // Create the next generation.
        var nextGen = new List<CarAIData>();
        List<CarAi> SortedList = population.OrderBy(o => -o.GetParticipationObj().score).ToList();

        var best = SortedList[0];
        var bestScore = best.GetParticipationObj().score;

        if (GenerationNumber > 0) {
            Save();
        }
        if (this.bestScore <= bestScore) {
            this.bestScore = bestScore;
            best.SaveAs(TrainerName + "/best.dat");
            print("saved best so far...");
        }

        List<CarAIData> parents = new List<CarAIData>();
        foreach (var car in SortedList) {
            parents.Add(car.GetData());
        }
        for (int i = 0; i < PopulationSize; i += 2) {
            var parentA = GetParent(parents);
            var parentB = GetParent(parents);

            var childA = GetChild(parentA.Item1, parentB.Item1);
            var childB = GetChild(parentA.Item1, parentB.Item1);

            nextGen.Add(childA);
            nextGen.Add(childB);
            // print(parentA.Item2 + " : " + SortedList[parentA.Item2].GetParticipationObj().score);
        }

        for (int i = 0; i < PopulationSize; i++) {
            population[i].LoadFromData(nextGen[i]);
        }

        GenerationNumber++;
        currentBestScore = 0;
        RaceManager.instance.SetUIText(3, "Generation : " + GenerationNumber.ToString());
        RaceManager.instance.SetUIText(0, "Best Score : " + bestScore.ToString("0.00"));
        RaceManager.instance.SetUIText(1, "Best Generation Score : " + currentBestScore.ToString("0.00"));
        StartRun();
    }

    (CarAIData, int) GetParent(List<CarAIData> parents) {
        CarAIData current = parents[0];
        int index = 0;
        for (int i = 1; i < parents.Count; i++) {
            if (Random.value > priorityValue) {
                // if we got higher value then the pick threshold then try next child
                current = parents[i];
                index = i;
            } else {
                break;
            }
        }
        return (current, index);
    }

    bool LoadFromFile() {
        // idk read file and load

        var path = Application.persistentDataPath
                            + "/" + TrainerName + "/data.dat";
        if (File.Exists(path)) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath
                            + "/" + TrainerName + "/data.dat", FileMode.Open);
            GeneticTrainerData data = (GeneticTrainerData)bf.Deserialize(file);
            file.Close();
            LoadFromData(data);
            Debug.Log("Game data loaded!");
        } else {
            Debug.Log("There is no save data!");
            NewPopulation();
        }
        return false;
    }

    void LoadFromData(GeneticTrainerData data) {
        this.GenerationNumber = data.GenerationNumber;
        this.PopulationSize = data.PopulationSize;
        this.priorityValue = data.priorityValue;
        this.mutationRate = data.mutationRate;
        this.bestScore = data.bestScore;

        LoadGeneration(this.GenerationNumber);

        RaceManager.instance.SetUIText(2, "Generation : " + this.GenerationNumber.ToString());
        RaceManager.instance.SetUIText(0, "Best Score : " + bestScore.ToString("0.00"));
        RaceManager.instance.SetUIText(1, "Best Generation Score : " + currentBestScore.ToString("0.00"));

        print(this.GenerationNumber);
    }

    void LoadGeneration(int generationID) {
        population = new List<CarAi>();
        foreach (var car in population) {
            Destroy(car.gameObject);
        }

        population = new List<CarAi>();
        for (int i = 0; i < PopulationSize; i++) {
            var car = Instantiate(AiPrefab, genHolder);
            //car.GetRandom();
            car.LoadFrom(TrainerName + "/Gen " + generationID.ToString() + "/memeber[" + i.ToString() + "].dat");
            population.Add(car);
            car.GetParticipationObj().OnScoreChange += TryUpdateScore;
        }
        StartRun();
    }

    void Save() {
        var path = Application.persistentDataPath
                    + "/" + TrainerName + "/data.dat";
        BinaryFormatter bf = new BinaryFormatter();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        FileStream file = File.Create(path);
        GeneticTrainerData data = new GeneticTrainerData();
        data.bestScore = bestScore;
        data.GenerationNumber = GenerationNumber;
        data.mutationRate = mutationRate;
        data.PopulationSize = PopulationSize;
        data.priorityValue = priorityValue;
        // data.savedInt = intToSave;
        // data.savedFloat = floatToSave;
        // data.savedBool = boolToSave;
        bf.Serialize(file, data);
        file.Close();

        for (int i = 0; i < PopulationSize; i++) {
            population[i].SaveAs(TrainerName + "/Gen " + GenerationNumber.ToString() + "/memeber[" + i.ToString() + "].dat");
        }

        Debug.Log("Model saved!");
    }

    CarAIData GetChild(CarAIData parentA, CarAIData parentB) {
        CarAIData child = new CarAIData();
        child.weights = new double[parentA.weights.Length][,];
        child.color = new float[3];
        child.layers = parentA.layers;
        child.activations = parentA.activations;
        child.ydump = parentA.ydump;
        child.rayRatio = parentA.rayRatio;
        for (int i = 0; i < 3; i++) {
            var p = Random.value;
            child.color[i] = parentA.color[i] * p + parentB.color[i] * (1 - p);

            p = Random.value;
            if (p < mutationRate / 2) {
                child.color[i] = Random.value;
            } else if (p < mutationRate) {
                child.color[i] = Mathf.Clamp01(child.color[i] + (Random.value - 0.5f) * 0.5f);
            }
        }


        for (int layer = 0; layer < parentA.weights.Length; layer++) {
            child.weights[layer] = parentA.weights[layer].Clone() as double[,];
            for (int i = 0; i < parentA.weights[layer].GetLength(0); i++) {
                for (int j = 0; j < parentA.weights[layer].GetLength(1); j++) {
                    if (Random.value > 0.5) {
                        child.weights[layer][i, j] = parentB.weights[layer][i, j];
                    }

                    var p = Random.value;
                    if (p < mutationRate / 4) {
                        child.weights[layer][i, j] = Random.value * 2 - 1;
                        print("m1");
                    } else if (p < mutationRate * 2 / 4) {
                        child.weights[layer][i, j] += (Random.value - 0.5f) * 0.5f;
                        print("m2");
                    } else if (p < mutationRate * 3 / 4) {
                        child.weights[layer][i, j] *= -1;
                        print("m3");
                    } else if (p < mutationRate) {
                        child.weights[layer][i, j] = 0f;
                        print("m4");
                    }
                }
            }
        }
        return child;
    }

    // Update is called once per frame
    void Update() {
        CurrentLapTime = lapTimerTimestamp >= 0 ? Time.time - lapTimerTimestamp : 0;
        if (CurrentLapTime > MaxGenerationTime) {
            NextGen();
            lapTimerTimestamp = Time.time;
        }
        RaceManager.instance.SetTime(CurrentLapTime);
    }

    void TryUpdateScore(float score) {
        if (score > currentBestScore) {
            currentBestScore = score;
            RaceManager.instance.SetUIText(1, "Best Generation Score : " + currentBestScore.ToString("0.00"));
        }

        if (score > bestScore) {
            bestScore = score;
            RaceManager.instance.SetUIText(0, "Best Score : " + bestScore.ToString("0.00"));
        }
    }
}
