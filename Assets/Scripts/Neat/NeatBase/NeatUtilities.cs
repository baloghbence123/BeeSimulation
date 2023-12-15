using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class NeatUtilities : MonoBehaviour
{
    private static int InputNodes = 6;
    private static int OutputNodes = 2;
    private static int HiddenNodes = 2;
    public static System.Random random= new System.Random();
    public static float[] GetDisjointAndExcessTogether(List<ConGene> firstCons, List<ConGene> secondCons)
    {
        int disjointAndExcess =0, firstGenomeEnd = 0, secondGenomeEnd = 0;
        
        if (firstCons.Count != 0)
        {
            firstGenomeEnd = firstCons.Count; //2
        }

        if (secondCons.Count != 0)
        {
            secondGenomeEnd = secondCons.Count; //4
        }
        //1 x x x 5
        //1 2 3 4 x

        //Getting both connection's inovation numbers
        List<int> inovNumbers = new List<int>();
        foreach (var item in firstCons)
        {
            if (!inovNumbers.Contains(item.innovNum))
            {
                inovNumbers.Add(item.innovNum);
            }
        }
        foreach (var item in secondCons)
        {
            if (!inovNumbers.Contains(item.innovNum))
            {
                inovNumbers.Add(item.innovNum);
            }
        }
        List<float> weightDifference= new List<float>();

        foreach (int inov in inovNumbers)
        {
            var tmpCon1 = HasInov(firstCons, inov);
            var tmpCon2 = HasInov(secondCons, inov);

            if (tmpCon1 != null && tmpCon2 != null)
            {
                weightDifference.Add(Math.Abs(tmpCon1.weight - tmpCon2.weight));
            }
            else
            {
                disjointAndExcess += 1;
            }
            
        }

        float[] disJAndWeightDif = { disjointAndExcess, weightDifference.Count != 0 ? weightDifference.Average() :0 };

        return disJAndWeightDif;

    }

    private static ConGene HasInov(List<ConGene> connections, int searchInov)
    {
        foreach (ConGene con in connections)
        {
            if (con.innovNum == searchInov)
            {
                return con;
            }

        }
        return null;
    }

    public static NeatGenome CrossOver(NeatGenome genomeOne,NeatGenome genomeTwo)
    {
        NeatGenome retVal = new NeatGenome();
        genomeOne.conGenes = genomeOne.conGenes.OrderBy(t => t.innovNum).ToList();
        genomeTwo.conGenes = genomeTwo.conGenes.OrderBy(t => t.innovNum).ToList();

        int index_g1 = 0,index_g2 = 0;
        int g1_size=genomeOne.conGenes.Count;
        int g2_size=genomeTwo.conGenes.Count;

        while (index_g1<g1_size && index_g2<g2_size)
        {
            ConGene conOne = genomeOne.conGenes[index_g1];
            ConGene conTwo = genomeTwo.conGenes[index_g2];
            int inoOne = conOne.innovNum;
            int inoTwo = conTwo.innovNum;

            if (inoOne == inoTwo)  
            {
                if (UnityEngine.Random.Range(0.0f,1.0f)>0.5f)
                {
                    retVal.conGenes.Add(conOne);
                }
                else
                {
                    retVal.conGenes.Add(conTwo);
                }
                index_g1++;
                index_g2++;
            }
            else if (inoOne>inoTwo)
            {
                retVal.conGenes.Add(conTwo);
                index_g2++;
            }
            else
            {
                retVal.conGenes.Add(conOne);
                index_g1++;
            }
        }
        while (index_g1 < g1_size)
        {
            ConGene tmp = genomeOne.conGenes[index_g1];
            retVal.conGenes.Add(tmp);
            index_g1++;

        }
        while (index_g2<g2_size)
        {
            ConGene tmp = genomeTwo.conGenes[index_g2];
            retVal.conGenes.Add(tmp);
            index_g2++;
        }

        //the connections are ready. Now we need the nodes too. Which we can get easily from the congenes.
        retVal.nodeGenes.AddRange(genomeOne.nodeGenes.Where(t=>t.type == NodeGene.TYPE.Input));
        retVal.nodeGenes.AddRange(genomeOne.nodeGenes.Where(t => t.type == NodeGene.TYPE.Output));
        foreach (ConGene con in retVal.conGenes)
        {
            if (!retVal.nodeGenes.Any(t=>t.id == con.inputNode))
            {
                
                retVal.nodeGenes.Add(new NodeGene(con.inputNode, NodeGene.TYPE.Hidden));
            }
            if (!retVal.nodeGenes.Any(t => t.id == con.outputNode))
            {
                retVal.nodeGenes.Add(new NodeGene(con.outputNode, NodeGene.TYPE.Hidden));
            }
        }
        foreach (NodeGene node in retVal.nodeGenes)
        {
            if (node.type == NodeGene.TYPE.Hidden && node.y==0 && node.x==0)
            {
                var tmp = genomeOne.nodeGenes.FirstOrDefault(t => t.id == node.id);
                if (tmp != null)
                {
                    node.y = tmp.y;
                    node.x = tmp.x;
                }
                else
                {
                    tmp = genomeTwo.nodeGenes.FirstOrDefault(t => t.id == node.id);
                    if (tmp!=null)
                    {
                        node.y = tmp.y;
                        node.x = tmp.x;
                    }
                    
                }
            }
        }
        for (int i = 0; i < retVal.conGenes.Count; i++)
        {
            var con = retVal.conGenes[i];
            var firstnode = retVal.nodeGenes.FirstOrDefault(t => t.id == con.inputNode);
            var secondnode = retVal.nodeGenes.FirstOrDefault(t => t.id == con.outputNode);
            if (firstnode.x >= secondnode.x)
            {
                retVal.conGenes.Remove(con);
            }

        }

        return retVal;
    }



    public static void SaveGenome(NeatGenome genome)
    {
        NeatGenomeJson genomeJson = new NeatGenomeJson();
        foreach (NodeGene node in genome.nodeGenes)
        {
            NodeGeneJson nodeJson = new NodeGeneJson();
            nodeJson.id = node.id;
            nodeJson.x = node.x;
            nodeJson.y = node.y;
            nodeJson.type = (NodeGeneJson.TYPE)node.type;
            genomeJson.nodeGenes.Add(nodeJson);
        }
        foreach (ConGene con in genome.conGenes)
        {
            ConGeneJson conJson = new ConGeneJson();
            conJson.inputNode = con.inputNode;
            conJson.outputNode = con.outputNode;
            conJson.weight = con.weight;
            conJson.isActive = con.isActive;
            conJson.innovNum = con.innovNum;
            genomeJson.conGenes.Add(conJson);
        }

        string json = JsonUtility.ToJson(genomeJson);
        File.WriteAllText(Application.dataPath + "/save.txt", json);
        print(json);
    }

    public static NeatGenome LoadGenome()
    {
        string genomeString = File.ReadAllText(Application.dataPath + "/save.txt");
        NeatGenomeJson savedGenome = JsonUtility.FromJson<NeatGenomeJson>(genomeString);
        NeatGenome loadedGenome = new NeatGenome();
        foreach (NodeGeneJson savedNode in savedGenome.nodeGenes)
        {
            NodeGene newNode = new NodeGene(savedNode.id, (NodeGene.TYPE)savedNode.type,(float)savedNode.x, (float)savedNode.y);
            loadedGenome.nodeGenes.Add(newNode);
        }
        foreach (ConGeneJson savedCon in savedGenome.conGenes)
        {
            ConGene newCon = new ConGene(savedCon.inputNode, savedCon.outputNode, savedCon.weight, savedCon.isActive, savedCon.innovNum);
            loadedGenome.conGenes.Add(newCon);
        }

        return loadedGenome;
    }
}


[System.Serializable]
public class NeatGenomeJson
{
    public List<NodeGeneJson> nodeGenes = new List<NodeGeneJson>();
    public List<ConGeneJson> conGenes = new List<ConGeneJson>();
}

[System.Serializable]
public class NodeGeneJson
{
    public int id;
    public float x;
    public float y;
    public enum TYPE
    {
        Input, Output, Hidden
    };
    public TYPE type;
}

[System.Serializable]
public class ConGeneJson
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isActive;
    public int innovNum;
}


