using UnityEngine;
using System.Collections;

public class WormList {

        public float Fitness;
        public NeuronalNetwork Controller;
        public int FoodEaten;
        public Color WormColor;

        public WormList(float fitness, NeuronalNetwork controller,int foodeaten,Color color)
        {
            Controller = new NeuronalNetwork(controller);
            Fitness = fitness;
            FoodEaten = foodeaten;
            WormColor = color;
        }

}
