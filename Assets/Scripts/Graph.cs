using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Graph : MonoBehaviour
{
    public class valuelist //holds the values for different lines
    {
        public List<float> Values = new List<float>();
        public float HighestValue = 1f;
    }
    public GameObject DrawOrigin; //Point where drawing starts
    public GameObject Board;//gameobject where things are drawn
    public Material LinerendererMaterial;
    public List<Color> Lines;
    

    private List<GameObject> Linerenderers = new List<GameObject>(); //List of gameobjects that have linerenderers to draw lines
    private List<valuelist> ListsOfValues = new List<valuelist>(); //Lists containing values for different lines
    
    
    private float width, height; //Self explanatory
    private float BiggestValue; // Used to scale the graph in y direction

    void Start()
    {
        width = Board.GetComponent<Renderer>().bounds.size.x; //get the plane width 
        height = Board.GetComponent<Renderer>().bounds.size.z; // get the plane height
        for (int i = 0; i < Lines.Count; i++) //Add empty gameobjects and Linerenderers to them
        {
            
            print("Adding linerenderer " + i);
            Linerenderers.Add(new GameObject("linerenderer")); //Instantiate empty gameobject
            Linerenderers[i].transform.localScale = new Vector3(1f, 1f, 1f); //Stop Invalid AABB aabb errors occuring
            Linerenderers[i].transform.parent = Board.transform; //Set the empty gameobject as a child to the board
            Linerenderers[i].transform.position = DrawOrigin.transform.position; //Set the linerenderers to correct position
            Linerenderers[i].AddComponent(typeof(LineRenderer)); //Add linerenderer to the empty gameobject
            Linerenderers[i].GetComponent<LineRenderer>().SetWidth(0.1f, 0.1f); //set line width
            //Material mat = new Material(Shader.Find("Unlit/Texture")); //Create material
            Linerenderers[i].GetComponent<LineRenderer>().material = LinerendererMaterial; //Assign the material
            Linerenderers[i].GetComponent<LineRenderer>().material.color = Lines[i]; //Add color from the editor to the line
            ListsOfValues.Add(new valuelist()); //Add valuelists
            
        }
    }

    public void AddValues(params float[] value)
    {
        if(value.Length != Lines.Count) //See if the right amount of values are passed to the graph
        {
            print("Incorrect amount of values passed to graph. Expected: " + Lines.Count + " Got: " + value.Length);
            return;
        }
        else
        {
            for (int i = 0; i < value.Length; i++) //Go through Lists of value lists and add values to the value list :)
            {
                //print(ListsOfValues[i].Values.Count);
                if (value[i] > ListsOfValues[i].HighestValue) ListsOfValues[i].HighestValue = value[i]; //Update the biggest value if necessary
                ListsOfValues[i].Values.Add((float)value[i]); //Add value to value list
            }
        }

    }

    public void Draw()
    {

        for (int i = 0; i < Lines.Count; i++) //Draw every line
        {
            float spacing = (width - 1)/ ListsOfValues[i].Values.Count; //Set spacing for the values
            float HeightMultiplier = ListsOfValues[i].HighestValue / (height - 1);
            Linerenderers[i].GetComponent<LineRenderer>().SetVertexCount(ListsOfValues[i].Values.Count); //Set the right amount of vertex' to linerenderer
            for (int j = 0; j < ListsOfValues[i].Values.Count; j++) //Draw line
            {
                //print(ListsOfValues[i].Values[j]);
                Linerenderers[i].GetComponent<LineRenderer>().SetPosition(j, new Vector3(DrawOrigin.transform.position.x + spacing * j, DrawOrigin.transform.position.y, DrawOrigin.transform.position.z + ListsOfValues[i].Values[j] / HeightMultiplier));
                //Linerenderers[i].GetComponent<LineRenderer>().SetPosition(j, new Vector3(DrawOrigin.transform.position.x + spacing * j, DrawOrigin.transform.position.y, DrawOrigin.transform.position.z + ListsOfValues[i].Values[j] / HeightMultiplier));
            }
        }
    }

}
