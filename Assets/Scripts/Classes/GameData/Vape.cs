using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

public class Vape
{

    [XmlElement("Name")]
    public string name;

    [XmlElement("Sprite")]
    public string sprite;

    [XmlElement("Coil")]
    public Coil[] coils;

    [XmlElement("Tank")]
    public Tank[] tanks;
}
