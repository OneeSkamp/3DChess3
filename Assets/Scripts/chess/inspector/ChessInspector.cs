using UnityEngine;
using System.Collections.Generic;
using option;
using chess;
using move;
using board;
using movements;

namespace inspector {
    public struct AttackInfo {
        public FixedMovement attack;
        public Vector2Int? defPos;
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
        public static Result<List<AttackInfo>, CheckError> GetPotentialAttackInfos(
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

            var checkInfos = new List<AttackInfo>();
            var allMovement = new List<Movement>();


            var boardCloneRes = MoveEngine.GetBoardWithoutColor(kingLoc.board, king.color);

            if (boardCloneRes.IsErr()) {
                return Result<List<AttackInfo>, CheckError>.Err(
                    InterpMoveEngineErr(boardCloneRes.AsErr())
                );
            }

            var boardClone = boardCloneRes.AsOk();
            boardClone[kingLoc.pos.x, kingLoc.pos.y] = kingOpt;

            foreach (var movement in Movements.queenMovement) {
                var moveType = movement.moveType;
                var dir = movement.linear.Value.dir;
                var length = BoardEngine.GetLinearLength(kingLoc.pos, dir, boardClone);
                if (movement.linear.Value.length < 0) {
                    var queenMovement = Movement.Linear(LinearMovement.Mk(length, dir), moveType);
                    allMovement.Add(queenMovement);
                }
            }
            allMovement.AddRange(Movements.knightMovement);

            var linearCheckInfosRes = GetLinearPotentialAttackInfos(
                allMovement,
                kingLoc,
                boardClone
            );
            if (linearCheckInfosRes.IsErr()) {
                return Result<List<AttackInfo>, CheckError>.Err(linearCheckInfosRes.AsErr());
            }
            checkInfos.AddRange(linearCheckInfosRes.AsOk());

            var squareCheckInfosRes = GetSquarePotentialAttackInfos(
                allMovement,
                kingLoc,
                boardClone
            );
            if (squareCheckInfosRes.IsErr()) {
                return Result<List<AttackInfo>, CheckError>.Err(squareCheckInfosRes.AsErr());
            }
            checkInfos.AddRange(squareCheckInfosRes.AsOk());

            return Result<List<AttackInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<AttackInfo>, CheckError> GetSquarePotentialAttackInfos(
            List<Movement> movements,
            FigLoc kingLoc,
            Option<Fig>[,] boardClone
        ) {
            var kingOpt = kingLoc.board[kingLoc.pos.x, kingLoc.pos.y];
            if (kingOpt.IsNone()) {
                return Result<List<AttackInfo>, CheckError>.Err(CheckError.NoFigureOnPos);
            }

            var king = kingOpt.Peel();
            if (king.type != FigureType.King) {
                return Result<List<AttackInfo>, CheckError>.Err(CheckError.ImposterKing);
            }

            var checkInfos = new List<AttackInfo>();
            foreach (var movement in movements) {
                if (movement.linear.HasValue) {
                    continue;
                }

                var square = BoardEngine.GetSquarePath(kingLoc.pos, movement.square.Value);
                var knightPath = BoardEngine.RemoveSquareParts(square, 0, 1);

                if (knightPath.Count == 0) {
                    continue;
                }

                foreach (var cell in knightPath) {
                    if (!BoardEngine.IsOnBoard(cell, kingLoc.board)){
                        continue;
                    }

                    var figOpt = boardClone[cell.x, cell.y];
                    if (figOpt.IsNone()) {
                        continue;
                    }

                    if (figOpt.Peel().type == FigureType.Knight) {
                        var knightMovement = Movement.Square(SquareMovement.Mk(5), MoveType.Move);

                        checkInfos.Add(
                            new AttackInfo {
                                attack = FixedMovement.Mk(cell, knightMovement)
                            }
                        );
                    }
                }
            }

            return Result<List<AttackInfo>, CheckError>.Ok(checkInfos);
        }

        public static Result<List<AttackInfo>, CheckError> GetLinearPotentialAttackInfos(
            List<Movement> movements,
            FigLoc figLoc,
            Option<Fig>[,] boardClone
        ) {
            var checkInfos = new List<AttackInfo>();
            foreach (var movement in movements) {
                if (movement.square.HasValue) {
                    continue;
                }

                var fixedMovement = FixedMovement.Mk(figLoc.pos, movement);
                var start = fixedMovement.start;
                var linear = movement.linear.Value;
                var figPos = BoardEngine.GetLastOnPathPos(start, linear, boardClone);

                if (!figPos.HasValue) {
                    continue;
                }

                if (boardClone[figPos.Value.x, figPos.Value.y].IsNone()) {
                    continue;
                }

                var figOpt = boardClone[figPos.Value.x, figPos.Value.y];
                if (figOpt.IsNone()) {
                    continue;
                }
                var fig = figOpt.Peel();

                var figMovementsRes = MoveEngine.GetRealMovement(
                    FigLoc.Mk(figPos.Value, boardClone)
                );
                if (figMovementsRes.IsErr()) {
                    return Result<List<AttackInfo>, CheckError>.Err(
                        InterpMoveEngineErr(figMovementsRes.AsErr())
                    );
                }

                var figMovements = figMovementsRes.AsOk();
                foreach (var figMovement in figMovements) {
                    if (!figMovement.linear.HasValue) {
                        continue;
                    }

                    var moveType = figMovement.moveType;
                    var figDir = -figMovement.linear.Value.dir;
                    var dir = fixedMovement.movement.linear.Value.dir;
                    var length = figMovement.linear.Value.length;
                    if (dir != figDir) {
                        continue;
                    }

                    var attackLength = Mathf.Abs(figLoc.pos.x - figPos.Value.x);

                    if (moveType == MoveType.Attack && attackLength <= length) {
                        checkInfos.Add(
                            new AttackInfo {
                                attack = fixedMovement,
                            }
                        );
                    }
                }
            }

            return Result<List<AttackInfo>, CheckError>.Ok(checkInfos);
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

            var potentialAttackInfosRes = GetPotentialAttackInfos(kingLoc);
            if (potentialAttackInfosRes.IsErr()) {
                return Result<List<AttackInfo>, CheckError>.Err(potentialAttackInfosRes.AsErr());
            }
            var potentialCheckInfos = GetPotentialAttackInfos(kingLoc).AsOk();
            var attackInfos = new List<AttackInfo>();

            foreach (var potCheckInfo in potentialCheckInfos) {
                var attackInfo = new AttackInfo();
                var checkInfoRes = GetRealAttackInfo(kingLoc, potCheckInfo);
                if (checkInfoRes.IsErr()) {
                    return Result<List<AttackInfo>, CheckError>.Err(checkInfoRes.AsErr());
                }

                var checkInfo = checkInfoRes.AsOk();
                if (!checkInfo.HasValue) {
                    continue;
                }
                if (!checkInfo.Value.defPos.HasValue) {
                    attackInfo = new AttackInfo {
                        attack = checkInfo.Value.attack
                    };
                }

                attackInfos.Add(checkInfo.Value);
            }

            return Result<List<AttackInfo>, CheckError>.Ok(attackInfos);
        }

        public static Result<AttackInfo?, CheckError> GetRealAttackInfo(
            FigLoc kingLoc,
            AttackInfo potentialAttackInfo
        ) {
            var linear = potentialAttackInfo.attack.movement.linear;
            if (linear.HasValue) {
                var dir = linear.Value.dir;
                var length = potentialAttackInfo.attack.movement.linear.Value.length;
                var checkPath = BoardEngine.GetLinearPath(
                    kingLoc.pos,
                    linear.Value,
                    kingLoc.board
                );

                var defPositionsRes = GetDefPositions(kingLoc, checkPath);
                if (defPositionsRes.IsErr()) {
                    return Result<AttackInfo?, CheckError>.Err(defPositionsRes.AsErr());
                }

                var defPositions = defPositionsRes.AsOk();
                if (defPositions.Count == 1) {
                    var newCheckInfo = potentialAttackInfo;
                    newCheckInfo.defPos = defPositions[0];
                    return Result<AttackInfo?, CheckError>.Ok(newCheckInfo);
                }

                if (defPositions.Count == 0) {
                    var checkInfo = new AttackInfo {
                        attack = potentialAttackInfo.attack,
                    };

                    return Result<AttackInfo?, CheckError>.Ok(checkInfo);
                }

                return Result<AttackInfo?, CheckError>.Ok(null);

            } else {
                return Result<AttackInfo?, CheckError>.Ok(potentialAttackInfo);
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

            var movementsRes = MoveEngine.GetRealMovement(figLoc);
            if (movementsRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(
                    InterpMoveEngineErr(movementsRes.AsErr())
                );
            }
            var movements = movementsRes.AsOk();

            var kingsPosRes = FindKingsPos(figLoc.board);
            if (kingsPosRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(kingsPosRes.AsErr());
            }

            var kingsPos = kingsPosRes.AsOk();
            var kingPos = kingsPos.black;

            if (color == FigColor.White) {
                kingPos = kingsPos.white;
            }

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
                if (attackInfo.defPos.HasValue) {
                    var defPos = attackInfo.defPos;
                    if (defPos == figLoc.pos) {
                        var moves = GetCoveredFigMove(figLoc, attackInfo, lastMove);
                        if (moves.IsErr()) {
                            return Result<List<MoveInfo>, CheckError>.Err(moves.AsErr());
                        }

                        return Result<List<MoveInfo>, CheckError>.Ok(moves.AsOk());
                    }
                    continue;
                }

                if (attackInfo.attack.movement.linear.HasValue) {
                    var possMoves = GetLinearPossibleMove(figLoc, attackInfo, lastMove);
                    if (possMoves.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(possMoves.AsErr());
                    }

                    possibleMoves.AddRange(possMoves.AsOk());
                } else if (attackInfo.attack.movement.square.HasValue) {
                    var possMoves = GetSquarePossibleMove(figLoc, attackInfos, lastMove);
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

        public static Result<List<MoveInfo>, CheckError> GetCoveredFigMove(
            FigLoc defLoc,
            AttackInfo attackInfo,
            MoveInfo lastMove
        ) {
            var movement = new List<Movement>();
            if (defLoc.board[defLoc.pos.x, defLoc.pos.y].Peel().type == FigureType.Knight) {
                return Result<List<MoveInfo>, CheckError>.Ok(null);
            }

            var defMovementsRes = MoveEngine.GetRealMovement(defLoc);
            if (defMovementsRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(
                    InterpMoveEngineErr(defMovementsRes.AsErr())
                );
            }
            var defMovements = defMovementsRes.AsOk();

            if (attackInfo.attack.movement.linear.HasValue) {
                var linear = attackInfo.attack.movement.linear.Value;
                var dir = linear.dir;
                foreach (var defMovement in defMovements) {
                    if (defMovement.linear.Value.dir == linear.dir) {
                        var length = defMovement.linear.Value.length;
                        var moveType = defMovement.moveType;
                        if (length < 0) {
                            length = BoardEngine.GetLinearLength(defLoc.pos, dir, defLoc.board);
                        }
                        movement.Add(Movement.Linear(LinearMovement.Mk(length, dir), moveType));
                    }
                }
            }

            var movesRes = MoveEngine.GetMoves(defLoc, movement, lastMove);
            if (movesRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(
                    InterpMoveEngineErr(movesRes.AsErr())
                );
            }

            var moves = MoveEngine.GetMoves(defLoc, movement, lastMove).AsOk();

            return Result<List<MoveInfo>, CheckError>.Ok(moves);
        }

        public static Result<List<MoveInfo>, CheckError> GetSquarePossibleMove(
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
                if (!attackInfo.defPos.HasValue) {
                    var attackPos = attackInfo.attack.start;
                    if (attackInfo.attack.movement.square.HasValue) {
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

        public static Result<List<MoveInfo>, CheckError> GetLinearPossibleMove(
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
            var movementsRes = MoveEngine.GetRealMovement(figLoc);
            if (movementsRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(
                    InterpMoveEngineErr(movementsRes.AsErr())
                );
            }

            var movement = movementsRes.AsOk();
            var possMoves = new List<MoveInfo>();
            if (!attackInfo.defPos.HasValue) {
                if (attackInfo.attack.movement.linear.HasValue) {
                    var movesRes = MoveEngine.GetMoves(figLoc, movement, lastMove);
                    if (movesRes.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(
                            InterpMoveEngineErr(movesRes.AsErr())
                        );
                    }
                    var moves = movesRes.AsOk();
                    foreach (var move in moves) {
                        var firstTo = move.move.first.Value.to;
                        var start = attackInfo.attack.start;
                        var linear = attackInfo.attack.movement.linear.Value;
                        var path = BoardEngine.GetLinearPathToFigure(
                            start,
                            linear,
                            figLoc.board
                        );

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