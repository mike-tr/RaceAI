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
}