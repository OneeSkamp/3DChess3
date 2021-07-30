namespace chess {
    public enum figureType {
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
        public bool check;
        public figureType type;
    }

}
