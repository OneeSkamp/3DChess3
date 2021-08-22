using System.Collections.Generic;
using board;
using UnityEngine;
using option;

namespace chess {
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
        public FigureType type;

        public static Fig CreateFig(bool white, FigureType type) {
            return new Fig {
                white = white,
                type = type
            };
        }
    }

    public struct MoveTypes {

    }

    public static class ChessEngine {

        // public static MovementType GetMovementType(FigureType type) {
        //     var movementType = new MovementType();
        //     movementType = MoveTypes.moveTypes[type];
        //     return movementType;
        // }

        public static Vector2Int? GetLastLinearPosition (List<Vector2Int> linearMoves) {
            if (linearMoves.Count != 0) {
                return linearMoves[linearMoves.Count - 1];
            }
            return null;
        }
    }
}

