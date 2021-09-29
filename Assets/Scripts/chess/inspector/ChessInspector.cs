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