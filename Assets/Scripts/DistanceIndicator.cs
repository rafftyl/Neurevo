using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DistanceIndicator : MonoBehaviour {
	public Transform target;
	public Text text;

	Transform camTrans;

	// Use this for initialization
	void Start () {
		if (Camera.main != null) {
			camTrans = Camera.main.transform;
		}
	}

	Vector2 screenSize = new Vector2 (1800, 900);
	// Update is called once per frame
	void Update () {
		if (camTrans != null) {
			Vector3 relativePos = camTrans.InverseTransformPoint (target.position); 
			relativePos.y = 0;
			text.text = relativePos.magnitude.ToString ();
			relativePos.Normalize ();

			float upInt = screenSize.y * 0.5f / relativePos.z;
			float rightInt = screenSize.x * 0.5f / relativePos.x;

			if (upInt * relativePos.x > -screenSize.x * 0.5f && upInt * relativePos.x < screenSize.x * 0.5f) {
				GetComponent<RectTransform> ().anchoredPosition = (relativePos.z > 0 ? upInt : -upInt) * relativePos.x * Vector2.right + (relativePos.z > 0 ? screenSize.y : -screenSize.y) * Vector2.up * 0.5f; 
			} else {
				GetComponent<RectTransform> ().anchoredPosition = (relativePos.x > 0 ? rightInt : -rightInt) * relativePos.z * Vector2.up + (relativePos.x > 0 ? screenSize.x : -screenSize.x) * Vector2.right * 0.5f; 
			}
		}
	}
}
