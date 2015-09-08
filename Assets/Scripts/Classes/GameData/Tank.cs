using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

public class Tank
{
    [XmlElement("Level")]
    public int level;

    [XmlElement("Cost")]
    public float cost;

    [XmlElement("Time")]
    public float time;
}
