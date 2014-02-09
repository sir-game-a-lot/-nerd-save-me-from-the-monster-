using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/UI/Core/tk2dUISpriteAnimator")]
public class blaze2dUISpriteAnimator : blaze2dSpriteAnimator {
	public override void LateUpdate()
	{
		UpdateAnimation(blaze2dUITime.deltaTime);
	}
}