using System.Collections.Generic;
using System.Xml.Serialization;


public class LevelPacks
{
    [XmlElement("Vape")]
    public Vape vape;

    //LevelPacks Class   
    [XmlElement("Levels")]
    public List<Levels> levels;
    
    
}
