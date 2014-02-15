using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour
{
	public static HUD instance;

	//public tk2dUIItem startGameBut;
	public GameObject ScoreTestRoot;
	public TextMesh ScoreText;
	// Use this for initialization
	void Awake ()
	{
		instance = this;
//		startGameBut.OnClick += () => 
//		{
//			startGameBut.gameObject.SetActive(false);
//		};
	}

//	public void ShowStartGameOverlay ()
//	{
//		startGameBut.gameObject.SetActive(true);
//	}
}
