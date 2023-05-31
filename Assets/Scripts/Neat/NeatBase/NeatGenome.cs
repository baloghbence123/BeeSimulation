using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NeatGenome
{
    public List<NodeGene> nodeGenes;
    public List<ConGene> conGenes;
    public static List<ConGene> innovConGene = new List<ConGene>();
    //Creating chances
    float createEdgeChance = 5f;
    float createNodeChance = 2f;

    //Weight mutation chances
    float randomWeightChance = 5f;
    float perturbWeightChance = 90f;

    //Weight mutation mutation multiplayer range => if multiplier is 1 then the range between -0.5 and 0.5 
    float perturbWeightMultiplier = 0.25f;

    public NeatGenome()
    {
        nodeGenes = new List<NodeGene>();
        conGenes = new List<ConGene>();
    }

    public NeatGenome(List<NodeGene> nodeGens, List<ConGene> conGens)
    {
        nodeGenes = nodeGens;
        conGenes = conGens;
    }

    public void MutateGenome()
    {

        float chanceEdge = UnityEngine.Random.Range(0f, 100f);
        float chanceNode = UnityEngine.Random.Range(0f, 100f);

        if (chanceNode <= createNodeChance)
        {
            // Create Random New Node
            AddRandomNode();
        }
        if (chanceEdge <= createEdgeChance)
        {
            // Create Random New Edge
            AddRandomConnection();
        }
        // Mutate The Weights
        MutateWeights();
    }

    private void AddRandomNode()
    {
        if (conGenes.Count != 0)
        {
            int randomCon = UnityEngine.Random.Range(0, conGenes.Count);
            ConGene mutatingCon = conGenes[randomCon];
            int firstNode = mutatingCon.inputNode;
            int secondNode = mutatingCon.outputNode;

            // Disable the mutating connection
            mutatingCon.isActive = false;

            int newId = GetNextNodeId();
            
            //these 2 lines provide the x nad y location to the new node
            float xPoint = (nodeGenes.FirstOrDefault(t => t.id == firstNode).x + nodeGenes.FirstOrDefault(t => t.id == secondNode).x) / 2;
            float yPoint = (nodeGenes.FirstOrDefault(t => t.id == firstNode).y + nodeGenes.FirstOrDefault(t => t.id == secondNode).y) / 2;

            NodeGene newNode = new NodeGene(newId, NodeGene.TYPE.Hidden,xPoint,yPoint);
            nodeGenes.Add(newNode);

            //int nextInovNum = GetNextInovNum();
            int nextInovNum = GettingInovNumber(firstNode,newNode.id);
            if (nextInovNum == -1) //no connection like that
            {
                innovConGene.Add(new ConGene(firstNode, newNode.id, 0, true, innovConGene.Count));//Adding the brand new connection to the static list

                ConGene firstNewCon = new ConGene(firstNode, newNode.id, 1f, true, innovConGene.Count);
                conGenes.Add(firstNewCon);
            }
            else //if there is a connection like that then the new one will get the same inovnumber
            {
                ConGene firstNewCon = new ConGene(firstNode, newNode.id, 1f, true, nextInovNum);
                conGenes.Add(firstNewCon);
            }

            //nextInovNum = GetNextInovNum();
            nextInovNum = GettingInovNumber(newNode.id, secondNode);
            if (nextInovNum == -1) //no connection like that
            {
                innovConGene.Add(new ConGene(newNode.id, secondNode, 0, true, innovConGene.Count)); //Adding the brand new connection to the static list

                ConGene secondNewCon = new ConGene(newNode.id, secondNode, mutatingCon.weight, true, innovConGene.Count);
                conGenes.Add(secondNewCon);
            }
            else //if there is a connection like that then the new one will get the same inovnumber
            {
                ConGene secondNewCon = new ConGene(newNode.id, secondNode, mutatingCon.weight, true, nextInovNum);
                conGenes.Add(secondNewCon);
            }



            
        }
    }

    private int GetNextNodeId()
    {
        int nextID = 0;
        foreach (NodeGene node in nodeGenes)
        {
            if (nextID <= node.id)
            {
                nextID = node.id;
            }
        }
        nextID = nextID + 1;
        return nextID;
    }
    private bool AddRandomConnection()
    {
        int firstNode = UnityEngine.Random.Range(0, nodeGenes.Count);
        int secondNode = UnityEngine.Random.Range(0, nodeGenes.Count);
        NodeGene.TYPE firstType = nodeGenes[firstNode].type;
        NodeGene.TYPE secondType = nodeGenes[secondNode].type;


        if ((firstType == secondType && firstType != NodeGene.TYPE.Hidden) || nodeGenes[firstNode].x == nodeGenes[secondNode].x)
        {
            return AddRandomConnection();
        }

        foreach (ConGene con in conGenes)
        {
            if ((firstNode == con.inputNode && secondNode == con.outputNode) ||
                (secondNode == con.inputNode && firstNode == con.outputNode))
            {
                return false;
            }
        }

        if (firstType == NodeGene.TYPE.Output || (firstType == NodeGene.TYPE.Hidden
            && secondType == NodeGene.TYPE.Input))
        {
            int tmp = firstNode;
            firstNode = secondNode;
            secondNode = tmp;

            firstType = nodeGenes[firstNode].type;
            secondType = nodeGenes[secondNode].type;
        }

        //this modification doesn't allow the connection to go backward. As a result there is no any loops or backward connection at all
        if ((nodeGenes[firstNode].x >= nodeGenes[secondNode].x))
        {
            return false;
        }
        //int innov = GetNextInovNum();
        float weight = UnityEngine.Random.Range(-1f, 1f);
        bool act = true;
        int inov = GettingInovNumber(firstNode, secondNode);
        
       
        if (inov == -1) //no connection like that
        {
            innovConGene.Add(new ConGene(firstNode, secondNode, 0, act, innovConGene.Count)); //Adding the brand new connection to the static list

            ConGene newCon = new ConGene(firstNode, secondNode, weight, act, innovConGene.Count);
            conGenes.Add(newCon);
        }
        else //if there is a connection like that then the new one will get the same inovnumber
        {
            ConGene newCon = new ConGene(firstNode, secondNode, weight, act, inov);
            conGenes.Add(newCon);
        }
        
        return true;
    }

    public int GettingInovNumber(int inputNode, int outputNode)
    {
        foreach (var item in innovConGene)
        {
            if (inputNode == item.inputNode && outputNode == item.outputNode)
            {
                return item.innovNum; //If somewhere  is this  kind of connection it will return its innovation number
            }
        }


        return -1; //if not then returns -1
    }

    private void MutateWeights()
    {

        float chanceRandom = UnityEngine.Random.Range(0f, 100f);
        float chancePerturb = UnityEngine.Random.Range(0f, 100f);

        if (chanceRandom <= randomWeightChance)
        {
            // Randomize Single Weight
            RandomizeSingleWeight();
        }
        if (chancePerturb <= perturbWeightChance)
        {
            // Perturb Group of Weight
            PerturbWeights();
        }
    }

    private void RandomizeSingleWeight()
    {
        if (conGenes.Count != 0)
        {
            int randomConIndex = UnityEngine.Random.Range(0, conGenes.Count);
            ConGene connection = conGenes[randomConIndex];
            connection.weight = UnityEngine.Random.Range(-1f, 1f);
        }
    }

    private void PerturbWeights()
    {
        foreach (ConGene con in conGenes)
        {
            con.weight = con.weight + (UnityEngine.Random.Range(-0.5f, 0.5f) * perturbWeightMultiplier);
        }
    }

}

public class NodeGene
{
    public int id;
    public float x;
    public float y;
    public enum TYPE
    {
        Input, Output, Hidden
    };
    public TYPE type;

    public NodeGene(int givenID, TYPE givenType)
    {
        id = givenID;
        type = givenType;
    }
    public NodeGene(int givenID, TYPE givenType, float x, float y)
    {
        this.id = givenID;
        this.type = givenType;
        this.x=x;
        this.y=y;
    }
}

public class ConGene
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isActive;
    public int innovNum;

    public ConGene(int inNode, int outNode, float wei, bool active, int inov)
    {
        inputNode = inNode;
        outputNode = outNode;
        weight = wei;
        isActive = active;
        innovNum = inov;
    }
}

