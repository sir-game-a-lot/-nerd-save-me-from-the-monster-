using UnityEngine;
using System.Collections;

public class StatsScreen : MonoBehaviour
{

	public static StatsScreen instance;

	public GameObject root;
	public TouchItem retryButton;
	// Use this for initialization
	void Awake ()
	{
		instance = this;
	}

	void Start ()
	{
		retryButton.onUp += (TouchItem) => 
		{
			root.gameObject.SetActive(false);
			GameManager.ToMainMenu();
			//Debug.Log(this.name +"\t|\t"+Time.time);
		};
	}
	
	// Update is called once per frame
	public void ShowStatsScreen ()
	{
		root.gameObject.SetActive(true);
	}
}
