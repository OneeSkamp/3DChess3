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
        ImposterKing,
    }

    public static class ChessInspector {
        public static Result<Option<Fig>[,], CheckError> GetBoardWithoutColor(
            Option<Fig>[,] board,
            FigColor color
        ) {
            if (board == null) {
                return Result<Option<Fig>[,], CheckError>.Err(CheckError.BoardIsNull);
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));

            var boardClone = BoardEngine.CopyBoard(board);

            for (int i = 0; i < boardClone.GetLength(0); i++) {
                for (int j = 0; j < boardClone.GetLength(1); j++) {
                    if (boardClone[i, j].IsSome()) {
                        var currentFig = boardClone[i, j].Peel();
                        if (currentFig.color == color) {
                            boardClone[i, j] = Option<Fig>.None();
                        }
                    }
                }
            }

            return Result<Option<Fig>[,], CheckError>.Ok(boardClone);
        }

        public static Result<List<CheckInfo>, CheckError> GetPotentialCheckInfos(
            FigLoc kingLoc
        ) {
            if (kingLoc.board == null) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(kingLoc.pos, kingLoc.board)) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var kingOpt = kingLoc.board[kingLoc.pos.x, kingLoc.pos.y];
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

            var boardCloneRes = GetBoardWithoutColor(kingLoc.board, king.color);
            if (boardCloneRes.IsErr()) {
                return Result<List<CheckInfo>, CheckError>.Err(boardCloneRes.AsErr());
            }

            var boardClone = boardCloneRes.AsOk();
            boardClone[kingLoc.pos.x, kingLoc.pos.y] = kingOpt;

            var linearCheckInfosRes = GetLinearPotentialCheckInfos(
                allMovement,
                kingLoc,
                boardClone
            );
            if (linearCheckInfosRes.IsErr()) {
                return Result<List<CheckInfo>, CheckError>.Err(linearCheckInfosRes.AsErr());
            }
            checkInfos.AddRange(linearCheckInfosRes.AsOk());

            var squareCheckInfosRes = GetSquarePotentialCheckInfos(
                allMovement,
                kingLoc,
                boardClone
            );
            if (squareCheckInfosRes.IsErr()) {
                return Result<List<CheckInfo>, CheckError>.Err(squareCheckInfosRes.AsErr());
            }
            checkInfos.AddRange(squareCheckInfosRes.AsOk());

            return Result<List<CheckInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<CheckInfo>, CheckError> GetSquarePotentialCheckInfos(
            List<Movement> movements,
            FigLoc kingLoc,
            Option<Fig>[,] boardClone
        ) {
            var kingOpt = kingLoc.board[kingLoc.pos.x, kingLoc.pos.y];
            if (kingOpt.IsNone()) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var king = kingOpt.Peel();
            if (king.type != FigureType.King) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.ImposterKing);
            }

            var checkInfos = new List<CheckInfo>();
            foreach (var movement in movements) {
                if (movement.linear.HasValue) {
                    continue;
                }

                var square = BoardEngine.GetSquarePath(kingLoc.pos, 5);
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
                        if (!BoardEngine.IsOnBoard(cell, kingLoc.board)){
                            continue;
                        }

                        var figOpt = kingLoc.board[cell.x, cell.y];
                        if (figOpt.IsNone()) {
                            continue;
                        }

                        var fig = figOpt.Peel();

                        if (fig.type == FigureType.Knight && fig.color != king.color) {
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

            return Result<List<CheckInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<CheckInfo>, CheckError> GetLinearPotentialCheckInfos(
            List<Movement> movements,
            FigLoc figLoc,
            Option<Fig>[,] boardClone
        ) {
            var checkInfos = new List<CheckInfo>();
            foreach (var movement in movements) {
                if (movement.square.HasValue) {
                    continue;
                }

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

            return Result<List<CheckInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<CheckInfo>, CheckError> GetCheckInfos(
            FigLoc kingLoc
        ) {
            if (kingLoc.board == null) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(kingLoc.pos, kingLoc.board)) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var kingOpt = kingLoc.board[kingLoc.pos.x, kingLoc.pos.y];
            if (kingOpt.IsNone()) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var king = kingOpt.Peel();
            if (king.type != FigureType.King) {
                return Result<List<CheckInfo>, CheckError>.Err(CheckError.ImposterKing);
            }

            var potentialCheckInfosRes = GetPotentialCheckInfos(kingLoc);
            if (potentialCheckInfosRes.IsErr()) {
                return Result<List<CheckInfo>, CheckError>.Err(potentialCheckInfosRes.AsErr());
            }
            var potentialCheckInfos = GetPotentialCheckInfos(kingLoc).AsOk();
            var checkInfos = new List<CheckInfo>();

            foreach (var checkInfo in potentialCheckInfos) {
                if (checkInfo.attack.movement.linear.HasValue) {
                    var checkPath = checkInfo.path;
                    var blockCounter = 0;
                    var defPos = new Vector2Int();

                    foreach (var cell in checkPath) {
                        var figOpt = kingLoc.board[cell.x, cell.y];
                        if (figOpt.IsNone()) {
                            continue;
                        }

                        var fig = figOpt.Peel();
                        if (fig.color == king.color) {
                            defPos = cell;
                            blockCounter++;
                        }
                    }

                    if (blockCounter == 1) {
                        var newCheckInfo = checkInfo;
                        newCheckInfo.defPos = defPos;
                        checkInfos.Add(newCheckInfo);
                    }

                    if (blockCounter == 0) {
                        checkInfos.Add(checkInfo);
                    }
                }

                if (checkInfo.attack.movement.square.HasValue) {
                    checkInfos.Add(checkInfo);
                }

            }

            return Result<List<CheckInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<MoveInfo>, CheckError> GetPossibleMoves(
            FigLoc figLoc,
            MoveInfo lastMove
        ) {
            if (figLoc.board == null) {
                return Result<List<MoveInfo>, CheckError>.Err(CheckError.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(figLoc.pos, figLoc.board)) {
                return Result<List<MoveInfo>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return Result<List<MoveInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var fig = figOpt.Peel();

            var movement = new List<Movement>();
            var color = fig.color;

            var kingsPosRes = FindKingsPos(figLoc.board);
            if (kingsPosRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(kingsPosRes.AsErr());
            }
            var kingsPos = kingsPosRes.AsOk();
            var kingPos = kingsPos.black;
            var check = false;
            if (color == FigColor.White) {
                kingPos = kingsPos.white;
            } else {
                kingPos = kingsPos.black;
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

                var kingMovesRes = MoveEngine.GetMoves(figLoc, movements, lastMove);

                var kingMoves = MoveEngine.GetMoves(figLoc, movements, lastMove).AsOk();
                foreach (var move in kingMoves) {
                    var moveTo = move.move.first.Value.to;
                    var moveToLoc = FigLoc.Mk(moveTo, figLoc.board);
                    if (!MoveEngine.IsUnderAttackPos(moveToLoc, FigColor.Black, lastMove).AsOk()){
                        kingPossMoves.Add(move);
                    }
                }
                return Result<List<MoveInfo>, CheckError>.Ok(kingPossMoves);
            }

            foreach (var checkInfo in checkInfos) {
                var attackPos = checkInfo.attack.start;
                if (checkInfo.defPos == null) {
                    if (checkInfo.attack.movement.linear.HasValue) {
                        var moves = MoveEngine.GetMoves(figLoc, movements, lastMove).AsOk();
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
                        var moves = MoveEngine.GetMoves(figLoc, movements, lastMove).AsOk();
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

                    var moves = MoveEngine.GetMoves(figLoc, movement, lastMove).AsOk();
                    return Result<List<MoveInfo>, CheckError>.Ok(moves);
                }

            }
            var defaultMoves = MoveEngine.GetMoves(figLoc, movements, lastMove).AsOk();
            return Result<List<MoveInfo>, CheckError>.Ok(defaultMoves);
        }

        public static Result<KingsPos, CheckError> FindKingsPos(Option<Fig>[,] board) {
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
                            if (fig.color == FigColor.White) {
                                kingsPos.white = new Vector2Int(i, j);
                            }
                            if (fig.color == FigColor.Black) {
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