using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class QueenController : MonoBehaviour
{

    public SpawnController mySpawner;

    //Networks to work with:
    // One for the bee itself and a list of networks to represent the RL reproduction
    public NeatNetwork myNetwork;
    public List<NeatNetwork> repNets=new List<NeatNetwork>();

    //needed to make a spawnerPoint out of the this bee:
    public GameObject spawnerPrefab;
    //Sensor settings
    private float[] sensors;
    private float hitDivider = 25f;
    private float rayDistance = 25f;

    public int maxTimeTilDeath = 0;

    public float surviveTime = 0;


    public int inputNodes, outputNodes, hiddenNodes;

    public int myBrainIndex;

    int layerMask;
    public int overallFitness = 0;


    // Start is called before the first frame update
    private void Awake()
    {
        repNets = new List<NeatNetwork>();
    }
    void Start()
    {
        layerMask = LayerMask.GetMask("ObjectsToSee");
        sensors = new float[inputNodes];

    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        InputSensors();
        float[] outputs = myNetwork.FeedForwardNetwork(sensors);
        MoveBee(outputs[0], outputs[1]);
        surviveTime += Time.deltaTime;
        if (surviveTime>= maxTimeTilDeath)
        {
            Death();
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Wall")
        {
            //Debug.Log("Bee Died");
            // overallFitness = 0;
            Destroy(gameObject);
        }
        //if it collides with another bee then we take his network.
        if (collision.transform.tag == "Bee")
        {
            if (collision.transform.GetComponent<BeeController>()!=null)
            {
                var tmpNetwork = collision.transform.GetComponent<BeeController>().myNetwork;
                if (!repNets.Contains(tmpNetwork))
                {
                    repNets.Add(tmpNetwork);
                }
            }


        }

    }

    private void InputSensors()
    {
        sensors = sensors.Select(t => t = -1).ToArray();
        Vector2 curr = transform.position + (transform.up * (transform.localScale.y * 1.05f));

        RaycastHit2D hit = Physics2D.Raycast(curr, transform.up, rayDistance, layerMask);
        //3 sensor to see wall
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Wall")
            {
                sensors[0] = hit.distance / hitDivider;
                //Debug.DrawLine(curr, hit.point, Color.white);
            }
        }


        hit = Physics2D.Raycast(curr, transform.up + transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Wall")
            {

                sensors[1] = hit.distance / hitDivider;
                //Debug.DrawLine(curr, hit.point, Color.white);
            }
        }

        hit = Physics2D.Raycast(curr, transform.up - transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Wall")
            {

                sensors[2] = hit.distance / hitDivider;
                // Debug.DrawLine(curr, hit.point, Color.white);
            }
        }
        //3 sensor to see food
        hit = Physics2D.Raycast(curr, transform.up, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {
                sensors[3] = hit.distance / hitDivider;
                //Debug.DrawLine(curr, hit.point, Color.yellow);
            }

        }

        hit = Physics2D.Raycast(curr, transform.up + transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {

                sensors[4] = hit.distance / hitDivider;
                //Debug.DrawLine(curr, hit.point, Color.yellow);
            }
        }

        hit = Physics2D.Raycast(curr, transform.up - transform.right, rayDistance, layerMask);
        if (hit.collider != null)
        {
            if (hit.transform.tag == "Food")
            {

                sensors[5] = hit.distance / hitDivider;
                //Debug.DrawLine(curr, hit.point, Color.yellow);
            }
        }
        //3 sensor to know where the Hive is
        //no hive detection needed for queen
        //hit = Physics2D.Raycast(curr, transform.up, rayDistance, layerMask);
        //if (hit.collider != null)
        //{
        //    if (hit.transform.tag == "Hive")
        //    {
        //        sensors[6] = hit.distance / hitDivider;
        //        Debug.DrawLine(curr, hit.point, Color.blue);
        //    }

        //}

        //hit = Physics2D.Raycast(curr, transform.up + transform.right, rayDistance, layerMask);
        //if (hit.collider != null)
        //{
        //    if (hit.transform.tag == "Hive")
        //    {

        //        sensors[7] = hit.distance / hitDivider;
        //        Debug.DrawLine(curr, hit.point, Color.blue);
        //    }
        //}

        //hit = Physics2D.Raycast(curr, transform.up - transform.right, rayDistance, layerMask);
        //if (hit.collider != null)
        //{
        //    if (hit.transform.tag == "Hive")
        //    {

        //        sensors[8] = hit.distance / hitDivider;
        //        Debug.DrawLine(curr, hit.point, Color.blue);
        //    }
        //}

        sensors[9] = 0;



    }
    private void Death()
    {
        //GameObject.FindObjectOfType<NeatGManager>().Death(overallFitness, myBrainIndex);


        SpawnerCreating();
        Destroy(gameObject);
    }
    private void SpawnerCreating()
    {

        var spawnerObject = Instantiate(spawnerPrefab, this.transform.position, this.transform.rotation);
        //needed to randomize the stats a bit more, and also modify the stats that are not modified
        //below
        var spawnerController = spawnerObject.GetComponent<SpawnController>();
        spawnerController.inputNodes = mySpawner.inputNodes;
        spawnerController.outputNodes = mySpawner.outputNodes;
        spawnerController.queensNetwork = myNetwork;
        spawnerController.SpawnerInit(this.repNets);
        spawnerController.maxSpawnedBeeAtOnce = UnityEngine.Random.RandomRange(6, 8);


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
