namespace board {
    public struct MoveType {
        public bool lineMove;
        public bool circularMove;
        public bool diagonalMove;
    }

    public struct MovePath {
        public Position pos;
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
}
