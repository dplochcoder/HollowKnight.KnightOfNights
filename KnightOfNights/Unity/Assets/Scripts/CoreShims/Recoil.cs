using UnityEngine;

public class Recoil : MonoBehaviour
{
    public bool freezeInPlace = false;
    public bool stopVelocityXWhenRecoilingUp = true;
    public bool preventRecoilUp = false;
    public float recoilSpeedBase = 15;
    public float recoilDuration = 0.15f;
}
