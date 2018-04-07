using UnityEngine;
using System.Collections;

namespace LinearAlgebra{
	public class Vector {
		public int Size{get; private set;}
		public float[] Values{ get; private set; }

		public Vector(int size) {
			Size = size;
			Values = new float[size];
		}

		public Vector(int size, float[] vals) {
			Size = size;
			Values = vals;
		}

		public static Vector operator+(Vector one,  Vector other) {
			if (one.Size != other.Size) {
				throw new UnityException ("Addition of vectors of different length is impossible");
			}
			Vector result = new Vector (one.Size);
			for (int i = 0; i < one.Size; ++i) {
				result.Values [i] = one.Values [i] + other.Values [i];
			}

			return result;
		}

		public static Vector operator-(Vector one,  Vector other) {
			if (one.Size != other.Size) {
				throw new UnityException ("Addition of vectors of different length is impossible");
			}
			Vector result = new Vector (one.Size);
			for (int i = 0; i < one.Size; ++i) {
				result.Values [i] = one.Values [i] - other.Values [i];
			}

			return result;
		}

		public static Vector operator-(float scalar,  Vector vec) {			
			Vector result = new Vector (vec.Size);
			for (int i = 0; i < vec.Size; ++i) {
				result.Values [i] = scalar - vec.Values [i];
			}

			return result;
		}

		public static Vector operator*(Vector one,  Vector other) {
			if (one.Size != other.Size) {
				throw new UnityException ("Addition of vectors of different length is impossible");
			}
			Vector result = new Vector (one.Size);
			for (int i = 0; i < one.Size; ++i) {
				result.Values [i] = one.Values [i] * other.Values [i];
			}

			return result;
		}

		public float MagnitudeSqr {
			get {
				float sum = 0;
				foreach (float val in Values) {
					sum += val * val;
				}

				return sum;
			}
		}

		public float Magnitude {
			get {
				return Mathf.Sqrt (MagnitudeSqr);
			}
		}

		public override string ToString ()
		{
			string str = "[ ";
			foreach (float val in Values) {
				str += val.ToString() + ", ";
			}
			str += " ]";
			return str;
		}
	}

	public class Matrix {
		public int Rows {get; private set;}
		public int Cols {get; private set;}
		public float[,] Values { get; private set; }
		public Matrix(int rows, int cols) {
			Rows = rows;
			Cols = cols;
			Values = new float[rows, cols];
		}
	
		public Matrix Transposed {
			get {
				var ret = new Matrix (Cols, Rows);
				for (int i = 0; i < Cols; i++) {
					for (int j = 0; j < Rows; j++) {
						ret.Values [i, j] = Values [j, i];
					}
				}

				return ret;
			}
		}

		public void SetRandomValues(float minVal, float maxVal) {
			for (int i = 0; i < Rows; ++i) {				
				for (int j = 0; j < Cols; ++j) {
					Values [i, j] = Random.Range (minVal, maxVal);
				}
			}
		}

		public void Perturbate(float minPerc, float maxPerc)  {
			for (int i = 0; i < Rows; ++i) {				
				for (int j = 0; j < Cols; ++j) {
					Values [i, j] *= (1 + Random.Range (minPerc, maxPerc));
				}
			}
		}

		//returns the index of the next unused data in valArray
		public int SetValues(float[] valArray, int startIndex) {
			int matIndex = (Rows - 1) * Cols + Cols + startIndex - 1;
			if (matIndex > valArray.Length - 1) {
				throw new UnityException ("Value array is to short, array length is " + valArray.Length + " but " + matIndex + " is needed");
			}
			for (int i = 0; i < Rows; ++i) {				
				for (int j = 0; j < Cols; ++j) {
					int index = i * Cols + j + startIndex;
					Values [i, j] = valArray [index];
				}
			}
			return matIndex + 1;
		}

		public static Vector operator*(Matrix mat, Vector vec) {
			if (mat.Cols != vec.Size) {
				throw new UnityException ("Matrix and vector sizes are not equal");
			}
			Vector result = new Vector (mat.Rows);
			for (int i = 0; i < mat.Rows; ++i) {
				result.Values [i] = 0;
				for (int j = 0; j < mat.Cols; ++j) {
					result.Values [i] += mat.Values [i, j] * vec.Values [j];
				}
			}
			return result;
		}

		public static Matrix operator*(Matrix mat, float scalar) {	
			Matrix result = new Matrix (mat.Rows, mat.Cols);
			for (int i = 0; i < mat.Rows; ++i) {				
				for (int j = 0; j < mat.Cols; ++j) {
					result.Values [i, j] = mat.Values [i, j] * scalar;
				}
			}
			return result;
		}

		public static Matrix operator*(float scalar, Matrix mat) {	
			return mat * scalar;
		}

		public static Matrix operator/(Matrix mat, float scalar) {	
			Matrix result = new Matrix (mat.Rows, mat.Cols);
			for (int i = 0; i < mat.Rows; ++i) {				
				for (int j = 0; j < mat.Cols; ++j) {
					result.Values [i, j] = mat.Values [i, j] / scalar;
				}
			}
			return result;
		}


		public static Matrix operator+(Matrix one, Matrix other) {
			if (one.Rows != other.Rows || one.Cols != other.Cols) {
				throw new UnityException ("Matrix and vector sizes are not equal");
			}
			Matrix result = new Matrix (one.Rows, one.Cols);
			for (int i = 0; i < one.Rows; ++i) {				
				for (int j = 0; j < one.Cols; ++j) {
					result.Values [i, j] = one.Values [i, j] + other.Values [i, j];
				}
			}
			return result;
		}

		public static Matrix operator-(Matrix one, Matrix other) {
			if (one.Rows != other.Rows || one.Cols != other.Cols) {
				throw new UnityException ("Matrix and vector sizes are not equal");
			}
			Matrix result = new Matrix (one.Rows, one.Cols);
			for (int i = 0; i < one.Rows; ++i) {				
				for (int j = 0; j < one.Cols; ++j) {
					result.Values [i, j] = one.Values [i, j] - other.Values [i, j];
				}
			}
			return result;
		}
	}
}
