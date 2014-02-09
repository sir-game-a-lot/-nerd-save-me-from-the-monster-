using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(blaze2dSpriteFromTexture))]
class tk2dSpriteFromTextureEditor : Editor {

	public override void OnInspectorGUI() {
		blaze2dSpriteFromTexture target = (blaze2dSpriteFromTexture)this.target;
		tk2dGuiUtility.LookLikeInspector();

		EditorGUI.BeginChangeCheck();

		Texture texture = EditorGUILayout.ObjectField("Texture", target.texture, typeof(Texture), false) as Texture;

		if (texture == null) {
			tk2dGuiUtility.LookLikeControls();
			tk2dGuiUtility.InfoBox("Drag a texture into the texture slot above.", tk2dGuiUtility.WarningLevel.Error);
		}

		blaze2dBaseSprite.Anchor anchor = target.anchor;
		blaze2dSpriteCollectionSize spriteCollectionSize = new blaze2dSpriteCollectionSize();
		spriteCollectionSize.CopyFrom( target.spriteCollectionSize );

		if (texture != null) {
			anchor = (blaze2dBaseSprite.Anchor)EditorGUILayout.EnumPopup("Anchor", target.anchor);
			tk2dGuiUtility.SpriteCollectionSize(spriteCollectionSize);
		}

		if (EditorGUI.EndChangeCheck()) {
			tk2dUndo.RecordObject( target, "Sprite From Texture" );
			target.Create( spriteCollectionSize, texture, anchor );
		}
	}

    [MenuItem("GameObject/Create Other/tk2d/Sprite From Selected Texture", true, 12952)]
    static bool ValidateCreateSpriteObjectFromTexture()
    {
    	return Selection.activeObject != null && Selection.activeObject is Texture;
    }

    [MenuItem("GameObject/Create Other/tk2d/Sprite From Texture", true, 12953)]
    static bool ValidateCreateSpriteObject()
    {
    	return Selection.activeObject == null || !(Selection.activeObject is Texture);
    }

    [MenuItem("GameObject/Create Other/tk2d/Sprite From Selected Texture", false, 12952)]
    [MenuItem("GameObject/Create Other/tk2d/Sprite From Texture", false, 12953)]
    static void DoCreateSpriteObjectFromTexture()
    {
    	Texture tex = Selection.activeObject as Texture;
 
 		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Sprite");
		go.AddComponent<blaze2dSprite>();
		blaze2dSpriteFromTexture sft = go.AddComponent<blaze2dSpriteFromTexture>();
		if (tex != null) {
			blaze2dSpriteCollectionSize scs = blaze2dSpriteCollectionSize.Default();
			if (blaze2dCamera.Instance != null) {
				scs = blaze2dSpriteCollectionSize.ForTk2dCamera(blaze2dCamera.Instance);
			}
			sft.Create( scs, tex, blaze2dBaseSprite.Anchor.MiddleCenter );
		}
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create Sprite From Texture");
    }
}
