using System.Xml.Serialization;
using System;
using System.IO;

public class Serializator
{
	public static void SaveXml(TimeChronology chronology, string datapath)
	{
		Type[] extraTypes={typeof(TimeSequence),typeof(TimeEvent)};
		XmlSerializer serializer = new XmlSerializer (typeof(TimeChronology), extraTypes);
		FileStream fs = new FileStream(datapath, FileMode.Create); 
		serializer.Serialize(fs, chronology); 
		fs.Close(); 
	}

	static public TimeChronology DeXml(string datapath){
		
		Type[] extraTypes= { typeof(TimeSequence), typeof(TimeEvent)};
		XmlSerializer serializer = new XmlSerializer(typeof(TimeChronology), extraTypes); 
		
		FileStream fs = new FileStream(datapath, FileMode.Open); 
		TimeChronology chrono = (TimeChronology)serializer.Deserialize(fs); 
		fs.Close(); 
		return chrono;
	}
}
