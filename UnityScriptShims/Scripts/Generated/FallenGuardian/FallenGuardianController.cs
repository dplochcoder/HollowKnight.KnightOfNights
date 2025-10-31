namespace KnightOfNights.Scripts.FallenGuardian
{
    public class FallenGuardianController : UnityEngine.MonoBehaviour
    {
        public FallenGuardianContainer Container;
        public float SequenceDelay;
        public float Telegraph;
        public float Deceleration;
        public float LongWait;
        public float ShortWait;
        public float SplitOffset;
        public float EscalationPause;
        public int StaggerCount;
        public float StaggerDistance;
        public UnityEngine.GameObject StaggerBurst;
        public UnityEngine.RuntimeAnimatorController SpellStartController;
        public UnityEngine.RuntimeAnimatorController SpellLoopController;
        public UnityEngine.RuntimeAnimatorController SpellEndController;
        public UnityEngine.RuntimeAnimatorController StaggerController;
        public UnityEngine.RuntimeAnimatorController StaggerToRecoverController;
        public UnityEngine.RuntimeAnimatorController SwordToSpellController;
        public System.Collections.Generic.List<FallenGuardianPhaseStats> PhaseStats;
        public void CastSpell() { }
        public void SetTangible() { }
        public void SetIntangible() { }
        public void TeleportOut() { }
        public void TeleportOutEvent() { }
        
    }
}