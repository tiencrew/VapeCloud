using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System;
using System.Collections.Generic;

[XmlRoot("GameData")]
public class GameData
{

    //Levels Class
    [XmlElement("LevelPacks")]
    public List<LevelPacks> levelPacks;
    

}