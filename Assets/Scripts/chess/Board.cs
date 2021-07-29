using System;

namespace chess {
    public class Board {
        
        public string fen;
        public Fig[,] boardMap;

        private Fig GetFig(bool white, figureType type) {
            Fig figure = new Fig();
            figure.white = white;
            figure.type = type;
            return figure;
        }
        public Board() {
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
            // initialFigurePlacement[7, 0] = whiteRook;
            // initialFigurePlacement[7, 7] = whiteRook;

            // initialFigurePlacement[7, 1] = whiteKnight;
            // initialFigurePlacement[7, 6] = whiteKnight;

            // initialFigurePlacement[7, 2] = whiteBishop;
            // initialFigurePlacement[7, 5] = whiteBishop;

            // initialFigurePlacement[7, 3] = whiteKing;
            // initialFigurePlacement[7, 4] = whiteQueen;
        }

        // public Board(string fen){
        //     this.fen = fen;
        //     boardMap = new char[8, 8];
        //     int fenCount = 0;

        //     for (int i = 0; i < boardMap.GetLength(0); i++) {
        //         for (int j = 0; j < boardMap.GetLength(1); j++) {
                    
        //             if (fen[fenCount] == '/') {
        //                 //Debug.Log($"+ {fen[fenCount]}");
        //                 fenCount++;
        //             } 
                    
        //             if (Char.IsDigit(fen[fenCount])) {
                        
        //                 for(int k = j; k < Int32.Parse(fen[fenCount].ToString()); k++) {
        //                     boardMap[i, k] = '.';
        //                     j = k;
        //                 }
        //                 fenCount++;
        //             } 

        //             if (!Char.IsDigit(fen[fenCount])&&fen[fenCount] != '/') {
        //                 boardMap[i, j] = fen[fenCount];
        //                 fenCount++;
                        
        //             }
        //         }
        //     }
        // }

    }
}

