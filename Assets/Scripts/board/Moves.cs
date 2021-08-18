using UnityEngine;

namespace board {
    public struct LinearMovement {
        public Vector2Int dir;
    }

    public struct CircularMovement {
        public Vector2Int radius;
    }

    public struct MovementType {
        public LinearMovement? linear;
        public CircularMovement? circular;
    }

    // public struct MoveType {
    //     public bool lineMove;
    //     public bool circularMove;
    //     public bool diagonalMove;
    //     public int maxLength;
    //     public MoveType (bool lineMove, bool circularMove, bool diagonalMove, int maxLength) {
    //         this.lineMove = lineMove;
    //         this.circularMove = circularMove;
    //         this.diagonalMove = diagonalMove;
    //         this.maxLength = maxLength;
    //     }
    // }

    public struct LinearPath {
        public Vector2Int pos;
        public Vector2Int dir;
        public int length;
    }

}
