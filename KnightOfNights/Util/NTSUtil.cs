using KnightOfNights.Scripts.SharedLib;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using PurenailCore.CollectionUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace KnightOfNights.Util;

internal static class NTSUtil
{
    static NTSUtil()
    {
        NtsGeometryServices.Instance = new(
            NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
            new PrecisionModel(PrecisionModels.FloatingSingle),
            srid: -1,
            GeometryOverlay.NG,
            new CoordinateEqualityComparer());
    }

    private static Geometry CreatePolygon(Collider2D collider) => CreatePolygon([.. collider.EnumeratePoints()]);

    private static Geometry CreatePolygon(PurenailCore.CollectionUtil.Rect rect) => CreatePolygon([
        rect.Min,
        new(rect.MinX, rect.MaxY),
        rect.Max,
        new(rect.MaxX, rect.MinY)]);

    private static Geometry CreatePolygon(List<Vector2> points)
    {
        List<Coordinate> coords = [.. points.Select(p => new Coordinate(p.x, p.y))];
        coords.Add(coords[0]);

        return NtsGeometryServices.Instance.CreateGeometryFactory().CreatePolygon([.. coords]);
    }

    private static bool Split(PurenailCore.CollectionUtil.Rect rect, float granularity, out List<PurenailCore.CollectionUtil.Rect> splits)
    {
        if (rect.Width > rect.Height)
        {
            if (rect.Width <= granularity)
            {
                splits = [];
                return false;
            }

            Vector2 newSize = new(rect.Width / 2, rect.Height);
            splits = [
                new(new((rect.Center.x + rect.MinX) / 2, rect.Center.y), newSize),
                new(new((rect.Center.x + rect.MaxX) / 2, rect.Center.y), newSize)];
            return true;
        }
        else
        {
            if (rect.Height <= granularity)
            {
                splits = [];
                return false;
            }

            Vector2 newSize = new(rect.Width, rect.Height / 2);
            splits = [
                new(new(rect.Center.x, (rect.Center.y + rect.MinY) / 2), newSize),
                new(new(rect.Center.x / 2, (rect.Center.y + rect.MaxY) / 2), newSize)];
            return true;
        }
    }

    internal static Func<Vector2, bool> QuantizedContainmentTest(this Collider2D collider, float unit = 1)
    {
        var factory = NtsGeometryServices.Instance.CreateGeometryFactory();
        var polygon = CreatePolygon(collider);
        var preparedPolygon = PreparedGeometryFactory.Prepare(polygon);

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
                PurenailCore.CollectionUtil.Rect rect = new(new Interval((x + minX) * unit, (x + minX + 1) * unit), new((y + minY) * unit, (y + minY + 1) * unit));
                var test = CreatePolygon(rect);

                if (preparedPolygon.Covers(test)) grid[x, y] = _ => true;
                else if (!preparedPolygon.Intersects(test)) grid[x, y] = _ => false;
                else
                {
                    var subsection = PreparedGeometryFactory.Prepare(test.Intersection(polygon));
                    grid[x, y] = p => subsection.Covers(factory.CreatePoint(new Coordinate(p.x, p.y)));
                }
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
