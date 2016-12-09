using UnityEngine;
using System.Collections.Generic;

public class God_BHV : MonoBehaviour {

    public GameObject BlambPrefab;
    public int adamsRibs; //genePoolSize
    public int gardenSize; //populationSize
    public int selectionSize;

    public float mutationRatio;
    public float mutationIntensity;
    public float generationTime;
	public bool automaticEvolve = false;
	public int generationNumber = 0;
	
    private List<Base_BHV> population = new List<Base_BHV>();

    private float timer;
	private float generationBestFit = 0f;
	private float alltimeBestFit = 0f;
    
    void Start() {
        Base_BHV.sectionNum = adamsRibs;
        timer = generationTime;
        InitPop();
    }

    void FixedUpdate() {
		if (automaticEvolve) {
			timer -= Time.fixedDeltaTime;
			if (timer <= 0) {
				timer = generationTime;
				NewGeneration();
			}
		}
		else if (Input.GetKeyDown(KeyCode.Space)) {
			NewGeneration();
		}
    }

    private void InitPop() {
        for (int i=0; i< gardenSize; i++) {
            GameObject newBlamb = (GameObject)Instantiate(BlambPrefab, new Vector3(0,10,0), Quaternion.identity);
            Base_BHV newBase = newBlamb.GetComponent<Base_BHV>();
            newBase.Randomize();
            population.Add(newBase);
        }
    }

    private void NewGeneration() {
		generationNumber++;
        List<Base_BHV> selection = Select();
        List<Base_BHV> children = Crossover(selection);
        Mutation(children);
        List<Base_BHV> entrees = new List<Base_BHV>();
        entrees.AddRange(population);
        List<Base_BHV> oldPopulation = new List<Base_BHV>();
        oldPopulation.AddRange(population);
        population.Clear();
        population.AddRange(children);
        Tournment(entrees);
        List<GameObject> objPop = new List<GameObject>();
        population.ForEach(b => objPop.Add(b.gameObject));
        foreach (Base_BHV oldBlamb in oldPopulation) {
            if (objPop.Contains(oldBlamb.gameObject)) {
				/*
                oldBlamb.transform.position = new Vector3(0, 5, 0);
                oldBlamb.transform.rotation = Quaternion.identity;
                oldBlamb.Reset();
				*/
				GameObject newObj = (GameObject)Instantiate(BlambPrefab, new Vector3(0, 10, 0), Quaternion.identity);
				Base_BHV newBlamb = newObj.GetComponent<Base_BHV>();
				newBlamb.CopyProperties(oldBlamb);
				newBlamb.ColorMeshes(oldBlamb.color);
				population.Remove(oldBlamb);
				population.Add(newBlamb);
				Destroy(oldBlamb.gameObject);
            }
            else {
                Destroy(oldBlamb.gameObject);
            }
        }
		if (alltimeBestFit < generationBestFit){
			alltimeBestFit = generationBestFit;
		}
		print("Generation " + generationNumber + ", AllTimeBest=" + alltimeBestFit);
		generationBestFit = 0f;
    }

    private List<Base_BHV> Select() {
        List<Base_BHV> selection = new List<Base_BHV>();
        population.Sort(new System.Comparison<Base_BHV>(Base_BHV.CompareDist));
		generationBestFit = population[population.Count-1].maxDistAchieved;
        selection.AddRange(population.GetRange((population.Count-1)-selectionSize, selectionSize));
        return selection;
    }

    private List<Base_BHV> Crossover(List<Base_BHV> selection) {
        List<Base_BHV> children = new List<Base_BHV>();
        for (int i=0; i<selection.Count; i+=2) {
            GameObject newChild = (GameObject)Instantiate(BlambPrefab, new Vector3(0, 10, 0), Quaternion.identity);
            Base_BHV newBase = newChild.GetComponent<Base_BHV>();
            newBase.DefinePropertiesFrom(selection[i], selection[i + 1]);
            children.Add(newBase);
        }
        return children;
    }
    
    private void Mutation(List<Base_BHV> children) {
        int mutationRate = Mathf.RoundToInt(children.Count * mutationRatio);
        for (int i=0; i<mutationRate; i++) {
            children[Random.Range(0,children.Count)].MutateProperties(mutationIntensity);
        }
    }

    private void Tournment(List<Base_BHV> entrees) {
        List<List<Base_BHV>> loosers = new List<List<Base_BHV>>();
        while (entrees.Count > 1) {
            List<Base_BHV> roundLoosers = new List<Base_BHV>();
            List<Base_BHV> roundWinners = new List<Base_BHV>();
            while (entrees.Count > 1) {
                int index = Random.Range(0, entrees.Count);
                Base_BHV first = entrees[index];
                entrees.RemoveAt(index);

                index = Random.Range(0, entrees.Count);
                Base_BHV second = entrees[index];
                entrees.RemoveAt(index);

                if (Base_BHV.CompareDist(first, second) > 0) {
                    roundWinners.Add(first);
                    roundLoosers.Add(second);
                }
                else {
                    roundWinners.Add(second);
                    roundLoosers.Add(first);
                }
            }
            roundLoosers.AddRange(entrees);
            loosers.Add(roundLoosers);
            entrees.Clear();
            entrees.AddRange(roundWinners);
        }
        population.AddRange(entrees);
        for (int i= loosers.Count-1; population.Count < gardenSize; i--) {
            if (loosers[i].Count > gardenSize - population.Count) {
                population.AddRange(loosers[i].GetRange(0,gardenSize-population.Count));
            }
            else {
                population.AddRange(loosers[i]);
            }
        }
    }

}
