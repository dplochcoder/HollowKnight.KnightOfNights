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
        public UnityEngine.GameObject DiveWarningParticles;
        public UnityEngine.GameObject StaggerBurst;
        public UnityEngine.GameObject TeleportBurst;
        public UnityEngine.RuntimeAnimatorController DiveAnticLoopController;
        public UnityEngine.RuntimeAnimatorController DiveAnticToDiveLoopController;
        public UnityEngine.RuntimeAnimatorController DiveImpactController;
        public UnityEngine.RuntimeAnimatorController DiveLoopController;
        public UnityEngine.RuntimeAnimatorController SpellCastToEndController;
        public UnityEngine.RuntimeAnimatorController SpellCastToLoopController;
        public UnityEngine.RuntimeAnimatorController SpellLoopController;
        public UnityEngine.RuntimeAnimatorController SpellLoopToSwordController;
        public UnityEngine.RuntimeAnimatorController SpellStartToLoopController;
        public UnityEngine.RuntimeAnimatorController StaggerController;
        public UnityEngine.RuntimeAnimatorController StaggerToRecoverController;
        public UnityEngine.RuntimeAnimatorController SwordToDiveAnticController;
        public UnityEngine.RuntimeAnimatorController SwordToDiveAnticNoLoopController;
        public UnityEngine.RuntimeAnimatorController SwordToSpellController;
        public UnityEngine.RuntimeAnimatorController TeleportInController;
        public UnityEngine.RuntimeAnimatorController TeleportOutController;
        public System.Collections.Generic.List<FallenGuardianPhaseStats> PhaseStats;
        public void CastSpellEvent() { }
        public void DiveEvent() { }
        public void TeleportIn() { }
        public void TeleportOut() { }
        public void TeleportOutEvent() { }
        
    }
}