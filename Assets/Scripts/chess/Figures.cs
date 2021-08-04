namespace chess {

    public struct Move {
        public Position from;
        public Position to;
    }

    public struct CastlingMove {
        public Position oldRookPos;
        public Position newRookPos;
    }

    public struct EnPassantMove {
        public Position pawnPos;
    }

    public struct MoveFigureRes {
        public MoveError error;
        public Position position;
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

    public enum MoveError {
        None,
        ImpossibleMove,
        MoveOnFigure
    }

    public enum FigureType {
        None,
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public struct Fig {
        public bool white;
        public bool firstMove;
        public bool castling;
        public bool enPassant;
        public bool check;
        public FigureType type;

        public static Fig CreateFig(bool white, FigureType type) {
            Fig figure = new Fig();
            figure.white = white;
            figure.type = type;
            figure.firstMove = true;
            return figure;
        }
    }

}
