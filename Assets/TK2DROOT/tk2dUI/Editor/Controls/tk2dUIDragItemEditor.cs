using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(blaze2dUIDragItem))]
public class tk2dUIDragItemEditor : tk2dUIBaseItemControlEditor
{
    protected bool hasUIManagerCheckBeenDone = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        blaze2dUIDragItem dragButton = (blaze2dUIDragItem)target;

        if (GUI.changed)
        {
            EditorUtility.SetDirty(dragButton);
        }
    }

}
