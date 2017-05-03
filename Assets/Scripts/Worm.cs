using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Worm : MonoBehaviour {

    public Rigidbody Head; //Head of the worm
    public Transform RaycastLocation;
    public float SeeingDistance = 60f;
    public LineRenderer Line;
    public Material LineMaterial;
    public Transform TailSpawnPoint;
    public GameObject Tail;
    public bool IsCannibal = false;

    private NeuronalNetwork Controller; //Brains of the worm
    private float Fitness = 0; //Fitness of the worm
    private int FoodEaten = 0;
    private float Energy = 4000f;
    private bool IsControllerSet = false; //Is the brain set
    private bool IsDead = false;
    private int SeeingLineIndex = 0; //Used to alterante drawing between seeing points

    //Seeing values
    private int ThingInfront = 0;
    private int ThingOnLeft1 = 0;
    private int ThingOnLeft2 = 0;
    private int ThingOnLeft3 = 0;
    private int ThinOnRight1 = 0;
    private int ThinOnRight2 = 0;
    private int ThinOnRight3 = 0;
    private float DisanceToLeft1 = 0f;
    private float DisanceToLeft2 = 0f;
    private float DisanceToLeft3 = 0f;
    private float DistanceToRight1 = 0f;
    private float DistanceToRight2 = 0f;
    private float DistanceToRight3 = 0f;

    private float DistanceToObject = -1f;
    //private List<GameObject> ListOfTails = new List<GameObject>();
    // Public functions 

    //Returns the Fitness value
    public float GetFitness() 
    {
        return Fitness;
    }

    //Returns the neural network controller of the worm
    public NeuronalNetwork GetNeuralNetwork()
    {
        return Controller;
    }

    //Install a brain
    public void SetNeuralNetwork(NeuronalNetwork NewController, Color color)
    {
        Controller = new NeuronalNetwork(NewController);
        IsControllerSet = true;
        SetColor(color);
    }

    void start ()
    {
        Head = GetComponent<Rigidbody>();
        //initialize sight line
        Line.SetWidth(0.02f, 0.02f);
        Line.material = LineMaterial;
        Line.SetVertexCount(2);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        if (IsControllerSet == true && IsDead == false)
        {
            UseBrain();
            Move();
        }
        if (Energy <= 0 && IsDead == false) DIE();
    }

    private void UseBrain()
    {
        See();
        Energy = Energy - 5f; //Thinking consumes energy
        Controller.SetInput(0, ThingInfront);
        Controller.SetInput(1, DistanceToObject);
        Controller.SetInput(2, ThingOnLeft1);
        Controller.SetInput(3, DisanceToLeft1);
        Controller.SetInput(4, ThingOnLeft2);
        Controller.SetInput(5, DisanceToLeft2);
        Controller.SetInput(6, ThingOnLeft3);
        Controller.SetInput(7, DisanceToLeft3);
        Controller.SetInput(8, ThinOnRight1);
        Controller.SetInput(9, DistanceToRight1);
        Controller.SetInput(10, ThinOnRight2);
        Controller.SetInput(11, DistanceToRight2);
        Controller.SetInput(12, ThinOnRight3);
        Controller.SetInput(13, DistanceToRight3);
        Controller.Calculate();
    }

    private void See()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        Vector3 Left1 = Quaternion.AngleAxis(-15, Vector3.up) * fwd;
        Vector3 Left2 = Quaternion.AngleAxis(-30, Vector3.up) * fwd;
        Vector3 Left3 = Quaternion.AngleAxis(-45, Vector3.up) * fwd;

        Vector3 Right1 = Quaternion.AngleAxis(15, Vector3.up) * fwd;
        Vector3 Right2 = Quaternion.AngleAxis(30, Vector3.up) * fwd;
        Vector3 Right3 = Quaternion.AngleAxis(45, Vector3.up) * fwd;
        SeeingLineIndex++;
        if (SeeingLineIndex > 6) SeeingLineIndex = 0;
        //Look forward
        CastRays(fwd, 0);
        //Look left
        CastRays(Left1,1);
        CastRays(Left2, 2);
        CastRays(Left3, 3);

        //Look Right
        CastRays(Right1, 4);
        CastRays(Right2, 5);
        CastRays(Right3, 6);
        SeeingLineIndex++;

    }

    void CastRays (Vector3 Direction,int index)
    {
        RaycastHit hit;

        if (Physics.Raycast(RaycastLocation.position, Direction, out hit, SeeingDistance))
        {
            //bug.DrawLine(RaycastLocation.position, hit.point,Color.red);
            //Draw seeing line
            //Line.SetPosition(0, RaycastLocation.position);
            //Line.SetPosition(1, hit.point);
            if(IsCannibal == false) { 
                switch (index)
                {
                    case 0:
                        if(SeeingLineIndex == 0)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Food")
                        {
                            Fitness = Fitness + 0.1f;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Worm") ThingInfront = -1;
                        if (hit.transform.tag == "Wall") ThingInfront = -2;
                        if (hit.transform.tag == "WormEater") ThingInfront = -3;
                        DistanceToObject = hit.distance;
                        break;
                    case 1:
                        if (SeeingLineIndex == 1)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Food")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Worm") ThingOnLeft1 = -1;
                        if (hit.transform.tag == "Wall") ThingOnLeft1 = -2;
                        if (hit.transform.tag == "WormEater") ThingOnLeft1 = -3;
                        DisanceToLeft1 = hit.distance;
                        break;
                    case 2:
                        if (SeeingLineIndex == 2)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Food")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Worm") ThingOnLeft2 = -1;
                        if (hit.transform.tag == "Wall") ThingOnLeft2 = -2;
                        if (hit.transform.tag == "WormEater") ThingOnLeft2 = -3;
                        DisanceToLeft2 = hit.distance;
                        break;
                    case 3:
                        if (SeeingLineIndex == 3)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Food")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Worm") ThingOnLeft3 = -1;
                        if (hit.transform.tag == "Wall") ThingOnLeft3 = -2;
                        if (hit.transform.tag == "WormEater") ThingOnLeft3 = -3;
                        DisanceToLeft3 = hit.distance;
                        break;
                    case 4:
                        if (SeeingLineIndex == 4)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Food")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Worm") ThinOnRight1 = -1;
                        if (hit.transform.tag == "Wall") ThinOnRight1 = -2;
                        if (hit.transform.tag == "WormEater") ThinOnRight1 = -3;
                        DistanceToRight1 = hit.distance;
                        break;
                    case 5:
                        if (SeeingLineIndex == 5)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Food")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Worm") ThinOnRight2 = -1;
                        if (hit.transform.tag == "Wall") ThinOnRight2 = -2;
                        if (hit.transform.tag == "WormEater") ThinOnRight2 = -3;
                        DistanceToRight2 = hit.distance;
                        break;
                    case 6:
                        if (SeeingLineIndex == 6)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Food")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Worm") ThinOnRight3 = -1;
                        if (hit.transform.tag == "Wall") ThinOnRight3 = -2;
                        if (hit.transform.tag == "WormEater") ThinOnRight3 = -3;
                        DistanceToRight3 = hit.distance;
                        break;
                }
            }

            if (IsCannibal == true)
            {
                switch (index)
                {
                    case 0:
                        if (SeeingLineIndex == 0)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Worm")
                        {
                            Fitness = Fitness + 0.1f;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Food") ThingInfront = -1;
                        if (hit.transform.tag == "Wall") ThingInfront = -2;
                        if (hit.transform.tag == "WormEater") ThingInfront = -3;
                        DistanceToObject = hit.distance;
                        break;
                    case 1:
                        if (SeeingLineIndex == 1)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Worm")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Food") ThingOnLeft1 = -1;
                        if (hit.transform.tag == "Wall") ThingOnLeft1 = -2;
                        if (hit.transform.tag == "WormEater") ThingOnLeft1 = -3;
                        DisanceToLeft1 = hit.distance;
                        break;
                    case 2:
                        if (SeeingLineIndex == 2)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Worm")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Food") ThingOnLeft2 = -1;
                        if (hit.transform.tag == "Wall") ThingOnLeft2 = -2;
                        if (hit.transform.tag == "WormEater") ThingOnLeft2 = -3;
                        DisanceToLeft2 = hit.distance;
                        break;
                    case 3:
                        if (SeeingLineIndex == 3)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Worm")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Food") ThingOnLeft3 = -1;
                        if (hit.transform.tag == "Wall") ThingOnLeft3 = -2;
                        if (hit.transform.tag == "WormEater") ThingOnLeft3 = -3;
                        DisanceToLeft3 = hit.distance;
                        break;
                    case 4:
                        if (SeeingLineIndex == 4)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Worm")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Food") ThinOnRight1 = -1;
                        if (hit.transform.tag == "Wall") ThinOnRight1 = -2;
                        if (hit.transform.tag == "WormEater") ThinOnRight1 = -3;
                        DistanceToRight1 = hit.distance;
                        break;
                    case 5:
                        if (SeeingLineIndex == 5)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Worm")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Food") ThinOnRight2 = -1;
                        if (hit.transform.tag == "Wall") ThinOnRight2 = -2;
                        if (hit.transform.tag == "WormEater") ThinOnRight2 = -3;
                        DistanceToRight2 = hit.distance;
                        break;
                    case 6:
                        if (SeeingLineIndex == 6)
                        {
                            Line.SetPosition(0, RaycastLocation.position);
                            Line.SetPosition(1, hit.point);
                        }
                        if (hit.transform.tag == "Worm")
                        {
                            //Fitness++;
                            ThingInfront = 1;
                        }
                        if (hit.transform.tag == "Food") ThinOnRight3 = -1;
                        if (hit.transform.tag == "Wall") ThinOnRight3 = -2;
                        if (hit.transform.tag == "WormEater") ThinOnRight3 = -3;
                        DistanceToRight3 = hit.distance;
                        break;
                }
            }
            /**********************************************************/
        }
        else
        {
            ThingInfront = 0;
            ThingOnLeft1 = 0;
            ThingOnLeft2 = 0;
            ThingOnLeft3 = 0;
            ThinOnRight1 = 0;
            ThinOnRight2 = 0;
            ThinOnRight3 = 0;
            DistanceToObject = -1f;
            DisanceToLeft1 = -1f;
            DisanceToLeft2 = -1f;
            DisanceToLeft3 = -1f;
            DistanceToRight1 = -1f;
            DistanceToRight2 = -1f;
            DistanceToRight3 = -1f;
            Line.SetPosition(0, RaycastLocation.position);
            Line.SetPosition(1, RaycastLocation.position);
        }


    }

    private void Move()
    {
        //Slow down the worm.
        Head.velocity = Head.velocity * 0.8f;
        Head.angularVelocity = Head.angularVelocity * 0.8f;

        //Add forces from cotroller
        float forward = Controller.GetOutput(0);
        float rotate = Controller.GetOutput(1);
        if (forward < -20f) forward = -20f;
        Head.AddForce(transform.forward * forward);
        Head.AddTorque(transform.up * rotate);

        //Keep the guy top up
        Vector3 temp = transform.eulerAngles;
        temp.x = 0f;
        temp.z = 0f;
        transform.eulerAngles = temp;

        //        Head.AddForce(transform.forward * Controller.GetOutput(0));
        //        Head.AddTorque(transform.up * Controller.GetOutput(1));
    }

    private void DIE()
    {

        //print("Deaed");
        //print(Fitness);
        Head.constraints = RigidbodyConstraints.FreezeAll;

        Head.velocity = new Vector3(0,0,0);
        Head.angularVelocity = new Vector3(0, 0, 0);
        IsDead = true;
        //Move the worm out of the playfield when dead
        transform.position = transform.position + new Vector3(transform.position.x + 110f, transform.position.y, transform.position.z + 110f);
        GameObject master = GameObject.FindGameObjectWithTag("GameMaster");
        master.GetComponent<GameMaster>().DyingWormsLastWishes(Controller, Fitness, this,FoodEaten, gameObject.GetComponentInChildren<Renderer>().material.color,IsCannibal);
    }

    void OnCollisionEnter(Collision collision)
    {
        //Eat food if not cannibal
        if (collision.gameObject.tag == "Food" && IsCannibal == false && IsDead == false)
        {
            Energy = Energy + 1000;
            Fitness = Fitness + 30f;
            FoodEaten++;
        }

        //If cannibal, get a energy from food bot do not gain fitness
        if (collision.gameObject.tag == "Food" && IsCannibal == false && IsDead == false)
        {
            Energy = Energy + 1000;
            FoodEaten++;
        }
        //Eat worm if cannibal
        if (collision.gameObject.tag == "Worm" && IsCannibal == true && IsDead == false)
        {
            Energy = Energy + 3000;
            Fitness = Fitness + 30f;
            FoodEaten++;
        }

        //Die if not cannibal and cannibal worm hits this worm
        if (collision.gameObject.tag == "WormEater" && IsCannibal == false && IsDead == false)
        {
            DIE();
        }

        //Die if worm hits wall.
        if (collision.gameObject.tag == "Wall" && IsDead == false)
        {
            DIE();
        }
    }

    private void SetColor(Color color)
    {
        gameObject.GetComponentInChildren<Renderer>().material.color = color;
        gameObject.GetComponentInChildren<TrailRenderer>().material.color = color;
    }
}
