using System;
using System.Collections.Generic;
using option;

namespace board {
    [Serializable]
    public class Board<T> {
        public bool whiteMove;
        public int height = 8;
        public int width = 8;
        public Option<T>[,] boardMap;

        public readonly List<Dir> lineDir = new List<Dir>();
        public readonly List<Dir> diagonalDir = new List<Dir>();
        public readonly List<Dir> circularDir = new List<Dir>();

        public Board(bool whiteMove, Option<T>[,] figs) {
            this.whiteMove = whiteMove;
            boardMap = new Option<T>[height, width];
            this.boardMap = figs;

            /*boardMap[0, 0] = Option<>.Some(Fig.CreateFig(false, FigureType.Rook));
            boardMap[0, 7] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Rook));

            boardMap[0, 1] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Knight));
            boardMap[0, 6] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Knight));

            boardMap[0, 2] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Bishop));
            boardMap[0, 5] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Bishop));

            boardMap[0, 3] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Queen));
            boardMap[0, 4] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.King));

            for (int x = 0; x <= 7; x++) {
                boardMap[1, x] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Pawn));
            }

            boardMap[7, 0] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Rook));
            boardMap[7, 7] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Rook));

            boardMap[7, 1] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Knight));
            boardMap[7, 6] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Knight));

            boardMap[7, 2] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Bishop));
            boardMap[7, 5] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Bishop));

            boardMap[7, 3] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Queen));
            boardMap[7, 4] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.King));

            for (int x = 0; x <= 7; x++) {
                boardMap[6, x] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Pawn));
            }*/

            lineDir.Add(new Dir(1, 0));
            lineDir.Add(new Dir(0, 1));
            lineDir.Add(new Dir(-1, 0));
            lineDir.Add(new Dir(0, -1));

            diagonalDir.Add(new Dir(1, 1));
            diagonalDir.Add(new Dir(1, -1));
            diagonalDir.Add(new Dir(-1, 1));
            diagonalDir.Add(new Dir(-1, -1));

            circularDir.Add(new Dir(2, 1));
            circularDir.Add(new Dir(2, -1));
            circularDir.Add(new Dir(-2, 1));
            circularDir.Add(new Dir(-2, -1));
            circularDir.Add(new Dir(1, 2));
            circularDir.Add(new Dir(-1, 2));
            circularDir.Add(new Dir(1, -2));
            circularDir.Add(new Dir(-1, -2));
        }
    }
}

