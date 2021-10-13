using System;
using System.Collections.Generic;
using UnityEngine;
using option;
using board;
using chess;
using math;

namespace move {
    public enum MoveErr {
        None,
        CantInterpMoveErr,
        PosOutsideBoard,
        BoardIsNull,
        MovementNotСontainLinear,
        MovementNotСontainSquare,
        NoFigureOnPos,
        MoveLengthErr,
        FigMovementsErr,
        FigMovesErr,
        LinearMovesErr,
        SquareMovesErr,
        SquarePointsErr,
        LastLinearPointErr,
        PotentialAttackInfosErr,
        BoardWithoutColorErr,
        PotentialFigMovementsErr,
        LinearAttackInfoErr
    }

    public struct KingsPos {
        public Vector2Int black;
        public Vector2Int white;
    }

    public struct AttackInfo {
        public Option<Vector2Int> defPos;
        public FixedMovement attack;

        public static AttackInfo Mk(Option<Vector2Int> defPos, FixedMovement attack) {
            return new AttackInfo{ defPos = defPos, attack = attack };
        }
    }

    public class MoveEngine {
        public static (Option<Fig>[,], MoveErr) GetBoardWithoutColor(
            Option<Fig>[,] board,
            FigColor color
        ) {
            if (board == null) {
                return (new Option<Fig>[1,1], MoveErr.BoardIsNull);
            }

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

            return (boardClone, MoveErr.None);
        }

        public static Result<KingsPos, MoveErr> FindKingsPos(Option<Fig>[,] board) {
            if (board == null) {
                return Result<KingsPos, MoveErr>.Err(MoveErr.BoardIsNull);
            }

            var kingsPos = new KingsPos();
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {

                    var figOpt = board[i, j];
                    if (figOpt.IsNone()) continue;

                    var fig = figOpt.Peel();
                    if (fig.type != FigureType.King) continue;

                    if (fig.color == FigColor.White) {
                        kingsPos.white = new Vector2Int(i, j);
                    } else {
                        kingsPos.black = new Vector2Int(i, j);
                    }
                }
            }

            return Result<KingsPos, MoveErr>.Ok(kingsPos);
        }

        public static (Option<Vector2Int>, MoveErr) GetLinearDefPoint(FigLoc figLoc, AttackInfo attackInfo) {
            var (kingPos, kingErr) = FindActiveKing(figLoc);
            var (linearAttackInfos, attackInfosErr) = GetAttackInfos(figLoc, kingPos);
            if (attackInfosErr != MoveErr.None) {
                return (Option<Vector2Int>.None(), MoveErr.LinearAttackInfoErr);
            }

            var (figMovements, figMovementsErr) = GetPossibleFigMovements(figLoc);
            if (attackInfosErr != MoveErr.None) {
                return (Option<Vector2Int>.None(), MoveErr.FigMovementsErr);
            }

            foreach (var figMovement in figMovements) {
                if (attackInfo.attack.figMovement.movement.square.HasValue) {
                    continue;
                }

                if (attackInfo.defPos.IsSome()) {
                    continue;
                }
                var attackLinear = attackInfo.attack.figMovement.movement.linear.Value;
                var attackStart = attackInfo.attack.start;
                var attackEnd = BoardEngine.GetLinearPoint(
                    attackStart,
                    attackLinear,
                    attackLinear.length
                );

                var attackSegmentInfo = MathEngine.FormStrLine(attackStart, attackEnd);
                var figLinear = figMovement.movement.linear.Value;
                var figEnd = BoardEngine.GetLinearPoint(
                    figLoc.pos,
                    figLinear,
                    figLinear.length
                );

                if (figLinear.length == 0) {
                    continue;
                }

                var figSegmentInfo = MathEngine.FormStrLine(figLoc.pos, figEnd);
                var attackSegment = Segment.Mk(attackStart, attackEnd);
                var figSegment = Segment.Mk(figLoc.pos, figEnd);
                var pointOpt = MathEngine.GetIntersectionPoint(
                    attackSegmentInfo,
                    figSegmentInfo
                );

                if (pointOpt.IsSome()) {
                    if (!BoardEngine.IsOnBoard(pointOpt.Peel(), figLoc.board)) {
                        continue;
                    }
                    var pointFigOpt = figLoc.board[pointOpt.Peel().x, pointOpt.Peel().y];
                    var figLocOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
                    if (pointFigOpt.Peel().color == figLocOpt.Peel().color){
                        continue;
                    }

                    var point = pointOpt.Peel();
                    if (MathEngine.IsPoinOnSegment(point, attackSegment)) {
                        if (MathEngine.IsPoinOnSegment(point, figSegment)) {
                            var figOpt = figLoc.board[point.x, point.y];
                            var fig = figLoc.board[figLoc.pos.x, figLoc.pos.y].Peel();
                            if (figOpt.IsSome() && figOpt.Peel().color == fig.color) {
                                return (Option<Vector2Int>.None(), MoveErr.None);
                            }
                            return (pointOpt, MoveErr.None);
                        }
                    }
                }

            }

            return (Option<Vector2Int>.None(), MoveErr.None);
        }

