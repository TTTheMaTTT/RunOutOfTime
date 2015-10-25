using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

[XmlType ("TimeEvent")]	//единичный класс, отражающий событие, которое занесётся в хронологию действий одного дубля
public class TimeEvent
{
	[XmlElement("Moment")]
	public float time{ get; set;}//с какого момента времени рассматривается событие

	[XmlElement("Location")]
	public Vector2 location{ get; set;}//какую скорость имел объект, начиная с этого момента

	[XmlElement("Action")]
	public string action{ get; set;}//совершил ли объект действие в этот момент и если совершил, то какое

	public TimeEvent(){} //пустой конструктор для сериализации

	public TimeEvent(float _time, Vector2 _location, string _action)
	{
		this.time = _time;
		this.location = _location;
		this.action = _action;
	}
}

[XmlType ("TimeSequence")]//последовательность действий одного дубля
[XmlInclude(typeof(TimeEvent))]
public class TimeSequence
{
	[XmlArray("TEvents")]
	[XmlArrayItem("TEvent")]
	public List<TimeEvent> sequence = new List<TimeEvent>(); // список действий одного дубля... 
	//Вполне возможен неудачный выбор коллекции

	public TimeSequence(){}//пустой конструктор

	public void AddEvent(TimeEvent tEvent)//Таким образом заполняется последовательность
	{
		sequence.Add (tEvent);
	}
}

[XmlType ("TimeChronology")]//последовательность действий одного дубля
[XmlInclude(typeof(TimeSequence))]
public class TimeChronology
{
	[XmlArray("TSequences")]
	[XmlArrayItem("TSequence")]
	public List<TimeSequence> chronology = new List<TimeSequence>(); // список последовательностей событий для различных дублей... 
	//Вполне возможен неудачный выбор коллекции
	
	public TimeChronology(){}//пустой конструктор
	
	public void AddSequence(TimeSequence tSequence)//Таким образом заполняется хронология
	{
		chronology.Add (tSequence);
	}
}
