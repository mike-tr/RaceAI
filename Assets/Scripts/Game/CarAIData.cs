using System;
using UnityEngine;

[Serializable]
public class CarAIData {
    public double[][,] weights;
    public int[] layers;
    public float[] color;
    public ActivationFunctions[] activations;
    public float ydump;
    public float rayRatio;
    public float rayDistance;
    public CarAITrainingStatistics trainingStatistics;
}

[Serializable]
public class CarAITrainingStatistics {
    public float bestLapTime;
    public int lapsCompleted;
    public float score;
}