        public static (List<AttackInfo>, MoveErr) GetPotentialAttackInfos(
            FigLoc figLoc,
            Vector2Int checkPos
        ) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return (null, MoveErr.NoFigureOnPos);
            }

            var color = figOpt.Peel().color;
            var (boardClone, err) = GetBoardWithoutColor(figLoc.board, color);
            if (err != MoveErr.None) {
                return (null, MoveErr.BoardWithoutColorErr);
            }

            boardClone[checkPos.x, checkPos.y] = Option<Fig>.Some(
                Fig.CreateFig(color, FigureType.Queen)
            );

            var queenCloneLoc = FigLoc.Mk(checkPos, boardClone);
            var (queenMovements, queenMovementsErr) = GetPotentialFigMovements(queenCloneLoc);
            if (queenMovementsErr != MoveErr.None) {
                return (null, MoveErr.FigMovesErr);
            }

            boardClone[checkPos.x, checkPos.y] = Option<Fig>.Some(
                Fig.CreateFig(color, FigureType.Knight)
            );

            var (knightMovements, knightMovementsErr) = GetPotentialFigMovements(queenCloneLoc);
            if (knightMovementsErr != MoveErr.None) {
                return (null, MoveErr.FigMovesErr);
            }

            var allMovements = new List<FigMovement>();
            allMovements.AddRange(queenMovements);
            allMovements.AddRange(knightMovements);

            var attackInfos = new List<AttackInfo>();

            foreach (var movement in allMovements) {
                if (movement.movement.square.HasValue) {
                    var square = movement.movement.square.Value;
                    var (squarePoints, error) = BoardEngine.GetSquarePoints(
                        queenCloneLoc.pos,
                        square,
                        boardClone
                    );
                    foreach (var point in squarePoints) {
                        var knightOpt = boardClone[point.x, point.y];
                        if (knightOpt.IsNone()) {
                            continue;
                        }
                        var knight = knightOpt.Peel();
                        if (knight.type != FigureType.Knight) {
                            continue;
                        }

                        attackInfos.Add(
                            AttackInfo.Mk(
                                Option<Vector2Int>.None(),
                                FixedMovement.Mk(point, movement)
                            )
                        );
                    }
                } else if (movement.movement.linear.HasValue) {
                    var linear = movement.movement.linear.Value;
                    var last = BoardEngine.GetLinearPoint(checkPos, linear, linear.length);

                    if (boardClone[last.x, last.y].IsNone()) {
                        continue;
                    }

                    if (linear.length == 0) {
                        continue;
                    }

                    var attackFigOpt = boardClone[last.x, last.y];

                    var figCloneLoc = FigLoc.Mk(last, boardClone);
                    var (figMovements, lineErr) = GetPotentialFigMovements(figCloneLoc);
                    if (lineErr != MoveErr.None) {
                        return (null, MoveErr.FigMovesErr);
                    }
                    foreach (var figMovement in figMovements) {
                        if (figMovement.movement.square.HasValue) {
                            continue;
                        }

                        var queenDir = movement.movement.linear.Value.dir;
                        var figDir = figMovement.movement.linear.Value.dir;
                        var length = figMovement.movement.linear.Value.length;
                        var queenLength = movement.movement.linear.Value.length;

                        if (queenDir == -figDir && figMovement.type == MoveType.Attack
                            && length >= queenLength && color != attackFigOpt.Peel().color) {
                            attackInfos.Add(
                                AttackInfo.Mk(
                                    Option<Vector2Int>.None(),
                                    FixedMovement.Mk(last, figMovement)
                                )
                            );
                        }
                    }
                }
            }

            return (attackInfos, MoveErr.None);
        }

        public static (Vector2Int, MoveErr) FindActiveKing(FigLoc figLoc) {
            if (figLoc.board == null) {
                return (default(Vector2Int), MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return (default(Vector2Int), MoveErr.NoFigureOnPos);
            }

            var color = figOpt.Peel().color;

            var kingsPosRes = FindKingsPos(figLoc.board);
            if (kingsPosRes.IsErr()) {
                return (default(Vector2Int), kingsPosRes.AsErr());
            }

            var kingPos = kingsPosRes.AsOk().black;
            if (color == FigColor.White) {
                kingPos = kingsPosRes.AsOk().white;
            }

            return (kingPos, MoveErr.None);
        }

        public static (List<AttackInfo>, MoveErr) GetAttackInfos(
            FigLoc figLoc,
            Vector2Int checkPos
        ) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var (potencialAttackInfos, err) = GetPotentialAttackInfos(figLoc, checkPos);
            if (err != MoveErr.None) {
                return (null, MoveErr.PotentialAttackInfosErr);
            }

            var attackInfos = new List<AttackInfo>();
            foreach (var potAttack in potencialAttackInfos) {
                if (potAttack.attack.figMovement.movement.square.HasValue) {
                    attackInfos.Add(potAttack);
                } else if (potAttack.attack.figMovement.movement.linear.HasValue) {
                    var linear = potAttack.attack.figMovement.movement.linear.Value;
                    var length = linear.length;
                    var pos = potAttack.attack.start;
                    var dir = linear.dir;
                    var attackFig = figLoc.board[pos.x, pos.y];

                    var counter = 0;
                    var defPos = new Vector2Int();
                    for (int i = 1; i <= length; i++) {
                        var nextPos = pos + i * dir;
                        if (!BoardEngine.IsOnBoard(nextPos, figLoc.board)) {
                            break;
                        }

                        var nextFigOpt = figLoc.board[nextPos.x, nextPos.y];
                        if (nextFigOpt.IsNone()) {
                            continue;
                        }

                        var fig = nextFigOpt.Peel();
                        if (fig.color != attackFig.Peel().color && fig.type != FigureType.King) {
                            defPos = nextPos;
                            counter++;
                        }

                        if (fig.color == attackFig.Peel().color) {
                            break;
                        }
                    }

                    if (counter == 1) {
                        var attackInfo = potAttack;
                        attackInfo.defPos = Option<Vector2Int>.Some(defPos);
                        attackInfos.Add(attackInfo);
                    }

                    if (counter == 0) {
                        attackInfos.Add(potAttack);
                    }
                }
            }

            return (attackInfos, MoveErr.None);
        }

        public static (List<FigMovement>, MoveErr) GetPossibleFigMovements(FigLoc figLoc) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return (null, MoveErr.NoFigureOnPos);
            }

            var (potencialFigMovements, err) = GetPotentialFigMovements(figLoc);
            if (err != MoveErr.None) {
                return (null, MoveErr.PotentialAttackInfosErr);
            }
            var (kingPos, kingErr) = FindActiveKing(figLoc);
            var (linearAttackInfos, error) = GetAttackInfos(figLoc, kingPos);

            var realFigMovement = new FigMovement();
            var realFigMovements = new List<FigMovement>();

            foreach (var lineAttackInfo in linearAttackInfos) {
                foreach (var figMovement in potencialFigMovements) {
                    if (linearAttackInfos.Count == 0) {
                        realFigMovements.Add(figMovement);
                        continue;
                    }

                    if (lineAttackInfo.defPos.IsNone()) {
                        continue;
                    }

                    var defPos = lineAttackInfo.defPos.Peel();
                    if (defPos != figLoc.pos) {
                        continue;
                    }

                    if (figMovement.movement.square.HasValue) {
                        return (realFigMovements, MoveErr.None);
                    }

                    var defLinear = figMovement.movement.linear.Value;
                    var attackLinear = lineAttackInfo.attack.figMovement.movement.linear.Value;
                    var shadow = figMovement.shadow;
                    if (defLinear.dir == -attackLinear.dir || defLinear.dir == attackLinear.dir) {
                        var linear = LinearMovement.Mk(defLinear.length, defLinear.dir);

                        realFigMovement = FigMovement.Mk(
                            figMovement.type,
                            Movement.Linear(linear),
                            shadow
                        );
                        realFigMovements.Add(realFigMovement);
                    }
                }
            }
            if (realFigMovements.Count != 0) {
                return (realFigMovements, MoveErr.None);
            }

            return (potencialFigMovements, MoveErr.None);
        }

        public static (List<FigMovement>, MoveErr) GetPotentialFigMovements(FigLoc figLoc) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return (null, MoveErr.NoFigureOnPos);
            }

            var fig = figOpt.Peel();
            var figMovements = new List<FigMovement>();
            Func<int, int, bool> cond;
            switch (fig.type) {
                case FigureType.Pawn:
                    var length = 1;
                    if (fig.counter == 0) {
                        length = 2;
                    }

                    for (int i = -1; i <= 1; i++) {
                        var pos = new Vector2Int(1, i);
                        if (fig.color == FigColor.White) {
                            pos *= -1;
                        }

                        var type = MoveType.Attack;
                        var len = 1;
                        if (i == 0) {
                            type = MoveType.Move;
                            len = length;
                        }

                        var linearMovement = LinearMovement.Mk(len, pos);
                        figMovements.Add(
                            ChessEngine.GetFigMovement(figLoc, type, linearMovement, true)
                        );
                    }
                    break;
                case FigureType.Bishop:
                    cond = (int i, int j) => i == 0 || j == 0;
                    figMovements = CreateFigMovements(figLoc, cond);
                    break;
                case FigureType.Rook:
                    cond = (int i, int j) => i == j || -i == j || i == -j;
                    figMovements = CreateFigMovements(figLoc, cond);
                    break;
                case FigureType.Knight:
                    figMovements.Add(FigMovement.Square(MoveType.Attack, 2, 2, false));
                    break;
                case FigureType.King:
                    figMovements.Add(
                            new FigMovement {
                                type = MoveType.Attack,
                                movement = Movement.Square(SquareMovement.Mk(1, null))
                            }
                        );
                    break;
                case FigureType.Queen:
                    cond = (int i, int j) => i == 0 && j == 0;
                    figMovements = CreateFigMovements(figLoc, cond);
                    break;
            }

            return (figMovements, MoveErr.None);
        }

        public static List<FigMovement> CreateFigMovements(
            FigLoc figLoc,
            Func<int, int, bool> condition
        ) {
            var figMovements = new List<FigMovement>();
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (condition(i, j)) continue;
                    var maxLength = Mathf.Max(
                        figLoc.board.GetLength(0),
                        figLoc.board.GetLength(1)
                    );

                    var dir = new Vector2Int(i, j);
                    var linear = LinearMovement.Mk(maxLength, dir);
                    figMovements.Add(
                        ChessEngine.GetFigMovement(figLoc, MoveType.Move, linear, false)
                    );
                    figMovements.Add(
                        ChessEngine.GetFigMovement(figLoc, MoveType.Attack, linear, false)
                    );
                }
            }

            return figMovements;
        }

        public static (List<MoveInfo>, MoveErr) GetLinearMoves(
            FigLoc figLoc,
            LinearMovement linear
        ) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return (null, MoveErr.NoFigureOnPos);
            }

            var (kingPos, kingErr) = FindActiveKing(figLoc);
            var (attackInfos, attackInfosErr) = GetAttackInfos(figLoc, kingPos);
            if (attackInfosErr != MoveErr.None) {
                return (null, MoveErr.LinearAttackInfoErr);
            }

            var linearMoves = new List<MoveInfo>();
            for (int i = 1; i <= linear.length; i++) {
                var moveInfo = new MoveInfo();
                var to = figLoc.pos + i * linear.dir;
                foreach (var attackInfo in attackInfos) {
                    if (attackInfo.defPos.IsNone()) {
                        var attack = attackInfo.attack.start;
                        if (attackInfo.attack.figMovement.movement.square.HasValue
                            && attack == to) {
                            moveInfo.move = DoubleMove.Mk(Move.Mk(figLoc.pos, attack), null);
                            if (figLoc.board[attack.x, attack.y].IsSome()) {
                                moveInfo.sentenced = attack;
                            }
                            linearMoves.Add(moveInfo);
                            return (linearMoves, MoveErr.None);
                        }

                        var (def, err) = GetLinearDefPoint(figLoc, attackInfo);
                        if (def.IsSome()) {
                            moveInfo.move = DoubleMove.Mk(Move.Mk(figLoc.pos, def.Peel()), null);
                            if (figLoc.board[def.Peel().x, def.Peel().y].IsSome()) {
                                moveInfo.sentenced = def.Peel();
                            }
                            linearMoves.Add(moveInfo);
                            return (linearMoves, MoveErr.None);
                        } else {
                            return (linearMoves, MoveErr.None);
                        }
                    }
                }

                var move = Move.Mk(figLoc.pos, to);

                if (!BoardEngine.IsOnBoard(to, figLoc.board)) {
                    break;
                }

                var fig = figOpt.Peel();
                if (fig.type == FigureType.Pawn && fig.counter == 0) {
                    var num = 1;
                    if (fig.color == FigColor.Black) {
                        num = -1;
                    }

                    moveInfo.shadow = new FigShadow {
                        figType = FigureType.Pawn,
                        pos = new Vector2Int(to.x + num, to.y)
                    };
                }

                if (figLoc.board[to.x, to.y].IsSome()) {
                    moveInfo.sentenced = to;
                }

                if (ChessEngine.IsPossibleMove(move, figLoc.board)) {
                    moveInfo.move = DoubleMove.Mk(move, null);
                    linearMoves.Add(moveInfo);
                }
            }

            return (linearMoves, MoveErr.None);
        }

        public static (List<MoveInfo>, MoveErr) GetSquareMoves(
            FigLoc figLoc,
            SquareMovement square
        ) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var (squarePoints, err) = BoardEngine.GetSquarePoints(
                figLoc.pos,
                square,
                figLoc.board
            );

            var (kingPos, kingErr) = FindActiveKing(figLoc);

            var (attackInfos, error) = GetAttackInfos(figLoc, kingPos);
            var squareMoves = new List<MoveInfo>();
            foreach (var point in squarePoints) {
                var to = point;
                var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
                if (figOpt.IsSome()) {
                    if (figOpt.Peel().type == FigureType.King) {
                        if (PositionIsUnderAttack(figLoc, to)) {
                            continue;
                        }
                    }
                }
                if (IsCheck(figLoc)) {
                    foreach (var attackInfo in attackInfos) {
                        if (attackInfo.attack.figMovement.movement.square.HasValue) {
                            if (to != attackInfo.attack.start) {
                                continue;
                            }

                            var move = Move.Mk(figLoc.pos, to);
                            var moveInfo = new MoveInfo();

                            if (figLoc.board[to.x, to.y].IsSome()) {
                                moveInfo.sentenced = to;
                            }

                            if (ChessEngine.IsPossibleMove(move, figLoc.board)) {
                                moveInfo.move = DoubleMove.Mk(move, null);
                                squareMoves.Add(moveInfo);
                            }
                        } else {
                            var start = attackInfo.attack.start;
                            var linear = attackInfo.attack.figMovement.movement.linear.Value;
                            var end = BoardEngine.GetLinearPoint(start, linear, linear.length);
                            var attackSegment = Segment.Mk(start, end);
                            if (MathEngine.IsPoinOnSegment(to, attackSegment)) {
                                var move = Move.Mk(figLoc.pos, to);
                                var moveInfo = new MoveInfo();

                                if (figLoc.board[to.x, to.y].IsSome()) {
                                    moveInfo.sentenced = to;
                                }

                                if (ChessEngine.IsPossibleMove(move, figLoc.board)) {
                                    moveInfo.move = DoubleMove.Mk(move, null);
                                    squareMoves.Add(moveInfo);
                                }
                            }
                        }
                    }
                } else {

                    var move = Move.Mk(figLoc.pos, to);
                    var moveInfo = new MoveInfo();

                    if (figLoc.board[to.x, to.y].IsSome()) {
                        moveInfo.sentenced = to;
                    }

                    if (ChessEngine.IsPossibleMove(move, figLoc.board)) {
                        moveInfo.move = DoubleMove.Mk(move, null);
                        squareMoves.Add(moveInfo);
                    }
                }
            }

            return (squareMoves, MoveErr.None);
        }

        public static bool PositionIsUnderAttack(FigLoc figLoc, Vector2Int pos) {
            var (attInfos, attackInfosErr) = GetAttackInfos(figLoc, pos);
            foreach (var att in attInfos) {
                if (att.defPos.IsNone()){
                    return true;
                }
            }

            return false;
        }

        public static bool IsCheck(FigLoc figLoc) {
            var (kingPos, kingErr) = FindActiveKing(figLoc);
            var (attackInfos, error) = GetAttackInfos(figLoc, kingPos);
            foreach (var attackInfo in attackInfos) {
                if (attackInfo.defPos.IsNone()) {
                    return true;
                }
            }

            return false;
        }

        public static (List<MoveInfo>, MoveErr) GetFigMoves(FigLoc figLoc) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return (null, MoveErr.NoFigureOnPos);
            }

            var figMoves = new List<MoveInfo>();
            var (figMovements, err) = GetPossibleFigMovements(figLoc);
            if (err != MoveErr.None) {
                return (null, MoveErr.FigMovementsErr);
            }

            foreach (var figMovement in figMovements) {
                var fixedMovement = FixedMovement.Mk(figLoc.pos, figMovement);
                if (figMovement.movement.linear.HasValue) {
                    var linear = figMovement.movement.linear.Value;
                    var (linearMovesRes, error) = GetLinearMoves(figLoc, linear);
                    if (error != MoveErr.None) {
                        return (null, MoveErr.LinearMovesErr);
                    }

                    var linearMoves = linearMovesRes;
                    figMoves.AddRange(linearMoves);
                } else if (figMovement.movement.square.HasValue) {
                    var square = figMovement.movement.square.Value;
                    var (squareMovesRes, error) = GetSquareMoves(figLoc, square);
                    if (error != MoveErr.None) {
                        return (null, MoveErr.SquareMovesErr);
                    }

                    var squareMoves = squareMovesRes;
                    figMoves.AddRange(squareMoves);
                }
            }

            return (figMoves, MoveErr.None);
        }

        public static void MoveFigure(Move move, Option<Fig>[,] board) {
            var posTo = move.to;
            var posFrom = move.from;

            board[posTo.x, posTo.y] = board[posFrom.x, posFrom.y];
            board[posFrom.x, posFrom.y] = Option<Fig>.None();

            var figure = board[move.to.x, move.to.y].Peel();
            figure.counter++;
            board[move.to.x, move.to.y] = Option<Fig>.Some(figure);
        }
    }
}