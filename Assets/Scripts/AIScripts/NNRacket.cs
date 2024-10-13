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
    List<List<List<List<float>>>> policyNN = new List<List<List<List<float>>>>();
    List<List<List<List<float>>>> valueNN = new List<List<List<List<float>>>>();

    List<List<List<List<float>>>> policyDerivatives = new List<List<List<List<float>>>>();
    List<List<List<List<float>>>> valueDerivatives = new List<List<List<List<float>>>>();

    List<List<List<float>>> lastCalculation = new List<List<List<float>>>();

    List<List<List<float>>> leftLastPolicyCalculation = new List<List<List<float>>>();
    List<List<List<float>>> leftLastValueCalculation = new List<List<List<float>>>();

    float decayRate;
    float policyLearningRate;
    float valueLearningRate;

    float lastStateValue;
    float lastOutputProbability;
    Vector2 lastBallPosition;

    int lastAction;

    float reward = 0;
    int currentAISave;

    public Text fitnessText;
    float bestFitnessValue = 0;

    public Transform ball;
    public AIControl aiControl;

    public GameObject ballObject;

    public NeuralNetworkController neuralNetworkController;
    
    bool skipOneFrameTraining = true;

    int saveTime = 1000000;
    int saveTimer = 0;

    float validationHits;
    float validationMisses;
    bool displayDebugInfo;

    bool terminalState;

    int batchsize = 100;
    int batchCount;

    void Awake()
    {
        aiControl = GameObject.Find("GameMaster").GetComponent<AIControl>();    
        neuralNetworkController = GameObject.Find("GameMaster").GetComponent<NeuralNetworkController>();  

        aiControl.SaveFile();
        currentAISave = aiControl.currentAISave;

        decayRate = aiControl.AISaves[currentAISave].decayRate;
        policyLearningRate = aiControl.AISaves[currentAISave].policyLearningRate;
        valueLearningRate = aiControl.AISaves[currentAISave].valueLearningRate;

        policyNN = aiControl.AISaves[currentAISave].policyNN;
        valueNN = aiControl.AISaves[currentAISave].valueNN;
        lastBallPosition = ballObject.transform.position;

        displayDebugInfo = aiControl.displayDebugInfo;
    }

    protected override void Movement()
    {
        Train();

        var output = neuralNetworkController.RunNNAndSave(policyNN, GetNNInputs(), false);
        var outputValues = output.output;  
        leftLastPolicyCalculation = output.calculations;

        var outputArray = SoftMaxFunction(outputValues);
        double x = UnityEngine.Random.value;
        int chosenAction = -1;
        while(x >= 0)
        {
            chosenAction ++;
            x -= outputArray[chosenAction];
        }
        lastAction = chosenAction;
        lastOutputProbability = (float)outputArray[chosenAction];

        if(Vector2.Distance(ballObject.transform.position, lastBallPosition) > 1)
        {
            skipOneFrameTraining = true;
        }

        lastBallPosition = ballObject.transform.position;
        if(displayDebugInfo)
        {
            print(outputValues[0]+ " " + outputValues[1]);
            print("Probabilities: " + outputArray[0] + " " + outputArray[1] + " " + chosenAction);
        }

        if (chosenAction == 0)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 1) * moveSpeed;
            return;
        }
        if (chosenAction == 1)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1) * moveSpeed;
            return;
        }

        GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);

    }

    private void Train()
    {

        var state = neuralNetworkController.RunNNAndSave(valueNN, GetNNInputs(), false);
        float stateValue = state.output[0];
        if(terminalState)
        {
            stateValue = 0;
        }
        float advantage = 0.999f * stateValue - lastStateValue + Reward();

        if(displayDebugInfo && !skipOneFrameTraining)
        {
            print("Advantage: " + advantage + " " + stateValue + " " + lastStateValue + " " + lastAction);
        }
        else if(skipOneFrameTraining && displayDebugInfo)
        {
            print("Skip");
        }

        if(!skipOneFrameTraining)
        {
            policyDerivatives.Add(neuralNetworkController.SetPartialDerivatives(policyNN, leftLastPolicyCalculation, false, lastAction, true));
            policyDerivatives.LastOrDefault().LastOrDefault().LastOrDefault().Add((float)(policyLearningRate * (advantage/Math.Clamp(lastOutputProbability+0.01, 0, 1))));

            valueDerivatives.Add(neuralNetworkController.SetPartialDerivatives(valueNN, leftLastValueCalculation, false));
            valueDerivatives.LastOrDefault().LastOrDefault().LastOrDefault().Add(valueLearningRate * advantage);
        }

        batchCount ++;
        if(batchCount > batchsize)
        {
            batchCount = 0;
            foreach(List<List<List<float>>> derivative in policyDerivatives)
            {
                policyNN = neuralNetworkController.GradientAscent(policyNN, derivative);
            }
            policyDerivatives.Clear();

            foreach(List<List<List<float>>> derivative in valueDerivatives)
            {
                valueNN = neuralNetworkController.GradientAscent(valueNN, derivative);
            }
            valueDerivatives.Clear();

            if(displayDebugInfo)
            {
                var runSave = neuralNetworkController.RunNN(policyNN, new List<float>(){1, 1, 1, 1, 1}, false);
                print("Comparison: " + neuralNetworkController.RunNN(valueNN, new List<float>(){1, 1, 1, 1, 1,}, false)[0] + " " + runSave[0] + " " + runSave[1]);
            }

        }

        skipOneFrameTraining = false;
        leftLastValueCalculation = state.calculations;
        lastStateValue = stateValue;

        saveTimer++;
        if(saveTimer > saveTime)
        {
            aiControl.AISaves[currentAISave].policyNN = policyNN;
            aiControl.AISaves[currentAISave].valueNN = valueNN;

            aiControl.SaveFile();
            saveTimer = 0;
        }

        if(validationHits + validationMisses >= 10000)
        {
            float accuracy = validationHits / (validationMisses + validationHits);
            print("validation: " + accuracy);
            validationHits = 0;
            validationMisses = 0;
        }

        if(terminalState)
        {
            terminalState = false;
            skipOneFrameTraining = true;
        }
    }

    private float Reward()
    {
        var output = reward;
        reward = 0;
        return output;
    }

    List<float> GetNNInputs()
    {
        List<float> output = new List<float>
        {
            gameObject.transform.position.y/6 + +.5f,
            ballObject.transform.position.x/16 + 0.5f,
            ballObject.transform.position.y/8 + 0.5f,
            ballObject.GetComponent<AITrainingBall>().ballRb.velocity.x / 20,
            ballObject.GetComponent<AITrainingBall>().ballRb.velocity.y / 5
        };

        output = new List<float>{
            gameObject.transform.position.y/8 + +.5f,
            ballObject.transform.position.y/8 + 0.5f,
            Vector2.Distance(ballObject.transform.position, gameObject.transform.position)/20f,
            ballObject.GetComponent<AITrainingBall>().ballRb.velocity.x / 20
        };

        if(displayDebugInfo)
        {
            print("new Inputs: "+ output[0] +" "+ output[1]+" "+ output[2]+" "+ output[3]);
        }

        return output;
    }

    double[] SoftMaxFunction(List<float> input)
    {
        double[] inputArray = Array.ConvertAll(input.ToArray(), x => (double)x);
        var inputArray_exp = inputArray.Select(Math.Exp);
        var sum_inputArray_exp = inputArray_exp.Sum();

        return inputArray_exp.Select(i => i / sum_inputArray_exp).ToArray();
    }

    public override void OnHit()
    {
        if(lastStateValue != 0)
        {
            reward += 1.25f - Math.Abs(gameObject.transform.position.y - ballObject.transform.position.y) > 0? 10:-10;

            if(displayDebugInfo)
            {
                print("reward: " + reward + " " + lastStateValue);
            }

            validationHits += 1.25f - Math.Abs(gameObject.transform.position.y - ballObject.transform.position.y) >= 0? 1:0;
            validationMisses += 1.25f - Math.Abs(gameObject.transform.position.y - ballObject.transform.position.y) < 0? 1:0;

            displayDebugInfo = aiControl.displayDebugInfo;
            terminalState = true;
        }
    }
}
