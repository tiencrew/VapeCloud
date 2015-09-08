using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

public class Multipliers  {

    [XmlElement("TimeValue")]
    public float TimeValue;

    [XmlElement("MultiplierValue")]
    public float MultiplierValue;
}
