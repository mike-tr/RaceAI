using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using MatrixMath;

public class GeneticTrainer : MonoBehaviour {
    public GeneticTrainer instance;
    [Header("Statistics of Runs!")]
    public float skipAfterXIdle = 10f;
    public int GenerationNumber = 0;
    public float mutationRate = 0.02f;
    public float bestScore = 0;
    public float currentBestScore = 0;
    public int highestCheckPoint = 0;
    public float maximumTimePerGeneration = 180f;

    [Range(0.1f, 0.75f)]
    public float priorityValue = 0.5f;

    [Header("Settings for brand new runs!")]
    public int PopulationSize = 1000;
    public string TrainerName = "SomeName";
    public int rayDistance = 25;
    public int[] layers;
    public ActivationFunctions[] activations;

    [Header("Other settings")]
    public Transform[] SpawnPoints;
    public CarAi AiPrefab;
    public int carLayer;

    [Range(0, 10)]
    [Tooltip("How many of the best models per generation to save.")]
    public int SaveTheBest = 0;

    // Hidden
    private List<CarAi> population = new List<CarAi>();
    private Transform genHolder;
    private float CurrentLapTime;
    private float lapTimerTimestamp = -1;
    private float lastRecordTime = 0f;
    private Dictionary<int, (float, int)> GenScores;
    private Dictionary<int, GenerationStatistcs> statistics;

    [Header("-------- Debug ------")]
    public bool printStats;

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
        // Reset all cars and put them in the spawning points
        // As well as reset all the timers and scores.
        foreach (var car in population) {
            var pos = SpawnPoints[Random.Range(0, SpawnPoints.Length)].position;
            car.transform.position = pos;
            car.transform.rotation = Quaternion.identity;
            car.StartRace();
        }
        lapTimerTimestamp = Time.time;
        lastRecordTime = 0f;
        highestCheckPoint = 0;
        currentBestScore = 0;

