using System.Collections.Generic;
using UnityEngine;
using option;
using board;
using chess;
using movements;

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
        BoardWithoutColorErr
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
            var (queenMovements, error) = GetPotentialLineFigMovements(queenCloneLoc);
            if (error != MoveErr.None) {
                return (null, MoveErr.FigMovesErr);
            }

            var attackInfos = new List<AttackInfo>();

            foreach (var queenMovement in queenMovements) {
                var linear = queenMovement.movement.linear.Value;
                var (last, lastErr) = BoardEngine.GetLastLinearPoint(kingPos, linear, boardClone);
                if (lastErr != BoardErr.None) {
                    return (null, MoveErr.LastLinearPointErr);
                }
                if (last.pos.IsNone()) {
                    continue;
                }

                var attackFigOpt = boardClone[last.pos.Peel().x, last.pos.Peel().y];
                var figCloneLoc = FigLoc.Mk(last.pos.Peel(), boardClone);
                var (figMovements, lineErr) = GetPotentialLineFigMovements(figCloneLoc);
                if (lineErr != MoveErr.None) {
                    return (null, MoveErr.FigMovesErr);
                }
                foreach (var figMovement in figMovements) {
                    if (figMovement.movement.square.HasValue) {
                        continue;
                    }

                    var queenDir = queenMovement.movement.linear.Value.dir;
                    var figDir = figMovement.movement.linear.Value.dir;

                    if (queenDir == -figDir && figMovement.type == MoveType.Attack) {
                        attackInfos.Add(
                            AttackInfo.Mk(
                                Option<Vector2Int>.None(),
                                FixedMovement.Mk(last.pos.Peel(), figMovement)
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

            var (potencialFigMovements, err) = GetPotentialLineFigMovements(figLoc);
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

        public static (List<FigMovement>, MoveErr) GetPotentialLineFigMovements(FigLoc figLoc) {
            if (figLoc.board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return (null, MoveErr.NoFigureOnPos);
            }

            var fig = figOpt.Peel();
            var figMovements = Movements.movements[fig.type];
            var newFigMovements = new List<FigMovement>();
            foreach (var figMovement in figMovements) {
                if (figMovement.movement.square.HasValue) {
                    return (figMovements, MoveErr.None);
                }

                var length = figMovement.movement.linear.Value.length;
                var dir = figMovement.movement.linear.Value.dir;

                if (fig.type == FigureType.Pawn) {
                    if (fig.color == FigColor.Black) {
                        dir = dir * -1;
                    }

                    if (fig.counter == 0 && figMovement.type == MoveType.Move) {
                        length = 2;
                    }
                }

                var movement = Movement.Linear(LinearMovement.Mk(length, dir));
                var newFigMovement = FigMovement.Mk(figMovement.type, movement);
                var (movelengthRes, err) = ChessEngine.GetLengthFromMoveType(
                    figLoc.pos,
                    newFigMovement,
                    figLoc.board
                );
                if (err != ChessErr.None) {
                    return (null, MoveErr.MoveLengthErr);
                }
                var movelength = movelengthRes;
                movement = Movement.Linear(LinearMovement.Mk(movelength, dir));
                newFigMovements.Add(FigMovement.Mk(figMovement.type, movement));
            }

            return (newFigMovements, MoveErr.None);
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

            var linearMoves = new List<MoveInfo>();
            for (int i = 1; i <= linear.length; i++) {
                var to = figLoc.pos + i * linear.dir;
                var move = Move.Mk(figLoc.pos, to);
                var moveInfo = new MoveInfo();

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