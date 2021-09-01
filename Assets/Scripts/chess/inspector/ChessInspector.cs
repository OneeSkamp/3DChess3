using UnityEngine;
using System.Collections.Generic;
using option;
using chess;
using move;
using movements;

namespace inspector {
    public class ChessInspector : MonoBehaviour {
        public static bool IsUnderAttackPos(Vector2Int pos, Option<Fig>[,] board) {
            var movesFromPos = new List<Move>();
            var figMoves = new List<Move>();
            var movements = Movements.GetMovements();
            var queenMoves = MoveEngine.GetFigureMoves(pos, movements[FigureType.Queen], board);
            var knightMoves = MoveEngine.GetFigureMoves(pos, movements[FigureType.Knight], board);

            movesFromPos.AddRange(queenMoves);
            movesFromPos.AddRange(knightMoves);

            foreach (var move in movesFromPos) {
                var figOpt = board[move.to.x, move.to.y];

                if (figOpt.IsSome()) {
                    var fig = figOpt.Peel();
                    var moves = MoveEngine.GetFigureMoves(move.to, movements[fig.type], board);
                    figMoves.AddRange(moves);
                }
            }

            foreach (var move in figMoves) {
                if (Equals(move.to, pos)) {
                    Debug.Log("check");
                }
            }

            return false;
        }
    }
}

