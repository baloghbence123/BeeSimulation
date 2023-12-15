using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public static class SpawnUtilities
{
    // Start is called before the first frame update
    
    //leaving the first 10% of the given population
    public static float elite = 0.1f;
    //removing the first 10% of the given population and make some brand new ones
    public static float remove = 0.1f;

    public static int popNeeded = 40;

   
    
    



    public static List<NeatNetwork> OffspringCreation(List<BeeController> pop)
    {
        List<NeatNetwork> retList = new List<NeatNetwork>();
        var bees = pop.OrderByDescending(t=>t.overallFitness).ToList();
        float fitnessSum = FitnessSum(bees);
        
        retList.AddRange(bees.Select(t=>t.myNetwork).Take((int)System.Math.Round(bees.Count * elite)));
        var tmpNew = new List<NeatNetwork>();
        tmpNew.AddRange(Enumerable.Range(0, (int)System.Math.Round(bees.Count * remove)).Select(t => new NeatNetwork(6,2,0)));
        for (int i = 0; i < 20; i++)
        {
            tmpNew = tmpNew.Select(t => t.MutateInitialNetwork()).ToList();
        }
        retList.AddRange(tmpNew);
        while (popNeeded>retList.Count)
        {
            //rouletting the two parent
            float tmpCounter = 0;
            int k = 0;
            float randomGenNumberOne = Random.RandomRange(0, fitnessSum);
            while (tmpCounter < randomGenNumberOne)
            {
                tmpCounter += bees[k].overallFitness;
                k++;
            }

            tmpCounter = 0;
            int c = 0;
            float randomGenNumberTwo = Random.RandomRange(0, fitnessSum);
            while (tmpCounter < randomGenNumberTwo)
            {
                tmpCounter += bees[c].overallFitness;
                c++;
            }
            var tmpNetwork =new NeatNetwork(NeatUtilities.CrossOver(bees[k].myNetwork.myGenome, bees[c].myNetwork.myGenome));
            retList.Add(tmpNetwork);
            var mutatedTmpNetwork = new NeatNetwork(tmpNetwork.myGenome).MutateInitialNetwork();
            retList.Add(mutatedTmpNetwork);
        }





        return retList;
    }
    private static float FitnessSum(List<BeeController> pop) 
    {
        return pop.Sum(t => t.overallFitness);
    }
    public static NeatNetwork ChildCreation(List<GameObject> pop)
    {
        var bees = pop.Select(pop => pop.gameObject.GetComponent<BeeController>()).OrderByDescending(t => t.overallFitness).ToList();
        float fitnessSum = FitnessSum(bees);

        float tmpCounter = 0;
        int k = 0;
        float randomGenNumberOne = Random.RandomRange(0, fitnessSum);
        while (tmpCounter < randomGenNumberOne)
        {
            tmpCounter += bees[k].overallFitness;
            k++;
        }

        tmpCounter = 0;
        int c = 0;
        float randomGenNumberTwo = Random.RandomRange(0, fitnessSum);
        while (tmpCounter < randomGenNumberTwo)
        {
            tmpCounter += bees[c].overallFitness;
            c++;
        }
        var tmpNetwork = new NeatNetwork(NeatUtilities.CrossOver(bees[k].myNetwork.myGenome, bees[c].myNetwork.myGenome));

        return tmpNetwork;


    }



}
