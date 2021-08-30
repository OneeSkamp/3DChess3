using System.Collections.Generic;
using UnityEngine;
using option;
using chess;
using board;

namespace king {
    public static class KingController {
        public static Vector2Int FindKingPos(bool whiteMove, Option<Fig>[,] board) {
            var kingPos = new Vector2Int();
            var width = board.GetLength(0);
            var height = board.GetLength(1);

            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    var figOpt = board[i, j];

                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();

                        if (fig.type == FigureType.King && fig.white == whiteMove) {
                            kingPos = new Vector2Int(i, j);
                        }
                    }
                }
            }
            return kingPos;
        }

        public static Fig GetLastLinearFig(List<Vector2Int> linearPath, Option<Fig>[,] board) {
            var fig = new Fig();
                if (linearPath.Count > 0) {
                    var lastPos = linearPath[linearPath.Count - 1];
                    var figOpt = board[lastPos.x, lastPos.y];

                    if (figOpt.IsSome()) {
                        fig = figOpt.Peel();
                    }
                }
            return fig;
        }

        public static bool CheckKing(
            Dictionary<FigureType, List<Movement>> moveTypes,
            List<Movement> allMovements,
            Vector2Int kingPos,
            Option<Fig>[,] board
        ) {
            foreach (Movement movement in allMovements) {
                var king = board[kingPos.x, kingPos.y].Peel();
                if (movement.linear.HasValue) {
                    var kingDir = movement.linear.Value.dir;
                    var length = BoardEngine.GetLinearLength(kingPos, kingDir, board);
                    var linearPath = BoardEngine.GetLinearPath(kingPos, kingDir, length, board);

                    var fig = GetLastLinearFig(linearPath, board);

                    if (linearPath.Count > 0) {
                        if (fig.type == FigureType.Pawn && length > 1) {
                            continue;
                        }

                        var moveList = moveTypes[fig.type];

                        foreach (Movement figMovement in moveList) {
                            if (figMovement.linear.HasValue) {
                                var figDir = figMovement.linear.Value.dir;

                                if (fig.type == FigureType.Pawn) {
                                    var forwardDir = new Vector2Int(-1, 0);

                                    if (figDir == forwardDir || figDir == -forwardDir) {
                                        continue;
                                    }
                                }

                                if (fig.white != king.white && figDir == kingDir * - 1) {
                                    Debug.Log("check");
                                }
                            }
                        }
                    }
                }

                // if (movement.square.HasValue) {
                //     var side = movement.square.Value.side;
                //     var squarePath = BoardEngine.GetSquarePath(kingPos, side);
                //     var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
                //     squarePath = BoardEngine.ChangeSquarePath(squarePath, 1);

                //     foreach (Vector2Int cell in squarePath) {
                //         if (BoardEngine.IsOnBoard(new Vector2Int(cell.x, cell.y), size)) {
                //             var figOpt = board[cell.x, cell.y];

                //             if (figOpt.IsSome()) {
                //                 var fig = figOpt.Peel();
                //                 if (fig.type == FigureType.Knight && fig.white != king.white) {
                //                     Debug.Log("check");
                //                 }
                //             }
                //         }
                //     }
                // }
            }
            return false;
        }
    }
}

