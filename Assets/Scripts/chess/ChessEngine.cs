using System.Collections.Generic;
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
        public FigureType type;

        public static Fig CreateFig(bool white, FigureType type) {
            return new Fig {
                white = white,
                type = type
            };
        }
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

        public static List<Move> PossibleSquareMoves(
            Vector2Int pos,
            List<Vector2Int> square,
            Option<Fig> [,] board
        ) {
            var possMoves = new List<Move>();
            var fig = board[pos.x, pos.y].Peel();

            foreach (var cell in square) {
                var move = new Move {
                    from = pos,
                    to = cell
                };

                if (IsPossibleMove(move, board)) {
                    possMoves.Add(move);
                }
            }
            return possMoves;
        }

        public static List<Move> CalcLinearMoves(
            Vector2Int pos,
            LinearMovement linear,
            int length,
            Option<Fig>[,] board
        ) {
            var moves = new List<Move>();
            var dirs = new List<Vector2Int>();

            if (linear.diagonal.HasValue) {
                dirs.AddRange(linear.diagonal.Value.diagonalDirs);
            }

            if (linear.straight.HasValue) {
                dirs.AddRange(linear.straight.Value.straightDirs);
            }

            foreach (Vector2Int dir in dirs) {
                var linearPath = BoardEngine.CalcLinearPath<Fig>(pos, dir, length, board);
                foreach (Vector2Int cell in linearPath) {
                    var move = new Move {
                        from = pos,
                        to = cell
                    };

                    if (IsPossibleMove(move, board)) {
                        moves.Add(move);
                    }
                }
            }
            return moves;
        }
    }
}

