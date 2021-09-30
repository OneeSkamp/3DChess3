using System.Collections.Generic;
using board;
using UnityEngine;
using option;

namespace chess {
    public enum FigColor {
        White,
        Black
    }

    public enum FigureType {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public enum MoveType {
        Attack,
        Move
    }

    public enum ChessErr {
        CantInterpMoveErr,
        PosOutsideBoard,
        BoardIsNull,
        NoFigureOnPos,
        NoSquareFig
    }

    public struct FigMovement {
        public MoveType type;
        public Movement movement;

        public static FigMovement Mk(MoveType type, Movement movement) {
            return new FigMovement { type = type, movement = movement };
        }
    }

    public struct FixedMovement {
        public Vector2Int start;
        public FigMovement figMovement;

        public static FixedMovement Mk(Vector2Int start, FigMovement figMovement) {
            return new FixedMovement { start = start, figMovement = figMovement };
        }
    }

    public struct FigLoc {
        public Vector2Int pos;
        public Option<Fig>[,] board;

        public static FigLoc Mk(Vector2Int pos, Option<Fig>[,] board) {
            return new FigLoc { pos = pos, board = board };
        }
    }

    public struct Fig {
        public FigColor color;
        public int counter;
        public FigureType type;

        public static Fig CreateFig(FigColor color, FigureType type) {
            return new Fig {
                color = color,
                type = type
            };
        }
    }

    public struct MoveInfo {
        public DoubleMove move;
        public Vector2Int? sentenced;
        public Vector2Int? shadow;
        public Vector2Int? promote;
    }

    public struct DoubleMove {
        public Move? first;
        public Move? second;

        public static DoubleMove Mk(Move? first, Move? second) {
            return new DoubleMove { first = first, second = second };
        }
    }

    public struct Move {
        public Vector2Int from;
        public Vector2Int to;

        public static Move Mk(Vector2Int from, Vector2Int to) {
            return new Move { from = from, to = to };
        }
    }

    public static class ChessEngine {
        public static Result<int, ChessErr> GetMoveLength(
            Vector2Int pos,
            FigMovement figMovement,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<int, ChessErr>.Err(ChessErr.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(pos, board)) {
                return Result<int, ChessErr>.Err(ChessErr.PosOutsideBoard);
            }

            var linear = figMovement.movement.linear.Value;
            var length = figMovement.movement.linear.Value.length;
            var maxLengthRes = BoardEngine.GetMaxLength(pos,linear, board);
            if (maxLengthRes.IsErr()) {
                return Result<int, ChessErr>.Err(InterpMoveEngineErr(maxLengthRes.AsErr()));
            }
            var maxLength = maxLengthRes.AsOk();
            if (length < 0) {
                length = maxLength;
            }

            var movementLocRes = BoardEngine.GetMovementLoc(pos, linear, board);
            if (movementLocRes.IsErr()) {
                return Result<int, ChessErr>.Err(InterpMoveEngineErr(movementLocRes.AsErr()));
            }

            var movementLoc = movementLocRes.AsOk();
            if (figMovement.type == MoveType.Move) {
                if (movementLoc.pos.IsSome()) {
                    length--;
                }
            }

            if (figMovement.type == MoveType.Attack) {
                if (movementLoc.pos.IsNone()) {
                    length--;
                }
            }

            return Result<int, ChessErr>.Ok(length);
        }

        public static Result<List<Vector2Int>, ChessErr> GetRealSquarePoints(
            FixedMovement fixedMovement,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<Vector2Int>, ChessErr>.Err(ChessErr.BoardIsNull);
            }

            var pos = fixedMovement.start;
            var figOpt = board[pos.x, pos.y];
            if (figOpt.IsNone()) {
                return Result<List<Vector2Int>, ChessErr>.Err(ChessErr.NoFigureOnPos);
            }

            var fig = figOpt.Peel();

            var square = BoardEngine.GetSquarePoints(
                pos,
                fixedMovement.figMovement.movement.square.Value
            );

            if (fig.type == FigureType.Knight) {
                var newSquare = BoardEngine.RemoveSquareParts(square, 0, 1, board);
                return Result<List<Vector2Int>, ChessErr>.Ok(newSquare);
            }

            if (fig.type == FigureType.King) {
                var newSquare = BoardEngine.RemoveSquareParts(square, 0, 0, board);
                return Result<List<Vector2Int>, ChessErr>.Ok(newSquare);
            }

            return Result<List<Vector2Int>, ChessErr>.Err(ChessErr.NoSquareFig);
        }

        public static bool IsPossibleMove(Move move, Option<Fig>[,] board) {
            var fromPos = move.from;
            var figOpt = board[fromPos.x, fromPos.y];

            if (figOpt.IsNone()) {
                return false;
            }

            var fig = figOpt.Peel();

            if (BoardEngine.IsOnBoard(move.to, board)) {
                var nextFigOpt = board[move.to.x, move.to.y];

                if (nextFigOpt.IsNone()) {
                    return true;
                }

                var nextFig = nextFigOpt.Peel();
                if (fig.color != nextFig.color) {
                    return true;
                }
            }
            return false;
        }

        public static ChessErr InterpMoveEngineErr(BoardErr err) {
            switch (err) {
                case BoardErr.BoardIsNull:
                    return ChessErr.BoardIsNull;
                case BoardErr.PosOutsideBoard:
                    return ChessErr.PosOutsideBoard;
            }

            return ChessErr.CantInterpMoveErr;
        }
    }
}