using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(blaze2dUIHoverItem))]
public class tk2dUIHoverItemEditor : tk2dUIBaseItemControlEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        blaze2dUIHoverItem hoverBtn = (blaze2dUIHoverItem)target;

        hoverBtn.overStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Over State GameObject", hoverBtn.overStateGO,target);
        hoverBtn.outStateGO = tk2dUICustomEditorGUILayout.SceneObjectField("Out State GameObject", hoverBtn.outStateGO,target);

        BeginMessageGUI();
        methodBindingUtil.MethodBinding( "On Toggle Hover", typeof(blaze2dUIHoverItem), hoverBtn.SendMessageTarget, ref hoverBtn.SendMessageOnToggleHoverMethodName );
        EndMessageGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(hoverBtn);
        }
    }

}
