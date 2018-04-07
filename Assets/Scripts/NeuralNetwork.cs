using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LinearAlgebra;

[System.Serializable]
public struct LayerData {
	public int size;
	public bool isLogistic;
}
public class NeuralNetwork {
	Vector mInput;
	Vector mOutput;
	public Vector Output {
		get {
			return mOutput;
		}
	}
	Matrix[] mTransitionMatrices;
	Vector[] mHiddenLayerOutputs;
	int mHiddenCount;
	int mOutputSize;

	LayerData[] mHiddenLayerData;
	bool mIsOutputLogistic;

	public NeuralNetwork(int inputSize, int outputSize, bool logisticOutput, params LayerData[] hiddenLayerData) {
		mInput = new Vector (inputSize + 1);
		mHiddenCount = hiddenLayerData.Length;
		mTransitionMatrices = new Matrix[mHiddenCount + 1];	
		mHiddenLayerData = hiddenLayerData;
		mHiddenLayerOutputs = new Vector[mHiddenCount];
		mIsOutputLogistic = logisticOutput;

		mOutputSize = outputSize;

		for (int i = 0; i < mHiddenCount + 1; ++i) {
			int prevLayerLength = (i == 0) ? inputSize + 1 : hiddenLayerData [i - 1].size + 1;
			int nextLayerLength = (i == mHiddenCount) ? outputSize : hiddenLayerData [i].size + 1;
			mTransitionMatrices [i] = new Matrix (nextLayerLength, prevLayerLength);
		}
	}

	public NeuralNetwork(NeuralNetwork other) {
		int inputSize = other.mInput.Size - 1;
		int outputSize = other.mOutputSize;
		mInput = new Vector (other.mInput.Size);
		mHiddenCount = other.mHiddenCount;
		mTransitionMatrices = new Matrix[mHiddenCount + 1];
		mHiddenLayerData = other.mHiddenLayerData;
		mHiddenLayerOutputs = new Vector[mHiddenCount];
		mIsOutputLogistic = other.mIsOutputLogistic;

		for (int i = 0; i < mHiddenCount + 1; ++i) {
			int prevLayerLength = (i == 0) ? inputSize + 1 : mHiddenLayerData [i - 1].size + 1;
			int nextLayerLength = (i == mHiddenCount) ? outputSize : mHiddenLayerData [i].size + 1;
			mTransitionMatrices [i] = new Matrix (nextLayerLength, prevLayerLength);

			for(int j = 0; j < nextLayerLength; ++j) {
				for (int k = 0; k < prevLayerLength; ++k) {
					mTransitionMatrices [i].Values [j, k] = other.mTransitionMatrices [i].Values [j, k];
				}
			}
		}

	}

	void Sigmoid(ref Vector vec) {		
		for (int i = 0; i < vec.Size; ++i) {
			float val = vec.Values [i];
			if (val >= 0) {
				float z = Mathf.Exp (-val);
				vec.Values [i] = 1 / (1 + z);
			} else {
				float z = Mathf.Exp (val);
				vec.Values [i] = z / (1 + z);
			}
		}
	}

	public float[] Process(float[] input) {
		if (input.Length != mInput.Size - 1) {
			throw new UnityException ("Input size not correct");
		}

		for (int i = 0; i < mInput.Size; ++i) {
			if (i < input.Length) {
				mInput.Values [i] = input [i];
			} else {
				mInput.Values[i] = 1;
			}
		}
			
		mOutput = mInput;
		for (int i = 0; i < mHiddenCount + 1; ++i) {
			mOutput = mTransitionMatrices [i] * mOutput;
			if (i != mHiddenCount) {
				if (mHiddenLayerData [i].isLogistic) {
					Sigmoid (ref mOutput);
				}
				mOutput.Values [mOutput.Size - 1] = 1;
				mHiddenLayerOutputs [i] = mOutput;
			} else if (mIsOutputLogistic) {
				Sigmoid (ref mOutput);
			}
		}

		return mOutput.Values;
	}

	public float[] TransitionValues {
		get {
			List<float> vals = new List<float> ();
			for (int i = 0; i < mHiddenCount + 1; ++i) {
				Matrix mat = mTransitionMatrices [i];
				for (int j = 0; j < mat.Rows; ++j) {
					for (int k = 0; k < mat.Cols; ++k) {
						vals.Add (mat.Values [j, k]);
					}
				}
			}
			return vals.ToArray();
		}

		set{
			int ind = 0;
			for (int i = 0; i < mHiddenCount + 1; ++i) {
				Matrix mat = mTransitionMatrices [i];
				ind = mat.SetValues (value, ind);
			}
		}
	}
		
