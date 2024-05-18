using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NNRacketVsHuman : Racket
{
    List<List<List<List<float>>>> NN;

    public AIControl aiControl;
    public NeuralNetworkController neuralNetworkController;
    public GameObject ballObject;

    // Start is called before the first frame update
    void Awake()
    {
        aiControl = GameObject.Find("GameMaster").GetComponent<AIControl>();    
        neuralNetworkController = GameObject.Find("GameMaster").GetComponent<NeuralNetworkController>();  

        NN = aiControl.SetUpRacket();
        print(NN[0][0][0][1]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

protected override void Movement()
    {
        Movement2();
        return;

        float[] moveAxesValue = neuralNetworkController.RunNN(NN, GetNNInputs(), NeuralNetworkController.ActivationFunctions.Sigmoid).ToArray();

        float yPos = (float)(6.486 * moveAxesValue[0] - 3.241);

        gameObject.transform.position = new Vector3(gameObject.transform.position.x, yPos, 0);
    }

    void Movement2()
    {
        float[] moveAxesValue = neuralNetworkController.RunNN(NN, GetNNInputs2(), NeuralNetworkController.ActivationFunctions.Sigmoid).ToArray();

        if (moveAxesValue[0] < 0.4)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 1) * moveSpeed;
            return;
        }
        if (moveAxesValue[0] < 0.6)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1) * moveSpeed;
            return;
        }

        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

    }

    List<float> GetNNInputs()
    {
        //print("new Inputs: "+ output[0] +" "+ output[1] +" "+ output[2] +" "+ output[3]);
        return new List<float>
        {
            ballObject.transform.position.x,
            ballObject.transform.position.y,
            ballObject.GetComponent<Ball>().ballRb.velocity.x,
            ballObject.GetComponent<Ball>().ballRb.velocity.y
        };
    }

    List<float> GetNNInputs2()
    {
        return new List<float>
        {
            gameObject.transform.position.x,
            gameObject.transform.position.y,
            ballObject.transform.position.x,
            ballObject.transform.position.y,
            ballObject.GetComponent<Ball>().ballRb.velocity.x,
            ballObject.GetComponent<Ball>().ballRb.velocity.y
        };
    }
    public override void NextNN()
    {}

    public override void HitBall()
    {}
}
