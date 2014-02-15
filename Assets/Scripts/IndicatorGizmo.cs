using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IndicatorGizmo : MonoBehaviour
{
	public string iconName; //= "white_box.png"
	public Bounds reticleBounds;
	public Color gizmoColor = Color.white;
	void OnDrawGizmos ()
	{
		Gizmos.color = gizmoColor;
		var cen = reticleBounds.center;
		var ext = reticleBounds.extents;
		Gizmos.DrawLine( transform.position + new Vector3(cen.x-ext.x, 0, 0), transform.position + new Vector3(cen.x+ext.x, 0, 0) );
		Gizmos.DrawLine( transform.position + new Vector3(0, cen.y-ext.y, 0), transform.position + new Vector3(0, cen.y+ext.y, 0) );
		if(  !string.IsNullOrEmpty(iconName) )
			Gizmos.DrawIcon( transform.position, iconName );
	}
}
