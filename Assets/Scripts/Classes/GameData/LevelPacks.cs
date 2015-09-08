using System.Xml.Serialization;


public class LevelPacks
{
    [XmlElement("Vape")]
    public Vape vape;

    //LevelPacks Class   
    [XmlElement("Levels")]
    public Levels[] levels;
    
    
}
