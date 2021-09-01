using UnityEngine;
using System.Collections.Generic;
using option;
using chess;
using board;
using move;

namespace inspector {
    public class ChessInspector : MonoBehaviour {
        public static bool CheckKing(
            Vector2Int pos,
            Dictionary<FigureType, List<Movement>> movements,
            Option<Fig>[,] board
        ) {
            var posFromKing = new List<Move>();
            var figMoves = new List<Move>();

            var queenMoves = MoveEngine.GetFigureMoves(pos, movements[FigureType.Queen], board);
            var knightMoves = MoveEngine.GetFigureMoves(pos, movements[FigureType.Knight], board);

            posFromKing.AddRange(queenMoves);
            posFromKing.AddRange(knightMoves);

            foreach (var move in posFromKing) {
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

