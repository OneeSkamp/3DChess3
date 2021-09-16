using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using option;
using chess;
using move;
using board;
using movements;

namespace inspector {
    public struct CheckInfo {
        public Vector2Int attackPos;
        public Vector2Int attackDir;
        public Vector2Int? defPos;
        public List<Vector2Int> path;
    }

    public struct KingsPos {
        public Vector2Int white;
        public Vector2Int black;
    }

    public class ChessInspector : MonoBehaviour {
        public static List<CheckInfo> GetCheckInfos(
            Vector2Int kingPos,
            Option<Fig>[,] board
        ) {
            var checkInfos = new List<CheckInfo>();
            var queenMovement = Movements.queenMovement;
            var knightMovemetn = Movements.knightMovement;

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
                            if (figMovement.linear.HasValue) {
                                var figDir = figMovement.linear.Value.dir;
                                if (dir == figDir) {
                                    var checkInfo = new CheckInfo();
                                    checkInfo.attackDir = dir;
                                    checkInfo.attackPos = figPos;
                                    checkInfo.path = path;
                                    checkInfos.Add(checkInfo);
                                }
                            }
                        }
                    }
                }
            }

            var newCheckInfos = new List<CheckInfo>();
            foreach (var checkInfo in checkInfos) {
                var checkPath = checkInfo.path;
                var blockCounter = 0;
                var defPos = new Vector2Int();

                foreach (var cell in checkPath) {
                    var figOpt = board[cell.x, cell.y];
                    if (figOpt.IsNone()) {
                        continue;
                    }

                    var fig = figOpt.Peel();
                    if (fig.white == king.white) {
                        defPos = cell;
                        blockCounter++;
                    }
                }

                if (blockCounter == 1) {
                    var newCheckInfo = checkInfo;
                    newCheckInfo.defPos = defPos;
                    newCheckInfos.Add(newCheckInfo);
                }

                if (blockCounter == 0) {
                    newCheckInfos.Add(checkInfo);
                }
            }

            return newCheckInfos;
        }

        public static List<MoveInfo> GetPossibleMoves(
            Vector2Int pos,
            MoveInfo lastMove,
            Option<Fig>[,] board
        ) {
            var movement = new List<Movement>();
            var white = board[pos.x, pos.y].Peel().white;
            var kingsPos = GetKingsPos(board);
            var kingPos = kingsPos.black;
            if (white) {
                kingPos = kingsPos.white;
            }
            var movements = Movements.movements[board[pos.x, pos.y].Peel().type];
            var checkInfos = GetCheckInfos(kingPos, board);
            foreach (var checkInfo in checkInfos) {
                if (checkInfo.defPos == pos) {
                    var defPos = checkInfo.defPos.Value;
                    if (board[defPos.x, defPos.y].Peel().type == FigureType.Knight) {

                        return null;
                    }
                    movement.Add(new Movement {
                        linear = new LinearMovement { dir = checkInfo.attackDir }
                    });

                    var moves = MoveEngine.GetMoves(pos, movement, lastMove, board);
                    return moves;
                }
            }

            return MoveEngine.GetMoves(pos, movements, lastMove, board);
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