        RaceManager.instance.SetUIText(0, "Best Score : " + this.bestScore.ToString("0.00"));
        RaceManager.instance.SetUIText(1, "Best Generation Score : " + currentBestScore.ToString("0.00"));
        RaceManager.instance.SetUIText(2, "Generation : " + GenerationNumber.ToString());
    }

    void NewPopulation() {
        // Create brand new generation.
        GenerationNumber = 0;

        population = new List<CarAi>();
        GenScores = null;
        statistics = new Dictionary<int, GenerationStatistcs>();
        for (int i = 0; i < PopulationSize; i++) {
            var car = Instantiate(AiPrefab, genHolder);
            car.activations = activations;
            car.layers = layers;
            car.rayDistance = rayDistance;
            car.GetRandom();
            population.Add(car);
            car.GetParticipationObj().OnScoreChange += TryUpdateScore;
        }

        StartRun();
    }

    void SaveGenerationStatistics(CarAi bestParticipant) {
        var bestGenScore = bestParticipant.GetParticipationObj().score;
        var bestLapTime = bestParticipant.GetParticipationObj().BestLapTime;
        if (bestLapTime == Mathf.Infinity) {
            bestLapTime = -1;
        }

        var stat = new GenerationStatistcs();
        stat.bestScore = bestGenScore;
        stat.checkpointsPassed = highestCheckPoint;
        stat.numberOfLapsCompleted = (int)(highestCheckPoint / RaceManager.instance.CheckPoints);
        stat.bestTimePerLap = bestLapTime;
        if (statistics.ContainsKey(GenerationNumber)) {
            statistics[GenerationNumber] = stat;
        } else {
            statistics.Add(GenerationNumber, stat);
        }

        if (this.bestScore <= bestGenScore) {
            this.bestScore = bestGenScore;
            bestParticipant.SaveAs(TrainerName + "/best.dat");
            bestParticipant.SaveAs(TrainerName + "/best" + GenerationNumber.ToString() + ".dat");
            print("saved best so far...");
        }

        if (GenerationNumber > 0) {
            Save();
        }
    }

    void NextGen() {
        // Save the current generation and create the next one.
        var nextGen = new List<CarAIData>();
        List<CarAi> SortedList = population.OrderBy(o => -o.GetParticipationObj().score).ToList();

        // Save the best n models of this generation ( can be 0 )
        for (int i = 0; i < SaveTheBest; i++) {
            SortedList[i].SaveAs(TrainerName + "/best_models/" + GenerationNumber + "/mb" + i.ToString() + ".dat");
        }
        SaveGenerationStatistics(SortedList[0]);


        // Create a list of Parents DNA in assending order from best parent to worst.
        List<CarAIData> parents = new List<CarAIData>();
        foreach (var car in SortedList) {
            parents.Add(car.GetData(false));
        }

        // Create new Child DNA.
        for (int i = 0; i < PopulationSize; i += 2) {
            var parentA = GetParent(parents);
            var parentB = GetParent(parents);

            var childA = GetChild(parentA.Item1, parentB.Item1);
            var childB = GetChild(parentA.Item1, parentB.Item1);

            nextGen.Add(childA);
            nextGen.Add(childB);
            // print(parentA.Item2 + " : " + SortedList[parentA.Item2].GetParticipationObj().score);
        }

        // Replace Car DNA with new one.
        for (int i = 0; i < PopulationSize; i++) {
            population[i].LoadFromData(nextGen[i]);
        }

        // This variable just controll how long each generation is at the start.
        skipAfterXIdle = Mathf.Clamp(CurrentLapTime * 0.55f, 10f, 120f);
        GenerationNumber++;
        // Start the next generation.
        StartRun();
    }

    (CarAIData, int) GetParent(List<CarAIData> parents) {
        // As it sounds by probability pick a parent, in this case i made it so parents who are ranked higher have much higher probability of being picked.
        // In fact the Probability of each parent is that of a Geometric distribution with is Geo~(p := priorityValue).
        // In the worst case the best ranking parent would be responsible for about 10% of all "childrens".
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
        // Return both the parent DNA and the rank of that parent ( the later is for Debug porposes to see how many times each parent was choosen ).
        return (current, index);
    }

    CarAIData GetChild(CarAIData parentA, CarAIData parentB) {
        // Generate new Child DNA

        // Copy some non-changable stats.
        CarAIData child = new CarAIData();
        child.weights = new double[parentA.weights.Length][,];
        child.color = new float[3];
        child.layers = parentA.layers;
        child.activations = parentA.activations;
        child.ydump = parentA.ydump;
        child.rayRatio = parentA.rayRatio;

        // Pick the new color of the child, with high mutation rate.
        // This color is basically only for visuallization so we can "kinda" know with parent was responsible for that child
        // The mutation rate is high so we would see new colors from time to time hopefully seeing new "linages", even tho they are closely related.
        for (int i = 0; i < 3; i++) {
            var p = Random.value;
            child.color[i] = parentA.color[i] * p + parentB.color[i] * (1 - p);

            // I make more mutations on colors so that cars would have more variety of colors.
            p = Random.value;
            if (p < 0.1f) {
                child.color[i] = Random.value;
            } else if (p < 0.2f) {
                child.color[i] = Mathf.Clamp01(child.color[i] + (Random.value - 0.5f) * 0.25f);
            }
        }

        // The "brain" of each car is a big neural network or not so big/
        // With probability 0.5 for each weight: Pick p~Normal(0,1)
        //          Then Set the new weight value as p * parentA and (1-p) * parentB.
        // Otherwise
        //          with probability 0.5, set the weight as ParentA, otherwise as ParentB.
        //
        // Mutations
        //      Here we have 4 possible mutations each having a probability fraction of the original mutation rate.
        //          1/4  of all mutations : override the weight to Normal(-1, 1)
        //          1/4  of all mutations : add to the weight Normal(-0.25, 0.25)
        //          1/4  of all mutations : Negate the value of the weight.
        //          1/4  of all mutations : Set the weight to 0.
        for (int layer = 0; layer < parentA.weights.Length; layer++) {
            child.weights[layer] = parentA.weights[layer].Clone() as double[,];
            for (int i = 0; i < parentA.weights[layer].GetLength(0); i++) {
                for (int j = 0; j < parentA.weights[layer].GetLength(1); j++) {
                    if (Random.value > 0.5) {
                        var pr = Random.value;
                        child.weights[layer][i, j] *= pr;
                        child.weights[layer][i, j] += (1 - pr) * parentB.weights[layer][i, j];

                    } else if (Random.value > 0.5) {
                        child.weights[layer][i, j] = parentB.weights[layer][i, j];
                    }

                    // Add mutation with probability mutationRate.
                    var p = Random.value;
                    if (p < mutationRate / 4) {
                        child.weights[layer][i, j] = Random.value * 2 - 1;
                    } else if (p < mutationRate * 2 / 4) {
                        child.weights[layer][i, j] += (Random.value - 0.5f) * 0.5f;
                    } else if (p < mutationRate * 3 / 4) {
                        child.weights[layer][i, j] *= -1;
                    } else if (p < mutationRate) {
                        child.weights[layer][i, j] = 0f;
                    }
                }
            }
        }
        return child;
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
        // Load Generation data etc...
        this.GenerationNumber = data.GenerationNumber;
        this.PopulationSize = data.PopulationSize;
        this.priorityValue = data.priorityValue;
        this.mutationRate = data.mutationRate;
        this.bestScore = data.bestScore;
        this.skipAfterXIdle = data.skipAfterXIdle;
        if (this.skipAfterXIdle < 10) {
            this.skipAfterXIdle = 10;
        }
        this.GenScores = data.scores;
        if (GenScores != null) {
            // Back compitability
            // Converst the old way that saved only float and int to the "better version".
            statistics = new Dictionary<int, GenerationStatistcs>();
            foreach (var key in GenScores.Keys) {
                var stats = new GenerationStatistcs();
                stats.bestScore = GenScores[key].Item1;
                stats.checkpointsPassed = GenScores[key].Item2;
                stats.numberOfLapsCompleted = (int)(GenScores[key].Item2 / RaceManager.instance.CheckPoints);
                statistics.Add(key, stats);
            }
            GenScores = null;
        } else {
            this.statistics = data.statistics;
            if (this.statistics == null) {
                this.statistics = new Dictionary<int, GenerationStatistcs>();
            }
        }
        LoadGeneration(this.GenerationNumber);

        RaceManager.instance.SetUIText(2, "Generation : " + this.GenerationNumber.ToString());
        RaceManager.instance.SetUIText(0, "Best Score : " + bestScore.ToString("0.00"));
        RaceManager.instance.SetUIText(1, "Best Generation Score : " + currentBestScore.ToString("0.00"));

        print(this.GenerationNumber);
    }

    void LoadGeneration(int generationID) {
        // Generate Cars, and Load their respective DNA.
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
            car.rayDistance = rayDistance;
        }
        StartRun();
    }

    void Save() {
        // Save everything.
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
        data.skipAfterXIdle = skipAfterXIdle;
        data.scores = null;
        data.statistics = statistics;
        // data.savedInt = intToSave;
        // data.savedFloat = floatToSave;
        // data.savedBool = boolToSave;
        bf.Serialize(file, data);
        file.Close();

        // Save each memeber of the popultaion.
        // This might have been wastefull but whatever.
        // This may let us the ability to take a look at Earlier Generations to see the progress.
        for (int i = 0; i < PopulationSize; i++) {
            population[i].SaveAs(TrainerName + "/Gen " + GenerationNumber.ToString() + "/memeber[" + i.ToString() + "].dat");
        }

        Debug.Log("Model saved!");
    }

    // Update is called once per frame
    void Update() {
        // Update the Time.
        CurrentLapTime = lapTimerTimestamp >= 0 ? Time.time - lapTimerTimestamp : 0;
        var timeFromRecord = CurrentLapTime - lastRecordTime;
        if (timeFromRecord > skipAfterXIdle || CurrentLapTime > maximumTimePerGeneration) {
            // Stop the generation and start next one.
            NextGen();
            lapTimerTimestamp = Time.time;
        }
        RaceManager.instance.SetUIText(3, "Time Passed : " + CurrentLapTime.ToString("0.00"));

        // Debug statistics.
        if (printStats) {
            PrintStatistics();
            printStats = false;
        }
    }

    void TryUpdateScore(float score, int id) {
        // When a Car has updated its respective score, we check if we might have a better participant.
        // This method is called from each CarAI when a new score has been achived.
        if (score > currentBestScore) {
            lastRecordTime = CurrentLapTime;
            currentBestScore = score;
            RaceManager.instance.SetUIText(1, "Best Generation Score : " + currentBestScore.ToString("0.00"));
        }

        if (score > bestScore) {
            bestScore = score;
            RaceManager.instance.SetUIText(0, "Best Score : " + bestScore.ToString("0.00"));
        }

        if (id > highestCheckPoint) {
            highestCheckPoint = id;
        }
    }

    void PrintStatistics() {
        string stats = "";
        foreach (var key in statistics.Keys.OrderByDescending(x => x)) {
            var stat = statistics[key];
            stats += "Generation : " + key.ToString() + " | Best score : " + stat.bestScore + " | Checkpoints passed : "
                    + stat.checkpointsPassed + " | Laps completed : " + stat.numberOfLapsCompleted;

            if (stat.numberOfLapsCompleted > 0) {
                if (stat.bestTimePerLap == 0) {
                    stats += " | Best Lap time : Missing \n";
                } else {
                    stats += " | Best Lap time : " + stat.bestTimePerLap + "\n";
                }
            } else {
                stats += "\n";
            }
        }
        print(stats);
    }
}
