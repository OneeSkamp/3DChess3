using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;
using collections;

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
        public static List<DoubleMove> GetPossibleMoves(
            Vector2Int start,
            List<Vector2Int> movePath,
            Option<Fig>[,] board
        ) {
            var possMoves = new List<DoubleMove>();
            var doubleMove = new DoubleMove();

            foreach (var cell in movePath) {
                var move = new Move {
                    from = start,
                    to = cell
                };

                if (ChessEngine.IsPossibleMove(move, board)) {
                    doubleMove.first = move;
                    possMoves.Add(doubleMove);
                }

                possMoves.AddRange(GetCastlingMoves(start, board));
            }
            return possMoves;
        }

        public static List<DoubleMove> GetPossibleLinearMoves(
            Vector2Int start,
            LinearMovement linear,
            int length,
            Option<Fig>[,] board
        ) {
            var linearPath = BoardEngine.GetLinearPath<Fig>(start, linear.dir, length, board);

            return GetPossibleMoves(start, linearPath, board);
        }

        public static List<DoubleMove> GetFigureMoves(
            Vector2Int pos,
            List<Movement> movements,
            Option<Fig>[,] board
        ) {
            var figMoves = new List<DoubleMove>();
            var fig = board[pos.x, pos.y];

            foreach (Movement type in movements) {
                if (type.square.HasValue) {
                    var squarePath = BoardEngine.GetSquarePath(pos, type.square.Value.side);
                    var square = new BindableList<Vector2Int>();

                    if (fig.Peel().type == FigureType.Knight) {
                        square = BoardEngine.RemoveSquareParts(squarePath, 0, 1);
                    }

                    if (fig.Peel().type == FigureType.King) {
                        square = BoardEngine.RemoveSquareParts(squarePath, 0, 0);
                    }

                    var list = new List<Vector2Int>();
                    foreach (var i in square) {
                        list.Add(i.value);
                    }

                    figMoves.AddRange(GetPossibleMoves(pos, list, board));

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

        public static List<DoubleMove> GetPawnMoves(Vector2Int pos, Option<Fig>[,] board) {
            var pawnMoves = new List<DoubleMove>();
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
            figure.counter++;
            board[move.to.x, move.to.y] = Option<Fig>.Some(figure);

            return moveRes;
        }

        public static List<DoubleMove> GetCastlingMoves(Vector2Int kingPos, Option<Fig>[,] board) {
            var castlingMoves = new List<DoubleMove>();
            var leftDir = new Vector2Int(0, -1);
            var rightDir = new Vector2Int(0, 1);

            var leftLength = BoardEngine.GetLinearLength(kingPos, leftDir, board);
            var rightLength = BoardEngine.GetLinearLength(kingPos, rightDir, board);

            var leftPath = BoardEngine.GetLinearPath(kingPos, leftDir, leftLength, board);
            var rightPath = BoardEngine.GetLinearPath(kingPos, rightDir, rightLength, board);

            var leftPos = new Vector2Int();
            var rightPos = new Vector2Int();

            var move = new DoubleMove();

            if (leftPath.Count > 0) {
                leftPos = leftPath[leftPath.Count - 1];
            }

            if (rightPath.Count > 0) {
                rightPos = rightPath[rightPath.Count - 1];
            }

            var fig = board[kingPos.x, kingPos.y].Peel();

            if (fig.type == FigureType.King && fig.counter == 0) {
                var leftFig = board[leftPos.x, leftPos.y].Peel();
                var rightFig = board[rightPos.x, rightPos.y].Peel();

                if (leftFig.type == FigureType.Rook && leftFig.counter == 0) {
                    move = DoubleMove.Mk(
                        Move.Mk(kingPos, new Vector2Int(kingPos.x, kingPos.y - 2)),
                        Move.Mk(leftPos, new Vector2Int(kingPos.x, kingPos.y - 1))
                    );

                    castlingMoves.Add(move);
                }

                if (rightFig.type == FigureType.Rook && rightFig.counter == 0) {
                    move = DoubleMove.Mk(
                        Move.Mk(kingPos, new Vector2Int(kingPos.x, kingPos.y + 2)),
                        Move.Mk(rightPos, new Vector2Int(kingPos.x, kingPos.y + 1))
                    );

                    castlingMoves.Add(move);
                }
            }

            return castlingMoves;
        }
    }
}