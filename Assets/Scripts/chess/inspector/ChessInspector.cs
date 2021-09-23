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

                if (knightPath.Count == 0) {
                    continue;
                }

                foreach (var cell in knightPath) {
                    if (!BoardEngine.IsOnBoard(cell, kingLoc.board)){
                        continue;
                    }

                    var figOpt = kingLoc.board[cell.x, cell.y];
                    if (figOpt.IsNone()) {
                        continue;
                    }

                    if (figOpt.Peel().type == FigureType.Knight) {
                        var knightMovement = Movement.Square(SquareMovement.Mk(5));

                        checkInfos.Add(
                            new CheckInfo {
                                attack = FixedMovement.Mk(cell, knightMovement),
                                path = knightPath
                            }
                        );
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

                if (linearPath.Count == 0) {
                    continue;
                }

                var figPos = linearPath[linearPath.Count - 1];
                var figOpt = boardClone[figPos.x, figPos.y];
                if (figOpt.IsNone()) {
                    continue;
                }

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
                    if (dir != figDir) {
                        continue;
                    }

                    var newMovement = Movement.Linear(LinearMovement.Mk(dir));

                    checkInfos.Add(
                        new CheckInfo {
                            attack = FixedMovement.Mk(figPos, newMovement),
                            path = linearPath
                        }
                    );
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

                    var defPositionsRes = GetDefPositions(kingLoc, checkPath);
                    if (defPositionsRes.IsErr()) {
                        return Result<List<CheckInfo>, CheckError>.Err(defPositionsRes.AsErr());
                    }

                    var defPositions = defPositionsRes.AsOk();
                    if (defPositions.Count == 1) {
                        var newCheckInfo = checkInfo;
                        newCheckInfo.defPos = defPositions[0];
                        checkInfos.Add(newCheckInfo);
                    }

                    if (defPositions.Count == 0) {
                        checkInfos.Add(checkInfo);
                    }

                } else if (checkInfo.attack.movement.square.HasValue) {
                    checkInfos.Add(checkInfo);
                }
            }

            return Result<List<CheckInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<Vector2Int>, CheckError> GetDefPositions(
            FigLoc kingLoc,
            List<Vector2Int> checkPath
        ) {
            if (kingLoc.board == null) {
                return Result<List<Vector2Int>, CheckError>.Err(CheckError.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(kingLoc.pos, kingLoc.board)) {
                return Result<List<Vector2Int>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var kingOpt = kingLoc.board[kingLoc.pos.x, kingLoc.pos.y];
            if (kingOpt.IsNone()) {
                return Result<List<Vector2Int>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var king = kingOpt.Peel();
            if (king.type != FigureType.King) {
                return Result<List<Vector2Int>, CheckError>.Err(CheckError.ImposterKing);
            }

            var blockCounter = 0;
            var defPositions = new List<Vector2Int>();
            foreach (var cell in checkPath) {
                var figOpt = kingLoc.board[cell.x, cell.y];
                if (figOpt.IsNone()) {
                    continue;
                }

                var fig = figOpt.Peel();
                if (fig.color == king.color) {
                    defPositions.Add(cell);
                    blockCounter++;
                }
            }

            return Result<List<Vector2Int>, CheckError>.Ok(defPositions);
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

            if (color == FigColor.White) {
                kingPos = kingsPos.white;
            }

            var movements = Movements.movements[fig.type];
            var possibleMoves = new List<MoveInfo>();
            var checkInfosRes = GetCheckInfos(FigLoc.Mk(kingPos, figLoc.board));
            if (checkInfosRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(checkInfosRes.AsErr());
            }

            var checkInfos = GetCheckInfos(FigLoc.Mk(kingPos, figLoc.board)).AsOk();

            if (fig.type == FigureType.King) {
                var kingPossMoves = GetKingPossibleMoves(figLoc, lastMove);
                if (kingPossMoves.IsErr()) {
                    return Result<List<MoveInfo>, CheckError>.Err(kingPossMoves.AsErr());
                }

                return Result<List<MoveInfo>, CheckError>.Ok(kingPossMoves.AsOk());
            }

            foreach (var checkInfo in checkInfos) {
                if (checkInfo.defPos == figLoc.pos && !IsCheck(checkInfos)) {
                    var defPos = checkInfo.defPos.Value;
                    if (figLoc.board[defPos.x, defPos.y].Peel().type == FigureType.Knight) {
                        return Result<List<MoveInfo>, CheckError>.Ok(null);
                    }

                    if (checkInfo.attack.movement.linear.HasValue) {
                        var linear = checkInfo.attack.movement.linear.Value;
                        movement.Add(new Movement {
                            linear = LinearMovement.Mk(linear.dir)
                        });
                    }

                    var moves = MoveEngine.GetMoves(figLoc, movement, lastMove).AsOk();
                    return Result<List<MoveInfo>, CheckError>.Ok(moves);
                }

                if (checkInfo.attack.movement.linear.HasValue) {
                    var possMoves = GetLinearPossibleMoves(figLoc, checkInfo, lastMove);
                    if (possMoves.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(possMoves.AsErr());
                    }

                    possibleMoves.AddRange(possMoves.AsOk());
                } else if (checkInfo.attack.movement.square.HasValue) {
                    var possMoves = GetSquarePossibleMoves(figLoc, checkInfos, lastMove);
                    if (possMoves.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(possMoves.AsErr());
                    }

                    possibleMoves.AddRange(possMoves.AsOk());;
                }

                if (possibleMoves != null) {
                    return Result<List<MoveInfo>, CheckError>.Ok(possibleMoves);
                }
            }

            var defaultMoves = MoveEngine.GetMoves(figLoc, movements, lastMove).AsOk();
            return Result<List<MoveInfo>, CheckError>.Ok(defaultMoves);
        }

        public static Result<List<MoveInfo>, CheckError> GetSquarePossibleMoves(
            FigLoc figLoc,
            List<CheckInfo> checkInfos,
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
            var movements = Movements.movements[fig.type];
            var possMoves = new List<MoveInfo>();
            foreach (var checkInfo in checkInfos) {
                var attackPos = checkInfo.attack.start;
                if (!checkInfo.defPos.HasValue) {
                    if (checkInfo.attack.movement.square.HasValue) {
                        var moves = MoveEngine.GetMoves(figLoc, movements, lastMove).AsOk();
                        foreach (var move in moves) {
                            var firstTo = move.move.first.Value.to;
                            if (firstTo == attackPos) {
                                possMoves.Add(move);
                            }
                        }

                    }
                }
            }
            return Result<List<MoveInfo>, CheckError>.Ok(possMoves);
        }

        public static Result<List<MoveInfo>, CheckError> GetLinearPossibleMoves(
            FigLoc figLoc,
            CheckInfo checkInfo,
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
            var movements = Movements.movements[fig.type];
            var possMoves = new List<MoveInfo>();
            if (!checkInfo.defPos.HasValue) {
                if (checkInfo.attack.movement.linear.HasValue) {
                    var moves = MoveEngine.GetMoves(figLoc, movements, lastMove).AsOk();
                    foreach (var move in moves) {
                        var firstTo = move.move.first.Value.to;
                        var path = checkInfo.path;

                        foreach (var cell in path) {
                            if (cell == firstTo) {
                                possMoves.Add(move);
                            }
                        }
                    }
                }
            }

            return Result<List<MoveInfo>, CheckError>.Ok(possMoves);
        }

        public static Result<List<MoveInfo>, CheckError> GetKingPossibleMoves(
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
            var color = fig.color;
            var movements = Movements.movements[fig.type];
            var kingPossMoves = new List<MoveInfo>();

            if (fig.type == FigureType.King) {
                var kingMovesRes = MoveEngine.GetMoves(figLoc, movements, lastMove);
                var kingMoves = kingMovesRes.AsOk();
                foreach (var move in kingMoves) {
                    var moveTo = move.move.first.Value.to;
                    var moveToLoc = FigLoc.Mk(moveTo, figLoc.board);
                    if (!MoveEngine.IsUnderAttackPos(moveToLoc, color, lastMove).AsOk()){
                        kingPossMoves.Add(move);
                    }
                }
            }

            return Result<List<MoveInfo>, CheckError>.Ok(kingPossMoves);
        }

        public static bool IsCheck(List<CheckInfo> checkInfos) {
            foreach (var checkInfo in checkInfos) {
                if (checkInfo.defPos == null) {
                    return true;
                }
            }

            return false;
        }

        public static Result<KingsPos, CheckError> FindKingsPos(Option<Fig>[,] board) {
            if (board == null) {
                return Result<KingsPos, CheckError>.Err(CheckError.BoardIsNull);
            }

            var kingsPos = new KingsPos();
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {

                    var figOpt = board[i, j];
                    if (figOpt.IsNone()) {
                        continue;
                    }

                    var fig = figOpt.Peel();
                    if (fig.type != FigureType.King) {
                        continue;
                    }

                    if (fig.color == FigColor.White) {
                        kingsPos.white = new Vector2Int(i, j);
                    } else {
                        kingsPos.black = new Vector2Int(i, j);
                    }
                }
            }

            return Result<KingsPos, CheckError>.Ok(kingsPos);
        }
    }
}