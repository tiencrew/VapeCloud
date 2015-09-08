using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.Collections.Generic;

public class Vape
{

    [XmlElement("Name")]
    public string name;

    [XmlElement("Sprite")]
    public string sprite;

    [XmlElement("Coil")]
    public List<Coil> coils;

    [XmlElement("Tank")]
    public List<Tank> tanks;
}
