using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class NeatGManager : MonoBehaviour
{
    public GameObject NeatBeePrefab;
    public GameObject[] allNeatFish;
    public NeatNetwork[] allNeatNetworks;

    public int inputNodes, outputNodes, hiddenNodes;

    [SerializeField] private int currentGeneration = 0;

    [SerializeField] private float currentbestFitness = 0;
    [SerializeField] private float overallBestFitness = 0;

    public int startingPopulation;

    public int keepBest, leaveWorst;

    public int currentAlive;
    private bool repoping = false;

    public bool spawnFromSave = false;
    public int bestTime = 100;
    public int addToBest = 50;

    private int range = 16;

    DrawNeuralNetwork drawer;


    void Start()
    {
        allNeatFish = new GameObject[startingPopulation];
        allNeatNetworks = new NeatNetwork[startingPopulation];
        drawer = GameObject.Find("GraphContainer").GetComponent<DrawNeuralNetwork>();
        if (spawnFromSave == true)
        {
            StartingSavedNetwork();
        }
        else
        {
            StartingNetworks();
        }

        MutatePopulation();
        SpawnBody();
        currentGeneration += 1;
    }

    void FixedUpdate()
    {
        currentAlive = CurrentAlive();
        if (repoping == false && currentAlive <= 0)
        {
            repoping = true;
            // Repopulate for next generation.
            RePopulate();
            repoping = false;
        }

    }

    public int CurrentAlive()
    {
        int alive = 0;
        for (int i = 0; i < allNeatFish.Length; i++)
        {
            if (allNeatFish[i].gameObject)
            {
                alive++;
            }
        }
        return alive;
    }

    private void RePopulate()
    {

        GettingAndDrawingTheBest();
        if (spawnFromSave == true)
        {
            bestTime = bestTime + addToBest;
            StartingSavedNetwork();
        }
        else
        {
            SortPopulation();
            SetNewPopulationNetworks();
        }


        MutatePopulation();
        GameObject.FindObjectOfType<FoodManager>().DestroyFood();
        GameObject.FindObjectOfType<FoodManager>().SpawnFood();
        SpawnBody();
        currentGeneration += 1;
    }
    private void GettingAndDrawingTheBest() // => make it only draw
    {
        currentbestFitness = allNeatNetworks.Max(t => t.fitness);
        if (currentbestFitness > overallBestFitness)
        {
            overallBestFitness = currentbestFitness;
        }
        drawer.neuralNetwork = allNeatNetworks.FirstOrDefault(t => t.fitness == currentbestFitness);
        
        drawer.Plot();
    }
    private void GetBest()
    {
        // return bestNn
    }
    private void MutatePopulation()
    {
        for (int i = keepBest; i < startingPopulation; i++)
        {
            allNeatNetworks[i].MutateInitialNetwork();
        }
    }
    private void SortPopulation()
    {

        allNeatNetworks = allNeatNetworks.OrderByDescending(t => t.fitness).ToArray();

        float[] tmp = NeatUtilities.GetDisjointAndExcessTogether(allNeatNetworks[0].myGenome.conGenes, allNeatNetworks[1].myGenome.conGenes);
        Debug.Log("Dis and exc:" + tmp[0] + " \t Avg weight difference:" + tmp[1]);

    }

    public void BestFound() 
    {


        spawnFromSave = true;

        //use GetBest() method instead this long crap
        NeatUtilities.SaveGenome(allNeatNetworks.FirstOrDefault(t => t.fitness == (allNeatNetworks.Max(t => t.fitness))).myGenome);
        

    }

    private void SetNewPopulationNetworks()
    {
        NeatNetwork[] newPopulation = new NeatNetwork[startingPopulation];
        for (int i = 0; i < startingPopulation - leaveWorst; i++)
        {
            newPopulation[i] = allNeatNetworks[i];
        }
        for (int i = startingPopulation - leaveWorst; i < startingPopulation; i++)
        {
            newPopulation[i] = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
        }
        allNeatNetworks = newPopulation;
    }

    private void StartingNetworks()
    {
        /*
            Creates Initial Group of Networks from StartingPopulation integer.
        */
        for (int i = 0; i < startingPopulation; i++)
        {
            allNeatNetworks[i] = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
        }
    }

    private void StartingSavedNetwork()
    {
        /*
            Creates initial Group of Networks from Saved Network.
        */
        spawnFromSave = false;
        for (int i = 0; i < startingPopulation; i++)
        {
            allNeatNetworks[i] = new NeatNetwork(NeatUtilities.LoadGenome());
        }
    }

    private void SpawnBody()
    {
        /* Creates Initial Group of Fish GameObjects from StartingPopulation integer 
        and matches fishObjects to their NetworkBrains. */

        for (int i = 0; i < startingPopulation; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-range, range), Random.Range(-range, range), 0);
            Vector3 tmp;
            Quaternion quat;
            transform.GetLocalPositionAndRotation(out tmp, out quat);
            pos += tmp;

            allNeatFish[i] = Instantiate(NeatBeePrefab, pos, transform.rotation);
            allNeatFish[i].gameObject.GetComponent<BeeController>().myBrainIndex = i;
            allNeatFish[i].gameObject.GetComponent<BeeController>().myNetwork = allNeatNetworks[i];
            allNeatFish[i].gameObject.GetComponent<BeeController>().inputNodes = inputNodes;
            allNeatFish[i].gameObject.GetComponent<BeeController>().outputNodes = outputNodes;
            allNeatFish[i].gameObject.GetComponent<BeeController>().hiddenNodes = hiddenNodes;
        }
    }

    public void Death(float fitness, int index)
    {
        allNeatNetworks[index].fitness = fitness;
    }
}

