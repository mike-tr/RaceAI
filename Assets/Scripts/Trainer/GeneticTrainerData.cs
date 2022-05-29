using System;
using UnityEngine;

[Serializable]
public class GeneticTrainerData {
    public int GenerationNumber;
    public int PopulationSize;
    public float priorityValue;
    public float mutationRate;
    public float bestScore;
}