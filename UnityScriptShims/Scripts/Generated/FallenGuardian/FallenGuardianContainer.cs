namespace KnightOfNights.Scripts.FallenGuardian
{
    public class FallenGuardianContainer : UnityEngine.MonoBehaviour
    {
        public KnightOfNights.Scripts.Proxy.HeroDetectorProxy Trigger;
        public FallenGuardianController Boss;
        public UnityEngine.BoxCollider2D Arena;
        public UnityEngine.BoxCollider2D DaggerBox;
        public UnityEngine.BoxCollider2D GorbStormFinaleBox;
        public System.Collections.Generic.List<UnityEngine.GameObject> DeactivateOnFight;
        public System.Collections.Generic.List<UnityEngine.GameObject> ActivateOnFight;
        public System.Collections.Generic.List<UnityEngine.ParticleSystem> DetectionParticles;
        
    }
}