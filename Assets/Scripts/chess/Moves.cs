namespace chess {
    public struct MoveTypes {
        public bool LineMove;
        public bool KnightMove;
        public bool EnPassant;
        public bool Castling;
        public bool DiagonalMove;
    }

    public struct MovePath {
        public Position pos;
        public Dir dir;
        public int Lenght;
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

    enum Move {
        LineMove,
        KnightMove,
        EnPassant,
        Castling
    }
}
