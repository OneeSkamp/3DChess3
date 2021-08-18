using System.Collections.Generic;
using board;
using UnityEngine;
using option;

namespace chess {
    public static class ChessEngine {
        public static MovementType[] diagonalMovementType = new MovementType[] {
            new MovementType {linear = new LinearMovement {dir = new Vector2Int(1, 1)}},
            new MovementType {linear = new LinearMovement {dir = new Vector2Int(1, -1)}},
            new MovementType {linear = new LinearMovement {dir = new Vector2Int(-1, 1)}},
            new MovementType {linear = new LinearMovement {dir = new Vector2Int(-1, -1)}}
        };

        public static MovementType[] verticalMovementType = new MovementType[] {
            new MovementType {linear = new LinearMovement {dir = new Vector2Int(1, 0)}},
            new MovementType {linear = new LinearMovement {dir = new Vector2Int(-1, 0)}},
        };

        public static MovementType[] horizontalMovementType = new MovementType[] {
            new MovementType {linear = new LinearMovement {dir = new Vector2Int(0, 1)}},
            new MovementType {linear = new LinearMovement {dir = new Vector2Int(0, -1)}},
        };

        // public static Dictionary<FigureType, List<MovementType[]>> moveTypes = 
        //     new Dictionary<FigureType, MovementType[]> {

        //         {FigureType.Bishop, new MoveType(false, false, true, 8)},
        //         {FigureType.Rook, new MoveType(true, false, false, 8)},
        //         {FigureType.Knight, new MoveType(false, true, false, 1)},
        //         {FigureType.Queen, new MoveType(true, false, true, 8)},
        //         {FigureType.Pawn, new MoveType(true, false, true, 1)},
        //         {FigureType.King, new MoveType(true, false, true, 1)}
        // };

        // public static List<MovePath> CalcFigurePaths(
        //     Position pos,
        //     FigureType type,
        //     Option<Fig>[,] boardMap,
        //     Board<Fig> board
        // ) {
        //     var figurePaths = new List<MovePath>();
        //     var dirs = new List<Dir>();
        //     var moveType = MoveTypes.moveTypes[type];

        //     if (moveType.lineMove) {
        //         dirs.AddRange(board.lineDir);
        //     }

        //     if (moveType.diagonalMove) {
        //         dirs.AddRange(board.diagonalDir);
        //     }

        //     if (moveType.circularMove) {
        //         dirs.AddRange(board.circularDir);
        //     }

        //     figurePaths.AddRange(MovePaths.CalcMovePaths<Fig>(
        //         pos,
        //         dirs,
        //         moveType.maxLength,
        //         boardMap
        //     ));

        //     return figurePaths;
        // }

        public static List<Vector2Int> CalcLinearMoves(
            Vector2Int pos,
            LinearMovement lineMove,
            int maxLength,
            Option<Fig>[,] board
        ) {
            List<Vector2Int> possibleMoves = new List<Vector2Int>();

            var length = MovePaths.CalcLinearMoveLength(pos, lineMove, maxLength, board);

                for (int i = 1; i <= length; i++) {
                    var posX = pos.x + i * lineMove.dir.x;
                    var posY = pos.y + i * lineMove.dir.y;

                    possibleMoves.Add(new Vector2Int(posX, posY));
                }

            return possibleMoves;
        }
    }
}

