using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(blaze2dStaticSpriteBatcher))]
class tk2dStaticSpriteBatcherEditor : Editor
{
	blaze2dStaticSpriteBatcher batcher { get { return (blaze2dStaticSpriteBatcher)target; } }
	
	// Like GetComponentsInChildren, but doesn't include self
	T[] GetComponentsInChildrenExcludeSelf<T>(Transform root) where T : Component {
		List<T> allTransforms = new List<T>( root.GetComponentsInChildren<T>() );
		return (from t in allTransforms where t.transform != root select t).ToArray();
	}

	void DrawEditorGUI()
	{
		if (GUILayout.Button("Commit"))
		{
			// Select all children, EXCLUDING self
			Transform[] allTransforms = batcher.transform.GetComponentsInChildren<Transform>();
			allTransforms = (from t in allTransforms where t != batcher.transform select t).ToArray();
			
			// sort sprites, smaller to larger z
			if (batcher.CheckFlag(blaze2dStaticSpriteBatcher.Flags.SortToCamera)) {
				blaze2dCamera tk2dCam = blaze2dCamera.CameraForLayer( batcher.gameObject.layer );
				Camera cam = tk2dCam ? tk2dCam.camera : Camera.main;
				allTransforms = (from t in allTransforms orderby cam.WorldToScreenPoint((t.renderer != null) ? t.renderer.bounds.center : t.position).z descending select t).ToArray();
			}
			else {
				allTransforms = (from t in allTransforms orderby ((t.renderer != null) ? t.renderer.bounds.center : t.position).z descending select t).ToArray();
			}
			
			// and within the z sort by material
			if (allTransforms.Length == 0)
			{
				EditorUtility.DisplayDialog("StaticSpriteBatcher", "Error: No child objects found", "Ok");
				return;
			}


#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
			MeshCollider[] childMeshColliders = GetComponentsInChildrenExcludeSelf<MeshCollider>(batcher.transform);
			BoxCollider[] childBoxColliders = GetComponentsInChildrenExcludeSelf<BoxCollider>(batcher.transform);
			BoxCollider2D[] childBoxCollider2Ds = GetComponentsInChildrenExcludeSelf<BoxCollider2D>(batcher.transform);
			EdgeCollider2D[] childEdgeCollider2Ds = GetComponentsInChildrenExcludeSelf<EdgeCollider2D>(batcher.transform);
			PolygonCollider2D[] childPolygonCollider2Ds = GetComponentsInChildrenExcludeSelf<PolygonCollider2D>(batcher.transform);

			if ((childMeshColliders.Length > 0 || childBoxColliders.Length > 0) && (childBoxCollider2Ds.Length > 0 || childEdgeCollider2Ds.Length > 0 || childPolygonCollider2Ds.Length > 0)) {
				EditorUtility.DisplayDialog("StaticSpriteBatcher", "Error: Can't mix 2D and 3D colliders", "Ok");
				return;
			}
#endif

			Dictionary<Transform, int> batchedSpriteLookup = new Dictionary<Transform, int>();
			batchedSpriteLookup[batcher.transform] = -1;

			Matrix4x4 batcherWorldToLocal = batcher.transform.worldToLocalMatrix;
			
			batcher.spriteCollection = null;
			batcher.batchedSprites = new blaze2dBatchedSprite[allTransforms.Length];
			List<blaze2dTextMeshData> allTextMeshData = new List<blaze2dTextMeshData>();

			int currBatchedSprite = 0;
			foreach (var t in allTransforms)
			{
				blaze2dBaseSprite baseSprite = t.GetComponent<blaze2dBaseSprite>();
				blaze2dTextMesh textmesh = t.GetComponent<blaze2dTextMesh>();

				blaze2dBatchedSprite bs = new blaze2dBatchedSprite();
				bs.name = t.gameObject.name;
				bs.position = t.localPosition;
				bs.rotation = t.localRotation;
				bs.relativeMatrix = batcherWorldToLocal * t.localToWorldMatrix;

				if (baseSprite)
				{
					bs.baseScale = Vector3.one;
					bs.localScale = new Vector3(t.localScale.x * baseSprite.scale.x, t.localScale.y * baseSprite.scale.y, t.localScale.z * baseSprite.scale.z);
					FillBatchedSprite(bs, t.gameObject);

					// temp redundant - just incase batcher expects to point to a valid one, somewhere we've missed
					batcher.spriteCollection = baseSprite.Collection;
				}
				else if (textmesh)
				{
					bs.spriteCollection = null;

					bs.type = blaze2dBatchedSprite.Type.TextMesh;
					bs.color = textmesh.color;
					bs.baseScale = textmesh.scale;
					bs.renderLayer = textmesh.SortingOrder;
					bs.localScale = new Vector3(t.localScale.x * textmesh.scale.x, t.localScale.y * textmesh.scale.y, t.localScale.z * textmesh.scale.z);
					bs.FormattedText = textmesh.FormattedText;

					blaze2dTextMeshData tmd = new blaze2dTextMeshData();
					tmd.font = textmesh.font;
					tmd.text = textmesh.text;
					tmd.color = textmesh.color;
					tmd.color2 = textmesh.color2;
					tmd.useGradient = textmesh.useGradient;
					tmd.textureGradient = textmesh.textureGradient;
					tmd.anchor = textmesh.anchor;
					tmd.kerning = textmesh.kerning;
					tmd.maxChars = textmesh.maxChars;
					tmd.inlineStyling = textmesh.inlineStyling;
					tmd.formatting = textmesh.formatting;
					tmd.wordWrapWidth = textmesh.wordWrapWidth;
					tmd.spacing = textmesh.Spacing;
					tmd.lineSpacing = textmesh.LineSpacing;

					bs.xRefId = allTextMeshData.Count;
					allTextMeshData.Add(tmd);
				}
				else
				{
					// Empty GameObject
					bs.spriteId = -1;
					bs.baseScale = Vector3.one;
					bs.localScale = t.localScale;
					bs.type = blaze2dBatchedSprite.Type.EmptyGameObject;
				}

				
				batchedSpriteLookup[t] = currBatchedSprite;
				batcher.batchedSprites[currBatchedSprite++] = bs;
			}
			batcher.allTextMeshData = allTextMeshData.ToArray();
			
			int idx = 0;
			foreach (var t in allTransforms)
			{
				var bs = batcher.batchedSprites[idx];

				bs.parentId = batchedSpriteLookup[t.parent];
				t.parent = batcher.transform; // unparent
				++idx;
			}
			
			Transform[] directChildren = (from t in allTransforms where t.parent == batcher.transform select t).ToArray();
			foreach (var t in directChildren)
			{
				GameObject.DestroyImmediate(t.gameObject);
			}
			
			Vector3 inverseScale = new Vector3(1.0f / batcher.scale.x, 1.0f / batcher.scale.y, 1.0f / batcher.scale.z);
			batcher.transform.localScale = Vector3.Scale( batcher.transform.localScale, inverseScale );
			batcher.Build();
			EditorUtility.SetDirty(target);
		}
	}

