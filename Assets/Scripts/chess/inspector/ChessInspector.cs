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

    public struct FigLoc {
        public Vector2Int pos;
        public Option<Fig>[,] board;
    }

    public enum CheckError {
        BoardIsNull,
        NoFigureOnPos,
        PosOutsideBoard,
        ImposterKing,
        IncorrectKingPos
    }

    public static class ChessInspector {
        public static Result<Option<Fig>[,], CheckError> GetFilteredBoard(FigLoc figLoc) {
            if (figLoc.board == null) {
                return Result<Option<Fig>[,], CheckError>.Err(CheckError.BoardIsNull);
            }

            var size = new Vector2Int(figLoc.board.GetLength(0), figLoc.board.GetLength(1));
            if (!BoardEngine.IsOnBoard(figLoc.pos, size)) {
                return Result<Option<Fig>[,], CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return Result<Option<Fig>[,], CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var boardClone = BoardEngine.CopyBoard(figLoc.board);
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
            FigLoc figLoc
        ) {
            if (figLoc.board == null) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.BoardIsNull);
            }

            var size = new Vector2Int(figLoc.board.GetLength(0), figLoc.board.GetLength(1));
            if (!BoardEngine.IsOnBoard(figLoc.pos, size)) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var kingOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (kingOpt.IsNone()) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var king = kingOpt.Peel();
            if (king.type != FigureType.King) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.ImposterKing);
            }

            var checkInfos = new List<CheckInfo>();
            var allMovement = new List<Movement>();
            allMovement.AddRange(Movements.queenMovement);
            allMovement.AddRange(Movements.knightMovement);

            var boardCloneRes = GetFilteredBoard(figLoc);
            if (boardCloneRes.IsErr()) {
                return Result<List<CheckInfo>, CheckError>.Err(boardCloneRes.AsErr());
            }

            var boardClone = GetFilteredBoard(figLoc).AsOk();

            foreach (var movement in allMovement) {
                if (movement.linear.HasValue) {
                    var dir = movement.linear.Value.dir;
                    var length = BoardEngine.GetLinearLength(figLoc.pos, dir, boardClone);
                    var linearPath = BoardEngine.GetLinearPath(figLoc.pos, dir, length, boardClone);

                    if (linearPath.Count != 0) {
                        var figPos = linearPath[linearPath.Count - 1];
                        var figOpt = boardClone[figPos.x, figPos.y];
                        if (figOpt.IsSome()) {
                            var fig = figOpt.Peel();
                            if (fig.type == FigureType.Pawn) {
                                if (linearPath.Count > 1) {
                                    continue;
                                }
                            }

                            var figMovements = Movements.movements[fig.type];
                            foreach (var figMovement in figMovements) {
                                if (!figMovement.linear.HasValue) {
                                    continue;
                                }

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

                if (movement.square.HasValue) {
                    var square = BoardEngine.GetSquarePath(figLoc.pos, 5);
                    var bindableKnightPath = BoardEngine.RemoveSquareParts(square, 0, 1);

                    var knightPath = new List<Vector2Int>();
                    if (knightPath != null) {
                        foreach (var cell in bindableKnightPath) {
                            if (cell != null) {
                                knightPath.Add(cell.value);
                            }
                        }
                    }

                    if (knightPath.Count != 0) {
                        foreach (var cell in knightPath) {
                            if (!BoardEngine.IsOnBoard(cell, size)){
                                continue;
                            }

                            var figOpt = figLoc.board[cell.x, cell.y];
                            if (figOpt.IsNone()) {
                                continue;
                            }

                            var fig = figOpt.Peel();

                            if (fig.type == FigureType.Knight && fig.white != king.white) {
                                var knightMovement = new Movement{
                                    square = new SquareMovement {
                                        side = 5
                                    }
                                };

                                checkInfos.Add(
                                    new CheckInfo {
                                        attack = new FixedMovement {
                                            start = cell,
                                            movement = knightMovement
                                        },
                                        path = knightPath
                                    }
                                );
                            }
                        }
                    }
                }
            }

            return Result<List<CheckInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<CheckInfo>, CheckError> GetCheckInfos(
            FigLoc figLoc
        ) {
            if (figLoc.board == null) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.BoardIsNull);
            }

            var size = new Vector2Int(figLoc.board.GetLength(0), figLoc.board.GetLength(1));
            if (!BoardEngine.IsOnBoard(figLoc.pos, size)) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var kingOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (kingOpt.IsNone()) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var king = kingOpt.Peel();
            if (king.type != FigureType.King) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.ImposterKing);
            }

            var boardClone = GetFilteredBoard(figLoc).AsOk();
            var checkInfos = GetPotentialCheckInfos(figLoc).AsOk();
            var newCheckInfos = new List<CheckInfo>();

            foreach (var checkInfo in checkInfos) {
                if (checkInfo.attack.movement.linear.HasValue) {
                    var checkPath = checkInfo.path;
                    var blockCounter = 0;
                    var defPos = new Vector2Int();

                    foreach (var cell in checkPath) {
                        var figOpt = figLoc.board[cell.x, cell.y];
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

                if (checkInfo.attack.movement.square.HasValue) {
                    newCheckInfos.Add(checkInfo);
                }

            }

            return Result<List<CheckInfo>, CheckError>.Ok(newCheckInfos);
        }

        public static Result<List<MoveInfo>, CheckError> GetPossibleMoves(
            FigLoc figLoc,
            MoveInfo lastMove
        ) {
            if (figLoc.board == null) {
                return Result<List<MoveInfo>, CheckError>.Err(CheckError.BoardIsNull);
            }

            var size = new Vector2Int(figLoc.board.GetLength(0), figLoc.board.GetLength(1));
            if (!BoardEngine.IsOnBoard(figLoc.pos, size)) {
                return Result<List<MoveInfo>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return Result<List<MoveInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var fig = figOpt.Peel();

            var movement = new List<Movement>();
            var white = fig.white;

            var kingsPosRes = GetKingsPos(figLoc.board);
            if (kingsPosRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(kingsPosRes.AsErr());
            }
            var kingsPos = kingsPosRes.AsOk();
            var kingPos = kingsPos.black;
            var check = false;
            if (white) {
                kingPos = kingsPos.white;
            }

            var movements = Movements.movements[figLoc.board[figLoc.pos.x, figLoc.pos.y].Peel().type];

            var checkInfosRes = GetCheckInfos(new FigLoc { pos = kingPos, board = figLoc.board});
            if (checkInfosRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(checkInfosRes.AsErr());
            }

            var checkInfos = GetCheckInfos(new FigLoc {pos = kingPos, board = figLoc.board}).AsOk();
            foreach (var checkInfo in checkInfos) {
                if (checkInfo.defPos == null) {
                    check = true;
                }
            }

            if (fig.type == FigureType.King) {
                var kingPossMoves = new List<MoveInfo>();

                var kingMovesRes = MoveEngine.GetMoves(figLoc.pos, movements, lastMove, figLoc.board);
                // if (kingMovesRes.IsErr()) {
                //     return Result<List<MoveInfo>, CheckError>.Err(kingMovesRes.AsErr());
                // }
                var kingMoves = MoveEngine.GetMoves(figLoc.pos, movements, lastMove, figLoc.board).AsOk();
                foreach (var move in kingMoves) {
                    var moveTo = move.move.first.Value.to;
                    if (!MoveEngine.IsUnderAttackPos(moveTo, fig.white, lastMove, figLoc.board).AsOk()){
                        kingPossMoves.Add(move);
                    }
                }
                return Result<List<MoveInfo>, CheckError>.Ok(kingPossMoves);
            }

            foreach (var checkInfo in checkInfos) {
                var attackPos = checkInfo.attack.start;
                if (checkInfo.defPos == null) {
                    if (checkInfo.attack.movement.linear.HasValue) {
                        var moves = MoveEngine.GetMoves(figLoc.pos, movements, lastMove, figLoc.board).AsOk();
                        var newMoves = new List<MoveInfo>();
                        foreach (var move in moves) {
                            var firstTo = move.move.first.Value.to;
                            var path = checkInfo.path;

                            foreach (var cell in path) {
                                if (cell == firstTo) {
                                    newMoves.Add(move);
                                }
                            }
                        }
                        return Result<List<MoveInfo>, CheckError>.Ok(newMoves);
                    }

                    if (checkInfo.attack.movement.square.HasValue) {
                        var moves = MoveEngine.GetMoves(figLoc.pos, movements, lastMove, figLoc.board).AsOk();
                        var newMoves = new List<MoveInfo>();
                        foreach (var move in moves) {
                            var firstTo = move.move.first.Value.to;
                            if (firstTo == attackPos) {
                                newMoves.Add(move);
                            }
                        }

                        return Result<List<MoveInfo>, CheckError>.Ok(newMoves);
                    }
                }

                if (checkInfo.defPos == figLoc.pos && !check) {
                    var defPos = checkInfo.defPos.Value;
                    if (figLoc.board[defPos.x, defPos.y].Peel().type == FigureType.Knight) {
                        return Result<List<MoveInfo>, CheckError>.Ok(null);
                    }

                    if (checkInfo.attack.movement.linear.HasValue) {
                        var linear = checkInfo.attack.movement.linear.Value;
                        movement.Add(new Movement {
                            linear = new LinearMovement { dir = linear.dir }
                        });
                    }

                    var moves = MoveEngine.GetMoves(figLoc.pos, movement, lastMove, figLoc.board).AsOk();
                    return Result<List<MoveInfo>, CheckError>.Ok(moves);
                }

            }
            var defaultMoves = MoveEngine.GetMoves(figLoc.pos, movements, lastMove, figLoc.board).AsOk();
            return Result<List<MoveInfo>, CheckError>.Ok(defaultMoves);
        }

        public static Result<KingsPos, CheckError> GetKingsPos(Option<Fig>[,] board) {
            if (board == null) {
                return Result<KingsPos, CheckError>.Err(CheckError.BoardIsNull);
            }

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

            return Result<KingsPos, CheckError>.Ok(kingsPos);
        }
    }
}