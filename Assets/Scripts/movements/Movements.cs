using System.Collections.Generic;
using UnityEngine;
using board;
using chess;

namespace movements {
    public static class Movements {
        public static readonly List<Movement> bishopMovement = new List<Movement> {
            new Movement { linear = new LinearMovement { dir = new Vector2Int(1, 1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(-1, 1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(1, -1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(-1, -1) } }
        };

        public static readonly List<Movement> rookMovement = new List<Movement> {
            new Movement { linear = new LinearMovement { dir = new Vector2Int(1, 0) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(0, 1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(0, -1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(-1, 0) } }
        };

        public static readonly List<Movement> knightMovemetn = new List<Movement> {
            new Movement { square = new SquareMovement { side = 5 } }
        };


        public static readonly List<Movement> kingMovement = new List<Movement> {
                new Movement { square = new SquareMovement { side = 3 } }
        };

        public static readonly List<Movement> queenMovement = new List<Movement>() {
            new Movement { linear = new LinearMovement { dir = new Vector2Int(1, 1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(-1, 1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(1, -1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(-1, -1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(1, 0) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(0, 1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(0, -1) } },
            new Movement { linear = new LinearMovement { dir = new Vector2Int(-1, 0) } }
        };

        public static readonly Dictionary<FigureType, List<Movement>> movements =
        new Dictionary<FigureType, List<Movement>> {
            { FigureType.Bishop, bishopMovement },
            { FigureType.Rook, rookMovement },
            { FigureType.Queen, queenMovement },
            { FigureType.Knight, knightMovemetn },
            { FigureType.Pawn, queenMovement },
            { FigureType.King, kingMovement }
        };
    }
}

