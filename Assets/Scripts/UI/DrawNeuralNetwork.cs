using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public class DrawNeuralNetwork : MonoBehaviour
{
    public NeatNetwork neuralNetwork;

    [SerializeField]
    private Sprite circleSprite;
    private RectTransform graphContainer;
    float nodeNumber = 0;
    Vector2 containerSize;
    float sphereSize;
    

    private void Awake()
    {
        //neuralNetwork = new NeatNetwork(NeatUtilities.LoadGenome());

    }
    private void Update()
    {
    }


    public void Plot()
    {
        ;
        if (neuralNetwork != null)
        {
            ResetGraph();
            graphContainer = transform.Find("RectContainer").GetComponent<RectTransform>();

            nodeNumber = neuralNetwork.nodes.Count * Mathf.PI;

            containerSize = graphContainer.sizeDelta;

            sphereSize = graphContainer.sizeDelta.x / (float)(nodeNumber/2);


            foreach (var item in neuralNetwork.nodes)
            {

                CreateCircle(GetNodePosition(item));
            }
            foreach (var item in neuralNetwork.connections)
            {
                if (item.isActive)
                {
                    CreateDotConnection(item);
                }
            }
        }
    }
    private Vector2 GetNodePosition(Node node)
    {
        return new Vector2(containerSize.x * node.x, -containerSize.y * node.y);
    }
    private void CreateCircle(Vector2 anchoredPosition)
    {
        GameObject gameObject = new GameObject("circle", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(sphereSize, sphereSize);
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
    }
    private void CreateDotConnection(Connection connection)
    {
        Node a = neuralNetwork.nodes.FirstOrDefault(t => t.id == connection.inputNode);
        Node b = neuralNetwork.nodes.FirstOrDefault(t => t.id == connection.outputNode);
        

        GameObject gameObject = new GameObject("connection",typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        //Getting the color: red>negative w | blue>positive w
        //float colorFloat = 0;
        //if (connection.weight>1)
        //{
        //    colorFloat = 1;
        //}
        //else if(connection.weight<-1) 
        //{
        //    colorFloat = -1;
        //}

        //Color tmpColor = new Color(127 - (colorFloat*127), 0, 127 + ( colorFloat*127));
        Color tmpColor;
        if (connection.weight<0)
        {
            tmpColor = new Color(255, 0, 0);
        }
        else if (connection.weight > 0)
        {
            tmpColor = new Color(0, 0, 255);
        }
        else
        {
            tmpColor = Color.green;
        }

        gameObject.GetComponent<Image>().color = tmpColor;

        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (GetNodePosition(b) - GetNodePosition(a)).normalized;
        float distance = Vector2.Distance(GetNodePosition(a), GetNodePosition(b));

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.sizeDelta = new Vector2(distance, Mathf.Abs(connection.weight)*3f);
        rectTransform.anchoredPosition = GetNodePosition(a) +(dir* distance*0.5f);
        
        rectTransform.localEulerAngles = new Vector3(0,0, GetAngleFromVectorFloat(dir));
        

    }

    private void ResetGraph()
    {
        if (transform.Find("RectContainer").childCount> 0)
        {
            foreach (Transform item in graphContainer.transform)
            {
                Destroy(item.gameObject);
            }

        }
        

    }
    private float GetAngleFromVectorFloat(Vector2 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) 
        {
            n += 360;
        }
        return n;
    }


}
