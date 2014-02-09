using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class tk2dSystemUtility
{
	static string GetObjectGUID(Object obj) { return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)); }

	// Make this asset loadable at runtime, using the guid and the given name
	// The guid MUST match the GUID of the object.
	// The name is arbitrary and should be unique to make all assets findable using name, 
	// but doesn't have to be. Name can be an empty string, but not null.
	public static bool MakeLoadableAsset(Object obj, string name)
	{
		string guid = GetObjectGUID(obj);

		// Check if it already is Loadable
		bool isLoadable = IsLoadableAsset(obj);
		if (isLoadable)
		{
			// Update name if it is different
			foreach (blaze2dResourceTocEntry t in blaze2dSystem.inst.Editor__Toc)
			{
				if (t.assetGUID == guid)
				{
					t.assetName = name;
					break;
				}
			}

			EditorUtility.SetDirty(blaze2dSystem.inst);

			// Already loadable
			return true;
		}

		// Create resource object
		string resourcePath = GetOrCreateResourcesDir() + "/" + "tk2d_" + guid + ".asset";
		blaze2dResource resource = ScriptableObject.CreateInstance<blaze2dResource>();
		resource.objectReference = obj;
		AssetDatabase.CreateAsset(resource, resourcePath);
		AssetDatabase.SaveAssets();

		// Add index entry
		blaze2dResourceTocEntry tocEntry = new blaze2dResourceTocEntry();
		tocEntry.resourceGUID = AssetDatabase.AssetPathToGUID(resourcePath);
		tocEntry.assetName = (name.Length == 0)?obj.name:name;
		tocEntry.assetGUID = guid;
		List<blaze2dResourceTocEntry> toc = new List<blaze2dResourceTocEntry>(blaze2dSystem.inst.Editor__Toc);
		toc.Add(tocEntry);
		blaze2dSystem.inst.Editor__Toc = toc.ToArray();

		EditorUtility.SetDirty(blaze2dSystem.inst);
		AssetDatabase.SaveAssets();

		return true;
	}

	// Deletes the asset from the global asset dictionary
	// and removes the associated the asset from the build
	public static bool UnmakeLoadableAsset(Object obj)
	{
		string guid = GetObjectGUID(obj);
		List<blaze2dResourceTocEntry> toc = new List<blaze2dResourceTocEntry>(blaze2dSystem.inst.Editor__Toc);
		foreach (blaze2dResourceTocEntry entry in toc)
		{
			if (entry.assetGUID == guid)
			{
				// Delete the corresponding resource object
				string resourceObjectPath = AssetDatabase.GUIDToAssetPath(entry.resourceGUID);
				AssetDatabase.DeleteAsset(resourceObjectPath);

				// remove from TOC
				toc.Remove(entry);
				break;
			}
		}

		if (blaze2dSystem.inst.Editor__Toc.Length == toc.Count)
		{
			Debug.LogError("tk2dSystem.UnmakeLoadableAsset - Unable to delete asset");
			return false;
		}
		else
		{
			blaze2dSystem.inst.Editor__Toc = toc.ToArray();
			EditorUtility.SetDirty(blaze2dSystem.inst);
			AssetDatabase.SaveAssets();
			
			return true;
		}
	}

	// Update asset name
	public static void UpdateAssetName(Object obj, string name)
	{
		MakeLoadableAsset(obj, name);
	}

	// This will return false if the system hasn't been initialized, without initializing it.
	public static bool IsLoadableAsset(Object obj)
	{
		string resourcesDir = GetResourcesDir();
		if (resourcesDir.Length == 0) // tk2dSystem hasn't been initialized yet
			return false;

		string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj));
		string resourcePath = GetResourcesDir() + "/tk2d_" + guid + ".asset";
		return System.IO.File.Exists(resourcePath);
	}

	// Returns the path to the global resources directory
	// It is /Assets/TK2DSYSTEM/Resources by default, but can be moved anywhere
	// When the tk2dSystem object exists, the path to the object will be returned
	public static string GetOrCreateResourcesDir()
	{
		blaze2dSystem inst = blaze2dSystem.inst;
		string assetPath = AssetDatabase.GetAssetPath(inst);
		if (assetPath.Length > 0)
		{
			// This has already been serialized, just return path as is
			return System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/'); // already serialized
		}
		else
		{
			// Create the system asset
			const string resPath = "Assets/Resources";
			if (!System.IO.Directory.Exists(resPath)) System.IO.Directory.CreateDirectory(resPath);

			const string basePath = resPath + "/tk2d";
			if (!System.IO.Directory.Exists(basePath)) System.IO.Directory.CreateDirectory(basePath);

			assetPath = basePath + "/" + blaze2dSystem.assetFileName;
			AssetDatabase.CreateAsset(inst, assetPath);
			
			return basePath;
		}
	}

	// Returns the path to the global resources directory
	// Will not create if it doesn't exists
	static string GetResourcesDir()
	{
		blaze2dSystem inst = blaze2dSystem.inst_NoCreate;
		if (inst == null) 
			return "";
		else return GetOrCreateResourcesDir(); // this already exists, so this function will follow the correct path
	}

	// Call when platform has changed
	public static void PlatformChanged()
	{
		List<blaze2dSpriteCollectionData> changedSpriteCollections = new List<blaze2dSpriteCollectionData>();
		blaze2dSpriteCollectionData[] allSpriteCollections = Resources.FindObjectsOfTypeAll(typeof(blaze2dSpriteCollectionData)) as blaze2dSpriteCollectionData[];
		foreach (blaze2dSpriteCollectionData scd in allSpriteCollections)
		{
			if (scd.hasPlatformData)
			{
				scd.ResetPlatformData();
				changedSpriteCollections.Add(scd);
			}
		}
		allSpriteCollections = null;

		blaze2dFontData[] allFontDatas = Resources.FindObjectsOfTypeAll(typeof(blaze2dFontData)) as blaze2dFontData[];
		foreach (blaze2dFontData fd in allFontDatas)
		{
			if (fd.hasPlatformData)
			{
				fd.ResetPlatformData();
			}
		}
		allFontDatas = null;

		if (changedSpriteCollections.Count == 0)
			return; // nothing worth changing has changed

		// Scan all loaded sprite assets and rebuild them
		foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
		{
			try
			{
				System.Type[] types = assembly.GetTypes();
				foreach (var type in types)
				{
					if (type.GetInterface("blaze2dRuntime.ISpriteCollectionForceBuild") != null)
					{
						Object[] objects = Resources.FindObjectsOfTypeAll(type);

						foreach (var spriteCollectionData in changedSpriteCollections)
						{
							foreach (var o in objects)
							{
								if (tk2dEditorUtility.IsPrefab(o))
									continue;

								blaze2dRuntime.ISpriteCollectionForceBuild isc = o as blaze2dRuntime.ISpriteCollectionForceBuild;
								if (isc.UsesSpriteCollection(spriteCollectionData))
									isc.ForceBuild();
							}
						}
					}
				}
			}
			catch { }
		}
	}

	public static void RebuildResources()
	{
		// Delete all existing resources
		string systemFileName = blaze2dSystem.assetFileName.ToLower();
		string tk2dIndexDir = "Assets/Resources/tk2d";
		if (System.IO.Directory.Exists(tk2dIndexDir))
		{
			string[] files = System.IO.Directory.GetFiles(tk2dIndexDir);
			foreach (string file in files)
			{
				string filename = System.IO.Path.GetFileName(file).ToLower();
				if (filename.IndexOf(systemFileName) != -1) continue; // don't delete system object
				if (filename.IndexOf("tk2d_") == -1)
				{
					Debug.LogError(string.Format("Unknown file '{0}' in tk2d resources directory, ignoring.", filename));
					continue;
				}
				AssetDatabase.DeleteAsset(file);
			}
		}

		// Delete all referenced resources, in the event they've been moved out of the directory
		if (blaze2dSystem.inst_NoCreate != null)
		{
			blaze2dSystem sys = blaze2dSystem.inst;
			blaze2dResourceTocEntry[] toc = sys.Editor__Toc;
			for (int i = 0; i < toc.Length; ++i)
			{
				string path = AssetDatabase.GUIDToAssetPath(toc[i].resourceGUID);
				if (path.Length > 0)
					AssetDatabase.DeleteAsset(path);
			}
			sys.Editor__Toc = new blaze2dResourceTocEntry[0]; // clear index
			EditorUtility.SetDirty(sys);
			AssetDatabase.SaveAssets();
		}

		AssetDatabase.Refresh();

		// Need to create new index?
		tk2dSpriteCollectionIndex[] spriteCollectionIndex = tk2dEditorUtility.GetExistingIndex().GetSpriteCollectionIndex();
		tk2dGenericIndexItem[] fontIndex = tk2dEditorUtility.GetExistingIndex().GetFonts();
		int numLoadableAssets = 0;
		foreach (tk2dGenericIndexItem font in fontIndex) { if (font.managed || font.loadable) numLoadableAssets++; }
		foreach (tk2dSpriteCollectionIndex sc in spriteCollectionIndex) { if (sc.managedSpriteCollection || sc.loadable) numLoadableAssets++; }

		// Need an index
		if (numLoadableAssets > 0)
		{
			// If it already existed, the index would have been cleared by now
			blaze2dSystem sys = blaze2dSystem.inst;

			foreach (tk2dGenericIndexItem font in fontIndex)
			{
				if (font.managed || font.loadable) AddFontFromIndex(font);
				tk2dEditorUtility.CollectAndUnloadUnusedAssets();
			}
			foreach (tk2dSpriteCollectionIndex sc in spriteCollectionIndex)
			{
				if (sc.managedSpriteCollection || sc.loadable) AddSpriteCollectionFromIndex(sc);
				tk2dEditorUtility.CollectAndUnloadUnusedAssets();
			}

			Debug.Log(string.Format("Rebuilt {0} resources for tk2dSystem", sys.Editor__Toc.Length));
		}

		tk2dEditorUtility.CollectAndUnloadUnusedAssets();
	}

	static void AddSpriteCollectionFromIndex(tk2dSpriteCollectionIndex indexEntry)
	{
		string path = AssetDatabase.GUIDToAssetPath( indexEntry.spriteCollectionDataGUID );
		blaze2dSpriteCollectionData data = AssetDatabase.LoadAssetAtPath(path, typeof(blaze2dSpriteCollectionData)) as blaze2dSpriteCollectionData;
		if (data == null)
		{
			Debug.LogError(string.Format("Unable to load sprite collection '{0}' at path '{1}'", indexEntry.name, path));
			return;
		}
		MakeLoadableAsset(data, indexEntry.managedSpriteCollection ? " " : data.assetName);
		data = null;
	}

	static void AddFontFromIndex(tk2dGenericIndexItem indexEntry)
	{
		string path = AssetDatabase.GUIDToAssetPath( indexEntry.dataGUID );
		blaze2dFontData data = AssetDatabase.LoadAssetAtPath(path, typeof(blaze2dFontData)) as blaze2dFontData;
		if (data == null)
		{
			Debug.LogError(string.Format("Unable to load font data '{0}' at path '{1}'", indexEntry.AssetName, path));
			return;
		}
		MakeLoadableAsset(data, ""); // can't make it directly loadable, hence no asset name
		data = null;
	}
}
