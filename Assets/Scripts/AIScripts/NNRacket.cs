using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Bson;
using UnityEngine;
using UnityEngine.UI;

public class NNRacket : Racket
{
    int currentAISave;
    int[] currentNNIndex;

    float fitnessValue;
    float lastYPos;

    bool flagFirstMove = true;
    int lives;
    int maxLives = 0;

    public Text fitnessText;
    float bestFitnessValue = 0;

    public List<List<List<List<float>>>> currentNN;

    public Transform ball;
    public AIControl aiControl;

    public GameObject ballObject;

    public NeuralNetworkController neuralNetworkController;

    void Awake()
    {
        aiControl = GameObject.Find("GameMaster").GetComponent<AIControl>();    
        neuralNetworkController = GameObject.Find("GameMaster").GetComponent<NeuralNetworkController>();  

        aiControl.SaveFile();
        currentAISave = aiControl.currentAISave;
        currentNNIndex = aiControl.AISaves[currentAISave].GiveNN();
        currentNN = aiControl.AISaves[currentAISave].GiveNNFromIndex(currentNNIndex);

        lives = maxLives;

    }

    public override void NextNN()
    {
        lives -= 1;

        if(lives <= 0)
        {
            fitnessValue += 0.1f;
            print("Fitness Value: " + fitnessValue);
            aiControl.AISaves[currentAISave].SetFitnessScore(currentNNIndex, fitnessValue * fitnessValue);

            if(fitnessValue >= bestFitnessValue)
            {
                bestFitnessValue = fitnessValue;
                fitnessText.text = bestFitnessValue.ToString();
            }

            fitnessValue = 0;
            flagFirstMove = true;

            currentNNIndex = aiControl.AISaves[currentAISave].GiveNextNN();
            currentNN = aiControl.AISaves[currentAISave].GiveNNFromIndex(currentNNIndex);

            gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0, 0);

            lives = maxLives;
        }

        
        ball.GetComponent<AITrainingBall>().Restart();
    }

    protected override void Movement()
    {
        Movement2();
        return;

        float[] moveAxesValue = neuralNetworkController.RunNN(currentNN, GetNNInputs(), NeuralNetworkController.ActivationFunctions.Sigmoid).ToArray();

        float yPos = (float)(6.486 * moveAxesValue[0] - 3.241);

        gameObject.transform.position = new Vector3(gameObject.transform.position.x, yPos, 0);

        if(flagFirstMove)
        {
            lastYPos = gameObject.transform.position.y;
            flagFirstMove = false;
        }
    }

    void Movement2()
    {
        float[] moveAxesValue = neuralNetworkController.RunNN(currentNN, GetNNInputs2(), NeuralNetworkController.ActivationFunctions.Sigmoid).ToArray();

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
        List<float> output = new List<float>
        {
            ballObject.transform.position.x,
            ballObject.transform.position.y,
            ballObject.GetComponent<AITrainingBall>().ballRb.velocity.x,
            ballObject.GetComponent<AITrainingBall>().ballRb.velocity.y
        };
        //print("new Inputs: "+ output[0] +" "+ output[1] +" "+ output[2] +" "+ output[3]);
        return output;
    }

    List<float> GetNNInputs2()
    {
        List<float> output = new List<float>
        {
            gameObject.transform.position.x,
            gameObject.transform.position.y,
            ballObject.transform.position.x,
            ballObject.transform.position.y,
            ballObject.GetComponent<AITrainingBall>().ballRb.velocity.x,
            ballObject.GetComponent<AITrainingBall>().ballRb.velocity.y
        };

        //print("new Inputs: "+ output[0] +" "+ output[1] +" "+ output[2] +" "+ output[3]);
        return output;
    }

    public override void HitBall()
    {
        float difficulty = Mathf.Clamp((float)(Math.Abs(gameObject.transform.position.y - lastYPos)/0.5), 0, 1);
        float accuracy = Mathf.Clamp((float)((2.5 - Math.Abs(gameObject.transform.position.y - ballObject.transform.position.y))/2.5), 0, 0.9f);

        fitnessValue += difficulty * accuracy + 0.1f;
    }
}
