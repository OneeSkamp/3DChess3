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

        public static Dir NewDir(int x, int y) {
            Dir dir = new Dir();
            dir.x = x;
            dir.y = y;
            return dir;
        }
    }
}
