namespace chess {

    public struct Move {
        public Position from;
        public Position to;
    }

    public struct MoveFigureRes {
        MoveError error;
        Position position;
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
