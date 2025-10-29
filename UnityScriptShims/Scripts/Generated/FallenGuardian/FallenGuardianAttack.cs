namespace KnightOfNights.Scripts.FallenGuardian
{
    public class FallenGuardianAttack : UnityEngine.MonoBehaviour
    {
        public AttackChoice Choice;
        public float Weight;
        public int Cooldown;
        public float WeightIncrease;
        public System.Collections.Generic.List<AttackChoice> ForbiddenPredecessors;
        
    }
}