using UnityEngine;
using UnityEditor;
using System.Collections;

[CanEditMultipleObjects]
[CustomEditor(typeof(blaze2dUIToggleControl))]
public class tk2dUIToggleControlEditor : tk2dUIToggleButtonEditor
{
    protected override void DrawGUI()
    {
        base.DrawGUI();

        blaze2dUIToggleControl toggleBtn = (blaze2dUIToggleControl)target;
        toggleBtn.descriptionTextMesh = EditorGUILayout.ObjectField("Description Text Mesh", toggleBtn.descriptionTextMesh, typeof(blaze2dTextMesh), true) as blaze2dTextMesh;
    }

}