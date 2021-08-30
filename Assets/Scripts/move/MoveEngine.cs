using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;

namespace move {
    public enum MoveError {
        None,
        ImpossibleMove,
        MoveOnFigure
    }

    public struct MoveRes {
        public Vector2Int pos;
        public MoveError error;

    }

    public static class MoveEngine {
        public static List<Move> GetPossibleMoves(
            Vector2Int start,
            List<Vector2Int> movePath,
            Option<Fig>[,] board
        ) {
            var possMoves = new List<Move>();

            foreach (var cell in movePath) {
                var move = new Move {
                    from = start,
                    to = cell
                };

                if (ChessEngine.IsPossibleMove(move, board)) {
                    possMoves.Add(move);
                }
            }
            return possMoves;
        }

        public static List<Move> GetPossibleLinearMoves(
            Vector2Int start,
            LinearMovement linear,
            int length,
            Option<Fig>[,] board
        ) {
            var possMoves = new List<Move>();
            var linearPath = BoardEngine.GetLinearPath<Fig>(start, linear.dir, length, board);

            return GetPossibleMoves(start, linearPath, board);
        }

        public static List<Vector2Int> GetFigureMoves(Vector2Int pos, Option<Fig> board) {
            var figMoves = new List<Vector2Int>();

            return figMoves;
        }

        public static MoveRes MoveFigure(Move move, Option<Fig>[,] board) {
            var moveRes = new MoveRes();
            var posTo = move.to;
            var posFrom = move.from;
            var figToOpt = board[posTo.x, posTo.y];

            moveRes.pos = posTo;
            moveRes.error = MoveError.ImpossibleMove;

            if (ChessEngine.IsPossibleMove(move, board)) {
                moveRes.error = MoveError.None;

                if (figToOpt.IsSome()) {
                    moveRes.error = MoveError.MoveOnFigure;
                }
            }

            board[posTo.x, posTo.y] = board[posFrom.x, posFrom.y];
            board[posFrom.x, posFrom.y] = Option<Fig>.None();

            var figure = board[move.to.x, move.to.y].Peel();
            board[move.to.x, move.to.y] = Option<Fig>.Some(figure);

            return moveRes;
        }

        public static bool IsCastlingMove (Move move, List<Move> castlingMoves) {
            foreach (Move castlMove in castlingMoves) {
                if (Equals(castlMove, move)) {
                    return true;
                }
            }
            return false;
        }

    }
}

