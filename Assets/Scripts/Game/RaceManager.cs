using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaceManager : MonoBehaviour {
    public static RaceManager instance;
    [SerializeField] int _checkPoints = 0;
    public int CheckPoints { get { return _checkPoints; } }

    [SerializeField] TextMeshProUGUI Score;
    [SerializeField] TextMeshProUGUI Time;
    [SerializeField] TextMeshProUGUI BestTime;
    [SerializeField] TextMeshProUGUI LastTime;
    // Start is called before the first frame update
    void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(this);
        }
    }

    public (int, float, bool) GetNextCheckPoint(int current) {
        if (current == _checkPoints) {
            return (1, _checkPoints * 25, true);
        }
        return (current + 1, current * 10, false);
    }

    public void SetScore(float score) {
        print(score);
        Score.text = "Score : " + score.ToString("0.00");
    }

    public void SetBestTime(float time) {
        BestTime.text = "Best lap time : " + time.ToString("0.00");
    }

    public void SetLastLapTime(float time) {
        LastTime.text = "Last lap time : " + time.ToString("0.00");
    }

    public void SetTime(float time) {
        //print(time);
        Time.text = "Current time : " + time.ToString("0.00");
    }

    // Update is called once per frame
    void Update() {

    }
}
