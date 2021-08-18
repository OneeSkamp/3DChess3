using System.Collections.Generic;
using UnityEngine;

namespace board {
    public struct LinearMovement {
        public Vector2Int dir;
        public int maxLength;
        public LinearMovement(Vector2Int dir, int maxLength) {
            this.dir = dir;
            this.maxLength = maxLength;
        }
    }

    public struct CircularMovement {
        public Vector2Int radius;
    }

    public struct MovementType {
        public List<LinearMovement?> linear;
        public MovementType(List<LinearMovement?> linear) {
            this.linear = linear;
        }
    }

    public struct LinearPath {
        public Vector2Int pos;
        public Vector2Int dir;
        public int length;
    }

}
