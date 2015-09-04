using System.Xml.Serialization;


public class LevelPacks
{
    [XmlAttribute("VapeName")]
    public string vapeName;

    //LevelPacks Class   
    [XmlElement("Levels")]
    public Levels[] levels;
    
    
}
