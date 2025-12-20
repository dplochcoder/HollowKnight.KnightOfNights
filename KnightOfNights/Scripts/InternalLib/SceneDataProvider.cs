using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

public abstract class SceneDataProvider : MonoBehaviour
{
    public abstract object GetSceneData();
}
