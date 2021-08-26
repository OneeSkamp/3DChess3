using UnityEngine;
using System.Collections.Generic;
using chess;
using board;
using option;

namespace master {
    public static class Master {
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
                    var lastPos = new Vector2Int();
                    var length = BoardEngine.GetLinearLength(kingPos, kingDir, board);
                    var linearPath = BoardEngine.GetLinearPath(kingPos, kingDir, length, board);

                    if (linearPath.Count > 0) {
                        lastPos = linearPath[linearPath.Count - 1];
                        var figOpt = board[lastPos.x, lastPos.y];

                        if (figOpt.IsSome()) {
                            var fig = figOpt.Peel();
                            var moveList = moveTypes[fig.type];

                            if (fig.type == FigureType.Pawn && length > 1) {
                                continue;
                            }

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
                }

                if (movement.square.HasValue) {
                    var side = movement.square.Value.side;
                    var squarePath = BoardEngine.GetSquarePath(kingPos, side);
                    var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
                    squarePath = BoardEngine.ChangeSquarePath(squarePath, 1);

                    foreach (Vector2Int cell in squarePath) {
                        if (BoardEngine.IsOnBoard(new Vector2Int(cell.x, cell.y), size)) {
                            var figOpt = board[cell.x, cell.y];

                            if (figOpt.IsSome()) {
                                var fig = figOpt.Peel();
                                if (fig.type == FigureType.Knight && fig.white != king.white) {
                                    Debug.Log("check");
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static List<Move> ChangePawnMoves(
            Vector2Int pos,
            List<Move> moves,
            Option<Fig>[,] board
        ) {
            var newMoves = new List<Move>();
            var fig = board[pos.x, pos.y].Peel();
            var prop = 1;

            if (fig.white) {
                prop = -1;
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var nextFig = board[pos.x + prop, pos.y];
            var leftPos = new Vector2Int(pos.x + prop, pos.y + prop);
            var rightPos = new Vector2Int(pos.x + prop, pos.y - prop);
            var leftOnBoard = BoardEngine.IsOnBoard(leftPos, size);
            var rightOnBoard = BoardEngine.IsOnBoard(rightPos, size);


            foreach (Move move in moves) {
                if (pos.x == 6 && prop == -1 || pos.x == 1 && prop == 1) {
                    var newPos = new Vector2Int(pos.x + prop * 2, pos.y);
                    if (Equals(newPos, move.to) && nextFig.IsNone()) {
                        newMoves.Add(move);
                    }
                }

                if (Equals(new Vector2Int(pos.x + prop, pos.y), move.to) && nextFig.IsNone()) {
                    newMoves.Add(move);
                }

                if (leftOnBoard && Equals(leftPos, move.to) 
                    && board[pos.x + prop, pos.y + prop].IsSome()) {

                    newMoves.Add(move);
                }

                if (rightOnBoard && Equals(rightPos, move.to) 
                    && board[pos.x + prop, pos.y - prop].IsSome()) {

                    newMoves.Add(move);
                }
            }
            return newMoves;
        }
    }
}

