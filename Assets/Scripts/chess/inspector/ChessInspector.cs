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

        public static List<MoveInfo> GetPossibleMoves(
            Vector2Int pos,
            Vector2Int kingPos,
            bool whiteMove,
            Option<Fig>[,] board
        ) {
            var possibleMoves = new List<MoveInfo>();
            var queenMovement = Movements.queenMovement;

            var boardClone = BoardEngine.CopyBoard(board);
            var king = boardClone[kingPos.x, kingPos.y].Peel();

            for (int i = 0; i < boardClone.GetLength(0); i++) {
                for (int j = 0; j < boardClone.GetLength(1); j++) {
                    if (boardClone[i, j].IsSome()) {
                        var fig = boardClone[i, j].Peel();

                        if (fig.white == whiteMove && fig.type != FigureType.King) {
                            boardClone[i, j] = Option<Fig>.None();
                        }
                    }
                }
            }

            foreach (var movement in queenMovement) {
                var dir = movement.linear.Value.dir;
                var length = BoardEngine.GetLinearLength(kingPos, dir, board);
                var path = BoardEngine.GetLinearPath(kingPos, dir, length, board);

                if (path.Count != 0) {
                    var figPos = path[path.Count - 1];
                    var figOpt = boardClone[figPos.x, figPos.y];
                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();
                        var figMovements = Movements.movements[fig.type];

                        foreach (var figMovement in figMovements) {
                            var figDir = figMovement.linear.Value.dir;
                            if (fig.white != king.white && dir == figDir) {
                                Debug.Log("check");
                            }
                        }
                    }
                }

            }

            return possibleMoves;
        }
        // public static List<MoveInfo> GetPossibleMoves(
        //     Vector2Int pos,
        //     Vector2Int kingPos,
        //     MoveInfo lastMove,
        //     Option<Fig>[,] board
        // ) {
        //     var figPossMoves = new List<MoveInfo>();
        //     var movements = Movements.movements[board[pos.x, pos.y].Peel().type];
        //     var figMoves = MoveEngine.GetFigureMoves(pos, movements, lastMove, board);

        //     var boardClone = BoardEngine.CopyBoard(board);

        //     foreach (var figMove in figMoves) {
        //         var to = figMove.move.first.Value.to;
        //         var from = figMove.move.first.Value.from;
        //         var kPos = kingPos;
        //         if (board[from.x, from.y].Peel().type == FigureType.King) {
        //             kPos = to;
        //         }

        //         if (boardClone[to.x, to.y].IsNone()) {
        //             boardClone[to.x, to.y] = boardClone[from.x, from.y];
        //             boardClone[from.x, from.y] = Option<Fig>.None();
        //             if (!MoveEngine.IsUnderAttackPos(kPos, true, lastMove, boardClone)) {
        //                 figPossMoves.Add(figMove);
        //             }

        //             boardClone[from.x, from.y] = boardClone[to.x, to.y];
        //             boardClone[to.x, to.y] = Option<Fig>.None();
        //         }

        //         if (boardClone[to.x, to.y].IsSome()) {
        //             var fig = boardClone[to.x, to.y];
        //             boardClone[to.x, to.y] = boardClone[from.x, from.y];
        //             boardClone[from.x, from.y] = Option<Fig>.None();
        //             if (!MoveEngine.IsUnderAttackPos(kPos, true, lastMove, boardClone)) {
        //                 figPossMoves.Add(figMove);
        //             }

        //             boardClone[from.x, from.y] = boardClone[to.x, to.y];
        //             boardClone[figMove.move.first.Value.to.x, figMove.move.first.Value.to.y] = fig;
        //         }
        //         kPos = kingPos;
        //     }
        //     return figPossMoves;
        // }

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