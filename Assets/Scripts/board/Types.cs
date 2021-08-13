using System.Collections.Generic;

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
    }
}

