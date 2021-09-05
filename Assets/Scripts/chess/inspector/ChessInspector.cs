using UnityEngine;
using System.Collections.Generic;
using option;
using chess;
using move;
using movements;

namespace inspector {
    public class ChessInspector : MonoBehaviour {
        public static bool IsUnderAttackPos(Vector2Int pos, Option<Fig>[,] board) {
            var figMoves = new List<DoubleMove>();
            var movements = Movements.movements;
            var moves = MoveEngine.GetFigureMoves(pos, movements[FigureType.Queen], board);
            moves.AddRange(MoveEngine.GetFigureMoves(pos, movements[FigureType.Knight], board));

            foreach (var move in moves) {
                var figOpt = board[move.first.Value.to.x, move.first.Value.to.y];

                if (figOpt.IsSome()) {
                    var fig = figOpt.Peel();
                    var dmoves = MoveEngine.GetFigureMoves(
                        move.first.Value.to,
                        movements[fig.type],
                        board
                    );
                    figMoves.AddRange(dmoves);
                }
            }

            foreach (var move in figMoves) {
                if (Equals(move.first.Value.to, pos)) {
                    return true;
                }
            }

            return false;
        }

        public static List<DoubleMove> GetFigurePossibleMoves(
            List<DoubleMove> figMoves,
            Vector2Int kingPos,
            Option<Fig>[,] board
        ) {
            var figPossMoves = new List<DoubleMove>();
            var savePos = new DoubleMove();
            Option<Fig>[,] boardClone = (Option<Fig>[,])board.Clone();

            foreach (var figMove in figMoves) {
                savePos = figMove;
                var to = figMove.first.Value.to;
                var from = figMove.first.Value.from;
                var kPos = kingPos;
                if (board[from.x, from.y].Peel().type == FigureType.King) {
                    kPos = to;
                }

                if (boardClone[to.x, to.y].IsNone()) {
                    boardClone[to.x, to.y] = boardClone[from.x, from.y];
                    boardClone[from.x, from.y] = Option<Fig>.None();
                    if (!IsUnderAttackPos(kPos, boardClone)) {
                        figPossMoves.Add(figMove);
                    }

                    boardClone[from.x, from.y] = boardClone[to.x, to.y];
                    boardClone[to.x, to.y] = Option<Fig>.None();
                }

                if (boardClone[to.x, to.y].IsSome()) {
                    var fig = boardClone[to.x, to.y];
                    boardClone[to.x, to.y] = boardClone[from.x, from.y];
                    boardClone[from.x, from.y] = Option<Fig>.None();
                    if (!IsUnderAttackPos(kPos, boardClone)) {
                        figPossMoves.Add(figMove);
                    }

                    boardClone[from.x, from.y] = boardClone[to.x, to.y];
                    boardClone[figMove.first.Value.to.x, figMove.first.Value.to.y] = fig;
                }
                kPos = kingPos;
            }

            return figPossMoves;
        }
    }
}