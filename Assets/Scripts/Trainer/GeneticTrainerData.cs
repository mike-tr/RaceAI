using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GeneticTrainerData {
    public int GenerationNumber;
    public int PopulationSize;
    public float priorityValue;
    public float mutationRate;
    public float bestScore;
    public float skipAfterXIdle;

    public Dictionary<int, GenerationStatistcs> statistics;
    // Unused.
    public Dictionary<int, (float, int)> scores;
}

[Serializable]
public class GenerationStatistcs {
    public float bestScore;
    public int checkpointsPassed;
    public int numberOfLapsCompleted;
    public float bestTimePerLap = -1;
}