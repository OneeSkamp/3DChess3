using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using chess;
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

    public struct CastlingRes {
        public MoveRes rookRes;
        public MoveRes kingRes;
    }
    public static class MoveController {
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

