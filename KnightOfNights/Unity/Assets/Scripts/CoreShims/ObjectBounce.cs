using UnityEngine;

public class ObjectBounce : MonoBehaviour
{
    public float bounceFactor = 0.4f;

    public float speedThreshold = 1f;

    public bool playSound = false;

    public AudioClip[] clips;

    public int chanceToPlay = 100;

    public float pitchMin = 1f;

    public float pitchMax = 1f;

    public bool playAnimationOnBounce = false;

    public bool sendFSMEvent = false;
}
