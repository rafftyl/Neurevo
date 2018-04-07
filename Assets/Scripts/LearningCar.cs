using UnityEngine;
using UnityEditor;
using System.Collections;

public class LearningCar : Learner {	
	[Header("Car Data")]
	public ProbeGroup[] probeGroups;
	public Transform targetTransform;
	//public float[] debugNetValues;
	Rigidbody mRigidbody;
	UnityStandardAssets.Vehicles.Car.CarController mCar;

	protected override int InputSize {
		get {
			int probeCount = 0;
			foreach (ProbeGroup gr in probeGroups) {
				probeCount += gr.probeCount;
			}
	
			return 9 + probeCount;
		}
	}

	protected override int OutputSize {
		get {
			return 4;
		}
	}

	protected override float[] Target {
		get {
			return new float[4] {
				(mCar.SteeringInput + 1) * 0.5f,
				mCar.AccelInput,
				mCar.BrakeInput,
				mCar.HandbrakeInput
			};
		}
	}

	protected override void Init ()
	{
		base.Init ();

		mRigidbody = GetComponent<Rigidbody> ();
		mCar = GetComponent<UnityStandardAssets.Vehicles.Car.CarController> ();
	}

	protected override void FillInput ()
	{
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
	}

	protected override void Run ()
	{
		mBrainOutput = mBrain.Process (mBrainInput);
		mCar.Move (2 * mBrainOutput [0] - 1, mBrainOutput [1], mBrainOutput [2], mBrainOutput [3]);
	}

	protected override void Learn (float deltaTime)
	{
		base.Learn (deltaTime);
		//debugNetValues = mBrain.TransitionValues;
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
