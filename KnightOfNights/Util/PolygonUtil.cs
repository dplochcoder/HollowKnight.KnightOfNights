using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Util;

internal static class PolygonUtil
{
    internal static Func<Vector2, bool> QuantizedContainmentTest(this Collider2D collider, float unit = 1)
    {
        PurenailCore.CollectionUtil.Rect bounds = new(collider.bounds);
        int minX = Mathf.FloorToInt(bounds.MinX / unit);
        int minY = Mathf.FloorToInt(bounds.MinY / unit);
        int xspan = Mathf.CeilToInt(bounds.MaxX / unit) - minX;
        int yspan = Mathf.CeilToInt(bounds.MaxY / unit) - minY;

        var grid = new Func<Vector2, bool>[xspan, yspan];
        for (int x = 0; x < xspan; x++)
        {
            for (int y = 0; y < yspan; y++)
            {
                List<bool> result = [];
                Vector2 center = new((x + minX + 0.5f) * unit, (y + minY + 0.5f) * unit);
                grid[x, y] = p =>
                {
                    if (result.Count == 0) result.Add((collider.ClosestPoint(center) - center).sqrMagnitude <= 0.04f);
                    return result[0];
                };
            }
        }

        return p =>
        {
            int ix = Mathf.FloorToInt(p.x / unit) - minX;
            int iy = Mathf.FloorToInt(p.y / unit) - minY;
            return ix >= 0 && ix < xspan && iy >= 0 && iy < yspan && grid[ix, iy](p);
        };
    }
}
