using System.Collections.Generic;
using option;
using UnityEngine;

namespace math {
    public struct Segment {
        public Vector2Int start;
        public Vector2Int end;

        public static Segment Mk(Vector2Int start, Vector2Int end) {
            return new Segment { start = start, end = end };
        }
    }

    public struct StrLineNormal {
        public Vector2Int point;
        public Vector2Int normal;
    }

    public static class MathEngine {
        public static StrLineNormal FormStrLine(Vector2Int p1, Vector2Int p2) {
            var dir = p2 - p1;
            return new StrLineNormal { point = p1, normal = new Vector2Int(dir.y, -dir.x) };
        }

        public static Option<Vector2Int> GetIntersectionPoint(
            StrLineNormal l1,
            StrLineNormal l2
        ) {
            var A1 = l1.normal.x;
            var A2 = l2.normal.x;
            var B1 = l1.normal.y;
            var B2 = l2.normal.y;
            var x1 = l1.point.x;
            var y1 = l1.point.y;
            var x2 = l2.point.x;
            var y2 = l2.point.y;

            if (A1*B2 - A2*B1 == 0) {
                return Option<Vector2Int>.None();
            }

            var y = (A1*A2*x2 - A2*A1*x1 - A2*B1*y1 + A1*B2*y2) / (A1*B2 - A2*B1);
            var x = (A1*x1 - B1*y + B1*y1) / A1;

            var result = new Vector2Int(x, y);

            return Option<Vector2Int>.Some(result);
        }

        public static (float, float) SortTwo(float v1, float v2) {
            if (v1 > v2) {
                return (v2, v1);
            } else {
                return (v1, v2);
            }
        }

        public static List<Vector2Int> GetSquareIntersectionPoints(
            StrLineNormal l1,
            int halfSide,
            Vector2Int center
        ) {
            var p1 = new Vector2Int(center.x - halfSide, center.y - halfSide);
            var p2 = new Vector2Int(center.x - halfSide, center.y + halfSide);
            var p3 = new Vector2Int(center.x + halfSide, center.y + halfSide);
            var p4 = new Vector2Int(center.x + halfSide, center.y - halfSide);

            var sides = new List<StrLineNormal> {
                FormStrLine(p1, p2),
                FormStrLine(p2, p3),
                FormStrLine(p3, p4),
                FormStrLine(p4, p1)
            };

            var intersectionPoints = new List<Vector2Int>();
            foreach (var side in sides) {
                if (side.normal.x == 0 && l1.normal.y == 0
                    || side.normal.y == 0 && l1.normal.x == 0) {
                    continue;
                }

                var intersectionPoint = GetIntersectionPoint(l1, side);
                var segment = Segment.Mk(
                    side.point,
                    new Vector2Int(side.point.x - side.normal.y, side.point.y + side.normal.x)
                );

                if (intersectionPoint.IsSome()) {
                    if (IsPoinOnSegment(intersectionPoint.Peel(), segment)) {
                        intersectionPoints.Add(intersectionPoint.Peel());
                    }
                }
            }

            return intersectionPoints;
        }

        public static bool IsPoinOnSegment(Vector2Int point, Segment segment) {
            var a = segment.end.y - segment.start.y;
            var b = segment.start.x - segment.end.x;
            var c = - a * segment.start.x - b * segment.start.y;
            if (System.Math.Abs(a * point.x + b * point.y + c) > 0) {
                return false;
            }
            var (minX, maxX) = SortTwo(segment.start.x, segment.end.x);
            var (minY, maxY) = SortTwo(segment.start.y, segment.end.y);

            return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
        }
    }
}
