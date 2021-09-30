using System.Collections.Generic;
using UnityEngine;
using option;
using board;
using chess;
using movements;

namespace move {
    public enum MoveErr {
        CantInterpMoveErr,
        PosOutsideBoard,
        BoardIsNull,
        MovementNotСontainLinear,
        MovementNotСontainSquare,
        NoFigureOnPos
    }

    public class MoveEngine {
        public static Result<List<FigMovement>, MoveErr> GetFigMovements(FigLoc figLoc) {
            if (figLoc.board == null) {
                return Result<List<FigMovement>, MoveErr>.Err(MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                Result<List<FigMovement>, MoveErr>.Err(MoveErr.NoFigureOnPos);
            }

            var fig = figOpt.Peel();
            var figMovements = Movements.movements[fig.type];
            var newFigMovements = new List<FigMovement>();
            foreach (var figMovement in figMovements) {
                if (figMovement.movement.square.HasValue) {
                    return Result<List<FigMovement>, MoveErr>.Ok(figMovements);
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
                var movelengthRes = ChessEngine.GetMoveLength(
                    figLoc.pos,
                    newFigMovement,
                    figLoc.board
                );
                if (movelengthRes.IsErr()) {
                    return Result<List<FigMovement>, MoveErr>.Err(
                        InterpChessErr(movelengthRes.AsErr())
                    );
                }
                var movelength = movelengthRes.AsOk();
                movement = Movement.Linear(LinearMovement.Mk(movelength, dir));
                newFigMovements.Add(FigMovement.Mk(figMovement.type, movement));
            }

            return Result<List<FigMovement>, MoveErr>.Ok(newFigMovements);
        }
        public static Result<List<MoveInfo>, MoveErr> GetLinearMoves(
            FixedMovement fixedMovement,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<MoveInfo>, MoveErr>.Err(MoveErr.BoardIsNull);
            }

            if (fixedMovement.figMovement.movement.square.HasValue) {
                return Result<List<MoveInfo>, MoveErr>.Err(MoveErr.MovementNotСontainLinear);
            }

            var linear = fixedMovement.figMovement.movement.linear.Value;
            var pos = fixedMovement.start;
            var linearMoves = new List<MoveInfo>();
            for (int i = 1; i <= linear.length; i++) {
                var to = pos + i * linear.dir;
                var move = Move.Mk(pos, to);
                var moveInfo = new MoveInfo();

                if (board[to.x, to.y].IsSome()) {
                    moveInfo.sentenced = to;
                }

                if (ChessEngine.IsPossibleMove(move, board)) {
                    moveInfo.move = DoubleMove.Mk(move, null);
                    linearMoves.Add(moveInfo);
                }
            }

            return Result<List<MoveInfo>, MoveErr>.Ok(linearMoves);
        }

        public static Result<List<MoveInfo>, MoveErr> GetSquareMoves(
            FixedMovement fixedMovement,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<MoveInfo>, MoveErr>.Err(MoveErr.BoardIsNull);
            }

            if (fixedMovement.figMovement.movement.linear.HasValue) {
                return Result<List<MoveInfo>, MoveErr>.Err(MoveErr.MovementNotСontainSquare);
            }

            var squarePointsRes = ChessEngine.GetRealSquarePoints(fixedMovement, board);
            if (squarePointsRes.IsErr()) {
                return Result<List<MoveInfo>, MoveErr>.Err(
                    InterpChessErr(squarePointsRes.AsErr())
                );
            }

            var squarePoints = squarePointsRes.AsOk();
            var pos = fixedMovement.start;
            var squareMoves = new List<MoveInfo>();
            foreach (var point in squarePoints) {
                var to = point;
                var move = Move.Mk(pos, to);
                var moveInfo = new MoveInfo();

                if (board[to.x, to.y].IsSome()) {
                    moveInfo.sentenced = to;
                }

                if (ChessEngine.IsPossibleMove(move, board)) {
                    moveInfo.move = DoubleMove.Mk(move, null);
                    squareMoves.Add(moveInfo);
                }
            }

            return Result<List<MoveInfo>, MoveErr>.Ok(squareMoves);
        }

        public static Result<List<MoveInfo>, MoveErr> GetFigMoves(FigLoc figLoc) {
            if (figLoc.board == null) {
                return Result<List<MoveInfo>, MoveErr>.Err(MoveErr.BoardIsNull);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return Result<List<MoveInfo>, MoveErr>.Err(MoveErr.NoFigureOnPos);
            }

            var figMoves = new List<MoveInfo>();
            var figMovementsRes = GetFigMovements(figLoc);
            if (figMovementsRes.IsErr()) {
                return Result<List<MoveInfo>, MoveErr>.Err(figMovementsRes.AsErr());
            }

            var figMovements = figMovementsRes.AsOk();
            foreach (var figMovement in figMovements) {
                var fixedMovement = FixedMovement.Mk(figLoc.pos, figMovement);
                if (figMovement.movement.linear.HasValue) {
                    var linearMovesRes = GetLinearMoves(fixedMovement, figLoc.board);
                    if (linearMovesRes.IsErr()) {
                        return Result<List<MoveInfo>, MoveErr>.Err(linearMovesRes.AsErr());
                    }

                    var linearMoves = linearMovesRes.AsOk();
                    figMoves.AddRange(linearMoves);
                } else if (figMovement.movement.square.HasValue) {
                    var squareMovesRes = GetSquareMoves(fixedMovement, figLoc.board);
                    if (squareMovesRes.IsErr()) {
                        return Result<List<MoveInfo>, MoveErr>.Err(squareMovesRes.AsErr());
                    }

                    var squareMoves = squareMovesRes.AsOk();
                    figMoves.AddRange(squareMoves);
                }
            }

            return Result<List<MoveInfo>, MoveErr>.Ok(figMoves);
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

        public static MoveErr InterpChessErr(ChessErr err) {
            switch (err) {
                case ChessErr.BoardIsNull:
                    return MoveErr.BoardIsNull;
                case ChessErr.PosOutsideBoard:
                    return MoveErr.PosOutsideBoard;
            }

            return MoveErr.CantInterpMoveErr;
        }
    }
}