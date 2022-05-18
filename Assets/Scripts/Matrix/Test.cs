using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MatrixMath;
using System.Diagnostics;

public class Test : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        // var matrix = new Matrix(new double[,] { { 1, 2, 3 }, { 4, 5, 6 } });
        // var matrix2 = new Matrix(new double[,] { { 1, 3 }, { 1, 3 }, { 1, 3 } });

        var x = Matrix.Rand(1, 5, -1, 1);
        var matrix = Matrix.Rand(5, 500, -1, 1);
        var matrix2 = Matrix.Rand(500, 4, -1, 1);

        Stopwatch sw = new Stopwatch();

        int times = 1000;
        sw.Start();
        for (int i = 0; i < times; i++) {
            var n = x * matrix * matrix2;
        }
        sw.Stop();
        print("time to run " + times + "multiplications is : " + sw.ElapsedMilliseconds + " ms");
        //var m2 = matrix + 1;
        print(matrix);
        print(matrix2);
        print(x * matrix * matrix2);
        // print(matrix[0, 0]);
        // print(matrix[1, 0]);

        print(x);
        print(x.T);
    }

    // Update is called once per frame
    void Update() {
        //Debug.Log("???2");
    }
}
