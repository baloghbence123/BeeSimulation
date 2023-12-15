using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using static UnityEditor.PlayerSettings;
using System.Runtime.InteropServices;

public class SpawnController : MonoBehaviour
{
    private static int _id = 0;
    public static int maxSpawner = 2;
    public int Id;
    private Vector3 position;
    private float radius = 2f;

    public int inputNodes = 6;
    public int outputNodes = 2;
    public int hiddenNodes = 0;

    [Header("Current information:")]
    public int currentBeeCounter = 0;
    public int startingPopulation = 0;

    public float liveTimer = 0;
    public float foodCounter = 0;

    [Header("Spawn and evolve options:")]
    public float spawnTimer = 0.5f;
    public int maxSpawnedBeeAtOnce = 40;
    public float maxTimeTilBeeDeath = 200;

    public float queenSpawnChance = 0.00f;

    public float hiveMaxTime = 1000;


    [Header("Starting options:")]
    public bool loadNeuralsFromSaved = false;

    public GameObject BeePrefab;
    public GameObject QueenPrefab;

    public List<GameObject> liveNeat;
    public List<GameObject> deadNeat;
    public List<NeatNetwork> spawnableNetworks;



    private int repCtr = 0;

    //if the queen "met" with other bees succesfully then we should cross over her net and one net
    //each time from the repNets

    private bool isRepSuccesful;
    public NeatNetwork printableNet;

    private NeatNetwork numberOneNetwork;
    private NeatNetwork numberTwoNetwork;

    private bool isThisFirstHive = true;

    [Header("Private variables(do not change them):")]

    private float _spawnTimer = 0;
    private float offspringTimer = 0;
    public readonly float offspringTime = 150;

    public float _maxFitness = 0;
    public NeatNetwork _bestNeat;

    private DrawNeuralNetwork drawer;

    

    // Update is called once per frame
    private void Start()
    {
        Id = _id++;
        liveNeat  = new List<GameObject>();
        deadNeat = new List<GameObject>();
        spawnableNetworks = new List<NeatNetwork>();
        
        drawer = GameObject.FindObjectOfType<DrawNeuralNetwork>(); //this ensures everyone has the
                                                                   //same drawer object.

        //queensNetwork = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
        if (loadNeuralsFromSaved)
        {
            var tmpLoadedNetwork = NeatUtilities.LoadGenome();
            for (int i = 0; i < maxSpawnedBeeAtOnce; i++)
            {
                spawnableNetworks.Add(new NeatNetwork(tmpLoadedNetwork));
            }
        }
        else
        {
            spawnableNetworks.AddRange(Enumerable.Range(0, maxSpawnedBeeAtOnce).Select(t => new NeatNetwork(inputNodes, outputNodes, hiddenNodes).MutateInitialNetwork()));
            
        }


        position = transform.position;

        
    }

