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
        public FixedMovement attack;
        public Vector2Int? defPos;
        public List<Vector2Int> path;
    }

    public struct KingsPos {
        public Vector2Int white;
        public Vector2Int black;
    }

    public enum CheckError {
        BoardIsNull,
        NoFigureOnPos,
        PosOutsideBoard,
        ImposterKing
    }

    public static class ChessInspector {
        public static Result<Option<Fig>[,], CheckError> GetFilteredBoard(
            Vector2Int safePos,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<Option<Fig>[,], CheckError>.Err(CheckError.BoardIsNull);
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (!BoardEngine.IsOnBoard(safePos, size)) {
                return Result<Option<Fig>[,], CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var figOpt = board[safePos.x, safePos.y];
            if (figOpt.IsNone()) {
                return Result<Option<Fig>[,], CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var boardClone = BoardEngine.CopyBoard(board);
            var fig = figOpt.Peel();

            for (int i = 0; i < boardClone.GetLength(0); i++) {
                for (int j = 0; j < boardClone.GetLength(1); j++) {
                    if (boardClone[i, j].IsSome()) {
                        var currentFig = boardClone[i, j].Peel();
                        if (currentFig.white == fig.white && fig.type != currentFig.type) {
                            boardClone[i, j] = Option<Fig>.None();
                        }
                    }
                }
            }

            return Result<Option<Fig>[,], CheckError>.Ok(boardClone);
        }

        public static Result<List<CheckInfo>, CheckError> GetPotentialCheckInfos(
            Vector2Int kingPos,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.BoardIsNull);
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (!BoardEngine.IsOnBoard(kingPos, size)) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var kingOpt = board[kingPos.x, kingPos.y];
            if (kingOpt.IsNone()) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var king = kingOpt.Peel();
            if (king.type != FigureType.King) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.ImposterKing);
            }

            var checkInfos = new List<CheckInfo>();
            var allMovement = Movements.queenMovement;
            allMovement.AddRange(Movements.knightMovement);
            var boardClone = GetFilteredBoard(kingPos, board).AsOk();

            foreach (var movement in allMovement) {
                if (!movement.linear.HasValue) {
                    continue;
                }

                var dir = movement.linear.Value.dir;
                var length = BoardEngine.GetLinearLength(kingPos, dir, boardClone);
                var linearPath = BoardEngine.GetLinearPath(kingPos, dir, length, boardClone);
                var square = BoardEngine.GetSquarePath(kingPos, 5);
                var bindableKnightPath = BoardEngine.RemoveSquareParts(square, 0, 1);

                if (linearPath.Count != 0) {
                    var figPos = linearPath[linearPath.Count - 1];
                    var figOpt = boardClone[figPos.x, figPos.y];
                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();
                        var figMovements = Movements.movements[fig.type];

                        foreach (var figMovement in figMovements) {
                            if (figMovement.linear.HasValue) {
                                var figDir = figMovement.linear.Value.dir;
                                if (dir == figDir) {
                                    var newMovement = new Movement {
                                        linear = new LinearMovement{
                                            dir = dir
                                        }
                                    };
                                    checkInfos.Add(
                                        new CheckInfo {
                                            attack = new FixedMovement {
                                                start = figPos,
                                                movement = newMovement
                                            },
                                            path = linearPath
                                        }
                                    );
                                }
                            }
                        }
                    }
                }
            }

            return Result<List<CheckInfo>, CheckError>.Ok(checkInfos);
        }

        public static List<CheckInfo> GetCheckInfos(
            Vector2Int kingPos,
            Option<Fig>[,] board
        ) {
            var allMovement = Movements.queenMovement;
            allMovement.AddRange(Movements.knightMovement);

            var boardClone = GetFilteredBoard(kingPos, board).AsOk();
            var king = boardClone[kingPos.x, kingPos.y].Peel();

            var checkInfos = GetPotentialCheckInfos(kingPos, board).AsOk();


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
                    if (checkInfo.attack.movement.linear.HasValue) {
                        var linear = checkInfo.attack.movement.linear.Value;
                        movement.Add(new Movement {
                            linear = new LinearMovement { dir = linear.dir }
                        });
                    }

                    var moves = MoveEngine.GetMoves(pos, movement, lastMove, board).AsOk();
                    return moves;
                }
            }

            return MoveEngine.GetMoves(pos, movements, lastMove, board).AsOk();
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