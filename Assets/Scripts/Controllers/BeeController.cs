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

    private float hitDivider = 6f;
    private float rayDistance = 6f;
    
   public bool paralised = false;


    [Header("Energy Options")]
    public float maxTimeTilDeath = 24;

    [Header("FitnessOptions")]
    public float overallFitness = 0;
    public float foodMultiplier = 2;
    public float depositMultiplier = 4;
    public float foodCounter = 0;
    public float depositCounter = 0;

    public float plusTimePerFood = 8;
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
        sensors = new float[inputNodes];
        


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!paralised)
        {
            InputSensors();
            float[] outputs = myNetwork.FeedForwardNetwork(sensors);
            MoveBee(outputs[0], outputs[1]);
            surviveTime += Time.deltaTime;
            CalculateFitness();
        }

        
        

    }

    private void CalculateFitness()
    {
        
        overallFitness = (foodCounter * foodMultiplier) + (depositCounter * depositMultiplier) + 1;
        if (surviveTime > maxTimeTilDeath)
        {
            Death();
        }

    }
    private void Death()
    {
       
        //SetBestFitness();

        mySpawner.currentBeeCounter--;
        mySpawner.liveNeat.Remove(this.gameObject);

        var tmp = new GameObject();
        tmp.AddComponent<BeeController>();
        var tmpcontroller = tmp.gameObject.GetComponent<BeeController>();
        tmpcontroller.myBrainIndex = this.myBrainIndex;
        tmpcontroller.myNetwork = this.myNetwork;
        tmpcontroller.mySpawner = this.mySpawner;
        tmpcontroller.overallFitness = overallFitness;
        tmpcontroller.foodCounter = this.foodCounter;
        tmpcontroller.depositCounter = this.depositCounter;
        tmpcontroller.paralised = true;
        mySpawner.deadNeat.Add(tmp);


        Destroy(this.gameObject);
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
       
        if (collision.transform.tag == "Food")
        {
                
                collision.gameObject.GetComponent<FoodController>().SpawnSignleFood();
                Destroy(collision.gameObject);
                foodCounter++;
                mySpawner.foodCounter++;
                maxTimeTilDeath += plusTimePerFood;
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
        for (int i = 0; i < sensors.Length; i++)
        {
            sensors[i] = 0;
        }
        //3 sensor to see wall
        if (hit.collider!=null)
        {
            if (hit.transform.tag =="Wall")
            {
                sensors[0] = 1-(hit.distance / hitDivider);
                Debug.DrawLine(curr, hit.point, Color.white);
            }
        }
        

        hit = Physics2D.Raycast(curr, transform.up+transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Wall")
            {

                sensors[1] = 1 - (hit.distance / hitDivider);
                Debug.DrawLine(curr, hit.point, Color.white);
            }
        }

        hit = Physics2D.Raycast(curr, transform.up-transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Wall")
            {

                sensors[2] = 1 - (hit.distance / hitDivider);
                Debug.DrawLine(curr, hit.point, Color.white);
            }
        }
        //3 sensor to see food
        hit = Physics2D.Raycast(curr, transform.up, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {
                sensors[3] = 1 - (hit.distance / hitDivider);
                Debug.DrawLine(curr, hit.point, Color.yellow);
            }
            
        }

        hit = Physics2D.Raycast(curr, transform.up + transform.right, rayDistance   , layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {
                
                sensors[4]= 1 - (hit.distance / hitDivider);
                Debug.DrawLine(curr, hit.point, Color.yellow);
            }
        }

        hit = Physics2D.Raycast(curr, transform.up - transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {
                
                sensors[5] = 1 - (hit.distance / hitDivider);
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
        if (v==0)
        {
            v = 0.4f;
        }
        // Getting Next Position
        Vector3 input = Vector3.Lerp(Vector3.zero, new Vector3(0, v * 2f, 0), 0.1f);
        input = transform.TransformDirection(input);

        // Movement of Agent
        transform.position += input;

        // Rotation of Agent
        transform.eulerAngles += new Vector3(0, 0, (h * 90) * 0.1f);
    }
}
