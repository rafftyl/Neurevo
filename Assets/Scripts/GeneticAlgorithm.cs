using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneticAlgorithm {
	int mTournamentSize;
	float mMutationProbability;
	float mMaxMutation;
	public GeneticAlgorithm(int tournamentSize, float mutationProb, float maxMutation) {
		mTournamentSize = tournamentSize;
		mMutationProbability = mutationProb;
		mMaxMutation = maxMutation;
	}
	[System.Serializable]
	public class Chromosome {
		public string specimenId;
		[SerializeField]
		float[] mGenotype;
		public float[] Genotype { get { return mGenotype; } }
		public Chromosome(float[] genotype) {
			mGenotype = genotype;
		}

		public void Mutate(float maxMut) {		
			mGenotype[Random.Range(0, mGenotype.Length)] += Random.Range (-maxMut, maxMut);
		}

		public Chromosome Breed(Chromosome other) {	
			if (mGenotype.Length != other.mGenotype.Length) {
				throw new UnityException ("Genotypes of different lengths");
			}
			float[] childGenotype = new float[mGenotype.Length];
			for (int i = 0; i < mGenotype.Length; ++i) {
				//float blendParam = Random.Range (0f, 1f);
				//childGenotype [i] = (1 - blendParam) * mGenotype [i] + blendParam * other.mGenotype [i];
				childGenotype [i] = Random.Range (0, 2) == 0 ? mGenotype [i] : other.mGenotype [i];
			}
			return new Chromosome (childGenotype);
		}
			
	}

	int[] mDominationRanks = null;
	Chromosome[] mChromosomes = null;
	public Chromosome[] GetNewPopulationGeneticPool(List<Specimen> specimenList) {		
		if (mChromosomes == null) {	
			int populationSize = specimenList.Count;
			mChromosomes = new Chromosome[populationSize];
			mDominationRanks = new int[populationSize];
		}

		for (int i = 0; i < mDominationRanks.Length; ++i) {
			mDominationRanks [i] = 0;
		}

		for (int i = 0; i < specimenList.Count - 1; ++i) {
			for (int j = i + i; j < specimenList.Count; ++j) {
				int fitnessLength = specimenList [0].fitness.Length;
				Specimen s_1 = specimenList [i];
				Specimen s_2 = specimenList [j];
				bool dominated_1 = true;
				bool dominated_2 = true;
				for (int k = 0; k < fitnessLength; ++k) {
					if (s_1.fitness [k] > s_2.fitness [k]) {
						dominated_1 = false;
					}

					if (s_2.fitness [k] > s_1.fitness [k]) {
						dominated_2 = false;
					}
				}
				if (dominated_1) {
					mDominationRanks [i]++;
				}

				if (dominated_2) {
					mDominationRanks [j]++;
				}
			}
		}

		List<Specimen> parents = new List<Specimen> ();
		int globalMinDomination = int.MaxValue;
		Specimen bestBreeder = null;
		for (int i = 0; i < mChromosomes.Length; ++i) {
			int minDomination = int.MaxValue;
			Specimen chosenSpc = null;
			for (int j = 0; j < mTournamentSize; ++j) {
				int ind = Random.Range (0, specimenList.Count);
				if (mDominationRanks[ind] < minDomination) {
					minDomination = mDominationRanks [ind];
					chosenSpc = specimenList[ind];
				}
			}

			if (minDomination < globalMinDomination) {
				globalMinDomination = minDomination;
				bestBreeder = chosenSpc;
			}
			parents.Add (chosenSpc);
		}

		if (bestBreeder is NeuroCar) {
			(bestBreeder as NeuroCar).SaveBrain ();
		}

		string message = "Best breeder domination rank: " + globalMinDomination.ToString () + "\n" + "Fitness: ";
		for(int i = 0; i < bestBreeder.fitness.Length; ++i) {
			message += bestBreeder.fitness[i].ToString() + "; ";
		}
		Debug.Log (message);

		for (int i = 0; i < mChromosomes.Length; ++i) {
			Specimen parent_1 = parents [Random.Range(0,specimenList.Count)];
			Specimen parent_2 = parents [Random.Range(0,specimenList.Count)];
			Chromosome childChromosome = parent_1.chromosome.Breed (parent_2.chromosome);
			childChromosome.specimenId = specimenList [i].name;
			if (Random.Range (0f, 1f) < mMutationProbability) {
				childChromosome.Mutate (mMaxMutation);
			}
			mChromosomes [i] = childChromosome;
		}

		return mChromosomes;
	}
}
