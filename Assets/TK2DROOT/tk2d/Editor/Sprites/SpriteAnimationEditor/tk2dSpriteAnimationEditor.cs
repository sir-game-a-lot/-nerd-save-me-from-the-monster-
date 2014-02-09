using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(blaze2dSpriteAnimation))]
class tk2dSpriteAnimationEditor : Editor
{
    public static bool viewData = false;

    void OnEnable() {
        viewData = false;
    }

    public override void OnInspectorGUI()
    {
        blaze2dSpriteAnimation anim = (blaze2dSpriteAnimation)target;
        
        GUILayout.Space(8);
        if (anim != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open Editor...", GUILayout.MinWidth(120)))
            {
                tk2dSpriteAnimationEditorPopup v = EditorWindow.GetWindow( typeof(tk2dSpriteAnimationEditorPopup), false, "SpriteAnimation" ) as tk2dSpriteAnimationEditorPopup;
                v.SetSpriteAnimation(anim);
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        if (viewData) {
            GUILayout.BeginVertical("box");
            DrawDefaultInspector();
            GUILayout.EndVertical();
        }

        GUILayout.Space(64);
	}
	
    [MenuItem("CONTEXT/tk2dSpriteAnimation/View data")]
    static void ToggleViewData() {
        tk2dSpriteAnimationEditor.viewData = true;
    }

	[MenuItem("Assets/Create/tk2d/Sprite Animation", false, 10001)]
    static void DoAnimationCreate()
    {
		string path = tk2dEditorUtility.CreateNewPrefab("SpriteAnimation");
        if (path.Length != 0)
        {
            GameObject go = new GameObject();
            go.AddComponent<blaze2dSpriteAnimation>();
	        tk2dEditorUtility.SetGameObjectActive(go, false);

			Object p = PrefabUtility.CreateEmptyPrefab(path);
            PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
            GameObject.DestroyImmediate(go);
			
			tk2dEditorUtility.GetOrCreateIndex().AddSpriteAnimation(AssetDatabase.LoadAssetAtPath(path, typeof(blaze2dSpriteAnimation)) as blaze2dSpriteAnimation);
			tk2dEditorUtility.CommitIndex();

			// Select object
			Selection.activeObject = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
        }
    }	
}

