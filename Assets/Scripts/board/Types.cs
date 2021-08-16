using System.Collections.Generic;
using option;
using chess;

namespace board {

    public static class MoveTypes {
        public static Dictionary<FigureType, MoveType> MakeFigMoveTypes() {
            var figMoveTypes = new Dictionary<FigureType, MoveType>();
            var bishopMoveType = new MoveType();
            var rookMoveType = new MoveType();
            var knightMoveType = new MoveType();
            var queenMoveType = new MoveType();
            var kingMoveType = new MoveType();
            var pawnMoveType = new MoveType();

            bishopMoveType.diagonalMove = true;

            figMoveTypes.Add(FigureType.Bishop, bishopMoveType);

            rookMoveType.lineMove = true;
            figMoveTypes.Add(FigureType.Rook, rookMoveType);

            knightMoveType.circularMove = true;
            figMoveTypes.Add(FigureType.Knight, knightMoveType);

            queenMoveType.diagonalMove = true;
            queenMoveType.lineMove = true;
            figMoveTypes.Add(FigureType.Queen, queenMoveType);

            kingMoveType.diagonalMove = true;
            kingMoveType.lineMove = true;
            figMoveTypes.Add(FigureType.King, kingMoveType);

            pawnMoveType.lineMove = true;
            pawnMoveType.diagonalMove = true;
            figMoveTypes.Add(FigureType.Pawn, pawnMoveType);

            return figMoveTypes;
        }

        public static MovePath CalcPath(Position pos, Dir dir, Option<Fig>[,] board) {
            var movePath = new MovePath();
            var length = 0;
            movePath.pos = pos;

            for (int i = 1; i < board.GetLength(0); i++) {
                var posX = pos.x + i * dir.x;
                var posY = pos.y + i * dir.y;

                if (!IsOnBoard(new Position(posX, posY), board.GetLength(0), board.GetLength(1))) {
                    break;
                }

                if (board[posX, posY].IsNone()) {
                    length++;
                }

                if (!board[posX, posY].IsNone()) {
                    movePath.onWay = new Position(posX, posY);
                }
            }

            return movePath;
        }

        public static List<MovePath> CalcMovePaths(
            Position pos, 
            List<Dir> directions, 
            Option<Fig>[,] board
        ) {
            List<MovePath> movePaths = new List<MovePath>();

            foreach (Dir dir in directions) {
                var path = CalcPath(pos, dir, board);
                movePaths.Add(path);
            }

            return movePaths;
        }

        private static bool IsOnBoard(Position pos, int width, int height) {

            if (pos.x < height || pos.y < width || pos.x >= height || pos.y >= width) {
                return false;
            }

            return true;
        }
    }
}

