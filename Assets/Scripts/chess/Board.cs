using System;
namespace chess {

    [Serializable]
    public class Board {
        public bool whiteMove;
        public Fig[,] boardMap;

        public Board(bool whiteMove) {
            this.whiteMove = whiteMove;
            boardMap = new Fig[8, 8];

            boardMap[0, 0] = Fig.CreateFig(false, FigureType.Rook);
            boardMap[0, 7] = Fig.CreateFig(false, FigureType.Rook);

            boardMap[0, 1] = Fig.CreateFig(false, FigureType.Knight);
            boardMap[0, 6] = Fig.CreateFig(false, FigureType.Knight);

            boardMap[0, 2] = Fig.CreateFig(false, FigureType.Bishop);
            boardMap[0, 5] = Fig.CreateFig(false, FigureType.Bishop);

            boardMap[0, 3] = Fig.CreateFig(false, FigureType.Queen);
            boardMap[0, 4] = Fig.CreateFig(false, FigureType.King);

            for (int x = 0; x <= 7; x++) {
                boardMap[1, x] = Fig.CreateFig(false, FigureType.Pawn);
            }

            boardMap[7, 0] = Fig.CreateFig(true, FigureType.Rook);
            boardMap[7, 7] = Fig.CreateFig(true, FigureType.Rook);

            boardMap[7, 1] = Fig.CreateFig(true, FigureType.Knight);
            boardMap[7, 6] = Fig.CreateFig(true, FigureType.Knight);

            boardMap[7, 2] = Fig.CreateFig(true, FigureType.Bishop);
            boardMap[7, 5] = Fig.CreateFig(true, FigureType.Bishop);

            boardMap[7, 3] = Fig.CreateFig(true, FigureType.Queen);
            boardMap[7, 4] = Fig.CreateFig(true, FigureType.King);

            for (int x = 0; x <= 7; x++) {
                boardMap[6, x] = Fig.CreateFig(true, FigureType.Pawn);
            }
        }
    }
}

