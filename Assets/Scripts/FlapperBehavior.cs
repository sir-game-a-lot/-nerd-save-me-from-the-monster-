using UnityEngine;
using System.Collections;

public class FlapperBehavior : MonoBehaviour
{
	public AnimationCurve riseCurve;
	public float riseCurveDur = 0.15f;
	public AnimationCurve fallCurve;

	float riseCounter;
	float fallCounter;
	bool isRising = false;
	Vector3 intrmdtPos;
	Vector3 initialPosition;
	bool setDeadOnce;
	int score = 0;

	// Use this for initialization
	void Start ()
	{
		Application.targetFrameRate = 60;
		STATE.currentState = FlapperState.INIT;
		initialPosition = transform.position;
		intrmdtPos = transform.position;
		StartCoroutine( SetRotation() );

		if( HUD.instance != null )
		{
			HUD.instance.startGameBut.OnClick += () =>
			{
				setDeadOnce = false;
				STATE.currentState = FlapperState.FLAPPABLE;
				ObstacleManager.instance.StartSpawning();
			};
		}

		if( StatsScreen.instance != null )
		{
			StatsScreen.instance.retryBut.OnClick += () => 
			{
				ObstacleManager.instance.ClearObstacles();
				Reset();
			};
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( STATE.currentState == FlapperState.INIT )
			return;

		var tapDetected = Input.GetMouseButtonDown(0);

		if( transform.position.y < -4.5 )
		{
			STATE.currentState = FlapperState.DEAD;
		}

		if( STATE.currentState == FlapperState.DEAD )
		{
			//Debug.Log("GameOver!");
			tapDetected = false;
			if( !setDeadOnce )
			{
				StatsScreen.instance.ShowStatsScreen ();
				setDeadOnce = true;
			}
		}

		if( tapDetected )
		{
			intrmdtPos = transform.position;
			riseCounter = 0;
			fallCounter = 0;
			isRising = true;
			//Debug.Log("tapped");
		}
		if( isRising )
		{
			if( transform.position.y < 6 )
			{
				riseCounter += Time.deltaTime;
				//Debug.Log(  );
				transform.position = new Vector3( transform.position.x, intrmdtPos.y + riseCurve.Evaluate( riseCounter),transform.position.z);

				if(riseCounter >= riseCurveDur)
				{
					intrmdtPos = transform.position;
					riseCounter = 0;
					isRising = false;
				}
			}
			else
			{
				intrmdtPos = transform.position;
				riseCounter = 0;
				isRising = false;
			}
		}
		else
		{
			if( transform.position.y > -4.5 )
			{
				fallCounter += Time.deltaTime;
				//Debug.Log( fallCurve.Evaluate( fallCounter) );
				transform.position = new Vector3( transform.position.x, intrmdtPos.y + fallCurve.Evaluate( fallCounter),transform.position.z);
			}
		}
	}

	IEnumerator SetRotation()
	{
		while(true)
		{
			var pos0 = transform.position.y;
			yield return new WaitForEndOfFrame();
			if( STATE.currentState != FlapperState.INIT )
			{
				var posDel = transform.position.y - pos0;
				//Debug.Log( (posDel * 180) );
				transform.rotation =  Quaternion.Lerp( transform.rotation, Quaternion.Euler(0,0, (posDel * 90) ), Time.deltaTime * 5f );
			}
//			if( posDel > 0 )
//			{
//				transform.rotation =  Quaternion.Lerp( transform.rotation, Quaternion.Euler(0,0, 20 ), Time.deltaTime * 5f );
//			}
//			else
//			{
//				transform.rotation =  Quaternion.Lerp( transform.rotation, Quaternion.Euler(0,0, -90 ), Time.deltaTime * 1.25f );
//			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if( STATE.currentState == FlapperState.FLAPPABLE )
		{
			if( other.CompareTag("Obstacle") )
			{
				STATE.currentState = FlapperState.DEAD;
				//Debug.Log("Bam! "+Time.time);
				//isGameOver = true;
			}
			else if( other.CompareTag("ScoreTrigger") )
			{
				++score;
				HUD.instance.ScoreText.text = score.ToString();
			}
		}
	}

	void Reset()
	{
		score = 0;
		HUD.instance.ScoreText.text = score.ToString();
		STATE.currentState = FlapperState.INIT;
		transform.position = initialPosition;
		riseCounter = 0;
		fallCounter = 0;
		isRising = false;
		intrmdtPos = transform.position;
	}
}
