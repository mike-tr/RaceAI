using UnityEngine;
using System.Linq;
using System;

public class MathDirections {
    public static Vector3[] GetSphereDirections(int numDirections, float ydump) {
        var pts = new Vector3[numDirections];
        var inc = Math.PI * (3 - Math.Sqrt(5));
        var off = 2f / numDirections;

        foreach (var k in Enumerable.Range(0, numDirections)) {
            var y = k * off - 1 + (off / 2);
            y *= ydump;
            var r = Math.Sqrt(1 - y * y);
            var phi = k * inc;
            var x = (float)(Math.Cos(phi) * r);
            var z = (float)(Math.Sin(phi) * r);
            pts[k] = new Vector3(x, y, z);
        }

        return pts;
    }
}