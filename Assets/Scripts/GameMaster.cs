using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour {

    public GameObject Worm;
    public GameObject WormEater;
    public GameObject Foods;
    public float FeedingInterval = 1f;
    public int AmountOfFood = 80;
    public int AmountOfWorms = 20;
    public int AmountOfWormEaters = 20;
    public int PossibilityForMutation = 50;
    public int CutoffForMating = 10;
    public bool UseGenerations = false;
    public bool ContinousFoodSupply = true;
    public float GraphUpdateInterval = 1f;
    public GameObject GeneralInformationBoard;

    //Things that are in the UI
    public Text BestBrainValues;
    public Dropdown TimescaleDropdown;

    private int NumberOfAliveWorms = 0;
    private int NumberOfAliveWormEaters = 0;
    private int Generation = 0;
    private int FoodLeft = 0;
    private int TotalFoodEaten = 0;
    //List of Worm fitness and controllers
    private List<GameObject> ListOfWorms = new List<GameObject>();
    private List<GameObject> ListOfWormEaters = new List<GameObject>();
    private List<WormList> ListOfBestWormBrains = new List<WormList>();
    private List<WormList> ListOfBestWormEaterBrains = new List<WormList>();
    private List<WormList> MatingPool = new List<WormList>();
    private List<GameObject> FoodList = new List<GameObject>();
    private float FoodSpawnTimer = 0f;
    private float GraphUpdateTimer = 0f;
    private int NumberOfWormExtinctions = 0;
    private int NumberOfWormEaterExtinctions = 0;
    //Counters for continous mode
    private int NumberOfWormsAliveAfterExtinction = 0;
    private int NumberOfWormEatersAliveAfterExtinction = 0;

    //ValueList
    //private List<Values> valuelist = new List<Values>();

    //WormColorList
    private List<Color> ListWormStartupColors = new List<Color>();

    //GameControls
    

    // Use this for initialization
    void Start () {
        GenerateWormColors();
        GenerateWorms(AmountOfWorms);
        GenerateWormEaters(AmountOfWormEaters);
        AddControllersToWorms(false);
        AddControllersToWormeaters(false);
        AddFood(AmountOfFood);
        NumberOfAliveWorms = AmountOfWorms;
        NumberOfAliveWormEaters = AmountOfWormEaters;
        FoodLeft = AmountOfFood;

    }
	
	// Update is called once per frame
	void Update () {
        FoodSpawnTimer = FoodSpawnTimer + Time.deltaTime;
        if (FeedingInterval < FoodSpawnTimer && ContinousFoodSupply == true)
        {   
            AddFood(1);
            FoodLeft++;
            FoodSpawnTimer = 0;
            if(ListOfWorms.Count == 0)//IF all the worms are dead, revive
            {
                NumberOfWormExtinctions++; //Number of extinctions that have happende
                NumberOfWormsAliveAfterExtinction = 0;
                GenerateWorms(AmountOfWorms);
                AddControllersToWorms(false);
                NumberOfAliveWorms = AmountOfWorms;
            }
            if (ListOfWormEaters.Count == 0)//IF all the wormeaters are dead, revive
            {
                NumberOfWormEaterExtinctions++; //Number of extinctions that have happende
                NumberOfWormEatersAliveAfterExtinction = 0;
                GenerateWormEaters(AmountOfWormEaters);
                AddControllersToWormeaters(false);
            }
        }

        if (ListOfBestWormBrains.Count == AmountOfWorms && UseGenerations == true)
        {
            if(ContinousFoodSupply == false) {
                for (int i = 0; i < FoodList.Count; i++)
                {
                    GameObject thing = FoodList[i];
                    Destroy(thing);
                
                FoodList.Clear();

                AddFood(AmountOfFood);
                FoodLeft = AmountOfFood;
                }
            }
            NewGeneration();
        }

        //GraphUpdating

        GraphUpdateTimer = GraphUpdateTimer + Time.deltaTime;
        if (GraphUpdateTimer > GraphUpdateInterval)
        {
            GraphUpdateTimer = 0f;
            BestBrainValues.text = "Number of Worm extinctions: " + NumberOfWormExtinctions + "\nAlive worms: " + ListOfWorms.Count + "\nWorms that have lived since last extinction: " + NumberOfWormsAliveAfterExtinction + "\n\nWorm Eater extinctions: " + NumberOfWormEaterExtinctions + "\nAlive worm eaters: "+ ListOfWormEaters.Count + "\nWorm Eaters that have lived since last extinction: " + NumberOfWormEatersAliveAfterExtinction + "\n\nFood available: " + FoodList.Count;
            //valuelist.Add(new Values(NumberOfWormExtinctions, FoodLeft, NumberOfAliveWorms));
            GeneralInformationBoard.GetComponent<Graph>().AddValues(NumberOfWormExtinctions, FoodList.Count, ListOfWorms.Count,ListOfWormEaters.Count);
            GeneralInformationBoard.GetComponent<Graph>().Draw();
            
        }


    }


    private void NewGeneration()
    {
        KillAllWorms();
        Generation ++;
        GenerateMatingPool();

        //Print the values
        
        BestBrainValues.text ="Generation: "+ Generation + "\nTotal food Eaten last generation: " + TotalFoodEaten +"\n" +"Best Brain Fitnesses:\n";
        TotalFoodEaten = 0;
        for (int i = 0; i < ListOfBestWormBrains.Count; i++)
        {

            BestBrainValues.text += "Brain " + i + ": Fitness: " + ListOfBestWormBrains[i].Fitness.ToString("F1") + " Food Eaten: " + ListOfBestWormBrains[i].FoodEaten + "\n";
        }
        //Update information
        ListOfBestWormBrains.Clear(); // empty the list containing the best brains
        MutateControllers(); //Mutate Best controllers
        GenerateWorms(AmountOfWorms); //Generate new generation
        AddControllersToWorms(true); //Add controllers to the new worms
        MatingPool.Clear();
    }

    private void GenerateMatingPool()
    {
        for (int i = 0; i < CutoffForMating; i++)
        {
            for (int j = 0; j < CutoffForMating - i; j++)
            {
                MatingPool.Add(ListOfBestWormBrains[i]);
            }
        }
        
    }

    private void MutateControllers()
    {
        int RandomNumber = 0;
        for (int i = 0; i < ListOfBestWormBrains.Count; i++)
        {
            RandomNumber = Random.Range(0, 100);
            if(PossibilityForMutation >= RandomNumber)
            {
                MatingPool[i].Controller.Mutate(1);
            }
        }
    }

    private void GenerateWorms(int Amount)
    {
        for(int i = 0; i < Amount; i++) { 
        ListOfWorms.Add((GameObject)Instantiate(Worm, new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)),Quaternion.identity));
        }
    }

    private void GenerateWormEaters(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            ListOfWormEaters.Add((GameObject)Instantiate(WormEater, new Vector3(Random.Range(-30, 30), 0, Random.Range(-30, 30)), Quaternion.identity));
        }
    }

    private void AddFood(int Amount)
    {
        for (int i = 0; i < Amount; i++)
        {
            FoodList.Add((GameObject) Instantiate(Foods, new Vector3(Random.Range(-40, 40), -.5f, Random.Range(-40, 40)), Quaternion.identity));
        }
    }

    private void KillAllWorms()
    {
        for (int i = 0; i < ListOfWorms.Count; i++)
        {
            GameObject thing = ListOfWorms[i];
            Destroy(thing);
        }
        ListOfWorms.Clear();
    }

    private void AddControllersToWorms(bool UseExisting)
    {
        if(UseExisting == false)
        {

            for (int i = 0; i < ListOfWorms.Count; i++)
            {
                ListOfWorms[i].GetComponent<Worm>().SetNeuralNetwork(new NeuronalNetwork(14, 2, 10,10,10,10,10), ListWormStartupColors[i]);

            }
        }
        if (UseExisting == true)
        {
            int RandomNumber = 0;
            for (int i = 0; i < ListOfWorms.Count; i++) {
                RandomNumber = Random.Range(0, MatingPool.Count - 1);
                ListOfWorms[i].GetComponent<Worm>().SetNeuralNetwork(MatingPool[RandomNumber].Controller, MatingPool[RandomNumber].WormColor);
            }
        }
    }

    private void AddControllersToWormeaters(bool UseExisting)
    {
        if (UseExisting == false)
        {

            for (int i = 0; i < ListOfWormEaters.Count; i++)
            {
                ListOfWormEaters[i].GetComponent<Worm>().SetNeuralNetwork(new NeuronalNetwork(14, 2, 10, 10, 10, 10, 10), ListWormStartupColors[i]);

            }
        }
        if (UseExisting == true)
        {
            int RandomNumber = 0;
            for (int i = 0; i < ListOfWormEaters.Count; i++)
            {
                RandomNumber = Random.Range(0, MatingPool.Count - 1);
                ListOfWormEaters[i].GetComponent<Worm>().SetNeuralNetwork(MatingPool[RandomNumber].Controller, MatingPool[RandomNumber].WormColor);
            }
        }
    }

    //Public things

    public void DyingWormsLastWishes(NeuronalNetwork controller, float Fitness, Worm wormtoremove, int foodeaten,Color wormcolor,bool IsCannibal)
    {
        //Add worm controller to best list
        if(IsCannibal == false) { 
            ListOfBestWormBrains.Add(new WormList(Fitness, controller,foodeaten,wormcolor));
            NumberOfAliveWorms--;
            //Sort list from best to worst
            if (ListOfBestWormBrains.Count > 2)
            {
                ListOfBestWormBrains.Sort(delegate (WormList  b1, WormList b2) { return b1.Fitness.CompareTo(b2.Fitness); });
                ListOfBestWormBrains.Reverse();
            }
            if (ListOfBestWormBrains.Count > 100) ListOfBestWormBrains.RemoveAt(ListOfBestWormBrains.Count - 1); //Keep 100 best brains in safe;
        }
        //Add best worm eater controller to list containing the best ones
        if (IsCannibal == true) { 
            ListOfBestWormEaterBrains.Add(new WormList(Fitness, controller, foodeaten, wormcolor));
            if (ListOfBestWormEaterBrains.Count > 2)
            {
                ListOfBestWormEaterBrains.Sort(delegate (WormList b1, WormList b2) { return b1.Fitness.CompareTo(b2.Fitness); });
                ListOfBestWormEaterBrains.Reverse();
            }
            if (ListOfBestWormEaterBrains.Count > 100) ListOfBestWormEaterBrains.RemoveAt(ListOfBestWormEaterBrains.Count - 1); //Keep 100 best brains in safe;
        }
        //Do the things to keep worms alive
        if (UseGenerations == false && IsCannibal == false) {
            NumberOfWormsAliveAfterExtinction++;
            if(foodeaten >= 4)
            {
                int NumberToSpawn = foodeaten / 4;
                //if (NumberToSpawn > 4) NumberToSpawn = 4;
                //print(NumberToSpawn);
                if (NumberOfAliveWorms <= AmountOfWorms) { 
                    for (int i = 0; i < NumberToSpawn; i++)
                    {
                      NumberOfAliveWorms++;
                      GenerateWorms(1);
                      controller.Mutate(1);
                      ListOfWorms[ListOfWorms.Count - 1].GetComponent<Worm>().SetNeuralNetwork(controller, wormcolor);
                    }
                }
            }
            ListOfWorms.Remove(wormtoremove.gameObject);
            Destroy(wormtoremove.gameObject);
        }

        //Do things to keep wormeaters alive
        if (UseGenerations == false && IsCannibal == true)
        {
            NumberOfWormEatersAliveAfterExtinction++;
            if (foodeaten >= 2)
            {
                int NumberToSpawn = foodeaten / 2;
                //if (NumberToSpawn > 3) NumberToSpawn = 3;
                //print(NumberToSpawn);

                    for (int i = 0; i < NumberToSpawn; i++)
                    {
                        //NumberOfAliveWorm
                        GenerateWormEaters(1);
                        controller.Mutate(1);
                        ListOfWormEaters[ListOfWormEaters.Count - 1].GetComponent<Worm>().SetNeuralNetwork(controller, wormcolor);
                    }

            }
            ListOfWormEaters.Remove(wormtoremove.gameObject);
            Destroy(wormtoremove.gameObject);
        }

    }

    public void MinusFood(Food food)
    {
        FoodList.Remove(food.gameObject);
        Destroy(food.gameObject);
        FoodLeft--;
        TotalFoodEaten++;
    }


    //UI Control functions
    public void SetTimeScale()
    {
        switch (TimescaleDropdown.value) { 
            case 0:
                Time.timeScale = 1f;
                break;
            case 1:
                Time.timeScale = 2f;
                break;
            case 2:
                Time.timeScale = 4f;
                break;
            case 3:
                Time.timeScale = 8f;
                break;
            case 4:
                Time.timeScale = 16f;
                break;
            case 5:
                Time.timeScale = 32f;
                break;
        }
    }

    private void GenerateWormColors()
    {
        float Multiplier = 1f / (float)AmountOfWorms / 6f;
        int divider = AmountOfWorms / 3;
        for (int i = 0; i < AmountOfWorms; i++)
        {
            if (i <= divider)ListWormStartupColors.Add(new Color(Multiplier * i + .5f,0,0,1));
            if (i < divider * 2 && i >= divider) ListWormStartupColors.Add(new Color(0, Multiplier * i + .5f, 0, 1));
            if (i >= divider * 2) ListWormStartupColors.Add(new Color(0, 0, Multiplier * i + .5f, 1));

        }
    }
}
