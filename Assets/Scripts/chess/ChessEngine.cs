using System;
using board;
using UnityEngine;
using option;

namespace chess {
    public enum FigureType {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public struct Fig {
        public bool white;
        public int counter;
        public FigureType type;

        public static Fig CreateFig(bool white, FigureType type) {
            return new Fig {
                white = white,
                type = type
            };
        }
    }

    public struct DoubleMove {
        public Move first;
        public Move second;
    }

    public struct Move {
        public Vector2Int from;
        public Vector2Int to;
    }

    public static class ChessEngine {
        public static bool IsPossibleMove(Move move, Option<Fig>[,] board) {
            var fromPos = move.from;
            var figOpt = board[fromPos.x, fromPos.y];
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));

            if (figOpt.IsNone()) {
                return false;
            }

            var fig = figOpt.Peel();

            if (BoardEngine.IsOnBoard(move.to, size)) {
                var nextFigOpt = board[move.to.x, move.to.y];

                if (nextFigOpt.IsNone()) {
                    return true;
                }

                var nextFig = nextFigOpt.Peel();
                if (fig.white != nextFig.white) {
                    return true;
                }
            }
            return false;
        }
    }
}

