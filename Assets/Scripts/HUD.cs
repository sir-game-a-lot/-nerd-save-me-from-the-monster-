using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour
{
	public static HUD instance;
	
	public GameObject ScoreTestRoot;
	public TextMesh ScoreText;
	// Use this for initialization
	void Awake ()
	{
		instance = this;
	}
	
}
