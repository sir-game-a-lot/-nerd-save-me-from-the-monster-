using UnityEngine;
using System.Collections;

public class StatsScreen : MonoBehaviour
{

	public static StatsScreen instance;

	public GameObject root;
	public tk2dUIItem retryBut;
	// Use this for initialization
	void Awake ()
	{
		instance = this;
		retryBut.OnClick += () => 
		{
			root.gameObject.SetActive(false);
			HUD.instance.ShowStartGameOverlay ();
		};
	}
	
	// Update is called once per frame
	public void ShowStatsScreen ()
	{
		root.gameObject.SetActive(true);
	}
}
