
namespace chess {
    public class Board {
        public bool whiteMove;
        public Fig[,] boardMap;

        private Fig GetFig(bool white, figureType type) {
            Fig figure = new Fig();
            figure.white = white;
            figure.type = type;
            figure.firstMove = true;
            return figure;
        }
        public Board(bool whiteMove) {
            this.whiteMove = whiteMove;
            boardMap = new Fig[8, 8];

            boardMap[0, 0] = GetFig(false, figureType.Rook);
            boardMap[0, 7] = GetFig(false, figureType.Rook);

            boardMap[0, 1] = GetFig(false, figureType.Knight);
            boardMap[0, 6] = GetFig(false, figureType.Knight);

            boardMap[0, 2] = GetFig(false, figureType.Bishop);
            boardMap[0, 5] = GetFig(false, figureType.Bishop);

            boardMap[0, 3] = GetFig(false, figureType.Queen);
            boardMap[0, 4] = GetFig(false, figureType.King);

            for (int x = 0; x <= 7; x++) {
                boardMap[1, x] = GetFig(false, figureType.Pawn);
            }

            boardMap[7, 0] = GetFig(true, figureType.Rook);
            boardMap[7, 7] = GetFig(true, figureType.Rook);

            boardMap[7, 1] = GetFig(true, figureType.Knight);
            boardMap[7, 6] = GetFig(true, figureType.Knight);

            boardMap[7, 2] = GetFig(true, figureType.Bishop);
            boardMap[7, 5] = GetFig(true, figureType.Bishop);

            boardMap[7, 3] = GetFig(true, figureType.Queen);
            boardMap[7, 4] = GetFig(true, figureType.King);

            for (int x = 0; x <= 7; x++) {
                boardMap[6, x] = GetFig(true, figureType.Pawn);
            }
        }
    }
}

