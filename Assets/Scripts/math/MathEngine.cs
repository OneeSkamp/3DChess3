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
            return new StrLineNormal { point = p1, normal = new Vector2Int(dir.y, dir.x)};
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

            if (A2*B1 - A1*B2 == 0) {
                return Option<Vector2Int>.None();
            }

            var y = (A1*A2*x2 - A2*A1*x1 + A2*B1*y1 - A1*B2*y2) / (A2*B1 - A1*B2);
            var x = (A1*x1 + B1*y - B1*y1) / A1;

            var result = new Vector2Int(x, y);

            return Option<Vector2Int>.Some(result);
        }

        public static (float, float) SortTwo(float v1, float v2) {
            if (v1 < v2) {
                return (v1, v2);
            } else {
                return (v2, v1);
            }
        }

        public static bool IsPoinOnSegment(Vector2Int point, Segment segment) {
            var a = segment.end.y - segment.start.y;
            var b = segment.start.x - segment.end.x;
            var c = - a * segment.start.x - b * segment.start.y;
            if (System.Math.Abs(a * point.x + b * point.y + c) > 0) {
                return false;
            }
            // var (minX, maxX) = SortTwo(segment.start.x, segment.end.x);
            // var (minY, maxY) = SortTwo(segment.start.y, segment.end.y);
            // return point.x >= minX && point.x <= maxX && point.y >= minY && point.y >= maxY;
            if ((point.x >= segment.start.x && point.x <= segment.end.x
                || point.x <= segment.start.x && point.x >= segment.end.x)
                && (point.y >= segment.start.y && point.y <= segment.end.y 
                || point.y <= segment.start.y && point.y >= segment.end.y)) {
                return true;
            } else {
                return false;
            }
        }
    }
}
