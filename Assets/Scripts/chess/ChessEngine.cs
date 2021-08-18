using System.Collections.Generic;
using board;
using UnityEngine;
using option;

namespace chess {
    public static class ChessEngine {

        public static List<LinearMovement?> diagonalMovementType = new List<LinearMovement?> {
            new LinearMovement (new Vector2Int(1, 1), 8),
            new LinearMovement (new Vector2Int(1, -1), 8),
            new LinearMovement (new Vector2Int(-1, 1), 8),
            new LinearMovement (new Vector2Int(-1, -1), 8)
        };

        public static Dictionary<FigureType, MovementType> moveTypes =
            new Dictionary<FigureType, MovementType> {
                 {FigureType.Bishop, new MovementType(diagonalMovementType) },
            };

        public static List<Vector2Int> CalcLinearMoves(
            Vector2Int pos,
            MovementType movementType,
            Option<Fig>[,] board
        ) {
            List<Vector2Int> possibleMoves = new List<Vector2Int>();
            
            foreach (var line in movementType.linear) {
                var length = MovePaths.CalcLinearMoveLength(pos, line.Value, line.Value.maxLength, board);

                for (int i = 1; i <= length; i++) {
                    var posX = pos.x + i * line.Value.dir.x;
                    var posY = pos.y + i * line.Value.dir.y;

                    possibleMoves.Add(new Vector2Int(posX, posY));
                }
            }
            return possibleMoves;
        }
    }
}

