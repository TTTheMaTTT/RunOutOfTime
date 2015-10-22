using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour 
{
	private float speed=10f, jumpForce=600f;
	private int number, direction=1;
	private Rigidbody2D rigid;
	private GameObject groundCheck, sight, wallCheck1, wallCheck2, wallCheck3;
	private bool grounded, wall=false, wallAbove=false;
	private float grRadius=0.3f;

	public LayerMask whatIsGround;

	public void Awake()
	{

		rigid = gameObject.GetComponent<Rigidbody2D> ();
		groundCheck=transform.FindChild("GroundCheck").gameObject;
		sight = transform.FindChild ("Sight").gameObject;
		wallCheck1=transform.FindChild ("WallCheck1").gameObject;
		wallCheck2=transform.FindChild ("WallCheck2").gameObject;
		wallCheck3=transform.FindChild ("WallCheck3").gameObject;
	}

	public void FixedUpdate()
	{
		if ((!wall) && (Mathf.Abs (rigid.velocity.x) <= speed - 1f))
			rigid.velocity = Vector2.Lerp (rigid.velocity, new Vector2 (speed * direction, rigid.velocity.y), 2f);
		else if (!wall)
			rigid.velocity = new Vector2 (speed * direction, rigid.velocity.y);
		rigid.velocity = new Vector2 ((wall)? 0f: rigid.velocity.x, rigid.velocity.y);
		wallAbove = Physics2D.OverlapCircle (wallCheck3.transform.position, grRadius, whatIsGround);
		wall = (Physics2D.OverlapCircle (wallCheck1.transform.position, grRadius, whatIsGround)||
		        (Physics2D.OverlapCircle (wallCheck2.transform.position, grRadius, whatIsGround)&& !(wallAbove))||
		        Physics2D.OverlapCircle (sight.transform.position, grRadius, whatIsGround));
		grounded = Physics2D.OverlapCircle (groundCheck.transform.position, grRadius, whatIsGround);
		if (Input.GetKeyDown (KeyCode.Space)&&(grounded))
			rigid.AddForce (new Vector2 (0f, jumpForce));


	}

	void Update()
	{
		if (Input.GetButtonDown("Cancel"))
		{
			Application.LoadLevel (Application.loadedLevel);
		}

	}

}
