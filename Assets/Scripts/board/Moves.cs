namespace board {
    public struct MoveType {
        public bool lineMove;
        public bool circularMove;
        public bool diagonalMove;
        public MoveType (bool lineMove, bool circularMove, bool diagonalMove) {
            this.lineMove = lineMove;
            this.circularMove = circularMove;
            this.diagonalMove = diagonalMove;
        }
    }

    public struct MovePath {
        public Position pos;
        public Position onWay;
        public Dir dir;
        public int Length;
    }

    public struct Dir {
        public int x;
        public int y;
        public Dir (int x, int y) {
            this.x = x;
            this.y = y;
        }
    }

    public struct Position {
        public int x;
        public int y;

        public Position(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
}
