using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dSpriteFromTexture")]
[ExecuteInEditMode]
public class blaze2dSpriteFromTexture : MonoBehaviour {

	public Texture texture = null;
	public blaze2dSpriteCollectionSize spriteCollectionSize = new blaze2dSpriteCollectionSize();
	public blaze2dBaseSprite.Anchor anchor = blaze2dBaseSprite.Anchor.MiddleCenter;
	blaze2dSpriteCollectionData spriteCollection;

	blaze2dBaseSprite _sprite;
	blaze2dBaseSprite Sprite {
		get {
			if (_sprite == null) {
				_sprite = GetComponent<blaze2dBaseSprite>();
				if (_sprite == null) {
					Debug.Log("tk2dSpriteFromTexture - Missing sprite object. Creating.");
					_sprite = gameObject.AddComponent<blaze2dSprite>();
				}
			}
			return _sprite;
		}
	}

	void Awake() {
		Create( spriteCollectionSize, texture, anchor );
	}

	public bool HasSpriteCollection {
		get { return spriteCollection != null; }
	}

	void OnDestroy() {
		DestroyInternal();
		if (renderer != null) {
			renderer.material = null;
		}
	}

	public void Create( blaze2dSpriteCollectionSize spriteCollectionSize, Texture texture, blaze2dBaseSprite.Anchor anchor ) {
		DestroyInternal();
		if (texture != null) {
			// Copy values
			this.spriteCollectionSize.CopyFrom( spriteCollectionSize );
			this.texture = texture;
			this.anchor = anchor;

			GameObject go = new GameObject("tk2dSpriteFromTexture - " + texture.name);
			go.transform.localPosition = Vector3.zero;
			go.transform.localRotation = Quaternion.identity;
			go.transform.localScale = Vector3.one;
			go.hideFlags = HideFlags.DontSave;
			
			Vector2 anchorPos = blaze2dSpriteGeomGen.GetAnchorOffset( anchor, texture.width, texture.height );
			spriteCollection = blaze2dRuntime.SpriteCollectionGenerator.CreateFromTexture(
				go, 
				texture, 
				spriteCollectionSize,
				new Vector2(texture.width, texture.height),
				new string[] { "unnamed" } ,
				new Rect[] { new Rect(0, 0, texture.width, texture.height) },
				null,
				new Vector2[] { anchorPos },
				new bool[] { false } );

			string objName = "SpriteFromTexture " + texture.name;
			spriteCollection.spriteCollectionName = objName;
			spriteCollection.spriteDefinitions[0].material.name = objName;
			spriteCollection.spriteDefinitions[0].material.hideFlags = HideFlags.DontSave | HideFlags.HideInInspector;

			Sprite.SetSprite( spriteCollection, 0 );
		}
	}

	public void Clear() {
		DestroyInternal();
	}

	public void ForceBuild() {
		DestroyInternal();
		Create( spriteCollectionSize, texture, anchor );
	}

	void DestroyInternal() {
		if (spriteCollection != null) {
			if (spriteCollection.spriteDefinitions[0].material != null) {
				DestroyImmediate( spriteCollection.spriteDefinitions[0].material );
			}
			DestroyImmediate( spriteCollection.gameObject );
			spriteCollection = null;
		}
	}
}
