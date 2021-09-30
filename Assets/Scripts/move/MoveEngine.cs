using System.Collections.Generic;
using UnityEngine;
using option;
using board;
using chess;
using movements;

namespace move {
    public enum MoveError {
        MovementNotСontainLinear,
        MovementNotСontainSquare
    }

    public class MoveEngine {
        public static List<FigMovement> GetFigMovements(FigLoc figLoc) {
            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            var fig = figOpt.Peel();
            var movements = Movements.movements[fig.type];
            var newFigMovements = new List<FigMovement>();

            foreach (var figMovement in movements) {
                var length = figMovement.movement.linear.Value.length;
                // if (length < 0) {
                //     length 
                // }

                if (fig.type == FigureType.Pawn) {
                    var dir = figMovement.movement.linear.Value.dir;

                    if (figMovement.type == MoveType.Move && fig.counter == 0) {
                        length = 2;
                    }

                    if (fig.color == FigColor.Black) {
                        dir = dir * -1;
                    }

                    newFigMovements.Add(
                        FigMovement.Mk(
                            figMovement.type,
                            Movement.Linear(LinearMovement.Mk(length, dir))
                        )
                    );
                }

                return newFigMovements;
            }

            return movements;
        }
        public static Result<List<MoveInfo>, MoveError> GetLinearMoves(
            FixedMovement fixedMovement,
            Option<Fig>[,] board
        ) {
            if (fixedMovement.figMovement.movement.square.HasValue) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.MovementNotСontainLinear);
            }

            var linear = fixedMovement.figMovement.movement.linear.Value;
            var pos = fixedMovement.start;
            var linearMoves = new List<MoveInfo>();
            for (int i = 1; i <= linear.length; i++) {
                var to = pos + i * linear.dir;
                var move = Move.Mk(pos, to);
                if (ChessEngine.IsPossibleMove(move, board)) {
                    linearMoves.Add(new MoveInfo { move = DoubleMove.Mk(move, null)});
                }
            }

            return Result<List<MoveInfo>, MoveError>.Ok(linearMoves);
        }

        public static Result<List<MoveInfo>, MoveError> GetSquareMoves(
            FixedMovement fixedMovement,
            Option<Fig>[,] board
        ) {
            if (fixedMovement.figMovement.movement.linear.HasValue) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.MovementNotСontainSquare);
            }

            var squarePoints = ChessEngine.GetRealSquarePoints(fixedMovement, board);
            var pos = fixedMovement.start;
            var squareMoves = new List<MoveInfo>();
            foreach (var point in squarePoints) {
                var to = point;
                var move = Move.Mk(pos, to);

                if (ChessEngine.IsPossibleMove(move, board)) {
                    squareMoves.Add(new MoveInfo { move = DoubleMove.Mk(move, null) });
                }
            }

            return Result<List<MoveInfo>, MoveError>.Ok(squareMoves);
        }

        public static Result<List<MoveInfo>, MoveError> GetFigMoves(FigLoc figLoc) {
            var figMoves = new List<MoveInfo>();
            var fig = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            var figMovements = GetFigMovements(figLoc);

            foreach (var figMovement in figMovements) {
                var fixedMovement = FixedMovement.Mk(figLoc.pos, figMovement);
                if (figMovement.movement.linear.HasValue) {
                    var linearMovesRes = GetLinearMoves(fixedMovement, figLoc.board);
                    if (linearMovesRes.IsErr()) {
                        return Result<List<MoveInfo>, MoveError>.Err(linearMovesRes.AsErr());
                    }

                    var linearMoves = linearMovesRes.AsOk();
                    figMoves.AddRange(linearMoves);
                } else if (figMovement.movement.square.HasValue) {
                    var squareMovesRes = GetSquareMoves(fixedMovement, figLoc.board);
                    if (squareMovesRes.IsErr()) {
                        return Result<List<MoveInfo>, MoveError>.Err(squareMovesRes.AsErr());
                    }

                    var squareMoves = squareMovesRes.AsOk();
                    figMoves.AddRange(squareMoves);
                }
            }

            return Result<List<MoveInfo>, MoveError>.Ok(figMoves);
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
