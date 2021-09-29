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
            Debug.Log(linear.length);
            var movementLoc = BoardEngine.GetMovementLoc(pos, linear, board);
            Debug.Log(movementLoc.pos.Peel());
            if (moveType == MoveType.Move) {
                Debug.Log("moveType == MoveType.Move");
                if (movementLoc.pos.IsSome()) {
                    Debug.Log("movementLoc.pos.IsSome");
                    linear.length--;
                }
            }

            if (moveType == MoveType.Attack) {
                Debug.Log("moveType == MoveType.Attack");
                if (movementLoc.pos.IsNone()) {
                    linear.length--;
                }
            }

            return linear.length;
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