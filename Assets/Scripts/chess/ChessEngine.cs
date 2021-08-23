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
            var fig = board[move.from.x, move.from.y].Peel();

            if (BoardEngine.IsOnBoard(move.to, board.GetLength(0), board.GetLength(1))) {
                var nextFig = board[move.to.x, move.to.y].Peel();
                if (board[move.to.x, move.to.y].IsNone()) {
                    return true;
                }
                if (fig.white != nextFig.white) {
                    return true;
                }
            }
            return false;
        }

        public static List<Vector2Int> CalcSquareMoves(
            Vector2Int pos,
            List<Vector2Int> square,
            Option<Fig> [,] board
        ) {
            var possMoves = new List<Vector2Int>();
            var fig = board[pos.x, pos.y].Peel();

            foreach (var cell in square) {
                var move = new Move {
                    from = pos,
                    to = cell
                };

                if (IsPossibleMove(move, board)) {
                    possMoves.Add(move.to);
                }
            }
            return possMoves;
        }

        public static List<Vector2Int> CalcLinearMoves(
            Vector2Int pos,
            LinearMovement linear,
            Option<Fig>[,] board
        ) {
            var moves = new List<Vector2Int>();
            var dirs = new List<Vector2Int>();

            if (linear.diagonal != null) {
                dirs.AddRange(linear.diagonal.Value.diagonalDirs);
            }

            if (linear.straight != null) {
                dirs.AddRange(linear.straight.Value.straightDirs);
            }

            foreach (Vector2Int dir in dirs) {
                var linearPath = BoardEngine.CalcLinearPath<Fig>(pos, dir, board);
                foreach (Vector2Int cell in linearPath) {
                    var move = new Move {
                        from = pos,
                        to = cell
                    };

                    if (IsPossibleMove(move, board)) {
                        moves.Add(move.to);
                    }
                }
            }
            return moves;
        }
    }
}

