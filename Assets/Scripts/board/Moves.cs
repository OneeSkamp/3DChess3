using System.Collections.Generic;
using UnityEngine;

namespace board {
    public struct LinearMovement {
        public Vector2Int dir;
    }

    public struct CircularMovement {
        public int radius;
    }

    public struct MovementType {
        public List<LinearMovement?> linear;
        public int maxLength;
    }

    public struct LinearPath {
        public Vector2Int pos;
        public Vector2Int dir;
        public int length;
    }

}
