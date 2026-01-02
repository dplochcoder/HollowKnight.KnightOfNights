using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class CustomDefaultEnvironment : MonoBehaviour
{
    [ShimField] public CustomEnvironmentType EnvironmentType;

    private bool localEnabled = false;
    private bool localLoaded = false;

    private void OnEnable()
    {
        localEnabled = true;
        TryLoad();
    }

    private void Update()
    {
        if (localEnabled) TryLoad();
    }

    private void TryLoad()
    {
        if (localLoaded) return;

        bool any = false;
        foreach (var sceneManager in FindObjectsOfType<SceneManager>(false))
        {
            sceneManager.environmentType = EnvironmentType.ToIntId();
            any = true;
        }

        if (!any) return;
        localLoaded = true;

        var pd = PlayerData.instance;
        pd.SetInt(nameof(pd.environmentType), EnvironmentType.ToIntId());
        pd.SetInt(nameof(pd.environmentTypeDefault), EnvironmentType.ToIntId());
        HeroController.instance.checkEnvironment();
    }

    private void OnDisable()
    {
        localEnabled = false;
        localLoaded = false;
    }
}
