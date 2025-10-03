namespace KnightOfNights.Scripts.FallenGuardian;

internal enum SlashAttackResult
{
    PENDING,
    PARRIED,
    NOT_PARRIED,
}

internal class SlashAttack
{
    public SlashAttackResult Result { get; private set; }

    public static void Spawn(SlashAttackSpec spec)
    {
        // FIXME
    }

    internal void Cancel()
    {
        // FIXME
    }
}
