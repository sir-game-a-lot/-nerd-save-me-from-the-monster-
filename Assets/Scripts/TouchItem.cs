using UnityEngine;
using System.Collections;
using System;

[ExecuteInEditMode]
public class TouchItem : MonoBehaviour
{
	Rect colliderRect = new Rect();
	bool downOnce = false;
	bool holdingDown = false;
	bool upOnce = false;

	public Action<TouchItem> onDown;
	public Action<TouchItem> onUp;
	// Use this for initialization
	void Start ()
	{
		colliderRect.x = transform.position.x - collider.bounds.extents.x;
		colliderRect.y = transform.position.y - collider.bounds.extents.y;
		colliderRect.width = collider.bounds.extents.x*2;
		colliderRect.height = collider.bounds.extents.y*2;
	}
	
	// Update is called once per frame
	void Update ()
	{
		Vector3 touchPos = new Vector3(-1000, -1000);
#if UNITY_EDITOR
		if( Input.GetMouseButton(0) )
			touchPos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
#else
		if(Input.touchCount == 1)
			touchPos = Camera.main.ScreenToWorldPoint( Input.touches[0].position );
#endif
		if( colliderRect.Contains( new Vector2(touchPos.x, touchPos.y) ) )
		{
			holdingDown = true;
			upOnce = false;
			if( !downOnce )
			{
				if( onDown != null )
					onDown(this);
				//Debug.Log( "buttonDown" );
				downOnce = true;
			}
		}
		else
		{
			if( holdingDown )
			{
				downOnce = false;
				if( !upOnce )
				{
					if( onUp != null )
						onUp(this);
					//Debug.Log( "buttonUp" );
					upOnce = true;
				}
				holdingDown = false;
			}
		}

	}

#if UNITY_EDITOR
	void OnDrawGizmos ()
	{
		colliderRect.x = transform.position.x - collider.bounds.extents.x;
		colliderRect.y = transform.position.y - collider.bounds.extents.y;
		colliderRect.width = collider.bounds.extents.x*2;
		colliderRect.height = collider.bounds.extents.y*2;

		Gizmos.color = Color.blue;
		var point1 = new Vector3( colliderRect.x, colliderRect.y );
		var point2 = new Vector3( colliderRect.x + colliderRect.width, colliderRect.y );
		var point3 = new Vector3( colliderRect.x, colliderRect.y + colliderRect.height );
		var point4 = new Vector3( colliderRect.x + colliderRect.width, colliderRect.y + colliderRect.height );
		Gizmos.DrawSphere( point1, 0.1f);
		Gizmos.DrawSphere( point2, 0.1f);
		Gizmos.DrawSphere( point3, 0.1f);
		Gizmos.DrawSphere( point4, 0.1f);
		Gizmos.DrawLine( point1, point2 );
		Gizmos.DrawLine( point2, point4 );
		Gizmos.DrawLine( point4, point3 );
		Gizmos.DrawLine( point3, point1 );

//		Gizmos.DrawSphere( transform.position + collider.bounds.center + new Vector3( collider.bounds.extents.x, -collider.bounds.extents.y ) , 0.2f);
	}
#endif
}
