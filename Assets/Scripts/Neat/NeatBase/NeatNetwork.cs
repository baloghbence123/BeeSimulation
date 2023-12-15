using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NeatNetwork
{

    public NeatGenome myGenome; // this genome is the base of  the network generation
    public List<Node> nodes;
    public List<Node> inputNodes;
    public List<Node> outputNodes;
    public List<Node> hiddenNodes;
    public List<Connection> connections;
    public float fitness;

    public NeatNetwork(int inp, int outp, int hid)
    {
        myGenome = CreateInitialGenome(inp, outp, hid);
        nodes = new List<Node>();
        inputNodes = new List<Node>();
        outputNodes = new List<Node>();
        hiddenNodes = new List<Node>();
        connections = new List<Connection>();
        CreateNetwork();
    }

    public NeatNetwork(NeatGenome genome)
    {
        myGenome = new NeatGenome(genome.nodeGenes, genome.conGenes);
        nodes = new List<Node>();
        inputNodes = new List<Node>();
        outputNodes = new List<Node>();
        hiddenNodes = new List<Node>();
        connections = new List<Connection>();
        CreateNetwork();
    }
    #region Instanitate the NeatNetwork
    //auto called in ctor
    private NeatGenome CreateInitialGenome(int inp, int outp, int hid)
    {
        List<NodeGene> newNodeGenes = new List<NodeGene>();
        List<ConGene> newConGenes = new List<ConGene>();
        int nodeId = 0;
        float yPointInp = 1 / (float)(inp + 1);
        float yPointOutp = 1 / (float)(outp + 1);
        float yPointHid = 1 / (float)(hid + 1);

        for (int i = 0; i < inp; i++)
        {
            NodeGene newNodeGene = new NodeGene(nodeId, NodeGene.TYPE.Input,0.1f, yPointInp * (i+1));
            newNodeGenes.Add(newNodeGene);
            nodeId += 1;
        }

        for (int i = 0; i < outp; i++)
        {
            NodeGene newNodeGene = new NodeGene(nodeId, NodeGene.TYPE.Output,0.9f, yPointOutp * (i + 1));
            newNodeGenes.Add(newNodeGene);
            nodeId += 1;
        }

        for (int i = 0; i < hid; i++)
        {
            
            NodeGene newNodeGene= new NodeGene(nodeId, NodeGene.TYPE.Hidden, 0.5f, yPointHid * (i + 1));
            newNodeGenes.Add(newNodeGene);
            nodeId += 1;
        }

        NeatGenome newGenome = new NeatGenome(newNodeGenes, newConGenes);
        return newGenome;
    }

    //auto called in ctor
    private void CreateNetwork()
    {
        ResetNetwork();
        // Creation of Network Structure: Nodes
        foreach (NodeGene nodeGene in myGenome.nodeGenes)
        {
            Node newNode = new Node(nodeGene.id,nodeGene.x,nodeGene.y);
            this.nodes.Add(newNode);

            if (nodeGene.type == NodeGene.TYPE.Input)
            {
                this.inputNodes.Add(newNode);
            }
            else if (nodeGene.type == NodeGene.TYPE.Hidden)
            {
                this.hiddenNodes.Add(newNode);
            }
            else if (nodeGene.type == NodeGene.TYPE.Output)
            {
                this.outputNodes.Add(newNode);
            }
        }

        // Creation of Network Structure: Edges
        foreach (ConGene conGene in myGenome.conGenes)
        {
            if (conGene.isActive == true)
            {
                Connection newCon = new Connection(conGene.inputNode, conGene.outputNode, conGene.weight, conGene.isActive);
                connections.Add(newCon);
            }
        }

        // Creation of Network Structure: Node Neighbors
        foreach (Node node in nodes)
        {
            foreach (Connection con in connections)
            {
                if (con.inputNode == node.id)
                {
                    node.outputConnections.Add(con);
                }
                else if (con.outputNode == node.id)
                {
                    node.inputConnections.Add(con);
                }
            }
        }

        hiddenNodes = hiddenNodes.OrderBy(t=>t.x).ToList();
    }
    //auto called in ctor
    private void ResetNetwork()
    {
        nodes.Clear();
        inputNodes.Clear();
        outputNodes.Clear();
        hiddenNodes.Clear();
        connections.Clear();
    }
    #endregion
    //call it manually everytime when you want to mutate your network
    public NeatNetwork MutateInitialNetwork()
    {
        myGenome.MutateGenome();
        CreateNetwork();
        return this;
    }
    public NeatNetwork ReturnMutatedNetwork()
    {
        var tmpNetwork = new NeatNetwork(this.myGenome);
        tmpNetwork.MutateInitialNetwork();
        tmpNetwork.CreateNetwork();
        return tmpNetwork;
    }
    // Main Function for the Nn => call it manually when you want to generate output from input
    public float[] FeedForwardNetwork(float[] inputs)
    {
        float[] outputs = new float[outputNodes.Count];
        
        for (int i = 0; i < inputNodes.Count; i++)
        {
            inputNodes[i].SetInputNodeValue(inputs[i]);
            inputNodes[i].FeedForwardValue();
            inputNodes[i].value = 0;
        }
        for (int i = 0; i < hiddenNodes.Count; i++)
        {
            hiddenNodes[i].SetHiddenNodeValue();
            hiddenNodes[i].FeedForwardValue();
            hiddenNodes[i].value = 0;
        }
        for (int i = 0; i < outputNodes.Count; i++)
        {
            outputNodes[i].SetOutputNodeValue();
            outputs[i] = outputNodes[i].value;
            outputNodes[i].value = 0;
        }

        return outputs;
    }
}

public class Node
{
    public int id;
    public float value;
    public float x;
    public float y;
    public List<Connection> inputConnections;
    public List<Connection> outputConnections;


    public Node(int id, float x, float y)
    {
        this.id = id;
        this.x = x;
        this.y = y;
        inputConnections = new List<Connection>();
        outputConnections = new List<Connection>();
    }
    //The region below is about the activation functions and mainly some "helper" functions
    // to feedforward easily
    #region NeededToFeedForward
    public void SetInputNodeValue(float val)
    {
        //val = Sigmoid(val);
        this.value = val;
    }
    public void SetHiddenNodeValue()
    {
        float val = 0;
        foreach (Connection con in inputConnections)
        {
            val += (con.weight * con.inputNodeValue);
        }
        this.value = TanHMod1(val);
    }
    public void SetOutputNodeValue()
    {
        float val = 0;
        foreach (Connection con in inputConnections)
        {
            val += (con.weight * con.inputNodeValue);
        }
        this.value = TanHMod1(val);
    }

    public void FeedForwardValue()
    {
        foreach (Connection con in outputConnections)
        {
            con.inputNodeValue = this.value;
        }
    }

    // Activation Functons
    private float Sigmoid(float x)
    {
        return (1 / (1 + Mathf.Exp(-x)));
    }

    private float TanH(float x)
    {
        return ((2 / (1 + Mathf.Exp(-2 * x))) - 1);
    }

    private float TanHMod1(float x)
    {
        return ((2 / (1 + Mathf.Exp(-4 * x))) - 1);
    }
    #endregion
}

//needed connection class to make connections => WOW
public class Connection
{
    public int inputNode;
    public int outputNode;
    public float weight;
    public bool isActive;
    public float inputNodeValue;
    public Connection(int inNode, int outNode, float wei, bool active)
    {
        inputNode = inNode;
        outputNode = outNode;
        weight = wei;
        isActive = active;
    }
}
