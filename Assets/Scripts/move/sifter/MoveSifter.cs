using System.Collections.Generic;
using board;
using chess;
using move;
using option;
using inspector;
using movements;


namespace sifter {
    public class MoveSifter {
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
                    ChessInspector.InterpMoveEngineErr(movementsRes.AsErr())
                );
            }
            var movements = movementsRes.AsOk();

            var kingsPosRes = ChessInspector.FindKingsPos(figLoc.board);
            if (kingsPosRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(kingsPosRes.AsErr());
            }

            var kingsPos = kingsPosRes.AsOk();
            var kingPos = kingsPos.black;

            if (color == FigColor.White) {
                kingPos = kingsPos.white;
            }

            var possibleMoves = new List<MoveInfo>();
            var newFigLoc = FigLoc.Mk(kingPos, figLoc.board);
            var checkInfosRes = ChessInspector.GetAttackInfos(newFigLoc);
            if (checkInfosRes.IsErr()) {
                return Result<List<MoveInfo>, CheckError>.Err(checkInfosRes.AsErr());
            }

            var attackInfos = ChessInspector.GetAttackInfos(newFigLoc).AsOk();

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
                   ChessInspector.InterpMoveEngineErr(defaultMovesRes.AsErr())
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
                   ChessInspector.InterpMoveEngineErr(defMovementsRes.AsErr())
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
                   ChessInspector.InterpMoveEngineErr(movesRes.AsErr())
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
                                ChessInspector.InterpMoveEngineErr(movesRes.AsErr())
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
                    ChessInspector.InterpMoveEngineErr(movementsRes.AsErr())
                );
            }

            var movement = movementsRes.AsOk();
            var possMoves = new List<MoveInfo>();
            if (!attackInfo.defPos.HasValue) {
                if (attackInfo.attack.movement.linear.HasValue) {
                    var movesRes = MoveEngine.GetMoves(figLoc, movement, lastMove);
                    if (movesRes.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(
                            ChessInspector.InterpMoveEngineErr(movesRes.AsErr())
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
                        ChessInspector.InterpMoveEngineErr(kingMovesRes.AsErr())
                    );
                }
                var kingMoves = kingMovesRes.AsOk();
                foreach (var move in kingMoves) {
                    var moveTo = move.move.first.Value.to;
                    var moveToLoc = FigLoc.Mk(moveTo, figLoc.board);
                    var isUnderAttackRes = MoveEngine.IsUnderAttackPos(moveToLoc, color, lastMove);
                    if (isUnderAttackRes.IsErr()) {
                        return Result<List<MoveInfo>, CheckError>.Err(
                            ChessInspector.InterpMoveEngineErr(isUnderAttackRes.AsErr())
                        );
                    }

                    if (!isUnderAttackRes.AsOk()){
                        kingPossMoves.Add(move);
                    }
                }
            }

            return Result<List<MoveInfo>, CheckError>.Ok(kingPossMoves);
        }
    }
}