	static void RestoreBoxColliderSettings( GameObject go, float offset, float extents ) {
		BoxCollider boxCollider = go.GetComponent<BoxCollider>();
		if (boxCollider != null) {
			Vector3 p = boxCollider.center;
			p.z = offset;
			boxCollider.center = p;
			p = boxCollider.size;
			p.z = extents * 2;
			boxCollider.size = p;
		}
	}
	
	public static void FillBatchedSprite(blaze2dBatchedSprite bs, GameObject go) {
		blaze2dSprite srcSprite = go.transform.GetComponent<blaze2dSprite>();
		blaze2dTiledSprite srcTiledSprite = go.transform.GetComponent<blaze2dTiledSprite>();
		blaze2dSlicedSprite srcSlicedSprite = go.transform.GetComponent<blaze2dSlicedSprite>();
		blaze2dClippedSprite srcClippedSprite = go.transform.GetComponent<blaze2dClippedSprite>();

		blaze2dBaseSprite baseSprite = go.GetComponent<blaze2dBaseSprite>();
		bs.spriteId = baseSprite.spriteId;
		bs.spriteCollection = baseSprite.Collection;
		bs.baseScale = baseSprite.scale;
		bs.color = baseSprite.color;
		bs.renderLayer = baseSprite.SortingOrder;
		if (baseSprite.boxCollider != null)
		{
			bs.BoxColliderOffsetZ = baseSprite.boxCollider.center.z;
			bs.BoxColliderExtentZ = baseSprite.boxCollider.size.z * 0.5f;
		}
		else {
			bs.BoxColliderOffsetZ = 0.0f;
			bs.BoxColliderExtentZ = 1.0f;
		}

		if (srcSprite) {
			bs.type = blaze2dBatchedSprite.Type.Sprite;
		}
		else if (srcTiledSprite) {
			bs.type = blaze2dBatchedSprite.Type.TiledSprite;
			bs.Dimensions = srcTiledSprite.dimensions;
			bs.anchor = srcTiledSprite.anchor;
			bs.SetFlag(blaze2dBatchedSprite.Flags.Sprite_CreateBoxCollider, srcTiledSprite.CreateBoxCollider);
		}
		else if (srcSlicedSprite) {
			bs.type = blaze2dBatchedSprite.Type.SlicedSprite;
			bs.Dimensions = srcSlicedSprite.dimensions;
			bs.anchor = srcSlicedSprite.anchor;
			bs.SetFlag(blaze2dBatchedSprite.Flags.Sprite_CreateBoxCollider, srcSlicedSprite.CreateBoxCollider);
			bs.SetFlag(blaze2dBatchedSprite.Flags.SlicedSprite_BorderOnly, srcSlicedSprite.BorderOnly);
			bs.SlicedSpriteBorderBottomLeft = new Vector2(srcSlicedSprite.borderLeft, srcSlicedSprite.borderBottom);
			bs.SlicedSpriteBorderTopRight = new Vector2(srcSlicedSprite.borderRight, srcSlicedSprite.borderTop);
		}
		else if (srcClippedSprite) {
			bs.type = blaze2dBatchedSprite.Type.ClippedSprite;
			bs.ClippedSpriteRegionBottomLeft = srcClippedSprite.clipBottomLeft;
			bs.ClippedSpriteRegionTopRight = srcClippedSprite.clipTopRight;
			bs.SetFlag(blaze2dBatchedSprite.Flags.Sprite_CreateBoxCollider, srcClippedSprite.CreateBoxCollider);
		}
	}

