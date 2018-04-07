using UnityEngine;
using System.Collections;

public abstract class Specimen : MonoBehaviour {
	public GeneticAlgorithm.Chromosome chromosome;
	public float[] fitness;
	protected bool mIsInit = false;
	public virtual void Init () {
		mIsInit = true;
	}
	protected abstract void Evaluate();

	protected virtual void Update() {
		if (mIsInit) {
			Evaluate ();
		}
	}
}
