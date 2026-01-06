namespace KnightOfNights.Scripts.Proxy
{
    [UnityEngine.RequireComponent(typeof(UnityEngine.Collider2D))]
    public class BreakableProxy : UnityEngine.MonoBehaviour
    {
        public System.Collections.Generic.List<UnityEngine.GameObject> WholeParts;
        public System.Collections.Generic.List<UnityEngine.GameObject> RemnantParts;
        public System.Collections.Generic.List<UnityEngine.GameObject> DebrisParts;
        public float AngleOffset = -60;
        public UnityEngine.Vector3 EffectOffset;
        public float FlingSpeedMin;
        public float FlingSpeedMax;
        
    }
}