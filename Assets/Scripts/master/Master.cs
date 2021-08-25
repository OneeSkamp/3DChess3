using UnityEngine;
using System.Collections.Generic;
using chess;
using board;
using option;

namespace master {
    public enum MoveError {
        None,
        ImpossibleMove,
        MoveOnFigure
    }
    public struct MoveRes {
        public Vector2Int pos;
        public MoveError error;

    }

    public struct CastlingRes {
        public MoveRes rookRes;
        public MoveRes kingRes;
    }
    public static class Master {
        public static MoveRes MoveFigure(Move move, Option<Fig>[,] board) {
            var moveRes = new MoveRes();
            var posTo = move.to;
            var posFrom = move.from;
            var figToOpt = board[posTo.x, posTo.y];

            moveRes.pos = posTo;
            moveRes.error = MoveError.ImpossibleMove;

            if (ChessEngine.IsPossibleMove(move, board)) {
                moveRes.error = MoveError.None;

                if (figToOpt.IsSome()) {
                    moveRes.error = MoveError.MoveOnFigure;
                }
            }

            board[posTo.x, posTo.y] = board[posFrom.x, posFrom.y];
            board[posFrom.x, posFrom.y] = Option<Fig>.None();

            var figure = board[move.to.x, move.to.y].Peel();
            board[move.to.x, move.to.y] = Option<Fig>.Some(figure);

            return moveRes;
        }

        public static Vector2Int FindEnemyKing(bool whiteMove, Option<Fig>[,] board) {
            var kingPos = new Vector2Int();
            var width = board.GetLength(0);
            var height = board.GetLength(1);

            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    var figOpt = board[i, j];

                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();
                        if (fig.type == FigureType.King && fig.white != whiteMove) {
                            kingPos = new Vector2Int(i, j);
                            Debug.Log(fig.type + " " + fig.white);
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
                    var length = BoardEngine.CalcLinearLength(kingPos, kingDir, board);
                    var linearPath = BoardEngine.CalcLinearPath(kingPos, kingDir, length, board);

                    if (linearPath.Count > 0) {
                        lastPos = linearPath[linearPath.Count - 1];
                        var figOpt = board[lastPos.x, lastPos.y];

                        if (figOpt.IsSome()) {
                            var fig = figOpt.Peel();
                            var moveList = moveTypes[fig.type];

                            foreach (Movement figMovement in moveList) {
                                var figDir = figMovement.linear.Value.dir;
                                if (fig.white != king.white && figDir == kingDir * - 1) {
                                    Debug.Log("check");
                                }
                            }
                        }
                    }
                }

                if (movement.square.HasValue) {
                    var side = movement.square.Value.side;
                    var squarePath = BoardEngine.CalcSquarePath(kingPos, side);
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

