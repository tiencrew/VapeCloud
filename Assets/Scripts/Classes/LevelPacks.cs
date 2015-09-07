using System.Xml.Serialization;


public class LevelPacks
{
    [XmlElement("VapeName")]
    public string vapeName;

    //LevelPacks Class   
    [XmlElement("Levels")]
    public Levels[] levels;
    
    
}
