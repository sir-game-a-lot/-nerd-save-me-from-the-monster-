using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour
{
	public static HUD instance;

	public blaze2dUIItem startGameBut;
	public GameObject ScoreTestRoot;
	public blaze2dTextMesh ScoreText;
	// Use this for initialization
	void Awake ()
	{
		instance = this;
		startGameBut.OnClick += () => 
		{
			startGameBut.gameObject.SetActive(false);
		};
	}
	
	// Update is called once per frame
	public void ShowStartGameOverlay ()
	{
		startGameBut.gameObject.SetActive(true);
	}
}
