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

    public static class ChessEngine {
        public static List<LinearMovement?> diagonalMovementType = new List<LinearMovement?> {
            new LinearMovement {dir = new Vector2Int(1, 1)},
            new LinearMovement {dir = new Vector2Int(1, -1)},
            new LinearMovement {dir = new Vector2Int(-1, 1)},
            new LinearMovement {dir = new Vector2Int(-1, -1)}
        };

        public static List<LinearMovement?> horizontalMovementType = new List<LinearMovement?> {
            new LinearMovement {dir = new Vector2Int(1, 0)},
            new LinearMovement {dir = new Vector2Int(0, 1)},
            new LinearMovement {dir = new Vector2Int(-1, 0)},
            new LinearMovement {dir = new Vector2Int(0, -1)}
        };

        public static List<LinearMovement?> mixedMovementType = new List<LinearMovement?> {
            new LinearMovement {dir = new Vector2Int(1, 0)},
            new LinearMovement {dir = new Vector2Int(0, 1)},
            new LinearMovement {dir = new Vector2Int(-1, 0)},
            new LinearMovement {dir = new Vector2Int(0, -1)},
            new LinearMovement {dir = new Vector2Int(1, 1)},
            new LinearMovement {dir = new Vector2Int(1, -1)},
            new LinearMovement {dir = new Vector2Int(-1, 1)},
            new LinearMovement {dir = new Vector2Int(-1, -1)}
        };

        public static MovementType squareMovementType = new MovementType {
            square = new SquareMovement {side = 5}
        };

        public static Dictionary<FigureType, MovementType> moveTypes =
            new Dictionary<FigureType, MovementType> {
                {
                    FigureType.Bishop,
                    new MovementType {
                        linear = diagonalMovementType
                    }
                },

                {
                    FigureType.Rook,
                    new MovementType {
                        linear = horizontalMovementType
                    }
                },

                {
                    FigureType.Queen,
                    new MovementType {
                        linear = mixedMovementType
                    }
                },

                {
                    FigureType.King,
                    new MovementType {
                        linear = mixedMovementType
                    }
                },

                {
                    FigureType.Pawn,
                    new MovementType {
                        linear = mixedMovementType
                    }
                },

                {FigureType.Knight, squareMovementType}
        };

        public static MovementType GetMovementType(FigureType type) {
            var movementType = new MovementType();
            movementType = moveTypes[type];
            return movementType;
        }

        public static Vector2Int? GetLastLinearPosition (List<Vector2Int> linearMoves) {
            if (linearMoves.Count != 0) {
                return linearMoves[linearMoves.Count - 1];
            }
            return null;
        }
    }
}