	Matrix[] mSpeedMatrices;
	Matrix[] mGradients;
	int mSampleCounter = 0;
	float mErrorSum = 0;
	public void Backpropagation(float[] input, float[] targetValues, int miniBatchSize, float learningRate, float regularization, float momentum) {
		if (mSpeedMatrices == null) {
			mSpeedMatrices = new Matrix[mHiddenCount + 1];	
			mGradients = new Matrix[mHiddenCount + 1];
			for (int i = 0; i < mHiddenCount + 1; ++i) {				
				mSpeedMatrices [i] = new Matrix (mTransitionMatrices[i].Rows, mTransitionMatrices[i].Cols);
				mGradients[i] = new Matrix (mTransitionMatrices[i].Rows, mTransitionMatrices[i].Cols);
			}
		}

		if (mSampleCounter == 0) {
			foreach (Matrix grad in mGradients) {
				for (int i = 0; i < grad.Rows; i++) {
					for (int j = 0; j < grad.Cols; j++) {
						grad.Values [i, j] = 0;
					}
				}

			}
			mErrorSum = 0;
		}

		Process (input);

		Vector target = new Vector (targetValues.Length, targetValues);
		//Debug.Log ("Target: " + target);
		//Debug.Log ("Output: " + mOutput);
		Vector dif = mOutput - target;
		mErrorSum += dif.MagnitudeSqr;
		Vector currentGrad = null;
		for (int layerIndex = mHiddenCount; layerIndex > -1; layerIndex--) {
			Vector currentLayerOutput = layerIndex < mHiddenCount ? mHiddenLayerOutputs[layerIndex] :  mOutput;
			currentGrad = layerIndex < mHiddenCount ? mTransitionMatrices[layerIndex + 1].Transposed * currentGrad : dif;

			if ((layerIndex < mHiddenCount && mHiddenLayerData [layerIndex].isLogistic) ||
				(layerIndex == mHiddenCount && mIsOutputLogistic)) {
				currentGrad = currentLayerOutput * (1 - currentLayerOutput) * currentGrad;
			}

			//Debug.Log ("Grad: " + currentGrad);

			Vector previousLayerOutput = layerIndex > 0 ? mHiddenLayerOutputs [layerIndex - 1] : mInput;

			Matrix gradMat = mGradients [layerIndex];
			for (int i = 0; i < gradMat.Rows; i++) {
				for (int j = 0; j < gradMat.Cols; j++) {					
					gradMat.Values [i, j] = previousLayerOutput.Values [j] * currentGrad.Values [i] + regularization * mTransitionMatrices[layerIndex].Values[i,j];
				}
			}
		}

		mSampleCounter++;

		if (mSampleCounter == miniBatchSize) {
			for (int m = 0; m < mGradients.Length; m++) {
				Matrix grad = mGradients [m]/miniBatchSize;
				Matrix speed = mSpeedMatrices [m];
				speed *= momentum;
				speed -= learningRate * grad;

				mTransitionMatrices [m] += speed;
			}

			mSampleCounter = 0;

			Debug.Log ("Learning error: " + mErrorSum/miniBatchSize);
		}

	}

	public void RandomInit(float minVal, float maxVal) {
		for (int i = 0; i < mHiddenCount + 1; ++i) {
			Matrix mat = mTransitionMatrices [i];
			mat.SetRandomValues (minVal, maxVal);
		}
	}

	public void Perturbate(float minPerc, float maxPerc) {
		for (int i = 0; i < mHiddenCount + 1; ++i) {
			Matrix mat = mTransitionMatrices [i];
			mat.Perturbate (minPerc, maxPerc);
		}
	}

	public static void Save(NeuralNetwork net, string path) {
		using (BinaryWriter wr = new BinaryWriter (File.Open (path, FileMode.Create))) {
			wr.Write (net.mInput.Size - 1);
			wr.Write (net.mOutput.Size);
			wr.Write (net.mIsOutputLogistic);

			wr.Write (net.mHiddenCount);
			foreach (LayerData dat in net.mHiddenLayerData) {
				wr.Write (dat.size);
				wr.Write (dat.isLogistic);
			}

			float[] vals = net.TransitionValues;
			wr.Write (vals.Length);
			foreach (float val in vals) {
				wr.Write (val);
			}
		}
	}

	public static NeuralNetwork Load(string path) {
		NeuralNetwork net = null;
		using (BinaryReader reader = new BinaryReader (File.OpenRead (path))) {
			int inputSize = reader.ReadInt32 ();
			int outputSize = reader.ReadInt32 ();
			bool isLogistic = reader.ReadBoolean ();

			int hiddenUnitCount = reader.ReadInt32 ();
			LayerData[] layerData = new LayerData[hiddenUnitCount];
			for (int i = 0; i < hiddenUnitCount; ++i) {
				layerData [i] = new LayerData () {
					size = reader.ReadInt32(),
					isLogistic = reader.ReadBoolean()
				};
			}

			int valLength = reader.ReadInt32 ();
			float[] vals = new float[valLength];
			for (int i = 0; i < valLength; ++i) {
				vals [i] = reader.ReadSingle ();
			}

			net = new NeuralNetwork (inputSize, outputSize, isLogistic, layerData);
			net.TransitionValues = vals;
		}

		return net;
	}
}
