using UnityEngine;
using System.Collections;

public abstract class blaze2dUILayoutContainer : blaze2dUILayout {
	protected Vector2 innerSize = Vector2.zero;
	public Vector2 GetInnerSize() {
		return innerSize;
	}

	protected abstract void DoChildLayout();

	public event System.Action OnChangeContent;

	public override void Reshape(Vector3 dMin, Vector3 dMax, bool updateChildren) {
		bMin += dMin;
		bMax += dMax;
		// Anchor top-left
		Vector3 origin = new Vector3(bMin.x, bMax.y);
		transform.position += origin;
		bMin -= origin;
		bMax -= origin;

		DoChildLayout();

		if (OnChangeContent != null)
			OnChangeContent();
	}

	public void AddLayout(blaze2dUILayout layout, blaze2dUILayoutItem item) {
		item.gameObj = layout.gameObject;
		item.layout = layout;
		layoutItems.Add(item);

		layout.gameObject.transform.parent = transform;

		Refresh();
	}

	public void AddLayoutAtIndex(blaze2dUILayout layout, blaze2dUILayoutItem item, int index) {
		item.gameObj = layout.gameObject;
		item.layout = layout;
		layoutItems.Insert(index, item);

		layout.gameObject.transform.parent = transform;

		Refresh();
	}

	public void RemoveLayout(blaze2dUILayout layout) {
		foreach (var item in layoutItems) {
			if (item.layout == layout) {
				layoutItems.Remove(item);
				layout.gameObject.transform.parent = null;
				break;
			}
		}

		Refresh();
	}
}