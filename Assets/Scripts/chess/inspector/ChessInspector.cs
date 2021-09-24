using UnityEngine;
using System.Collections.Generic;
using option;
using chess;
using move;
using board;
using movements;

namespace inspector {
    public struct CheckInfo {
        public LimitedMovement attack;
        public Vector2Int? defPos;
        public List<Vector2Int> path;
    }

    public struct CoveredInfo {
        public LimitedMovement attack;
        public Vector2Int defPos;
    }

    public struct AttackInfo {
        public CheckInfo? checkInfo;
        public CoveredInfo? coveredInfo;
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
        CantInterpMoveErr
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
                                attack = new LimitedMovement {
                                    fixedMovement = FixedMovement.Mk(cell, knightMovement)
                                },
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

                var fixedMovement = FixedMovement.Mk(figLoc.pos, movement);
                var limMovement = BoardEngine.GetLimitedMovement(fixedMovement, boardClone);
                var figPos = BoardEngine.GetLastOnPathPos(limMovement, boardClone);

                if (!figPos.HasValue) {
                    continue;
                }

                var figOpt = boardClone[figPos.Value.x, figPos.Value.y];
                if (figOpt.IsNone()) {
                    continue;
                }
                var fig = figOpt.Peel();
                if (fig.type == FigureType.Pawn) {
                    if (limMovement.length > 1) {
                        continue;
                    }
                }

