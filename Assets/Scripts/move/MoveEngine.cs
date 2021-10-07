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

            return Result<KingsPos, MoveErr>.Ok(kingsPos);
        }

        public static (Option<Vector2Int>, MoveErr) GetDefPoint(FigLoc figLoc) {
            var (linearAttackInfos, attackInfosErr) = GetLinearAttackInfo(figLoc);
            if (attackInfosErr != MoveErr.None) {
                return (Option<Vector2Int>.None(), MoveErr.LinearAttackInfoErr);
            }

            var (figMovements, figMovementsErr) = GetFigMovements(figLoc);
            if (attackInfosErr != MoveErr.None) {
                return (Option<Vector2Int>.None(), MoveErr.FigMovementsErr);
            }

            foreach (var figMovement in figMovements) {
                foreach (var attackInfo in linearAttackInfos) {
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
                    var figSegmentInfo = MathEngine.FormStrLine(figLoc.pos, figEnd);
                    var attackSegment = Segment.Mk(attackStart, attackEnd);
                    var figSegment = Segment.Mk(figLoc.pos, figEnd);
                    var point = MathEngine.GetIntersectionPoint(attackSegmentInfo, figSegmentInfo);
                    if (point.IsSome()) {
                        if (MathEngine.IsPoinOnSegment(point.Peel(), attackSegment)) {
                            if (MathEngine.IsPoinOnSegment(point.Peel(), figSegment)) {
                                return (point, MoveErr.None);
                            }
                        }
                    }
                }
            }

            return (Option<Vector2Int>.None(), MoveErr.None);
        }

        public static (List<AttackInfo>, MoveErr) GetPotentialAttackInfos(FigLoc figLoc) {
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

            var kingsPosRes = FindKingsPos(figLoc.board);
            if (kingsPosRes.IsErr()) {
                return (null, kingsPosRes.AsErr());
            }

            var kingPos = kingsPosRes.AsOk().black;
            if (color == FigColor.White) {
                kingPos = kingsPosRes.AsOk().white;
            }

            boardClone[kingPos.x, kingPos.y] = Option<Fig>.Some(
                Fig.CreateFig(color, FigureType.Queen)
            );

            var queenCloneLoc = FigLoc.Mk(kingPos, boardClone);
            var (queenMovements, error) = GetPotentialFigMovements(queenCloneLoc);
            if (error != MoveErr.None) {
                return (null, MoveErr.FigMovesErr);
            }

            var attackInfos = new List<AttackInfo>();

            foreach (var queenMovement in queenMovements) {
                if (queenMovement.movement.square.HasValue) {
                    continue;
                }

                var linear = queenMovement.movement.linear.Value;
                var last = BoardEngine.GetLinearPoint(kingPos, linear, linear.length);

                if (boardClone[last.x, last.y].IsNone()) {
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

                    var queenDir = queenMovement.movement.linear.Value.dir;
                    var figDir = figMovement.movement.linear.Value.dir;

                    if (queenDir == -figDir && figMovement.type == MoveType.Attack 
                        && color != attackFigOpt.Peel().color) {
                        attackInfos.Add(
                            AttackInfo.Mk(
                                Option<Vector2Int>.None(),
                                FixedMovement.Mk(last, figMovement)
                            )
                        );
                    }
                }
            }

            return (attackInfos, MoveErr.None);
        }

        public static (List<AttackInfo>, MoveErr) GetLinearAttackInfo(FigLoc figLoc) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return (null, MoveErr.NoFigureOnPos);
            }

            var (potencialAttackInfos, err) = GetPotentialAttackInfos(figLoc);
            if (err != MoveErr.None) {
                return (null, MoveErr.PotentialAttackInfosErr);
            }

            var attackInfos = new List<AttackInfo>();
            foreach (var potAttack in potencialAttackInfos) {
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

            return (attackInfos, MoveErr.None);
        }

        public static (List<FigMovement>, MoveErr) GetFigMovements(FigLoc figLoc) {
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
            var (linearAttackInfos, error) = GetLinearAttackInfo(figLoc);

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
                        continue;
                    }

                    var defLinear = figMovement.movement.linear.Value;
                    var attackLinear = lineAttackInfo.attack.figMovement.movement.linear.Value;
                    if (defLinear.dir == -attackLinear.dir || defLinear.dir == attackLinear.dir) {
                        var linear = LinearMovement.Mk(defLinear.length, defLinear.dir);

                        realFigMovement = FigMovement.Mk(
                            figMovement.type,
                            Movement.Linear(linear)
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
            switch (fig.type) {
                case FigureType.Pawn:
                    var length = 1;
                    if (fig.counter == 0) {
                        length = 2;
                    }

                    if (fig.color == FigColor.White) {
                        figMovements = new List<FigMovement> {
                            FigMovement.Linear(MoveType.Move, new Vector2Int(-1, 0), length),
                            FigMovement.Linear(MoveType.Attack, new Vector2Int(-1, 1), 1),
                            FigMovement.Linear(MoveType.Attack, new Vector2Int(-1, -1), 1)
                        };
                    }

                    if (fig.color == FigColor.Black) {
                        figMovements = new List<FigMovement> {
                            FigMovement.Linear(MoveType.Move, new Vector2Int(1, 0), length),
                            FigMovement.Linear(MoveType.Attack, new Vector2Int(1, 1), 1),
                            FigMovement.Linear(MoveType.Attack, new Vector2Int(1, -1), 1)
                        };
                    }
                    break;
                case FigureType.Bishop:
                    Func<int, int, bool> bishopCond = (int i, int j) => i == 0 || j == 0;
                    figMovements = CreateFigMovements(bishopCond);
                    break;
                case FigureType.Rook:
                    Func<int, int, bool> rookCond = (int i, int j) => i == j || -i == j || i == -j;
                    figMovements = CreateFigMovements(rookCond);
                    break;
                case FigureType.Knight:
                    figMovements = new List<FigMovement> {
                        FigMovement.Square(MoveType.Attack, 2)
                    };
                    break;
                case FigureType.King:
                    figMovements = new List<FigMovement> {
                        FigMovement.Square(MoveType.Attack, 1)
                    };
                    break;
                case FigureType.Queen:
                    Func<int, int, bool> condition = (int i, int j) => i == 0 && j == 0;
                    figMovements = CreateFigMovements(condition);
                    break;
            }
            figMovements = ChessEngine.CorrectFigMovementsLength(figLoc, figMovements);

            return (figMovements, MoveErr.None);
        }

        public static List<FigMovement> CreateFigMovements(Func<int, int, bool> condition) {
            var figMovements = new List<FigMovement>();
            for (int i = -1; i <= 1; i++) {
                for (int j = -1; j <= 1; j++) {
                    if (condition(i, j)) continue;

                    var dir = new Vector2Int(i, j);
                    figMovements.Add(FigMovement.Linear(MoveType.Move, dir, -1));
                    figMovements.Add(FigMovement.Linear(MoveType.Attack, dir, -1));
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

            var (attackInfos, attackInfosErr) = GetLinearAttackInfo(figLoc);
            if (attackInfosErr != MoveErr.None) {
                return (null, MoveErr.LinearAttackInfoErr);
            }

            var linearMoves = new List<MoveInfo>();
            for (int i = 1; i <= linear.length; i++) {
                var moveInfo = new MoveInfo();
                foreach (var attackInfo in attackInfos) {
                    if (attackInfo.defPos.IsNone()) {
                        var (def, err) = GetDefPoint(figLoc);
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

                var to = figLoc.pos + i * linear.dir;
                var move = Move.Mk(figLoc.pos, to);

                if (!BoardEngine.IsOnBoard(to, figLoc.board)) {
                    break;
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

            var (squarePointsRes, err) = ChessEngine.GetFigureSquarePoints(figLoc, square);
            if (err != ChessErr.None) {
                return (null, MoveErr.SquareMovesErr);
            }

            var squarePoints = squarePointsRes;
            var squareMoves = new List<MoveInfo>();
            foreach (var point in squarePoints) {
                var to = point;
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

            return (squareMoves, MoveErr.None);
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
            var (figMovements, err) = GetFigMovements(figLoc);
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