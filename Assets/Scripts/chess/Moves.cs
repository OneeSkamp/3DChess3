using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace chess {
    public struct Move {
        public Position from;
        public Position to;
    }

    public struct Dir {
        public int x;
        public int y;

        public static Dir NewDir(int x, int y) {
            Dir dir = new Dir();
            dir.x = x;
            dir.y = y;
            return dir;
        }
    }

    public struct CastlingMove {
        public Position oldRookPos;
        public Position newRookPos;
    }

    public struct MoveFigureRes {
        public MoveError error;
        public Position position;
    }

    public enum MoveError {
        None,
        ImpossibleMove,
        MoveOnFigure
    }
}
