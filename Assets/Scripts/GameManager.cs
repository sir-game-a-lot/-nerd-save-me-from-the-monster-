using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public FlapperBehavior flapper;
	static GameManager instance;
	void Awake ()
	{
		instance = this;
	}

	void Start ()
	{
	
	}

	public static void InitGame()
	{
		STATE.currentState = FlapperState.FLAPPABLE;
		ObstacleManager.instance.StartSpawning();
	}

	public static void ToMainMenu()
	{
		instance.flapper.Reset();
		MenuScreen.Show();
		STATE.currentState = FlapperState.INIT;
		ObstacleManager.instance.ClearObstacles();
	}

}
