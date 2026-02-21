using UnityEngine;

/// <summary>
/// Attach to the HitFx_Player prefab. Plays the hit animation and destroys itself when the animation finishes.
/// </summary>
public class HitEffect : MonoBehaviour
{
    [SerializeField] [Tooltip("If 0, uses Animator state length. Otherwise destroys after this many seconds.")]
    private float lifetime = 0f;

    private void Start()
    {
        float duration = lifetime;
        if (duration <= 0f)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                duration = stateInfo.length;
            }
            if (duration <= 0f)
                duration = 0.5f;
        }
        Destroy(gameObject, duration);
    }
}
