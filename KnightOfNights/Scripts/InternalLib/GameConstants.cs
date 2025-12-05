using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class GameConstants
{
    public const float CAMERA_HALF_WIDTH = 14.6f;
    public const float CAMERA_HALF_HEIGHT = 8.3f;
    public static float GRAVITY = Mathf.Abs(Physics.gravity.y);
}
