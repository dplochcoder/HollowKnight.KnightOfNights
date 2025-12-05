using KnightOfNights.Scripts.SharedLib;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
public interface IParryResponder
{
    void Parried(float direction);
}
