using System.Collections.Generic;
using UnityEngine;
using board;
using chess;

namespace movements {
    public static class Movements {
        public static readonly List<FigMovement> bishopMovement = new List<FigMovement> {
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, 1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, 1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, -1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, -1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, 1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, 1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, -1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, -1)))
            )
        };

        public static readonly List<FigMovement> rookMovement = new List<FigMovement> {
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(0, 1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(0, -1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, 0)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, 0)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(0, 1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(0, -1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, 0)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, 0)))
            ),
        };

        public static readonly List<FigMovement> knightMovement = new List<FigMovement> {
            FigMovement.Mk(MoveType.Attack, Movement.Square(SquareMovement.Mk(5)))
        };

        public static readonly List<FigMovement> kingMovement = new List<FigMovement> {
            FigMovement.Mk(MoveType.Attack, Movement.Square(SquareMovement.Mk(3)))
        };

        public static readonly List<FigMovement> queenMovement = new List<FigMovement>() {
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, 1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, 1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, -1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, -1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, 1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, 1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, -1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, -1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(0, 1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(0, -1)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, 0)))
            ),
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, 0)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(0, 1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(0, -1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(1, 0)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(-1, new Vector2Int(-1, 0)))
            ),
        };


        public static readonly List<FigMovement> pawnMovement = new List<FigMovement>() {
            FigMovement.Mk(
                MoveType.Move, Movement.Linear(LinearMovement.Mk(1, new Vector2Int(-1, 0)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(1, new Vector2Int(-1, -1)))
            ),
            FigMovement.Mk(
                MoveType.Attack, Movement.Linear(LinearMovement.Mk(1, new Vector2Int(-1, 1)))
            ),
        };

        public static readonly Dictionary<FigureType, List<FigMovement>> movements =
            new Dictionary<FigureType, List<FigMovement>> {
                { FigureType.Bishop, bishopMovement },
                { FigureType.Rook, rookMovement },
                { FigureType.Queen, queenMovement },
                { FigureType.Knight, knightMovement },
                { FigureType.Pawn, pawnMovement },
                { FigureType.King, kingMovement }
            };
    }
}