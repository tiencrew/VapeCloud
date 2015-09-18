using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

public class Levels {

    [XmlElement("Name")]
    public string name;

    [XmlElement("PointValue")]
    public float pointValue;

    [XmlElement("Multipliers")]
    public List<Multipliers> multipliers;

    [XmlElement("PointsToNextLevel")]
    public float pointsToNextLevel;
      

}
