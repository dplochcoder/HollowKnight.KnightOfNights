namespace KnightOfNights.Scripts.Proxy
{
    [UnityEngine.RequireComponent(typeof(UnityEngine.BoxCollider2D))]
    public class GrassProxy : UnityEngine.MonoBehaviour
    {
        public UnityEngine.SpriteRenderer GrassPreview;
        public UnityEngine.Material GrassParticles;
        public int NumParticles = 25;
        public float HeightOffset;
        public float SwayAmount;
        public float SwayAmountVariance;
        public float SwaySpeed;
        public float SwaySpeedVariance;
        
    }
}