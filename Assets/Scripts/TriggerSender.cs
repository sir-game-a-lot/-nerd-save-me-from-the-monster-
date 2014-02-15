using UnityEngine;
using System.Collections;
using System;

public class TriggerSender : MonoBehaviour
{
	public Action<Collider> onTriggerEnter;

	
	// Update is called once per frame
	void OnTriggerEnter (Collider other)
	{
		if( onTriggerEnter!= null )
		{
			onTriggerEnter(other);
		}
	}
}
