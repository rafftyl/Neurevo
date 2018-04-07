using UnityEngine;
using System.Collections;

public class RandPositioner : MonoBehaviour {
	public Transform reference;
	
	public float reposTime = 8f;
	public float maxDif = 60;
	float timer;
	void Update () {
		timer += Time.deltaTime;
		if (timer > reposTime) {
			timer = 0;

			transform.position = reference.position + (new Vector3 (Random.Range (-maxDif, maxDif), 0, Random.Range (-maxDif, maxDif)));

		}
	}
}
