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
            Y = transform.position.y,
            Text = new BoxedString(Text.Replace("\n", "<br>"))
        };

        var obj = deployer.Deploy();
        obj.transform.localScale = Vector3.one;
        obj.transform.SetPositionZ(3.1f);

        Destroy(gameObject);
    }
}
