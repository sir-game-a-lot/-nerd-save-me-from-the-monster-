using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleManager : MonoBehaviour
{
	public static ObstacleManager instance;
	
	public GameObject obstaclePrefab;
	public Transform spawnPoint;
	public Transform destroyPoint;
	public Transform lowerBound;
	public Transform upperBound;
	public Vector3 speed;
	public float initialDelay = 2;
	public float frequency = 2;

	public List<GameObject> spawnnedObstacles;

	void Awake ()
	{
		instance = this;
	}

	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		if( STATE.currentState != FlapperState.FLAPPABLE )
		   return;

		for( int i=0; i< spawnnedObstacles.Count; ++i )
		{
			var g = spawnnedObstacles[i];
			g.rigidbody.MovePosition( g.rigidbody.position + speed * Time.deltaTime );

			// Destroying obstacles
			if( g.transform.position.x < destroyPoint.position.x )
			{
				var temp = g;
				spawnnedObstacles.Remove( g );
				GameObject.Destroy( temp );
			}
		}
	}

	public void StartSpawning()
	{
		StartCoroutine( SpawnObstaclesCoroutine() );
	}

	IEnumerator SpawnObstaclesCoroutine()
	{
		yield return new WaitForSeconds( initialDelay );
		while(STATE.currentState == FlapperState.FLAPPABLE)
		{
			var finalPosition = spawnPoint.position;
			finalPosition.y = Random.Range( lowerBound.position.y, upperBound.position.y );
			spawnnedObstacles.Add( Instantiate( obstaclePrefab, finalPosition, Quaternion.identity ) as GameObject );
			yield return new WaitForSeconds( frequency );
		}
	}

	public void ClearObstacles()
	{
		for( int i=0; i< spawnnedObstacles.Count; ++i )
		{
			GameObject.Destroy( spawnnedObstacles[i] );
		}
		spawnnedObstacles.Clear();
	}
}
