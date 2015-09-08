using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

public class Coil
{

    [XmlElement("Level")]
    public int level;

    [XmlElement("Cost")]
    public float cost;

    [XmlElement("Overheat")]
    public float Overheat;
}
