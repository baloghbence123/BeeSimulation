using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BeeController : MonoBehaviour
{
    public SpawnController mySpawner;
    public NeatNetwork myNetwork;

    private float[] sensors;

    private float hitDivider = 7f;
    private float rayDistance = 7f;


    

    [Header("Energy Options")]
    public float startingEnergy = 20;
    public float currentEnergy = 0;
    public float maxTimeTilDeath = 250;
    public float timeLived = 0;

    [Header("FitnessOptions")]
    public float overallFitness = 0;
    public float foodMultiplier = 2;
    public float depositMultiplier = 4;
    public float foodCounter = 0;
    public float depositCounter = 0;

    public float foodCapacity = 10;

    [Header("Network Settings")]
    public int myBrainIndex;
    [DoNotSerialize]
    public int inputNodes, outputNodes, hiddenNodes;

    public float surviveTime = 0;

    
    int layerMask;
    // [Range(-1f,1f)]
    // public float a,t;

    void Start()
    {
        layerMask = LayerMask.GetMask("ObjectsToSee");
        currentEnergy = startingEnergy;
        sensors = new float[inputNodes];
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        InputSensors();
        float[] outputs = myNetwork.FeedForwardNetwork(sensors);
        MoveBee(outputs[0], outputs[1]);
        CalculateFitness();

        surviveTime += Time.deltaTime;
    }

    private void CalculateFitness()
    {
        UpdateEnergy();
        overallFitness = (foodCounter * foodMultiplier) + (depositCounter * depositMultiplier);
        timeLived+= Time.deltaTime;
        if (currentEnergy <= 0 || timeLived>maxTimeTilDeath)
        {
            Death();
        }

    }
    private void UpdateEnergy()
    {
        currentEnergy -= Time.deltaTime;
    }
    private void SetBestFitness()
    {
        if (mySpawner._maxFitness < this.overallFitness)
        {
            mySpawner._maxFitness = this.overallFitness;
            mySpawner.queensNetwork = this.myNetwork;
            NeatUtilities.SaveGenome(this.myNetwork.myGenome);

        }
    }
    private void Death()
    {
       
        //SetBestFitness();

        mySpawner.currentBeeCounter--;
        mySpawner.allNeatBee.Remove(this.gameObject);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "Food")
        {

                collision.gameObject.GetComponent<FoodController>().SpawnSignleFood();
                Destroy(collision.gameObject);
                foodCounter++;
                currentEnergy += foodMultiplier;
                mySpawner.foodCounter += this.foodCounter;
                //Debug.Log("Bee Ate Food");
        }
        //else if (collision.transform.tag == "Hive")
        //{
        //    if (foodCounter > 0)
        //    {
        //        collision.gameObject.GetComponent<SpawnController>().foodCounter += this.foodCounter;
        //        this.depositCounter += this.foodCounter;
        //        this.currentEnergy += this.depositMultiplier * foodCounter;
        //        foodCounter = 0;
        //    }

        //}
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Wall")
        {
            //Debug.Log("Bee Died");
            // overallFitness = 0;
            Death();
        }


    }
    

    private void InputSensors()
    {
        sensors = sensors.Select(t => t = 0).ToArray();
        Vector2 curr = transform.position + (transform.up * (transform.localScale.y*1.05f));

        RaycastHit2D hit = Physics2D.Raycast(curr, transform.up, rayDistance,layerMask);
        //3 sensor to see wall
        if (hit.collider!=null)
        {
            if (hit.transform.tag =="Wall")
            {
                sensors[0] = hit.distance / hitDivider;
                Debug.DrawLine(curr, hit.point, Color.white);
            }
        }
        

        hit = Physics2D.Raycast(curr, transform.up+transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Wall")
            {

                sensors[1] = hit.distance / hitDivider;
                Debug.DrawLine(curr, hit.point, Color.white);
            }
        }

        hit = Physics2D.Raycast(curr, transform.up-transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Wall")
            {

                sensors[2] = hit.distance / hitDivider;
                Debug.DrawLine(curr, hit.point, Color.white);
            }
        }
        //3 sensor to see food
        hit = Physics2D.Raycast(curr, transform.up, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {
                sensors[3] = hit.distance / hitDivider;
                Debug.DrawLine(curr, hit.point, Color.yellow);
            }
            
        }

        hit = Physics2D.Raycast(curr, transform.up + transform.right, rayDistance   , layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {
                
                sensors[4]= hit.distance / hitDivider;
                Debug.DrawLine(curr, hit.point, Color.yellow);
            }
        }

        hit = Physics2D.Raycast(curr, transform.up - transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {
                
                sensors[5] = hit.distance / hitDivider;
                Debug.DrawLine(curr, hit.point, Color.yellow);
            }
        }
        //3 sensor to know where the Hive is
        //hit = Physics2D.Raycast(curr, transform.up, rayDistance, layerMask);
        //if (hit.collider != null)
        //{
        //    if (hit.transform.tag == "Hive")
        //    {
        //        sensors[6] = hit.distance / hitDivider;
        //        //Debug.DrawLine(curr, hit.point, Color.blue);
        //    }

        //}

        //hit = Physics2D.Raycast(curr, transform.up + transform.right, rayDistance, layerMask);
        //if (hit.collider != null)
        //{
        //    if (hit.transform.tag == "Hive")
        //    {

        //        sensors[7] = hit.distance / hitDivider;
        //        //Debug.DrawLine(curr, hit.point, Color.blue);
        //    }
        //}

        //hit = Physics2D.Raycast(curr, transform.up - transform.right, rayDistance, layerMask);
        //if (hit.collider != null)
        //{
        //    if (hit.transform.tag == "Hive")
        //    {

        //        sensors[8] = hit.distance / hitDivider;
        //        //Debug.DrawLine(curr, hit.point, Color.blue);
        //    }
        //}
        ////Food capacity of the bee.
        //sensors[9] = foodCounter / foodCapacity;

        ////Angle and distance to home
        //Vector3 toHome = mySpawner.transform.position - transform.position;
        //float distance = toHome.magnitude;
        //sensors[10] = distance;
        //var dotProduct = Vector3.Dot(toHome.normalized, -mySpawner.transform.up.normalized);

        //sensors[11] = dotProduct;
        //if (myBrainIndex==0)
        //{
        //    Debug.Log(dotProduct);
        //    Debug.Log(distance);
        //}

    }
    public void MoveBee(float v, float h)
    {
        // Getting Next Position
        Vector3 input = Vector3.Lerp(Vector3.zero, new Vector3(0, v * 2f, 0), 0.1f);
        input = transform.TransformDirection(input);

        // Movement of Agent
        transform.position += input;

        // Rotation of Agent
        transform.eulerAngles += new Vector3(0, 0, (h * 90) * 0.1f);
    }
}
