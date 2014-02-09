using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(blaze2dUIScrollbar))]
public class tk2dUIScrollbarEditor : Editor
{
    tk2dUIMethodBindingHelper methodBindingUtil = new tk2dUIMethodBindingHelper();

    public override void OnInspectorGUI()
    {
        tk2dGuiUtility.LookLikeInspector();
        base.OnInspectorGUI();

		blaze2dUIScrollbar scrollbar = (blaze2dUIScrollbar)target;
		scrollbar.BarLayoutItem = EditorGUILayout.ObjectField("Bar LayoutItem", scrollbar.BarLayoutItem, typeof(blaze2dUILayout), true) as blaze2dUILayout;

        scrollbar.SendMessageTarget = methodBindingUtil.BeginMessageGUI(scrollbar.SendMessageTarget);
        methodBindingUtil.MethodBinding( "On Scroll", typeof(blaze2dUIScrollbar), scrollbar.SendMessageTarget, ref scrollbar.SendMessageOnScrollMethodName );
        methodBindingUtil.EndMessageGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(scrollbar);
        }
    }

    public void OnSceneGUI()
    {
        bool wasChange=false;
        blaze2dUIScrollbar scrollbar = (blaze2dUIScrollbar)target;
        bool isYAxis = scrollbar.scrollAxes == blaze2dUIScrollbar.Axes.YAxis;

		// Get rescaled transforms
		Matrix4x4 m = scrollbar.transform.localToWorldMatrix;
		Vector3 up = m.MultiplyVector(Vector3.up);
		Vector3 right = m.MultiplyVector(Vector3.right);
		
		float newScrollbarLength = tk2dUIControlsHelperEditor.DrawLengthHandles("Scrollbar Length", scrollbar.scrollBarLength, scrollbar.transform.position, isYAxis ? -up : right, Color.red, isYAxis ? .2f : -.2f, 0, .05f);
        if (newScrollbarLength != scrollbar.scrollBarLength)
        {
            tk2dUndo.RecordObject(scrollbar, "Scrollbar Length Changed");
            scrollbar.scrollBarLength = newScrollbarLength;
            wasChange = true;
        }

        if (scrollbar.thumbTransform != null)
        {
            Vector3 thumbStartPos = scrollbar.thumbTransform.position;
            if (isYAxis)
            {
                thumbStartPos += up*scrollbar.thumbLength/2;
            }
            else
            {
                thumbStartPos -= right*scrollbar.thumbLength/2;
            }
			
            float newThumbLength = tk2dUIControlsHelperEditor.DrawLengthHandles("Thumb Length", scrollbar.thumbLength, thumbStartPos, isYAxis ? -up : right, Color.blue, isYAxis ? -.15f : -.15f,isYAxis ? -.1f:.2f, .1f);
            if (newThumbLength != scrollbar.thumbLength)
            {
                tk2dUndo.RecordObject(scrollbar, "Thumb Length Changed");
                scrollbar.thumbLength = newThumbLength;
                wasChange = true;
            }
        }

        if (wasChange)
        {
            EditorUtility.SetDirty(scrollbar);
        }
    }

}
