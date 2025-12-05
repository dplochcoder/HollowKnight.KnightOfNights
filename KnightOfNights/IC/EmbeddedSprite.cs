using ItemChanger.Internal;

namespace KnightOfNights.IC;

internal class EmbeddedSprite : ItemChanger.EmbeddedSprite
{
    private static readonly SpriteManager manager = new(typeof(EmbeddedSprite).Assembly, "KnightOfNights.Resources.Sprites.");

    public EmbeddedSprite(string key) => this.key = key;

    public override SpriteManager SpriteManager => manager;
}
