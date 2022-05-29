using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceParticipant : MonoBehaviour {
    public int nextCheckPoint = 1;
    public float score = 0;

    public float CurrentLapTime;
    public float BestLapTime = Mathf.Infinity;
    public float LastLapTime = Mathf.Infinity;
    public float lapTimerTimestamp = -1;
    public int LapCount = 0;

    public delegate void scoreChange(float score);
    public scoreChange OnScoreChange;

    [SerializeField] bool setUI = false;

    private float lastCheckPointTime = 0;
    // Start is called before the first frame update
    void Start() {
        LapCount = -1;
        //StartLap();
    }

    public void StartLap() {
        LapCount++;
        score = 0;
        nextCheckPoint = 1;
        BestLapTime = Mathf.Infinity;
        LastLapTime = Mathf.Infinity;
        lastCheckPointTime = 0;
        lapTimerTimestamp = Time.time;
    }

    void EndLap() {
        if (CurrentLapTime < BestLapTime) {
            BestLapTime = CurrentLapTime;
            if (setUI) {
                RaceManager.instance.SetBestTime(CurrentLapTime);
            }
        }
        LastLapTime = CurrentLapTime;
        StartLap();
        if (setUI) {
            RaceManager.instance.SetLastLapTime(CurrentLapTime);
        }
    }

    // Update is called once per frame
    void Update() {
        CurrentLapTime = lapTimerTimestamp >= 0 ? Time.time - lapTimerTimestamp : 0;
        if (setUI) {
            RaceManager.instance.SetTime(CurrentLapTime);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out CheckPoint checkPoint)) {
            if (checkPoint.ID == nextCheckPoint) {
                var values = RaceManager.instance.GetNextCheckPoint(nextCheckPoint);
                score += (values.Item2 * checkPoint.scoreFactor) / ((CurrentLapTime - lastCheckPointTime) + 10f);
                nextCheckPoint = values.Item1;
                lastCheckPointTime = CurrentLapTime;

                if (OnScoreChange != null) {
                    OnScoreChange.Invoke(score);
                }

                if (setUI) {
                    RaceManager.instance.SetScore(score);
                }
                if (values.Item3) {
                    EndLap();
                }
            }
        }
    }
}
