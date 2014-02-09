using UnityEngine;
using System.Collections;

[AddComponentMenu("blaze2D/Backend/blaze2dFont")]
public class blaze2dFont : MonoBehaviour 
{
	public TextAsset bmFont;
	public Material material;
	public Texture texture;
	public Texture2D gradientTexture;
    public bool dupeCaps = false; // duplicate lowercase into uc, or vice-versa, depending on which exists
	public bool flipTextureY = false;
	
	[HideInInspector]
	public bool proxyFont = false;

	[HideInInspector][SerializeField]
	private bool useTk2dCamera = false;
	[HideInInspector][SerializeField]
	private int targetHeight = 640;
	[HideInInspector][SerializeField]
	private float targetOrthoSize = 1.0f;

	public blaze2dSpriteCollectionSize sizeDef = blaze2dSpriteCollectionSize.Default();
	
	public int gradientCount = 1;
	
	public bool manageMaterial = false;
	
	[HideInInspector]
	public bool loadable = false;
	
	public int charPadX = 0;
	
	public blaze2dFontData data;

	public static int CURRENT_VERSION = 1;
	public int version = 0;

	public void Upgrade() {
		if (version >= CURRENT_VERSION) {
			return;
		}
		Debug.Log("Font '" + this.name + "' - Upgraded from version " + version.ToString());

		if (version == 0) {
			sizeDef.CopyFromLegacy( useTk2dCamera, targetOrthoSize, targetHeight );
		}

		version = CURRENT_VERSION;
	}
}