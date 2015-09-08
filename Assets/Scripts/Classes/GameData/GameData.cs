using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System;

[XmlRoot("GameData")]
public class GameData
{

    //Levels Class
    [XmlElement("LevelPacks")]
    public LevelPacks[] levelPacks;
    

}