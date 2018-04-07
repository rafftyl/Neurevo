using System;
using UnityEngine;

[System.Serializable]
public class ProbeGroup {
	public Vector3 center = Vector3.zero;
	public int probeCount = 10;
	[Range(0,1)]
	public float sinWeight = 0.5f;
	[Range(0,1)]
	public float cosWeight = 0.5f;
	public float maxRange = 10;
	public float minRange = 5;

	public float minOffset = 0;
	public float maxOffset = 2;

	public LayerMask collisionMask;

	public float GetTriggerDist(int probeIndex, Transform parentTransform) {
		if (probeIndex < 0 || probeIndex > probeCount - 1) {
			return 0;
		}

		Vector3 start;
		Vector3 end;

		GetProbePositions (probeIndex, parentTransform, out start, out end);

		Vector3 dif = end - start;
		RaycastHit hit;
		if (Physics.Raycast (start, dif, out hit, dif.magnitude, collisionMask, QueryTriggerInteraction.Ignore)) {
			if (hit.collider.transform.root != parentTransform) {					
				return hit.distance;
			}

			return dif.magnitude;
		}

		return dif.magnitude;
	}

	public void GetProbePositions(int probeIndex, Transform parentTransform, out Vector3 start, out Vector3 end) {
		if (probeIndex < 0 || probeIndex > probeCount - 1) {
			start = Vector3.zero;
			end = Vector3.zero;
		}

		float deltaAngle = 2 * Mathf.PI / probeCount;
		float angle = probeIndex * deltaAngle;

		float sin = Mathf.Sin (angle);
		float cos = Mathf.Cos (angle);

		float weight = (sinWeight * sin * sin + cosWeight * cos * cos) / (sinWeight + cosWeight);
		float range = minRange + (maxRange - minRange) * weight;
		float offset = minOffset + (maxOffset - minOffset) * weight;

		Vector3 dir = Vector3.forward * cos + Vector3.right * sin;
		Vector3 localPos = dir * offset + center;

		start =  parentTransform.TransformPoint (localPos);	
		end = parentTransform.TransformPoint (localPos + dir * range);
	}

	public float GetProbeRange(int probeIndex) {
		float deltaAngle = 2 * Mathf.PI / probeCount;
		float angle = probeIndex * deltaAngle;

		float sin = Mathf.Sin (angle);
		float cos = Mathf.Cos (angle);

		return minRange + (maxRange - minRange) * (sinWeight * sin * sin + cosWeight * cos * cos) / (sinWeight + cosWeight);
	}

	public Vector3 GetCenterPosition(Transform parentTransform) {
		return parentTransform.TransformPoint(center);
	}
}

