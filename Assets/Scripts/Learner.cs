using UnityEngine;
using System.Collections;

public abstract class Learner : MonoBehaviour {
	[Header("Network Data")]
	public LayerData[] hiddenLayerData;
	public bool isOutputLogistic;

	public bool learningMode;
	public float learningRate = 0.02f;
	public float momentum = 0.9f;
	public float samplingRate = 0.5f;
	[Range(0,1)]
	public float samplingProb = 0.45f;
	public float regularization = 0.1f;
	public int miniBatchSize = 10;
	public int logRate = 20;
	public bool readBrain;

	public string saveBrainName = "brain.dat";
	public string readBrainName = "GenBrain.dat";

	protected NeuralNetwork mBrain;
	protected float[] mBrainInput;
	protected float[] mBrainOutput;


	float samplingTimer =  0;
	int counter = 0;

	protected abstract int InputSize { get; }
	protected abstract int OutputSize { get; }
	protected abstract float[] Target { get; }

	protected virtual void Init() {
		mBrainInput = new float[InputSize];
		mBrainOutput = new float[OutputSize];

		if (!readBrain) {
			mBrain = new NeuralNetwork (InputSize, OutputSize, isOutputLogistic, hiddenLayerData);	
			mBrain.RandomInit (-0.01f, 0.01f);	
		} else {
			mBrain = NeuralNetwork.Load (System.IO.Path.Combine(Application.dataPath, readBrainName));
		}
	}

	protected abstract void FillInput ();
	public float[] debugTransValues;
	protected virtual void Learn(float deltaTime)  {
		samplingTimer += Time.deltaTime;
		if (samplingTimer > samplingRate) {	
			if (Random.Range (0f, 1f) < samplingProb) {	
				float[] target = Target;
				mBrain.Backpropagation (mBrainInput, target, miniBatchSize, learningRate, regularization, momentum);
				debugTransValues = mBrain.TransitionValues;
			}
			samplingTimer = 0;
		}

		if (Input.GetKeyDown (KeyCode.Keypad0)) {
			NeuralNetwork.Save (mBrain, System.IO.Path.Combine(Application.dataPath, saveBrainName));
		}
	}

	protected abstract void Run ();

	void Awake() {
		Init ();
	}

	void Update() {
		FillInput ();
		if (learningMode) {
			Learn (Time.deltaTime);
		} else {
			Run ();
		}
	}
}