    //this func is worthless because I forget that I also set the current best when a bee dies.
    float overallBest = 0;
    private NeatNetwork GetBest()
    {
        var tmpAllNeat = liveNeat.ToList();
        tmpAllNeat.AddRange(deadNeat);
        if (tmpAllNeat.Count>2)
        {
            float maxVal = tmpAllNeat.Max(t => t.gameObject.GetComponent<BeeController>().overallFitness);
            List<GameObject> tmpList = tmpAllNeat.OrderByDescending(t => t.gameObject.GetComponent<BeeController>().overallFitness).ToList();

            if (numberOneNetwork==null)
            {
                numberOneNetwork = new NeatNetwork(tmpList[0].gameObject.GetComponent<BeeController>().myNetwork.myGenome)
                {
                    fitness = maxVal,
                };
                numberTwoNetwork = new NeatNetwork(tmpList[1].gameObject.GetComponent<BeeController>().myNetwork.myGenome);
            }
            else if(numberOneNetwork.fitness<maxVal)
            {
                numberTwoNetwork = new NeatNetwork(numberOneNetwork.myGenome);
                numberOneNetwork = new NeatNetwork(tmpList[0].gameObject.GetComponent<BeeController>().myNetwork.myGenome)
                {
                    fitness = maxVal,
                };
                Debug.Log("Number one is now number two.");
            }
            
           
            if (overallBest<maxVal)
            {
                overallBest = maxVal;
                NeatUtilities.SaveGenome(numberOneNetwork.myGenome);
            }

            return numberOneNetwork;
        }
        return null;
        
    }
    private NeatNetwork GetBestByFitness()
    {
        var tmpAllNeat = liveNeat.ToList();
        tmpAllNeat.AddRange(deadNeat.ToList());
        var currentBest = tmpAllNeat.OrderByDescending(t => t.gameObject.GetComponent<BeeController>().overallFitness).FirstOrDefault();
        if (currentBest != null)
        {

            return currentBest.gameObject.GetComponent<BeeController>().myNetwork;
        }
        else
        {
            return spawnableNetworks.FirstOrDefault();
        }
        
    }
    private void FixedUpdate()
    {
        liveTimer += Time.deltaTime;
        offspringTimer += Time.deltaTime;
        //var tmpBest = GetBest();
        if (spawnableNetworks.Count==0)
        {
            //make offsprings
            var tmpAllNetworks = new List<BeeController>();
            tmpAllNetworks.AddRange(liveNeat.Select(t=>t.gameObject.GetComponent<BeeController>()).ToList());
            tmpAllNetworks.AddRange(deadNeat.Select(t => t.gameObject.GetComponent<BeeController>()).ToList());
            spawnableNetworks.AddRange(SpawnUtilities.OffspringCreation(tmpAllNetworks).ToList());
            deadNeat.ForEach(t => Destroy(t.gameObject));
            deadNeat = new List<GameObject>();
        }
        if (currentBeeCounter < maxSpawnedBeeAtOnce)
        {
            SetTimers();
            SingleBeeSpawnOnTime(spawnTimer);
        }
        
        //best is off because too much processing power needed -->> ##Devnote:
        //Also it should get the best of all population. Or atleast it should show the best per hive.

        
        if (Id == 0)
        {
            var tmpBest = GetBestByFitness();
            if (drawer.neuralNetwork != tmpBest)
            {
                drawer.neuralNetwork = tmpBest;
                drawer.Plot();
            }
            
        }


        currentBeeCounter = GetCurrentCount();

    }
    private int GetCurrentCount()
    {
        return liveNeat.Count;
    }
    private void SetTimers()
    {

        _spawnTimer += Time.deltaTime;
    }
    private void SingleBeeSpawnOnTime(float timeNeededToSpawnOne)
    {
        if (timeNeededToSpawnOne<=_spawnTimer)
        {
            _spawnTimer -= timeNeededToSpawnOne;
            
            SpawnABee(spawnableNetworks.First());            
            spawnableNetworks.RemoveAt(0);
        }
    }
    //basically it is a function to spawn a bee next to the hive
    private void SpawnABee(NeatNetwork printableNet)
    {
        //by a little chance the spawner will create a queen which is able to evolve to a spawner.
        if (UnityEngine.Random.Range(0.0f,1.0f)<queenSpawnChance && GameObject.FindObjectsOfType<SpawnController>().Length<maxSpawner)
        {
            Vector3 pos = GetPositionOutsideCircle(new Vector3(0, 0, 0), radius); //this makes the spawning a little less deterministic

            Vector3 tmpVector;
            Quaternion quat;
            transform.GetLocalPositionAndRotation(out tmpVector, out quat);
            Vector3 dir = pos - this.transform.position;

            pos += tmpVector;

            var spawnTmp = Instantiate(QueenPrefab, pos, this.transform.rotation);
            var controllerTmp = spawnTmp.gameObject.GetComponent<QueenController>();
            controllerTmp.transform.up = dir.normalized;
            controllerTmp.myBrainIndex = liveNeat.Count;


            //I try to mutate the network to make it more realistic. I suppose the mutation is similar IRL

            var tmpNetwork = new NeatNetwork(printableNet.myGenome);
            controllerTmp.myNetwork = tmpNetwork.MutateInitialNetwork();

            controllerTmp.inputNodes = tmpNetwork.inputNodes.Count;
            controllerTmp.outputNodes = tmpNetwork.outputNodes.Count;
            controllerTmp.hiddenNodes = tmpNetwork.hiddenNodes.Count;
            controllerTmp.mySpawner = this;
            controllerTmp.maxTimeTilDeath = 20;
            //queens are not added to the spawner hive's population

        }
        else
        {        
            Vector3 pos = GetPositionOutsideCircle(new Vector3(0, 0, 0), radius); //this makes the spawning a little less deterministic

            Vector3 tmpVector;
            Quaternion quat;
            transform.GetLocalPositionAndRotation(out tmpVector, out quat);
            Vector3 dir = pos - this.transform.position;

            pos += tmpVector;
        
            var spawnTmp = Instantiate(BeePrefab, pos, this.transform.rotation);
        
            var controllerTmp = spawnTmp.gameObject.GetComponent<BeeController>();
            controllerTmp.transform.up = dir.normalized ;
            controllerTmp.myBrainIndex = liveNeat.Count;

            //I try to mutate the network to make it more realistic. I suppose the mutation is
            //similar IRL. I mean there is a slight dns mutation between the parent's and child's
            //dns.
            var tmpNetwork = new NeatNetwork(printableNet.myGenome);
            controllerTmp.myNetwork = tmpNetwork.MutateInitialNetwork();

            controllerTmp.inputNodes = tmpNetwork.inputNodes.Count;
            controllerTmp.outputNodes = tmpNetwork.outputNodes.Count;
            controllerTmp.hiddenNodes = tmpNetwork.hiddenNodes.Count;
            controllerTmp.mySpawner = this;
            controllerTmp.maxTimeTilDeath = maxTimeTilBeeDeath;

            liveNeat.Add(spawnTmp);
            currentBeeCounter++;
        }


    }
    public static Vector3 GetPositionOutsideCircle(Vector3 currentPostion, float radius)
    {
        
        float angle = (float)NeatUtilities.random.NextDouble() * 2 * (float)Math.PI;
        float x = currentPostion.x + (radius) * (float)Math.Cos(angle);
        float y = currentPostion.y + (radius) * (float)Math.Sin(angle);

        return new Vector3(x,y,0);
    }
    private void Death()
    {
        foreach (var item in liveNeat)
        {
            Destroy(item);
        }
        Destroy(this);
    }

}
