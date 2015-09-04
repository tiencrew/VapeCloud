using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class GamePlay : MonoBehaviour
{



    // Use this for initialization
    void Start()
    {
        //Testing XML Loader

        GameData gameData;

        gameData = XmlIO.LoadXml<GameData>("LevelData");
        
        Debug.Log(gameData.levelPacks[1].levels[0].levelName); 
    }

    // Update is called once per frame
    void Update()
    {

    }

   
}
