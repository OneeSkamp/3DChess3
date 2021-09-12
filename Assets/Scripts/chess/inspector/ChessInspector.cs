using UnityEngine;
using System.Collections.Generic;
using option;
using chess;
using move;
using board;
using movements;

namespace inspector {
    public struct KingsPos {
        public Vector2Int white;
        public Vector2Int black;
    }
    public class ChessInspector : MonoBehaviour {
        public static bool IsUnderAttackPos(
            Vector2Int pos,
            bool white,
            MoveInfo lastMove,
            Option<Fig>[,] board
        ) {
            var figMoves = new List<MoveInfo>();
            var hasFig = true;
            var movements = Movements.movements;

            if (board[pos.x, pos.y].IsNone()) {
                var fig = Fig.CreateFig(white, FigureType.Knight);
                hasFig = false;
                board[pos.x, pos.y] = Option<Fig>.Some(fig);
            }
            var queen = movements[FigureType.Queen];
            var knight = movements[FigureType.Knight];
            var moves = MoveEngine.GetFigureMoves(pos, queen, lastMove, board);
            moves.AddRange(MoveEngine.GetFigureMoves(pos, knight, lastMove, board));

            foreach (var move in moves) {
                var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
                var toX = move.move.first.Value.to.x;
                var toY = move.move.first.Value.to.y;
                if (BoardEngine.IsOnBoard(new Vector2Int(toX, toY), size)) {
                    var figOpt = board[toX, toY];

                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();
                        var dmoves = MoveEngine.GetFigureMoves(
                            move.move.first.Value.to,
                            movements[board[toX, toY].Peel().type],
                            lastMove,
                            board
                        );
                        figMoves.AddRange(dmoves);
                    }
                }
            }

            foreach (var move in figMoves) {
                if (move.move.first.Value.to == pos) {
                    return true;
                }
            }

            if (!hasFig) {
                board[pos.x, pos.y] = Option<Fig>.None();
            }

            return false;
        }

        public static List<MoveInfo> GetPossibleMoves(
            Vector2Int pos,
            Vector2Int kingPos,
            MoveInfo lastMove,
            Option<Fig>[,] board
        ) {
            var figPossMoves = new List<MoveInfo>();
            var movements = Movements.movements[board[pos.x, pos.y].Peel().type];
            var figMoves = MoveEngine.GetFigureMoves(pos, movements, lastMove, board);

            var boardClone = BoardEngine.CopyBoard(board);

            foreach (var figMove in figMoves) {
                var to = figMove.move.first.Value.to;
                var from = figMove.move.first.Value.from;
                var kPos = kingPos;
                if (board[from.x, from.y].Peel().type == FigureType.King) {
                    kPos = to;
                }

                if (boardClone[to.x, to.y].IsNone()) {
                    boardClone[to.x, to.y] = boardClone[from.x, from.y];
                    boardClone[from.x, from.y] = Option<Fig>.None();
                    if (!IsUnderAttackPos(kPos, true, lastMove, boardClone)) {
                        figPossMoves.Add(figMove);
                    }

                    boardClone[from.x, from.y] = boardClone[to.x, to.y];
                    boardClone[to.x, to.y] = Option<Fig>.None();
                }

                if (boardClone[to.x, to.y].IsSome()) {
                    var fig = boardClone[to.x, to.y];
                    boardClone[to.x, to.y] = boardClone[from.x, from.y];
                    boardClone[from.x, from.y] = Option<Fig>.None();
                    if (!IsUnderAttackPos(kPos, true, lastMove, boardClone)) {
                        figPossMoves.Add(figMove);
                    }

                    boardClone[from.x, from.y] = boardClone[to.x, to.y];
                    boardClone[figMove.move.first.Value.to.x, figMove.move.first.Value.to.y] = fig;
                }
                kPos = kingPos;
            }
            return figPossMoves;
        }

        public static KingsPos GetKingsPos(Option<Fig>[,] board) {
            var kingsPos = new KingsPos();
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    var figOpt = board[i, j];
                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();
                        if (fig.type == FigureType.King) {
                            if (fig.white) {
                                kingsPos.white = new Vector2Int(i, j);
                            }
                            if (!fig.white) {
                                kingsPos.black = new Vector2Int(i, j);
                            }
                        }
                    }
                }
            }

            return kingsPos;
        }
    }
}