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

    public enum CastlingType {
        WShortCastling,
        WLongCastling,
        BShortCastling,
        BLongCastling
    }

    public struct CastlingInfo {
        public Vector2Int rookPos;
        public List<Move> castlingMoves;
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

        public static List<Move> GetFigureMoves(
            Vector2Int pos,
            List<Movement> movements,
            Option<Fig>[,] board
        ) {
            var figMoves = new List<Move>();
            var fig = board[pos.x, pos.y];

            foreach (Movement type in movements) {
                if (type.square.HasValue) {
                    var squarePath = BoardEngine.GetSquarePath(pos, type.square.Value.side);
                    var square = new List<Vector2Int>();

                    if (fig.Peel().type == FigureType.Knight) {
                        square = BoardEngine.RemoveSquareParts(squarePath, 0, 1);
                    }

                    if (fig.Peel().type == FigureType.King) {
                        square = BoardEngine.RemoveSquareParts(squarePath, 0, 0);
                    }

                    figMoves.AddRange(GetPossibleMoves(pos, square, board));

                } else {
                    if (fig.Peel().type == FigureType.Pawn) {
                        figMoves.AddRange(GetPawnMoves(pos, board));

                    } else {
                        var linear = type.linear.Value;
                        var length = BoardEngine.GetLinearLength(pos, linear.dir, board);

                        figMoves.AddRange(GetPossibleLinearMoves(pos, linear, length, board));
                    }
                }
            }

            return figMoves;
        }

        public static List<Move> GetPawnMoves(Vector2Int pos, Option<Fig>[,] board) {
            var pawnMoves = new List<Move>();
            var pawnPath = new List<Vector2Int>();
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var pawn = board[pos.x, pos.y].Peel();
            int prop = 1;
            var length = 1;

            if (pawn.white) {
                prop = -1;
            }

            var forwardDir = new Vector2Int(1 * prop, 0);
            var leftPos = new Vector2Int(pos.x + prop, pos.y - prop);
            var rightPos = new Vector2Int(pos.x + prop, pos.y + prop);

            if (pawn.white && pos.x == 6 || !pawn.white && pos.x == 1) {
                length = 2;
            }

            var forwardPath = BoardEngine.GetLinearPath(pos, forwardDir, length, board);

            foreach (var cell in forwardPath) {
                if (BoardEngine.IsOnBoard(cell, size) && board[cell.x, cell.y].IsNone()) {
                    pawnPath.Add(cell);
                }
            }

            if (BoardEngine.IsOnBoard(rightPos, size) && board[rightPos.x, rightPos.y].IsSome()) {
                pawnPath.Add(rightPos);
            }

            if (BoardEngine.IsOnBoard(leftPos, size) && board[leftPos.x, leftPos.y].IsSome()) {
                pawnPath.Add(leftPos);
            }

            return GetPossibleMoves(pos, pawnPath, board);
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

        public static CastlingInfo GetCastlingInfo(
            Dictionary<CastlingType, bool> castlings,
            bool whiteMove,
            Option<Fig>[,] board
        ) {
            var castlingRes = new CastlingInfo();
            var castlingMoves = new List<Move>();
            var kingPos = new Vector2Int();
            //var kingPos = KingController.FindKingPos(whiteMove, board);
            var king = board[kingPos.x, kingPos.y].Peel();

            var right1 = board[kingPos.x, 5].IsNone();
            var right2 = board[kingPos.x, 6].IsNone();
            var left1 = board[kingPos.x, 3].IsNone();
            var left2 = board[kingPos.x, 2].IsNone();
            var left3 = board[kingPos.x, 1].IsNone();
            var move = new Move();
            var rookPos = new Vector2Int();
            move.from = kingPos;

            if (king.white) {
                if (right1 && right2 && castlings[CastlingType.WShortCastling]) {
                    move.to = new Vector2Int(kingPos.x, 6);
                    rookPos = new Vector2Int(kingPos.x, 7);
                    castlingMoves.Add(move);
                }

                if (left1 && left2 && left3 && castlings[CastlingType.WLongCastling]) {
                    move.to = new Vector2Int(kingPos.x, 2);
                    rookPos = new Vector2Int(kingPos.x, 0);
                    castlingMoves.Add(move);
                }

            } else {
                if (right1 && right2 && castlings[CastlingType.BShortCastling]) {
                    move.to = new Vector2Int(kingPos.x, 6);
                    rookPos = new Vector2Int(kingPos.x, 7);
                    castlingMoves.Add(move);
                }

                if (left1 && left2 && left3 && castlings[CastlingType.BLongCastling]) {
                    move.to = new Vector2Int(kingPos.x, 2);
                    rookPos = new Vector2Int(kingPos.x, 0);
                    castlingMoves.Add(move);
                }
            }
            castlingRes.rookPos = rookPos;
            castlingRes.castlingMoves = castlingMoves;

            return castlingRes;
        }

        public static void UpdateCastlingValues(
            Dictionary<CastlingType, bool> castlings,
            Move move,
            Option<Fig>[,] board
        ) {
            var fig = board[move.from.x, move.from.y].Peel();

            if (fig.type == FigureType.King) {
                if (fig.white) {
                    castlings[CastlingType.WLongCastling] = false;
                    castlings[CastlingType.WShortCastling] = false;
                } else {
                    castlings[CastlingType.BLongCastling] = false;
                    castlings[CastlingType.BShortCastling] = false;
                }
            }

            if (fig.type == FigureType.Rook) {
                if (fig.white) {
                    if (move.from.y == 7) {
                        castlings[CastlingType.WShortCastling] = false;
                    } else {
                        castlings[CastlingType.WLongCastling] = false;
                    }
                } else {
                    if (move.from.y == 7) {
                        castlings[CastlingType.BShortCastling] = false;
                    } else {
                        castlings[CastlingType.BLongCastling] = false;
                    }
                }
            }
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