                var figMovements = Movements.movements[fig.type];
                foreach (var figMovement in figMovements) {
                    if (!figMovement.linear.HasValue) {
                        continue;
                    }

                    var figDir = figMovement.linear.Value.dir;
                    var dir = fixedMovement.movement.linear.Value.dir;
                    if (fixedMovement.movement.linear.Value.dir != figDir) {
                        continue;
                    }

                    var newMovement = Movement.Linear(LinearMovement.Mk(dir));

                    checkInfos.Add(
                        new CheckInfo {
                            attack = limMovement,
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

            foreach (var potCheckInfo in potentialCheckInfos) {
                var checkInfoRes = GetRealCheckInfo(kingLoc, potCheckInfo);
                if (checkInfoRes.IsErr()) {
                    return Result<List<CheckInfo>, CheckError>.Err(checkInfoRes.AsErr());
                }

                var checkInfo = checkInfoRes.AsOk();
                if (!checkInfo.HasValue) {
                    continue;
                }

                checkInfos.Add(checkInfo.Value);
            }

            return Result<List<CheckInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<AttackInfo>, CheckError> GetAttackInfos(
            FigLoc kingLoc
        ) {
            if (kingLoc.board == null) {
                return Result<List<AttackInfo>, CheckError>.Err(CheckError.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(kingLoc.pos, kingLoc.board)) {
                return Result<List<AttackInfo>, CheckError>.Err(CheckError.PosOutsideBoard);
            }

            var kingOpt = kingLoc.board[kingLoc.pos.x, kingLoc.pos.y];
            if (kingOpt.IsNone()) {
                return Result<List<AttackInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var king = kingOpt.Peel();
            if (king.type != FigureType.King) {
                return Result<List<AttackInfo>, CheckError>.Err(CheckError.ImposterKing);
            }

            var potentialCheckInfosRes = GetPotentialCheckInfos(kingLoc);
            if (potentialCheckInfosRes.IsErr()) {
                return Result<List<AttackInfo>, CheckError>.Err(potentialCheckInfosRes.AsErr());
            }
            var potentialCheckInfos = GetPotentialCheckInfos(kingLoc).AsOk();
            var attackInfos = new List<AttackInfo>();

            foreach (var potCheckInfo in potentialCheckInfos) {
                var attackInfo = new AttackInfo();
                var checkInfoRes = GetRealCheckInfo(kingLoc, potCheckInfo);
                if (checkInfoRes.IsErr()) {
                    return Result<List<AttackInfo>, CheckError>.Err(checkInfoRes.AsErr());
                }

                var checkInfo = checkInfoRes.AsOk();
                if (checkInfo.HasValue) {
                    attackInfo = new AttackInfo {
                        checkInfo = checkInfo
                    };
                }

                var coveredInfoRes = GetCoveredCheckInfo(kingLoc, potCheckInfo);
                if (checkInfoRes.IsErr()) {
                    return Result<List<AttackInfo>, CheckError>.Err(coveredInfoRes.AsErr());
                }

                var coveredInfo = coveredInfoRes.AsOk();
                if (coveredInfo.HasValue) {
                    attackInfo = new AttackInfo {
                        coveredInfo = coveredInfo
                    };
                }

                attackInfos.Add(attackInfo);
            }

            return Result<List<AttackInfo>, CheckError>.Ok(attackInfos);
        }

        public static Result<CoveredInfo?, CheckError> GetCoveredCheckInfo(
            FigLoc kingLoc,
            CheckInfo potentialCheckInfo
        ) {
            var linear = potentialCheckInfo.attack.fixedMovement.movement.linear;
            if (linear.HasValue) {
                var dir = linear.Value.dir;
                var length = potentialCheckInfo.attack.length;
                var checkPath = BoardEngine.GetLinearPath(
                    kingLoc.pos,
                    dir,
                    length,
                    kingLoc.board
                );

                var defPositionsRes = GetDefPositions(kingLoc, checkPath);
                if (defPositionsRes.IsErr()) {
                    return Result<CoveredInfo?, CheckError>.Err(defPositionsRes.AsErr());
                }

                var defPositions = defPositionsRes.AsOk();
                if (defPositions.Count == 1) {
                    var coveredCheckInfo = new CoveredInfo {
                        attack = potentialCheckInfo.attack,
                        defPos = defPositions[0]
                    };

                    return Result<CoveredInfo?, CheckError>.Ok(coveredCheckInfo);
                }

                return Result<CoveredInfo?, CheckError>.Ok(null);



            } else {
                return Result<CoveredInfo?, CheckError>.Ok(null);
            }
        }

        public static Result<CheckInfo?, CheckError> GetRealCheckInfo(
            FigLoc kingLoc,
            CheckInfo potentialCheckInfo
        ) {
            var linear = potentialCheckInfo.attack.fixedMovement.movement.linear;
            if (linear.HasValue) {
                var dir = linear.Value.dir;
                var length = potentialCheckInfo.attack.length;
                var checkPath = BoardEngine.GetLinearPath(
                    kingLoc.pos,
                    dir,
                    length,
                    kingLoc.board
                );

                var defPositionsRes = GetDefPositions(kingLoc, checkPath);
                if (defPositionsRes.IsErr()) {
                    return Result<CheckInfo?, CheckError>.Err(defPositionsRes.AsErr());
                }

                var defPositions = defPositionsRes.AsOk();
                if (defPositions.Count == 1) {
                    var newCheckInfo = potentialCheckInfo;
                    newCheckInfo.defPos = defPositions[0];
                    return Result<CheckInfo?, CheckError>.Ok(newCheckInfo);
                }

                if (defPositions.Count == 0) {
                    var checkInfo = new CheckInfo {
                        attack = potentialCheckInfo.attack,
                        path = checkPath
                    };

                    return Result<CheckInfo?, CheckError>.Ok(checkInfo);
                }

                return Result<CheckInfo?, CheckError>.Ok(null);

            } else {
                return Result<CheckInfo?, CheckError>.Ok(potentialCheckInfo);
            }
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
            var checkInfosRes = GetAttackInfos(FigLoc.Mk(kingPos, figLoc.board));
            if (checkInfosRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(checkInfosRes.AsErr());
            }

            var attackInfos = GetAttackInfos(FigLoc.Mk(kingPos, figLoc.board)).AsOk();

            if (fig.type == FigureType.King) {
                var kingPossMoves = GetKingPossibleMoves(figLoc, lastMove);
                if (kingPossMoves.IsErr()) {
                    return Result<List<MoveInfo>, CheckError>.Err(kingPossMoves.AsErr());
                }

                return Result<List<MoveInfo>, CheckError>.Ok(kingPossMoves.AsOk());
            }

            foreach (var attackInfo in attackInfos) {
                if (attackInfo.coveredInfo.HasValue) {
                    var coveredInfo = attackInfo.coveredInfo.Value;
                    var defPos = attackInfo.coveredInfo.Value.defPos;
                    if (defPos == figLoc.pos && !IsCheck(attackInfos)) {
                        if (figLoc.board[defPos.x, defPos.y].Peel().type == FigureType.Knight) {
                            return Result<List<MoveInfo>, CheckError>.Ok(null);
                        }

                        if (coveredInfo.attack.fixedMovement.movement.linear.HasValue) {
                            var linear = coveredInfo.attack.fixedMovement.movement.linear.Value;
                            movement.Add(new Movement {
                                linear = LinearMovement.Mk(linear.dir)
                            });
                        }

                        var movesRes = MoveEngine.GetMoves(figLoc, movement, lastMove);
                        if (movesRes.IsErr()) {
                            return Result<List<MoveInfo>, CheckError>.Err(
                                InterpMoveEngineErr(movesRes.AsErr())
                            );
                        }

                        var moves = MoveEngine.GetMoves(figLoc, movement, lastMove).AsOk();

                        return Result<List<MoveInfo>, CheckError>.Ok(moves);
                    }
                }

                if (attackInfo.coveredInfo.HasValue) {
                    continue;
                }

                var checkInfo = attackInfo.checkInfo.Value;
                if (checkInfo.attack.fixedMovement.movement.linear.HasValue) {
                    var possMoves = GetLinearPossibleMoves(figLoc, attackInfo, lastMove);
                    if (possMoves.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(possMoves.AsErr());
                    }

                    possibleMoves.AddRange(possMoves.AsOk());
                } else if (checkInfo.attack.fixedMovement.movement.square.HasValue) {
                    var possMoves = GetSquarePossibleMoves(figLoc, attackInfos, lastMove);
                    if (possMoves.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(possMoves.AsErr());
                    }

                    possibleMoves.AddRange(possMoves.AsOk());;
                }

                if (possibleMoves != null) {
                    return Result<List<MoveInfo>, CheckError>.Ok(possibleMoves);
                }
            }
            var defaultMovesRes = MoveEngine.GetMoves(figLoc, movements, lastMove);
            if (defaultMovesRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(
                    InterpMoveEngineErr(defaultMovesRes.AsErr())
                );
            }

            var defaultMoves = defaultMovesRes.AsOk();
            return Result<List<MoveInfo>, CheckError>.Ok(defaultMoves);
        }

        public static Result<List<MoveInfo>, CheckError> GetSquarePossibleMoves(
            FigLoc figLoc,
            List<AttackInfo> attackInfos,
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
            foreach (var attackInfo in attackInfos) {
                if (attackInfo.checkInfo.HasValue) {
                    var checkInfo = attackInfo.checkInfo.Value;
                    var attackPos = checkInfo.attack.fixedMovement.start;
                    if (checkInfo.attack.fixedMovement.movement.square.HasValue) {
                        var movesRes = MoveEngine.GetMoves(figLoc, movements, lastMove);
                        if (movesRes.IsErr()) {
                            return Result<List<MoveInfo>, CheckError>.Err(
                                InterpMoveEngineErr(movesRes.AsErr())
                            );
                        }

                        var moves = movesRes.AsOk();
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
            AttackInfo attackInfo,
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
            if (attackInfo.checkInfo.HasValue) {
            var checkInfo = attackInfo.checkInfo.Value;
                if (checkInfo.attack.fixedMovement.movement.linear.HasValue) {
                    var movesRes = MoveEngine.GetMoves(figLoc, movements, lastMove);
                    if (movesRes.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(
                            InterpMoveEngineErr(movesRes.AsErr())
                        );
                    }
                    var moves = movesRes.AsOk();
                    foreach (var move in moves) {
                        var firstTo = move.move.first.Value.to;
                        var start = checkInfo.attack.fixedMovement.start;
                        var dir = checkInfo.attack.fixedMovement.movement.linear.Value.dir;
                        var length = checkInfo.attack.length;
                        var path = BoardEngine.GetLinearPath(start, dir, length, figLoc.board);
                        if (path == null) {
                            continue;
                        }
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
                if (kingMovesRes.IsErr()) {
                    return Result<List<MoveInfo>, CheckError>.Err(
                        InterpMoveEngineErr(kingMovesRes.AsErr())
                    );
                }
                var kingMoves = kingMovesRes.AsOk();
                foreach (var move in kingMoves) {
                    var moveTo = move.move.first.Value.to;
                    var moveToLoc = FigLoc.Mk(moveTo, figLoc.board);
                    var isUnderAttackRes = MoveEngine.IsUnderAttackPos(moveToLoc, color, lastMove);
                    if (isUnderAttackRes.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(
                            InterpMoveEngineErr(isUnderAttackRes.AsErr())
                        );
                    }

                    if (!isUnderAttackRes.AsOk()){
                        kingPossMoves.Add(move);
                    }
                }
            }

            return Result<List<MoveInfo>, CheckError>.Ok(kingPossMoves);
        }

        public static bool IsCheck(List<AttackInfo> attackInfos) {
            foreach (var attackInfo in attackInfos) {
                if (attackInfo.checkInfo.HasValue) {
                    return true;
                }
            }

            return false;
        }

        public static CheckError InterpMoveEngineErr(MoveError err) {
            switch (err) {
                case MoveError.BoardIsNull:
                    return CheckError.BoardIsNull;
                case MoveError.PosOutsideBoard:
                    return CheckError.PosOutsideBoard;
                case MoveError.NoFigureOnPos:
                    return CheckError.PosOutsideBoard;
                case MoveError.FigureIsNotKing:
                    return CheckError.ImposterKing;
            }

            return CheckError.CantInterpMoveErr;
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