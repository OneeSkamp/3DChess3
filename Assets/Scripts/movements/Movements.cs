using System.Collections.Generic;
using UnityEngine;
using board;
using chess;

namespace movements {
    public static class Movements {
        public static readonly List<Movement> bishopMovement = new List<Movement> {
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 1)), MoveType.Move),
            // Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, -1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, -1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, -1)), MoveType.Attack),
            // Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, -1)), MoveType.Attack),
        };

        public static readonly List<Movement> rookMovement = new List<Movement> {
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, 1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, -1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 0)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 0)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, 1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, -1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 0)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 0)), MoveType.Attack)
        };

        public static readonly List<Movement> knightMovement = new List<Movement> {
            new Movement { square = new SquareMovement { side = 5 } }
        };


        public static readonly List<Movement> kingMovement = new List<Movement> {
            new Movement { square = new SquareMovement { side = 3 } }
        };

        public static readonly List<Movement> queenMovement = new List<Movement>() {
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, -1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, -1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, 1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, -1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 0)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 0)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, -1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, -1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, 1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, -1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 0)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 0)), MoveType.Attack)
        };

        public static readonly List<Movement> pawnMovement = new List<Movement>() {
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, 1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(0, -1)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 0)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 0)), MoveType.Move),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, 1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, 1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(1, -1)), MoveType.Attack),
            Movement.Linear(LinearMovement.Mk(new Vector2Int(-1, -1)), MoveType.Attack),
        };

        public static readonly Dictionary<FigureType, List<Movement>> movements =
            new Dictionary<FigureType, List<Movement>> {
                { FigureType.Bishop, bishopMovement },
                { FigureType.Rook, rookMovement },
                { FigureType.Queen, queenMovement },
                { FigureType.Knight, knightMovement },
                { FigureType.Pawn, pawnMovement },
                { FigureType.King, kingMovement }
            };
    }
}