	// This is used by other parts of code
	public static void RestoreBatchedSprite(GameObject go, blaze2dBatchedSprite bs) {
		blaze2dBaseSprite baseSprite = null;
		switch (bs.type) {
			case blaze2dBatchedSprite.Type.EmptyGameObject:
				{
					break;
				}
			case blaze2dBatchedSprite.Type.Sprite:
				{
					blaze2dSprite s = blaze2dBaseSprite.AddComponent<blaze2dSprite>(go, bs.spriteCollection, bs.spriteId);
					baseSprite = s;
					break;
				}
			case blaze2dBatchedSprite.Type.TiledSprite:
				{
					blaze2dTiledSprite s = blaze2dBaseSprite.AddComponent<blaze2dTiledSprite>(go, bs.spriteCollection, bs.spriteId);
					baseSprite = s;
					s.dimensions = bs.Dimensions;
					s.anchor = bs.anchor;
					s.CreateBoxCollider = bs.CheckFlag(blaze2dBatchedSprite.Flags.Sprite_CreateBoxCollider);
					RestoreBoxColliderSettings(s.gameObject, bs.BoxColliderOffsetZ, bs.BoxColliderExtentZ);
					break;
				}
			case blaze2dBatchedSprite.Type.SlicedSprite:
				{
					blaze2dSlicedSprite s = blaze2dBaseSprite.AddComponent<blaze2dSlicedSprite>(go, bs.spriteCollection, bs.spriteId);
					baseSprite = s;
					s.dimensions = bs.Dimensions;
					s.anchor = bs.anchor;

					s.BorderOnly = bs.CheckFlag(blaze2dBatchedSprite.Flags.SlicedSprite_BorderOnly);
					s.SetBorder(bs.SlicedSpriteBorderBottomLeft.x, bs.SlicedSpriteBorderBottomLeft.y, bs.SlicedSpriteBorderTopRight.x, bs.SlicedSpriteBorderTopRight.y);

					s.CreateBoxCollider = bs.CheckFlag(blaze2dBatchedSprite.Flags.Sprite_CreateBoxCollider);
					RestoreBoxColliderSettings(s.gameObject, bs.BoxColliderOffsetZ, bs.BoxColliderExtentZ);
					break;
				}
			case blaze2dBatchedSprite.Type.ClippedSprite:
				{
					blaze2dClippedSprite s = blaze2dBaseSprite.AddComponent<blaze2dClippedSprite>(go, bs.spriteCollection, bs.spriteId);
					baseSprite = s;
					s.clipBottomLeft = bs.ClippedSpriteRegionBottomLeft;
					s.clipTopRight = bs.ClippedSpriteRegionTopRight;

					s.CreateBoxCollider = bs.CheckFlag(blaze2dBatchedSprite.Flags.Sprite_CreateBoxCollider);
					RestoreBoxColliderSettings(s.gameObject, bs.BoxColliderOffsetZ, bs.BoxColliderExtentZ);
					break;
				}
		}
		if (baseSprite != null) {
			baseSprite.SortingOrder = bs.renderLayer;
			baseSprite.scale = bs.baseScale;
			baseSprite.color = bs.color;
		}
	}

