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

        public static List<MoveInfo> CheckInfo(
            Vector2Int kingPos,
            Option<Fig>[,] board
        ) {
            var kingsPos = GetKingsPos(board);
            var possibleMoves = new List<MoveInfo>();
            var queenMovement = Movements.queenMovement;
            var pathList = new List<List<Vector2Int>>();

            var boardClone = BoardEngine.CopyBoard(board);
            var king = boardClone[kingPos.x, kingPos.y].Peel();

            for (int i = 0; i < boardClone.GetLength(0); i++) {
                for (int j = 0; j < boardClone.GetLength(1); j++) {
                    if (boardClone[i, j].IsSome()) {
                        var fig = boardClone[i, j].Peel();

                        if (fig.white == king.white && fig.type != FigureType.King) {
                            boardClone[i, j] = Option<Fig>.None();
                        }
                    }
                }
            }

            foreach (var movement in queenMovement) {
                var dir = movement.linear.Value.dir;
                var length = BoardEngine.GetLinearLength(kingPos, dir, boardClone);
                var path = BoardEngine.GetLinearPath(kingPos, dir, length, boardClone);

                if (path.Count != 0) {
                    var figPos = path[path.Count - 1];
                    var figOpt = boardClone[figPos.x, figPos.y];
                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();
                        var figMovements = Movements.movements[fig.type];

                        foreach (var figMovement in figMovements) {
                            var figDir = figMovement.linear.Value.dir;
                            if (fig.white != king.white && dir == figDir && fig.type != FigureType.Pawn) {
                                pathList.Add(path);
                                Debug.Log(fig.type + " " + fig.white);
                            }
                        }
                    }
                }
            }

            var count = 0;
            var defenceCells = new List<Vector2Int>();
            foreach (var path in pathList) {
                foreach (var cell in path) {
                    var figOpt = board[cell.x, cell.y];
                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();
                        if (fig.white = king.white) {
                            defenceCells.Add(cell);
                            count++;
                        }
                    }
                }

                if (count >= 2) {
                    continue;
                }

                // if (count == 1) {
                //     var fig = board[defenceCells[0].x, defenceCells[0].y].Peel();
                //     var figMoves = MoveEngine.GetFigureMoves()
                //     foreach (var move in )
                // }

            }

            return possibleMoves;
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