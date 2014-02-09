using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Please don't add this component manually
[AddComponentMenu("")]
public class blaze2dUpdateManager : MonoBehaviour {
	static blaze2dUpdateManager inst;
	static blaze2dUpdateManager Instance {
		get {
			if (inst == null) {
				inst = GameObject.FindObjectOfType(typeof(blaze2dUpdateManager)) as blaze2dUpdateManager;
				if (inst == null) {
					GameObject go = new GameObject("@tk2dUpdateManager");
					go.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
					inst = go.AddComponent<blaze2dUpdateManager>();
					DontDestroyOnLoad(go);
				}
			}
			return inst;
		}
	}

	// Add textmeshes to the list here
	// Take care to not add twice
	// Never queue in edit mode
	public static void QueueCommit( blaze2dTextMesh textMesh ) {
#if UNITY_EDITOR
		if (!Application.isPlaying) {
			textMesh.DoNotUse__CommitInternal();
		}
		else 
#endif
		{
			Instance.QueueCommitInternal( textMesh );
		}
	}

	// This can be called more than once, and we do - to try and catch all possibilities
	public static void FlushQueues() {
#if UNITY_EDITOR
		if (Application.isPlaying) {
			Instance.FlushQueuesInternal();
		}
#else
		Instance.FlushQueuesInternal();
#endif
	}

	void OnEnable() {
		// for when the assembly is reloaded, coroutine is killed then
		StartCoroutine(coSuperLateUpdate());
	}

	// One in late update
	void LateUpdate() {
		FlushQueuesInternal();
	}

	IEnumerator coSuperLateUpdate() {
		FlushQueuesInternal();
		yield break;
	}

	void QueueCommitInternal( blaze2dTextMesh textMesh ) {
		textMeshes.Add( textMesh );
	}

	void FlushQueuesInternal() {
		int count = textMeshes.Count;
		for (int i = 0; i < count; ++i) {
			blaze2dTextMesh tm = textMeshes[i];
			if (tm != null) {
				tm.DoNotUse__CommitInternal();
			}
		}
		textMeshes.Clear();
	}

	// Preallocate these lists to avoid allocation later
	[SerializeField] List<blaze2dTextMesh> textMeshes = new List<blaze2dTextMesh>(64);
}