	void DrawInstanceGUI()
	{
		if (GUILayout.Button("Edit"))
	    {
			Vector3 batcherPos = batcher.transform.position;
			Quaternion batcherRotation = batcher.transform.rotation;
			batcher.transform.position = Vector3.zero;
			batcher.transform.rotation = Quaternion.identity;
			batcher.transform.localScale = Vector3.Scale(batcher.transform.localScale, batcher.scale);
			
			Dictionary<int, Transform> parents = new Dictionary<int, Transform>();
			List<Transform> children = new List<Transform>();

			List<GameObject> gos = new List<GameObject>();

			int id;

			id = 0;
			foreach (var bs in batcher.batchedSprites)
			{
				GameObject go = new GameObject(bs.name);
				go.layer = batcher.gameObject.layer;

				parents[id++] = go.transform;
				children.Add(go.transform);
				gos.Add (go);
			}

			id = 0;
			foreach (var bs in batcher.batchedSprites)
			{
				Transform parent = batcher.transform;
				if (bs.parentId != -1)
					parents.TryGetValue(bs.parentId, out parent);
				
				children[id++].parent = parent;
			}
			
			id = 0;
			foreach (var bs in batcher.batchedSprites)
			{
				GameObject go = gos[id];
				
				go.transform.localPosition = bs.position;
				go.transform.localRotation = bs.rotation;
				{
					float sx = bs.localScale.x / ((Mathf.Abs (bs.baseScale.x) > Mathf.Epsilon) ? bs.baseScale.x : 1.0f);
					float sy = bs.localScale.y / ((Mathf.Abs (bs.baseScale.y) > Mathf.Epsilon) ? bs.baseScale.y : 1.0f);
					float sz = bs.localScale.z / ((Mathf.Abs (bs.baseScale.z) > Mathf.Epsilon) ? bs.baseScale.z : 1.0f);
					go.transform.localScale = new Vector3(sx, sy, sz);
				}

				if (bs.type == blaze2dBatchedSprite.Type.TextMesh) {
					blaze2dTextMesh s = go.AddComponent<blaze2dTextMesh>();
					if (batcher.allTextMeshData == null || bs.xRefId == -1) {
						Debug.LogError("Unable to find text mesh ref");
					}
					else {
						blaze2dTextMeshData tmd = batcher.allTextMeshData[bs.xRefId];
						s.font = tmd.font;
						s.scale = bs.baseScale;
						s.SortingOrder = bs.renderLayer;
						s.text = tmd.text;
						s.color = bs.color;
						s.color2 = tmd.color2;
						s.useGradient = tmd.useGradient;
						s.textureGradient = tmd.textureGradient;
						s.anchor = tmd.anchor;
						s.scale = bs.baseScale;
						s.kerning = tmd.kerning;
						s.maxChars = tmd.maxChars;
						s.inlineStyling = tmd.inlineStyling;
						s.formatting = tmd.formatting;
						s.wordWrapWidth = tmd.wordWrapWidth;
						s.Spacing = tmd.spacing;
						s.LineSpacing = tmd.lineSpacing;
						s.Commit();
					}
				} 
				else {
					RestoreBatchedSprite(go, bs);
				}
				
				++id;
			}
			
			batcher.batchedSprites = null;
			batcher.Build();
			EditorUtility.SetDirty(target);

			batcher.transform.position = batcherPos;
			batcher.transform.rotation = batcherRotation;
		}

		batcher.scale = EditorGUILayout.Vector3Field("Scale", batcher.scale);

		batcher.SetFlag(blaze2dStaticSpriteBatcher.Flags.GenerateCollider, EditorGUILayout.Toggle("Generate Collider", batcher.CheckFlag(blaze2dStaticSpriteBatcher.Flags.GenerateCollider)));
		batcher.SetFlag(blaze2dStaticSpriteBatcher.Flags.FlattenDepth, EditorGUILayout.Toggle("Flatten Depth", batcher.CheckFlag(blaze2dStaticSpriteBatcher.Flags.FlattenDepth)));
		batcher.SetFlag(blaze2dStaticSpriteBatcher.Flags.SortToCamera, EditorGUILayout.Toggle("Sort to Camera", batcher.CheckFlag(blaze2dStaticSpriteBatcher.Flags.SortToCamera)));

		MeshFilter meshFilter = batcher.GetComponent<MeshFilter>();
		MeshRenderer meshRenderer = batcher.GetComponent<MeshRenderer>();

		if (meshFilter != null && meshFilter.sharedMesh != null && meshRenderer != null) {
			GUILayout.Label("Stats", EditorStyles.boldLabel);
			int numIndices = 0;
			Mesh mesh = meshFilter.sharedMesh;
			for (int i = 0; i < mesh.subMeshCount; ++i) {
				numIndices += mesh.GetTriangles(i).Length;
			}
			GUILayout.Label(string.Format("Triangles: {0}\nMaterials: {1}", numIndices / 3, meshRenderer.sharedMaterials.Length ));
		}
	}
	
    public override void OnInspectorGUI()
    {
		if (batcher.batchedSprites == null || batcher.batchedSprites.Length == 0) {
			DrawEditorGUI();
		}
		else {
			DrawInstanceGUI();
		}
    }
	
    [MenuItem("GameObject/Create Other/tk2d/Static Sprite Batcher", false, 13849)]
    static void DoCreateSpriteObject()
    {
		GameObject go = tk2dEditorUtility.CreateGameObjectInScene("Static Sprite Batcher");
		blaze2dStaticSpriteBatcher batcher = go.AddComponent<blaze2dStaticSpriteBatcher>();
		batcher.version = blaze2dStaticSpriteBatcher.CURRENT_VERSION;
		
		Selection.activeGameObject = go;
		Undo.RegisterCreatedObjectUndo(go, "Create Static Sprite Batcher");
    }
}

