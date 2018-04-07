using UnityEngine;
using System.Collections;
using UnityEditor;

public class NeuroCar : Specimen {
	public LayerData[] hiddenLayerData;
	public bool isOutputLogistic;
	NeuralNetwork mBrain;
	float[] mBrainInput;
	float[] mSteeringInput;

	public ProbeGroup[] probeGroups;

	public Transform targetTransform;

	Rigidbody mRigidbody;
	UnityStandardAssets.Vehicles.Car.CarController mCar;

	static NeuralNetwork mInitBrain;
	public bool readInitBrain = true;
	public bool perturbateInitBrain = true;

	public override void Init ()
	{
		if (mBrain == null) {
			int probeCount = 0;
			foreach (ProbeGroup gr in probeGroups) {
				probeCount += gr.probeCount;
			}

			//target, pos, forward vel, angVel, rot, probes
			int inputSize = 9 + probeCount;
			int outputSize = 4;

			mBrainInput = new float[inputSize];
			mSteeringInput = new float[outputSize];

			if (readInitBrain) {
				if (mInitBrain == null) {
					mInitBrain = NeuralNetwork.Load (System.IO.Path.Combine(Application.dataPath, "GenBrain.dat"));
				} 

				mBrain = new NeuralNetwork (mInitBrain);
			} else {
				mBrain = new NeuralNetwork (inputSize, outputSize, isOutputLogistic, hiddenLayerData);
			}

			fitness = new float[1];

			targetTransform = FindObjectOfType<RandPositioner> ().transform;
		}

		if (chromosome == null || chromosome.Genotype == null || chromosome.Genotype.Length == 0) {		
			if (!readInitBrain) {	
				mBrain.RandomInit (-3, 3);
			} else if (perturbateInitBrain) {
				mBrain.Perturbate (-0.1f, 0.1f);
			}
			chromosome = new GeneticAlgorithm.Chromosome (mBrain.TransitionValues);
		} else {
			mBrain.TransitionValues = chromosome.Genotype; 
		}

		mRigidbody = GetComponent<Rigidbody> ();
		mCar = GetComponent<UnityStandardAssets.Vehicles.Car.CarController> ();

		base.Init ();
	}
		
	int measurements = 0;
	float approachSum = 0;
	float velSum = 0;

	protected override void Evaluate ()
	{
		measurements++;
		Vector3 dir = (targetTransform.position - transform.position).normalized;
		approachSum += Vector3.Dot (mRigidbody.velocity, dir);
		fitness [0] = approachSum/measurements;

		/*velSum += Vector3.Dot (mRigidbody.velocity, transform.forward);
		fitness [1] = velSum / measurements;*/
	}

	protected override void Update() {
		base.Update ();	
		if (mIsInit) {	
			Vector3[] vals = new Vector3[3] {
				transform.InverseTransformPoint(targetTransform.position),
				transform.InverseTransformDirection(mRigidbody.velocity),
				transform.InverseTransformDirection(mRigidbody.angularVelocity)
			};

			for (int i = 0; i < 3; i++) {
				mBrainInput [3 * i] = vals [i].x;
				mBrainInput [3 * i + 1] = vals [i].y;
				mBrainInput [3 * i + 2] = vals [i].z;
			}

			int index = 9;
			foreach (ProbeGroup gr in probeGroups) {
				for (int i = 0; i < gr.probeCount; i++) {
					mBrainInput [index] = gr.GetTriggerDist (i, transform);
					index++;
				}
			}

			mSteeringInput = mBrain.Process (mBrainInput);

			mCar.Move (2 * mSteeringInput [0] - 1, mSteeringInput [1], mSteeringInput [2], mSteeringInput [3]);
		}
	}

	public void SaveBrain() {
		NeuralNetwork.Save (mBrain, System.IO.Path.Combine(Application.dataPath, "GenBrain.dat"));
	}

	void OnDrawGizmosSelected() {	
		if (probeGroups == null) {	
			return;
		}
		foreach (ProbeGroup gr in probeGroups) {
			Gizmos.color = Color.blue;
			Vector3 center = gr.GetCenterPosition (transform);
			Gizmos.DrawSphere (center, 0.5f);
			for (int i = 0; i < gr.probeCount; i++) {
				float dist = gr.GetTriggerDist (i, transform);

				Gizmos.color = dist > gr.GetProbeRange(i) * 0.999f ? Color.green : Color.red;

				Vector3 start, end;
				gr.GetProbePositions (i, transform, out start, out end);

				Gizmos.DrawCube (start, Vector3.one * 0.1f);
				Gizmos.DrawLine (start, end);
				Handles.Label (start + (end - start).normalized * dist, dist.ToString ());
			}
		}
	}
		
}

