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

    public struct SegmentInfo {
        public Vector2Int start;
        public Vector2Int normal;

        public static SegmentInfo Mk(Vector2Int start, Vector2Int normal) {
            return new SegmentInfo { start = start, normal = normal };
        }
    }

    public class MathEngine {
        public static SegmentInfo GetSegmentInfo(Vector2Int point1, Vector2Int point2) {
            return SegmentInfo.Mk(
                point1,
                new Vector2Int(point2.y - point1.y, point2.x - point1.x)
            );
        }

        public static Option<Vector2Int> GetIntersectionPoint(
            SegmentInfo segment1,
            SegmentInfo segment2
        ) {
            var A1 = segment1.normal.x;
            var A2 = segment2.normal.x;
            var B1 = segment1.normal.y;
            var B2 = segment2.normal.y;
            var x1 = segment1.start.x;
            var y1 = segment1.start.y;
            var x2 = segment2.start.x;
            var y2 = segment2.start.y;

            if (A2*B1 - A1*B2 == 0) {
                return Option<Vector2Int>.None();
            }

            var y = (A1*A2*x2 - A2*A1*x1 + A2*B1*y1 - A1*B2*y2) / (A2*B1 - A1*B2);
            var x = (A1*x1 + B1*y - B1*y1) / A1;

            var result = new Vector2Int(x, y);

            return Option<Vector2Int>.Some(result);
        }

        public static bool IsPoinOnSegment(Vector2Int point, Segment segment) {
            var a = segment.end.y - segment.start.y;
            var b = segment.start.x - segment.end.x;
            var c = - a * segment.start.x - b * segment.start.y;
            if (System.Math.Abs(a * point.x + b * point.y + c) > 0) {
                return false;
            }
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
