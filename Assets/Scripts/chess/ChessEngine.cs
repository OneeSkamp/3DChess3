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
            Fig figure = new Fig();
            figure.white = white;
            figure.type = type;
            figure.firstMove = true;
            return figure;
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

        public static MovementType circularMovementType = new MovementType {
            circular = new CircularMovement {radius = 2}
        };

        public static Dictionary<FigureType, MovementType> moveTypes =
            new Dictionary<FigureType, MovementType> {
                {FigureType.Bishop, new MovementType {
                    linear = diagonalMovementType,
                    maxLength = 8
                }},

                {FigureType.Rook, new MovementType {
                    linear = horizontalMovementType,
                    maxLength = 8
                }},

                {FigureType.Queen, new MovementType {
                    linear = mixedMovementType,
                    maxLength = 8
                }},

                {FigureType.King, new MovementType {
                    linear = mixedMovementType,
                    maxLength = 1
                }},

                {FigureType.Pawn, new MovementType {
                    linear = mixedMovementType,
                    maxLength = 1
                }},

                {FigureType.Knight, circularMovementType}
        };

        public static MovementType GetMovementType(FigureType type) {
            var movementType = new MovementType();
            movementType = moveTypes[type];
            return movementType;
        }

        public static List<Vector2Int> CalcLinearMoves(
            Vector2Int pos,
            LinearMovement linear,
            int maxLength,
            Option<Fig>[,] board
        ) {
            var linearMoves = new List<Vector2Int>();

            for (int i = 1; i <= maxLength; i++) {
                var posX = pos.x + i * linear.dir.x;
                var posY = pos.y + i * linear.dir.y;

                linearMoves.Add(new Vector2Int(posX, posY));
            }

            return linearMoves;
        }

        public static Vector2Int? GetLastLinearPosition (List<Vector2Int> linearMoves) {
            if (linearMoves.Count != 0) {
                return linearMoves[linearMoves.Count - 1];
            }
            return null;
        }
    }
}

