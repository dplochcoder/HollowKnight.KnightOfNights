namespace KnightOfNights.Scripts.Framework
{
    [UnityEngine.RequireComponent(typeof(UnityEngine.Collider2D))]
    public class GradientWindFieldZone : UnityEngine.MonoBehaviour
    {
        public UnityEngine.Transform PointA;
        public UnityEngine.Vector2 SpeedA;
        public UnityEngine.Transform PointB;
        public UnityEngine.Vector2 SpeedB;
        public int Priority;
        
    }
}