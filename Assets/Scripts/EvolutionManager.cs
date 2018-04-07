using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EvolutionManager : MonoBehaviour {
	public Specimen specimenPrefab;
	public int populationSize;
	public int batchSize = 1;
	public int tournamentSize;
	[Range(0,1)]
	public float mutationProbability;
	public float maxMutation = 60;

	public float evaluationTime;

	public Transform spawnPoint;

	float mTimer = 0;
	List<Specimen> mSpecimenList = new List<Specimen>();
	int mCurrentSpecimenIndex = -1;

	GeneticAlgorithm mGenAlgorithm;
	Transform mSpecimenFolder;

	public int specimenInRow = 5;
	public float specimenSeparation = 8;
	void SpawnSpecimen(int count) {
		for (int i = mCurrentSpecimenIndex; i > -1; --i) {
			mSpecimenList [i].gameObject.SetActive (false);	
		}

		for (int i = 0; i < count; ++i) {
			Specimen currentSpecimen = Instantiate<Specimen> (specimenPrefab);
			currentSpecimen.transform.position = spawnPoint.position + (i % specimenInRow) * specimenSeparation * Vector3.right - (i/specimenInRow) * specimenSeparation * Vector3.forward;
			currentSpecimen.transform.rotation = spawnPoint.rotation;
			currentSpecimen.transform.SetParent (mSpecimenFolder);
			currentSpecimen.Init ();
			mSpecimenList.Add (currentSpecimen);
			mCurrentSpecimenIndex++;
		}
	}

	void SpawnSpecimen(GeneticAlgorithm.Chromosome[] genPool, int startIndex, int count) {
		for (int i = mCurrentSpecimenIndex; i > -1; --i) {
			mSpecimenList [i].gameObject.SetActive (false);	
		}

		for (int i = 0; i < count; ++i) {
			Specimen currentSpecimen = Instantiate<Specimen> (specimenPrefab);
			currentSpecimen.transform.position = spawnPoint.position + (i % specimenInRow) * specimenSeparation * Vector3.right - (i/specimenInRow) * specimenSeparation * Vector3.forward;
			currentSpecimen.transform.rotation = spawnPoint.rotation;
			currentSpecimen.transform.SetParent (mSpecimenFolder);
			currentSpecimen.chromosome = newGenPool [startIndex + i];
			currentSpecimen.Init ();
			mSpecimenList.Add (currentSpecimen);
			mCurrentSpecimenIndex++;
		}
	}

	GeneticAlgorithm.Chromosome[] newGenPool;
	void BreedAndClear() {
		newGenPool = mGenAlgorithm.GetNewPopulationGeneticPool (mSpecimenList);
		for (int i = 0; i < newGenPool.Length; ++i) {
			Destroy (mSpecimenList [i].gameObject);
		}
		mSpecimenList.Clear ();
		mCurrentSpecimenIndex = -1;
		mGenerationCount++;
	}

	void Awake() {
		mGenAlgorithm = new GeneticAlgorithm (tournamentSize, mutationProbability, maxMutation);
		GameObject go = new GameObject ("Specimen");
		go.transform.SetParent (transform);
		mSpecimenFolder = go.transform;
		SpawnSpecimen (batchSize);
	}

	int mGenerationCount = 1;
	void Update() {	

		mTimer += Time.deltaTime;
		if (mTimer > evaluationTime) {			
			if (mCurrentSpecimenIndex < populationSize - 1) {	
				int spawnSize = Mathf.Min (batchSize, populationSize - mCurrentSpecimenIndex - 1);
				if (newGenPool == null) {
					SpawnSpecimen (spawnSize);
				} else {
					SpawnSpecimen (newGenPool, mCurrentSpecimenIndex, spawnSize);
				}
			} else {				
				BreedAndClear ();
				SpawnSpecimen (newGenPool, 0, Mathf.Min (batchSize, populationSize));
			}
			mTimer = 0;
		}
	}
}
