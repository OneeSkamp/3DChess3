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
        public static int GetMoveLength(
            Vector2Int pos,
            LinearMovement linear,
            MoveType moveType,
            Option<Fig>[,] board
        ) {
            linear.length = BoardEngine.GetLinearLength(pos, linear, board);
            var movementLoc = BoardEngine.GetMovementLoc(pos, linear, board);

            if (moveType == MoveType.Move) {
                if (movementLoc.pos.IsSome()) {
                    linear.length--;
                }
            }

            if (moveType == MoveType.Attack) {
                if (movementLoc.pos.IsNone()) {
                    linear.length--;
                }
            }

            return linear.length;
        }

        public static List<Vector2Int> GetRealSquarePoints(FixedMovement fixedMovement, Option<Fig>[,] board) {
            var pos = fixedMovement.start;
            var figOpt = board[pos.x, pos.y];

            var fig = figOpt.Peel();

            var square = BoardEngine.GetSquarePoints(pos, fixedMovement.figMovement.movement.square.Value);

            if (fig.type == FigureType.Knight) {
                return BoardEngine.RemoveSquareParts(square, 0, 1, board);
            }

            if (fig.type == FigureType.King) {
                return BoardEngine.RemoveSquareParts(square, 0, 0, board);
            }

            return null;
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
    }
}