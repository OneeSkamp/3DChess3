namespace board {
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

    public struct Position {
        public int x;
        public int y;

        public Position(int x, int y) {
            this.x = x;
            this.y = y;
        }
    }
}
