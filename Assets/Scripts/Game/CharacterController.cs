using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GAF.Core;

public class CharacterController : MonoBehaviour 
{
	private float speed=11f, jumpForce=700f, appearTime=0.8f;
	private int number=0, direction=1, actionNumber;
	private Rigidbody2D rigid;
	private GameObject groundCheck, sight, wallCheck1, wallCheck2, wallCheck3,controlCol, uncontrolCol;
	private bool grounded, wall=false, wallAbove=false,returning=false, jumping1=false;
	private float grRadius=0.3f, sightDistance=20f;//sightDistance - как далеко видит персонаж
	private float prevTime,prevTime1;
	private LevelController lvlController;
	private GAFMovieClip mov;

	public bool underControl=true;
	public LayerMask whatIsGround;
	public LayerMask whatIsUnMoveableGround;

	public void Awake()//инициализация всех используемых модулей
	{

		rigid = gameObject.GetComponent<Rigidbody2D> ();
		groundCheck=transform.FindChild("GroundCheck").gameObject;
		sight = transform.FindChild ("Sight").gameObject;
		wallCheck1=transform.FindChild ("WallCheck1").gameObject;
		wallCheck2=transform.FindChild ("WallCheck2").gameObject;
		wallCheck3=transform.FindChild ("WallCheck3").gameObject;
		mov = transform.FindChild("NewHero").GetComponent<GAFMovieClip> ();
		//controlCol=transform.FindChild ("ControlledCollider").gameObject;
		//uncontrolCol=transform.FindChild ("UncontrolledCollider").gameObject;
		lvlController = GameObject.FindGameObjectWithTag (Tags.controller).GetComponent<LevelController> ();
		returning = false;
	}

	public void FixedUpdate()//Здесь происходит анализ ситуации, в которой находится персонаж
	{
		if ((!wall) && (Mathf.Abs (rigid.velocity.x) <= speed - 1f)&&(!returning)&&!(appearTime>0f))
			rigid.velocity = Vector2.Lerp (rigid.velocity, new Vector2 (speed * direction, rigid.velocity.y), 0.15f);
		else if ((!wall)&&(!returning)&&!(appearTime>0f))
			rigid.velocity = new Vector2 (speed * direction, rigid.velocity.y);//Персонаж двигается здесь.
		rigid.velocity = new Vector2 ((wall)? 0f: rigid.velocity.x, rigid.velocity.y);
		wallAbove = Physics2D.OverlapCircle (wallCheck3.transform.position, grRadius, whatIsGround);
		wall = (Physics2D.Raycast (wallCheck1.transform.position,new Vector2(direction*1f,0f), grRadius, whatIsUnMoveableGround)||
		        (Physics2D.Raycast (wallCheck2.transform.position,new Vector2(direction*1f,0f), grRadius, whatIsUnMoveableGround)&& !(wallAbove))||
		        Physics2D.Raycast(sight.transform.position, new Vector2(direction*1f,0f),grRadius, whatIsUnMoveableGround));
		grounded = Physics2D.Raycast (groundCheck.transform.position,new Vector2(0f,-1f), grRadius, whatIsGround);
		if (underControl)
			ControlledActions ();
		else 
			UncontrolledActions ();
		//Анимируем персонажа
		if (returning)
		{
			//rigid.velocity=Vector2.Lerp (rigid.velocity, new Vector2 (0f, rigid.velocity.y), 0.05f);
			mov.setSequence ("TimeTravel", true);
			mov.setAnimationWrapMode(GAF.Core.GAFWrapMode.Once);
		}
		else if (appearTime > 0) {
			mov.setSequence ("Appear", true);
			mov.setAnimationWrapMode(GAF.Core.GAFWrapMode.Once);
		}
		else if ((grounded) && (Mathf.Abs (rigid.velocity.x) < 1)) {
			mov.setSequence ("Stand", true);
			mov.setAnimationWrapMode(GAF.Core.GAFWrapMode.Loop);
		} 
		else 
		{
			if (grounded)
				mov.setSequence ("Run", true);
			else if (rigid.velocity.y > 1f)
				mov.setSequence ("Jump", true);
			else if (rigid.velocity.y < -1f)
				mov.setSequence ("Fall", true);
		}

	}

	void Update()
	{
		if (appearTime > 0f)
			appearTime -= Time.deltaTime;
	}

