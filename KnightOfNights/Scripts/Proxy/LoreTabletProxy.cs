using ItemChanger;
using ItemChanger.Deployers;
using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Proxy;

[Shim]
internal class LoreTabletProxy : MonoBehaviour
{
    [TextArea(3, 12)] [ShimField] public string Text = "";

    private void Awake()
    {
        TabletDeployer deployer = new()
        {
            SceneName = GameManager.instance.sceneName,
            X = transform.position.x,
            Y = transform.position.y + 2.61f,
            Text = new BoxedString(Text.Replace("\n", "<br>"))
        };
        deployer.Deploy();
        Destroy(gameObject);
    }
}
