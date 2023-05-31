using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public GameObject foodPrefab;
    public int foodCount;
    public int curAlive;
    private GameObject[] foods;
    private bool repoping = false;
    private int xRange = 30;
    private int yRange = 16;

    void Awake()
    {
        curAlive = foodCount;
        foods = new GameObject[foodCount];
        SpawnFood();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curAlive = CurrentAlive();
        if (repoping == false && curAlive <= 0)
        {
            repoping = true;
            SpawnFood();
            repoping = false;
        }
    }

    public int CurrentAlive()
    {
        FoodController[] localFoods = FindObjectsOfType<FoodController>();
        // Debug.Log(localFoods.Length);
        return localFoods.Length;
    }

    public void DestroyFood()
    {
        FoodController[] localFoods = FindObjectsOfType<FoodController>();
        foreach (FoodController food in localFoods)
        {
            Destroy(food.gameObject);
        }
    }

    public void SpawnFood()
    {
        for (int i = 0; i < foodCount; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-xRange, xRange+1), Random.Range(-yRange, yRange+1), 0);
            
            Vector3 tmp;
            Quaternion quat;
            transform.GetLocalPositionAndRotation(out tmp, out quat);
            pos += tmp;



            foods[i] = Instantiate(foodPrefab, pos, transform.rotation);
            foods[i].gameObject.GetComponent<FoodController>().foodManager = this;
        }
    }

    public void SpawnSignleFood()
    {
        if (foodCount>curAlive)
        {
            Vector3 pos = new Vector3(Random.Range(-xRange, xRange + 1), Random.Range(-yRange, yRange + 1), 0);
            Vector3 tmp;
            Quaternion quat;
            transform.GetLocalPositionAndRotation(out tmp, out quat);
            pos += tmp;

            GameObject localFood = Instantiate(foodPrefab, pos, transform.rotation);
            localFood.gameObject.GetComponent<FoodController>().foodManager = this;
        }
        
    }
}
