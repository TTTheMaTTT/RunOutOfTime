using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterController : MonoBehaviour 
{
	private float speed=10f, jumpForce=600f;
	private int number=0, direction=1, actionNumber;
	private Rigidbody2D rigid;
	private GameObject groundCheck, sight, wallCheck1, wallCheck2, wallCheck3,controlCol, uncontrolCol;
	private bool grounded, wall=false, wallAbove=false,returning=false;
	private float grRadius=0.3f, sightDistance=20f;//sightDistance - как далеко видит персонаж
	private float prevTime,prevTime1;
	private LevelController lvlController;

	public bool underControl=true;
	public LayerMask whatIsGround;

	public void Awake()//инициализация всех используемых модулей
	{

		rigid = gameObject.GetComponent<Rigidbody2D> ();
		groundCheck=transform.FindChild("GroundCheck").gameObject;
		sight = transform.FindChild ("Sight").gameObject;
		wallCheck1=transform.FindChild ("WallCheck1").gameObject;
		wallCheck2=transform.FindChild ("WallCheck2").gameObject;
		wallCheck3=transform.FindChild ("WallCheck3").gameObject;
		//controlCol=transform.FindChild ("ControlledCollider").gameObject;
		//uncontrolCol=transform.FindChild ("UncontrolledCollider").gameObject;
		lvlController = GameObject.FindGameObjectWithTag (Tags.controller).GetComponent<LevelController> ();
		returning = false;
	}

	public void FixedUpdate()//Здесь происходит анализ ситуации, в которой находится персонаж
	{
		if ((!wall) && (Mathf.Abs (rigid.velocity.x) <= speed - 1f)&&(!returning))
			rigid.velocity = Vector2.Lerp (rigid.velocity, new Vector2 (speed * direction, rigid.velocity.y), 0.1f);
		else if ((!wall)&&(!returning))
			rigid.velocity = new Vector2 (speed * direction, rigid.velocity.y);//Персонаж двигается здесь.
		rigid.velocity = new Vector2 ((wall)? 0f: rigid.velocity.x, rigid.velocity.y);
		wallAbove = Physics2D.OverlapCircle (wallCheck3.transform.position, grRadius, whatIsGround);
		wall = (Physics2D.Raycast (wallCheck1.transform.position,new Vector2(direction*1f,0f), grRadius, whatIsGround)||
		        (Physics2D.Raycast (wallCheck2.transform.position,new Vector2(direction*1f,0f), grRadius, whatIsGround)&& !(wallAbove))||
		        Physics2D.Raycast(sight.transform.position, new Vector2(direction*1f,0f),grRadius, whatIsGround));
		grounded = Physics2D.OverlapCircle (groundCheck.transform.position, grRadius, whatIsGround);
		if (underControl)
			ControlledActions ();
		else 
			UncontrolledActions ();
	}

	void Update()
	{

	}

	void ControlledActions()//Что совершает персонаж, если он управляем игроком
	{
	/*	if (controlCol.GetComponent<BoxCollider2D> ().enabled == false) 
		{
			SwitchColMode (true);
		}*/
		if (Input.GetKeyDown (KeyCode.Space) && (grounded)) 
		{
			rigid.AddForce (new Vector2 (0f, jumpForce));
			WriteChronology	("Jump");	
		}
		if (lvlController.timer>prevTime+lvlController.refreshTime)
		{
			prevTime=lvlController.timer;
			if (!lvlController.CompareVelocityPrecisely(number,actionNumber, rigid.velocity))
			{
				WriteChronology("ChangeSpeed");
			}
		}
	}

	void UncontrolledActions()//Что делает дубль, если он неподконтролен игроком
	{
	/*	if (controlCol.GetComponent<BoxCollider2D> ().enabled == true) 
			SwitchColMode (false);*/
		bool doYouSeeYourself=false;//Видит ли дубль себя из будущего?
		if (lvlController.CompareTimer (number, actionNumber+1)) 
		{
			actionNumber++;
			if ((lvlController.ChronoAction(number,actionNumber)=="Jump")&&(grounded))
			{
				rigid.AddForce (new Vector2 (0f, jumpForce));
			}
			else if ((lvlController.ChronoAction(number,actionNumber)=="Return"))
			{
				returning=true;
				Destroy(gameObject,0.5f);
			}
		}
		//Отсюда начинаю писать о проверке условий на нарушение хронологии событий дубля
		if ((lvlController.timer>prevTime1+lvlController.revisionTime)&&(!returning))
		{
			prevTime1=lvlController.timer;
			RaycastHit2D ray=Physics2D.Raycast(sight.transform.position,new Vector2(direction*1f,0f),sightDistance);
			if (ray.collider.gameObject.tag==Tags.character)
				doYouSeeYourself=(ray.collider.gameObject.GetComponent<CharacterController>().GetNumber()>number);
			//Проверка условия на нарушение хронологии событий, которое может возникнуть из-за того, что дубль оказался не в 
			//хронологически каноничном месте или увидел самого себя из будущего
			if (!(lvlController.CompareVelocity(number,actionNumber, rigid.velocity))||(doYouSeeYourself))
			{
				prevTime=lvlController.timer+5*lvlController.refreshTime;
				lvlController.DeleteDoubles(number,this);
				//WriteChronology("ChangeSpeed");
			}

		}
	}

	public void SetNumber(int _number)//Устанавливает, какой это номер дубля
	{
		this.number=_number;
	}

	public void SetActNumber(int _number)//Устанавливает, какое по счёту действие совершает персонаж
	{
		this.actionNumber =_number;
	}

	public int GetNumber() //Какой это дубль по счёту?
	{
		return number;
	}
	
	public int GetActNumber() //Какое по счёту действие совершает персонаж?
	{
		return actionNumber;
	}

	/*void SwitchColMode(bool controlled)//Необходимый костыль для преодоления неподконтрольными персонажами препятствий
	{
		if (!controlled)
		{
			controlCol.GetComponent<BoxCollider2D>().enabled=false;
			uncontrolCol.GetComponent<BoxCollider2D>().enabled=true;
			uncontrolCol.GetComponent<CircleCollider2D>().enabled=true;
		}
		else 
		{
			controlCol.GetComponent<BoxCollider2D>().enabled=true;
			uncontrolCol.GetComponent<BoxCollider2D>().enabled=false;
			uncontrolCol.GetComponent<CircleCollider2D>().enabled=false;
		}
	}*/

	void WriteChronology(string action)//Функция записи нового действия, совершённого персонажем
	{
		//prevTime=lvlController.timer;//отсутствие этой строки обеспечивает независимость списка перемен скоростей и списка действий
		TimeEvent tEvent= new TimeEvent (lvlController.timer,rigid.velocity,action);
		lvlController.SetChronology(number, tEvent);
		actionNumber++;
	}
}
