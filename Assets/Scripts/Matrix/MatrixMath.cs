using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MatrixMath {
    public class Matrix {

        private double[,] matrix;
        private (int, int) _shape = (0, 0);
        public (int, int) shape {
            get => _shape;
        }

        public int rows {
            get => _shape.Item1;
        }

        public int columns {
            get => _shape.Item2;
        }

        public double this[int j, int i] {
            get => matrix[j, i];
            set => matrix[j, i] = value;
        }

        public Matrix T {
            get {
                return Transpose();
            }
        }

        public double[,] GetMatrix() {
            return matrix.Clone() as double[,];
        }

        public Matrix(int[,] matrix) {
            if (matrix.Length == 0) {
                this.matrix = new double[0, 0];
                return;
            }
            this._shape = (matrix.GetLength(0), matrix.GetLength(1));
            this.matrix = new double[rows, columns];

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) {
                    this.matrix[i, j] = matrix[i, j];
                }
            }
        }

        public Matrix(float[,] matrix) {
            if (matrix.Length == 0) {
                this.matrix = new double[0, 0];
                return;
            }
            this._shape = (matrix.GetLength(0), matrix.GetLength(1));
            this.matrix = new double[rows, columns];

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) {
                    this.matrix[i, j] = matrix[i, j];
                }
            }
        }

        public Matrix(double[,] matrix) {
            if (matrix.Length == 0) {
                this.matrix = new double[0, 0];
                return;
            }
            this._shape = (matrix.GetLength(0), matrix.GetLength(1));
            this.matrix = new double[rows, columns];

            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) {
                    this.matrix[i, j] = matrix[i, j];
                }
            }
        }


        public Matrix(Matrix matrix) {
            if (matrix.rows == 0) {
                this.matrix = new double[0, 0];
                return;
            }
            this._shape = (matrix.rows, matrix.columns);
            this.matrix = matrix.matrix.Clone() as double[,];
        }

        private Matrix(int rows, int columns) {
            this._shape = (rows, columns);
            this.matrix = new double[rows, columns];
        }

        public static Matrix Zeroes(int rows, int columns) {
            return new Matrix(rows, columns);
        }

        public static Matrix Rand(int rows, int columns, float min_val, float max_val) {
            var m = new Matrix(rows, columns);
            for (int i = 0; i < m.rows; i++) {
                for (int j = 0; j < m.columns; j++) {
                    m.matrix[i, j] = Random.value * (max_val - min_val) + min_val;
                }
            }
            return m;
        }

        public Matrix ApplyFunc(System.Func<double, double> f) {
            Matrix n = new Matrix(this);
            for (int i = 0; i < n.rows; i++) {
                for (int j = 0; j < n.columns; j++) {
                    n.matrix[i, j] = f(n.matrix[i, j]);
                }
            }
            return n;
        }

        public static Matrix operator +(Matrix m, double a) {
            Matrix n = new Matrix(m);
            for (int i = 0; i < m.rows; i++) {
                for (int j = 0; j < m.columns; j++) {
                    n.matrix[i, j] += a;
                }
            }
            return n;
        }

        public static Matrix operator -(Matrix m, double a) {
            Matrix n = new Matrix(m);
            for (int i = 0; i < m.rows; i++) {
                for (int j = 0; j < m.columns; j++) {
                    n.matrix[i, j] -= a;
                }
            }
            return n;
        }

        public static Matrix operator *(Matrix m, double a) {
            Matrix n = new Matrix(m);
            for (int i = 0; i < m.rows; i++) {
                for (int j = 0; j < m.columns; j++) {
                    n.matrix[i, j] *= a;
                }
            }
            return n;
        }

        public static Matrix operator /(Matrix m, double a) {
            Matrix n = new Matrix(m);
            for (int i = 0; i < m.rows; i++) {
                for (int j = 0; j < m.columns; j++) {
                    n.matrix[i, j] /= a;
                }
            }
            return n;
        }

        public static Matrix operator *(Matrix a, Matrix b) {
            Matrix n = new Matrix(a.rows, b.columns);
            for (int i = 0; i < n.rows; i++) {
                for (int j = 0; j < n.columns; j++) {

                    double sum = 0;
                    //print("n[" + i + "," + j + "] :=");
                    for (int k = 0; k < a.columns; k++) {
                        //print(a[i, k] + " * " + b[k, j]);
                        sum += a[i, k] * b[k, j];
                    }
                    n.matrix[i, j] = sum;
                    //n.matrix[i, j] *= a;
                }
            }
            return n;
        }

        public Matrix Transpose() {
            Matrix n = new Matrix(columns, rows);
            for (int i = 0; i < rows; i++) {
                for (int j = 0; j < columns; j++) {
                    n.matrix[j, i] = matrix[i, j];
                }
            }
            return n;
        }


        public override string ToString() {
            string me = "";
            for (int i = 0; i < rows; i++) {
                me += i > 0 ? "\n [" : " [";
                for (int j = 0; j < columns; j++) {
                    me += j > 0 ? ", " + matrix[i, j].ToString("0.##") : matrix[i, j].ToString("0.##");
                }
                me += "]";
            }
            return me;
        }
    }
}