	void ControlledActions()//Что совершает персонаж, если он управляем игроком
	{
	/*	if (controlCol.GetComponent<BoxCollider2D> ().enabled == false) 
		{
			SwitchColMode (true);
		}*/
		if  (grounded) 
		{
			bool jumping=false;
			if (Input.GetKeyDown(KeyCode.Space))
				jumping=true;
			if ((jumping)||(jumping1))
			{
				if (appearTime<=0)
				{
					rigid.AddForce (new Vector2 (0f, jumpForce));
					WriteChronology	("Jump");	
				}
				jumping=false;
				jumping1=false;
			}
		}
		if (lvlController.timer>prevTime+lvlController.refreshTime)
		{
			prevTime=lvlController.timer;
			if (!lvlController.CompareLocationPrecisely(number,actionNumber,VectorConverter(transform.position)))
			{
				WriteChronology("ChangeLocation");
			}
		}
	}

	void UncontrolledActions()//Что делает дубль, если он неподконтролен игроком
	{
	/*	if (controlCol.GetComponent<BoxCollider2D> ().enabled == true) 
			SwitchColMode (false);*/
		bool doYouSeeYourself=false;//Видит ли дубль себя из будущего?
		bool doYouMoveWrong = false;//Находится ли дубль в той же точке, в которой он должен находиться
		if (lvlController.CompareTimer (number, actionNumber+1)) 
		{
			prevTime1=lvlController.timer;
			actionNumber++;
			if ((lvlController.ChronoAction(number,actionNumber)=="Jump")&&(grounded))
			{
				transform.position=lvlController.Locate(transform.position, number, actionNumber);
				rigid.AddForce (new Vector2 (0f, jumpForce));
			}
			else if (lvlController.ChronoAction(number,actionNumber)=="ChangeDirection")
			{
				transform.position=lvlController.Locate(transform.position, number, actionNumber);
				ChangeDirection();
			}
			else if ((lvlController.ChronoAction(number,actionNumber)=="Return"))
			{
				returning=true;
				transform.position=lvlController.Locate(transform.position, number, actionNumber);
				Destroy(gameObject,0.5f);
			}
			else if (!(lvlController.CompareLocation(number,actionNumber, VectorConverter(transform.position))))
			{
				doYouMoveWrong=true;
			}
		}
		if (lvlController.timer>prevTime+lvlController.revisionTime)//Здесь prevTime нужен для контроля обнаружения персонажем самого себя из будущего
		{
			prevTime=lvlController.timer;
			RaycastHit2D ray=Physics2D.Raycast(sight.transform.position,new Vector2(direction*1f,0f),sightDistance);
			if (ray)
				if ((ray.collider.gameObject.tag==Tags.character)&&(appearTime<=0))
					doYouSeeYourself=(ray.collider.gameObject.GetComponent<CharacterController>().GetNumber()>number);

		}
		if ((lvlController.timer>prevTime1+lvlController.revisionTime)&&(!returning))//Здесь prevTime1 нужен для контроля неожиданного изменения положения персонажа, когда он должен стоять
		{
			prevTime1=lvlController.timer;
			if (!(lvlController.CompareLocation(number,actionNumber, VectorConverter(transform.position))))
				doYouMoveWrong=true;
		}

		//Проверка условия на нарушение хронологии событий, которое может возникнуть из-за того, что дубль оказался не в 
		//хронологически каноничном месте или увидел самого себя из будущего
		if (doYouMoveWrong||(doYouSeeYourself))
		{
			if (doYouMoveWrong)
				transform.position=lvlController.Locate(transform.position, number, actionNumber);//если нарушение событий вызвано изменением положения, то это изменение исправляется
			//prevTime=lvlController.timer+5*lvlController.refreshTime;
			lvlController.DeleteDoubles(number,this);
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

	public void WriteChronology(string action)//Функция записи нового действия, совершённого персонажем
	{
		//prevTime=lvlController.timer;//отсутствие этой строки обеспечивает независимость списка перемен скоростей и списка действий
		TimeEvent tEvent= new TimeEvent (lvlController.timer,VectorConverter(transform.position),action);
		lvlController.SetChronology(number, tEvent);
		actionNumber++;
	}

	public void ChangeDirection()
	{
		direction = -1 * direction;
		transform.localScale=new Vector3(transform.localScale.x*-1,
		                             transform.localScale.y,
		                             transform.localScale.z);
	}

	Vector2 VectorConverter(Vector3 vect)//функция для удобства
	{
		return new Vector2 (vect.x, vect.y);
	}

	public void SetJumping()
	{
		if (grounded)
			jumping1 = true;
	}

	public void SetReturning()
	{
		returning = true;
	}
}
