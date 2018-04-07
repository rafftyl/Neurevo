using UnityEngine;
using System.Collections;

public class ObstacleManager : MonoBehaviour {
	public Transform minTrans, maxTrans;

	public GameObject[] prefabs;
	public int obstacleCount = 100;

	void Awake() {
		Vector3 min = minTrans.position;
		Vector3 max = maxTrans.position;
		for (int i = 0; i < obstacleCount; i++) {
			GameObject obs = Instantiate<GameObject> (prefabs [Random.Range (0, prefabs.Length)]);
			obs.transform.SetParent (transform);
			float x = Random.Range (min.x, max.x);
			float y = Random.Range (min.y, max.y);
			float z = Random.Range (min.z, max.z);
			float rot = Random.Range (0, 360);

			obs.transform.position = new Vector3 (x, y, z);
			obs.transform.rotation = Quaternion.AngleAxis (rot, Vector3.up);
		}
	}
}
