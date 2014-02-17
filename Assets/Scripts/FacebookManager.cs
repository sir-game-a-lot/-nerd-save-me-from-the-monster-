using UnityEngine;
using System.Collections;

public class FacebookManager : MonoBehaviour
{

	// Use this for initialization
	void Awake ()
	{
		FB.Init(faceBookInitialized);
	}
	
	void faceBookInitialized()
	{
		if( !FB.IsLoggedIn )
			FB.Login("email,publish_actions",loggedIn);
		else
			ShowDialog();
	}

	void loggedIn(FBResult fbResult)
	{
		ShowDialog();
	}

	void ShowDialog()
	{
		FB.Feed(linkCaption: "I just swooshed through 28 usb cables in Flappy nerd!!!",
		        picture:"http://www.taste.com.au/images/articles/untitled-102121315.jpg"
		        );
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
