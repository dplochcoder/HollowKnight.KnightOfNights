namespace KnightOfNights.Scripts.FallenGuardian
{
    public class FallenGuardianPhaseStats : UnityEngine.MonoBehaviour
    {
        public int MinHP;
        public AttackChoice FirstAttack;
        public System.Collections.Generic.List<FallenGuardianAttack> Attacks;
        public float StaggerGracePeriod;
        public float StaggerInvuln;
        public float StaggerHitWait;
        public float StaggerMaxWait;
        public float StaggerNextAttackDelay;
        public float UltraInstinctInterval;
        public float UltraInstinctTail;
        public float UltraInstinctTelegraph;
        
    }
}