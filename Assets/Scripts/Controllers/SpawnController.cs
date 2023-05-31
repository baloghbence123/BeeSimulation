using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using static UnityEditor.PlayerSettings;
using System.Runtime.InteropServices;

public class SpawnController : MonoBehaviour
{

    private Vector3 position;
    private float radius = 2f;

    public int inputNodes = 10;
    public int outputNodes = 2;
    public int hiddenNodes = 0;

    [Header("Current information:")]
    public int currentBeeCounter = 0;
    public int startingPopulation = 0;

    public float liveTimer = 0;
    public float foodCounter = 0;

    [Header("Spawn and evolve options:")]
    public float mutationTimer = 5;
    public float spawnTimer = 0.5f;
    public int maxSpawnedBeeAtOnce = 25;
    public float maxTimeTilBeeDeath = 100;

    public float queenSpawnChance = 0.02f;

    public float hiveMaxTime = 1000;


    [Header("Starting options:")]
    public bool loadNeuralsFromSaved = false;

    public GameObject BeePrefab;
    public GameObject QueenPrefab;

    public List<GameObject> allNeatBee;

    public NeatNetwork queensNetwork;
    public List<NeatNetwork> repNets = new List<NeatNetwork>();
    private int repCtr = 0;

    //if the queen "met" with other bees succesfully then we should cross over her net and one net
    //each time from the repNets

    private bool isRepSuccesful;
    public NeatNetwork printableNet;

    private bool isThisFirstHive = true;

    [Header("Private variables(do not change them):")]
    private float _mutationTimer = 0;
    private float _spawnTimer = 0;

    public float _maxFitness = 0;
    public NeatNetwork _bestNeat;

    private DrawNeuralNetwork drawer;

    

    // Update is called once per frame
    private void Start()
    {
        allNeatBee  = new List<GameObject>();
        drawer = GameObject.FindObjectOfType<DrawNeuralNetwork>(); //this ensures everyone has the
        //same drawer object.

        //queensNetwork = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
        if (queensNetwork == null)
        {
            if (loadNeuralsFromSaved)
            {
                queensNetwork = new NeatNetwork(NeatUtilities.LoadGenome());
            }
            else
            {
                queensNetwork = new NeatNetwork(inputNodes, outputNodes, hiddenNodes);
            }
        }


        position = transform.position;

        if (isThisFirstHive)
        {
            repNets = new List<NeatNetwork>();
            isRepSuccesful = false;
            InitialPrintable();
            this.StartingPopulation();
        }
        
    }
    public void SpawnerInit(List<NeatNetwork> initRepList)
    {
        isThisFirstHive =false;
        repNets = initRepList;
        if (repNets.Count == 0)
        {
            isRepSuccesful = false;
        }
        else
        {
            isRepSuccesful = true;
        }
        InitialPrintable();

        this.StartingPopulation();

    }

    //this func is worthless because I forget that I also set the current best when a bee dies.
    private NeatNetwork GetBest()
    {
        if (allNeatBee.Count>0)
        {
            float maxVal = allNeatBee.Max(t => t.gameObject.GetComponent<BeeController>().overallFitness);
            return allNeatBee.FirstOrDefault(t => t.gameObject.GetComponent<BeeController>().overallFitness == maxVal)
                .gameObject.GetComponent<BeeController>().myNetwork;
        }
        return queensNetwork;
        
    }

    private void FixedUpdate()
    {
        liveTimer += Time.deltaTime;
        //spawning is off because of test ---WARNING ----
        if (currentBeeCounter < maxSpawnedBeeAtOnce)
        {
            SetTimers();
            PrintableGeenMutation(mutationTimer);
            SingleBeeSpawnOnTime(spawnTimer);
        }

        //best is off because too much processing power needed -->> ##Devnote:
        //Also it should get the best of all population. Or atleast it should show the best per hive.

        //var tmpBest = GetBest();
        //if (drawer.neuralNetwork != tmpBest)
        //{
        //    drawer.neuralNetwork = tmpBest;
        //    drawer.Plot();
        //}

        currentBeeCounter = GetCurrentCount();

    }
    private int GetCurrentCount()
    {
        return allNeatBee.Count;
    }
    private void SetTimers()
    {

        _mutationTimer += Time.deltaTime;
        _spawnTimer += Time.deltaTime;
    }

    private void InitialPrintable()
    {
        if (isRepSuccesful)
        {
            repCtr %= repNets.Count;
            printableNet = new NeatNetwork(
                NeatUtilities.CrossOver(queensNetwork.myGenome, repNets[repCtr].myGenome)
                );
            repCtr++;

        }
        else
        {

            //I try to mutate the initial network not just the "printable" beacuse
            //I want to have a biggerv variety
            printableNet = queensNetwork.MutateInitialNetwork();
        }
    }
    private void PrintableGeenMutation(float timeNeededToEvolve)
    {
        
        if (timeNeededToEvolve<=_mutationTimer)
        {
            _mutationTimer -= timeNeededToEvolve;
            if (isRepSuccesful)
            {
                repCtr %= repNets.Count;
                printableNet = new NeatNetwork(
                    NeatUtilities.CrossOver(queensNetwork.myGenome, repNets[repCtr].myGenome)
                    );
                repCtr++;

            }
            else
            {
                
                //I try to mutate the initial network not just the "printable" beacuse
                //I want to have a biggerv variety
                printableNet = queensNetwork.MutateInitialNetwork();
            }
            
        }
    }
    private void SingleBeeSpawnOnTime(float timeNeededToSpawnOne)
    {
        if (timeNeededToSpawnOne<=_spawnTimer)
        {
            _spawnTimer -= timeNeededToSpawnOne;
            SpawnABee(this.printableNet);
        }
    }
    private void StartingPopulation()
    {
        for (int i = 0; i < startingPopulation; i++)
        {
            this.SpawnABee(this.printableNet);
        }
    }
    //basically it is a function to spawn a bee next to the hive
    private void SpawnABee(NeatNetwork printableNet)
    {
        //by a little chance the spawner will create a queen which is able to evolve to a spawner.
        if (UnityEngine.Random.Range(0.0f,1.0f)<queenSpawnChance)
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
            controllerTmp.myBrainIndex = allNeatBee.Count;


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
            controllerTmp.myBrainIndex = allNeatBee.Count;

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

            allNeatBee.Add(spawnTmp);
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
        foreach (var item in allNeatBee)
        {
            Destroy(item);
        }
        Destroy(this);
    }

}
