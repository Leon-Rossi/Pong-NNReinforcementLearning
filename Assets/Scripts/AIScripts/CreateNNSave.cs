using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateNNSave : MonoBehaviour
{
    public AIControl aiInterface;

    float mutationFactor;
    float mutationThreshhold;
    int populationCount;

    int layerCount;
    int layerSize;
    int inputCount = 6;
    int outputCount = 1;

    string saveName;

    GameObject AIMenu;

    bool flag1 = false;
    bool flag2 = false;
    bool flag3 = false;
    bool flag4 = false;
    bool flag5 = false;  
    bool flag6 = false;

    void Start()
    {
        AIMenu = GameObject.Find("AIMenu");
    }

    public void ReadMutationFactor(string input)
    {
        mutationFactor = float.Parse(input);
        flag1 = true;
        Debug.Log("Test");
    }

    public void ReadMutaionThreshhold(string input)
    {
        mutationThreshhold = float.Parse(input);
        flag2 = true;
        Debug.Log("Test");
    }

    public void ReadPopulationCount(string input)
    {
        populationCount = int.Parse(input);
        flag3 = true;
        Debug.Log("Test");
    }

    public void ReadLayerCount(string input)
    {
        layerCount = int.Parse(input);
        flag4 = true;
        Debug.Log("Test");
    }

    public void ReadLayerSize(string input)
    {
        layerSize = int.Parse(input);
        flag5 = true;
        Debug.Log("Test");
    }

    public void ReadName(string input)
    {
        saveName = input;
        flag6 = true;
        Debug.Log("Test");
    }

    public void CreateNewSave()
    {
        Debug.Log("Start");
        Debug.Log(flag1);
        Debug.Log(flag2);
        Debug.Log(flag3);
        Debug.Log(flag4);
        Debug.Log(flag5);
        Debug.Log(flag6);

        if(flag1 & flag2 & flag3 & flag4 & flag5 & flag6)
        {
            Debug.Log("Test2");
            aiInterface = GameObject.Find("GameMaster").GetComponent<AIControl>();

            aiInterface.AISaves.Add(new AISave(mutationFactor, mutationThreshhold, populationCount, layerCount, layerSize, inputCount, outputCount, saveName));
            
            AIMenu.SetActive(true);
            GameObject.Find("CreateNNMenu").SetActive(false);

            Debug.Log("Test");

            aiInterface.SaveFile();
        }
    }
}
