using UnityEngine;
using System.Collections;

[AddComponentMenu("2D Toolkit/Sprite/tk2dAnimatedSprite (Obsolete)")]
public class blaze2dAnimatedSprite : blaze2dSprite
{
#region SpriteAnimatorRouting
	[SerializeField] blaze2dSpriteAnimator _animator = null;
	public blaze2dSpriteAnimator Animator {
		get {
			CheckAddAnimatorInternal();
			return _animator;
		}
	}

	void CheckAddAnimatorInternal() {
		if (_animator == null) {
			_animator = gameObject.GetComponent<blaze2dSpriteAnimator>();
			if (_animator == null) {
				_animator = gameObject.AddComponent<blaze2dSpriteAnimator>();
				_animator.Library = anim;
				_animator.DefaultClipId = clipId;
				_animator.playAutomatically = playAutomatically;
			}
		}
	}
#endregion

	// Required for serialization
	[SerializeField] private blaze2dSpriteAnimation anim;
	[SerializeField] private int clipId = 0;
	public bool playAutomatically = false;
	public bool createCollider = false;

	// Required for backwards compatility
	protected override bool NeedBoxCollider() {
		return createCollider;
	}
	
	public blaze2dSpriteAnimation Library {
		get {
			return Animator.Library;
		}
		set {
			Animator.Library = value;
		}
	}

	public int DefaultClipId {
		get {
			return Animator.DefaultClipId;
		}
		set {
			Animator.DefaultClipId = value;
		}
	}

	// Wrapped functions
	public static bool g_paused
	{
		get { return blaze2dSpriteAnimator.g_Paused; }
		set { blaze2dSpriteAnimator.g_Paused = value; }
	}

	public bool Paused
	{
		get { return Animator.Paused; }
		set { Animator.Paused = value; }
	}
	
	/// <summary>
	/// Animation complete delegate 
	/// </summary>
	public delegate void AnimationCompleteDelegate(blaze2dAnimatedSprite sprite, int clipId);
	/// <summary>
	/// Animation complete event. This is called when the animation has completed playing. Will not trigger on looped animations
	/// </summary>
	public AnimationCompleteDelegate animationCompleteDelegate;
	
	/// <summary>
	/// Animation event delegate.
	/// </summary>
	public delegate void AnimationEventDelegate(blaze2dAnimatedSprite sprite, blaze2dSpriteAnimationClip clip, blaze2dSpriteAnimationFrame frame, int frameNum);
	/// <summary>
	/// Animation event. This is called when the frame displayed has <see cref="tk2dSpriteAnimationFrame.triggerEvent"/> set.
	/// The triggering frame is passed to the delegate, and the eventInfo / Int / Float can be extracted from there.
	/// </summary>
	public AnimationEventDelegate animationEventDelegate;

	void ProxyCompletedHandler(blaze2dSpriteAnimator anim, blaze2dSpriteAnimationClip clip) {
		if (animationCompleteDelegate != null) {
			int clipId = -1;
			blaze2dSpriteAnimationClip[] clips = (anim.Library != null) ? anim.Library.clips : null;
			if (clips != null) {
				for (int i = 0; i < clips.Length; ++i) {
					if (clips[i] == clip) {
						clipId = i;
						break;
					}
				}
			}

			animationCompleteDelegate(this, clipId);
		}
	}

	void ProxyEventTriggeredHandler(blaze2dSpriteAnimator anim, blaze2dSpriteAnimationClip clip, int frame) {
		if (animationEventDelegate != null) {
			animationEventDelegate(this, clip, clip.frames[frame], frame);
		}
	}

	void OnEnable() {
		Animator.AnimationCompleted = ProxyCompletedHandler;
		Animator.AnimationEventTriggered = ProxyEventTriggeredHandler;
	}

	void OnDisable() {
		Animator.AnimationCompleted = null;
		Animator.AnimationEventTriggered = null;
	}

	// execution order on tk2dSpriteAnimator should be AFTER tk2dAnimatedSprite
	void Start()
	{
		CheckAddAnimatorInternal();
	}
	
	/// <summary>
	/// Adds a tk2dAnimatedSprite as a component to the gameObject passed in, setting up necessary parameters and building geometry.
	/// </summary>
	public static blaze2dAnimatedSprite AddComponent(GameObject go, blaze2dSpriteAnimation anim, int clipId)
	{
		var clip = anim.clips[clipId];
		blaze2dAnimatedSprite animSprite = go.AddComponent<blaze2dAnimatedSprite>();
		animSprite.SetSprite(clip.frames[0].spriteCollection, clip.frames[0].spriteId);
		animSprite.anim = anim;
		return animSprite;
	}
	
#region Play
	public void Play() {
		if (Animator.DefaultClip != null) {
			Animator.Play(Animator.DefaultClip);
		}
	}
	public void Play(float clipStartTime) {
		if (Animator.DefaultClip != null) {
			Animator.PlayFrom(Animator.DefaultClip, clipStartTime);
		}
	}
	public void PlayFromFrame(int frame) {
		if (Animator.DefaultClip != null) {
			Animator.PlayFromFrame(Animator.DefaultClip, frame);
		}
	}
	public void Play(string name) {
		Animator.Play(name);
	}
	public void PlayFromFrame(string name, int frame) {
		Animator.PlayFromFrame(name, frame);
	}
	public void Play(string name, float clipStartTime) {
		Animator.PlayFrom(name, clipStartTime);
	}
	public void Play(blaze2dSpriteAnimationClip clip, float clipStartTime) {
		Animator.PlayFrom(clip, clipStartTime);
	}
	public void Play(blaze2dSpriteAnimationClip clip, float clipStartTime, float overrideFps) {
		Animator.Play(clip, clipStartTime, overrideFps);
	}
#endregion

	public blaze2dSpriteAnimationClip CurrentClip { get { return Animator.CurrentClip; } }
	public float ClipTimeSeconds { get { return Animator.ClipTimeSeconds; } }
	
	public float ClipFps {
		get { return Animator.ClipFps; }
		set { Animator.ClipFps = value; }
	}
	
	public void Stop() {
		Animator.Stop();
	}
	
	public void StopAndResetFrame() {
		Animator.StopAndResetFrame();
	}
	
	[System.Obsolete]
	public bool isPlaying() {
		return Animator.Playing;
	}

	public bool IsPlaying(string name) {
		return Animator.Playing;
	}

	public bool IsPlaying(blaze2dSpriteAnimationClip clip) {
		return Animator.IsPlaying(clip);
	}


	public bool Playing { 
		get { return Animator.Playing; }
	}
	
	public int GetClipIdByName(string name) {
		return Animator.GetClipIdByName(name);
	}

	public blaze2dSpriteAnimationClip GetClipByName(string name) {
		return Animator.GetClipByName(name);
	}
	
	public static float DefaultFps { get { return blaze2dSpriteAnimator.DefaultFps; } }

	public void Pause() {
		Animator.Pause();
	}
	
	public void Resume() {
		Animator.Resume();
	}
	
	public void SetFrame(int currFrame) {
		Animator.SetFrame(currFrame);
	}

	public void SetFrame(int currFrame, bool triggerEvent) {
		Animator.SetFrame(currFrame, triggerEvent);
	}
	
	public void UpdateAnimation(float deltaTime) {
		Animator.UpdateAnimation(deltaTime);
	}
}
