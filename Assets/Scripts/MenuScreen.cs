using UnityEngine;
using System.Collections;

public class MenuScreen : MonoBehaviour
{
	public GameObject root;
	public TouchItem playButton;

	static MenuScreen instance;
	void Awake ()
	{
		instance = this;
	}

	void Start ()
	{
		playButton.onUp += (TouchItem) =>
		{
			//Debug.Log("Play Button Tapped");
			GameManager.InitGame();
			Hide();
		};
	}

	public static void Show ()
	{
		instance.StartCoroutine( instance._Show() );
	}

	public static void Hide ()
	{
		instance.StartCoroutine( instance._Hide() );
	}

	IEnumerator _Show()
	{
		Debug.Log(this.name +"\t|\t"+Time.time);
		yield return null;
		root.SetActive(true);
	}

	IEnumerator _Hide()
	{
		yield return null;
		root.SetActive(false);
	}
}
