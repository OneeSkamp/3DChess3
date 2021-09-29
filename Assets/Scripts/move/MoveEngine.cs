using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;
using movements;

namespace move {
    public enum MoveError {
        BoardIsNull,
        PosOutsideBoard,
        PathIsNull,
        IncorrectLength,
        NoFigureOnPos,
        FigureIsNotKing,
        FigureIsNotPawn
    }
    public static class MoveEngine {
        public static Result<List<MoveInfo>, MoveError> GetPathMoves(
            FigLoc startLoc,
            List<Vector2Int> movePath,
            MoveInfo lastMove
        ) {
            if (startLoc.board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            if (movePath == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PathIsNull);
            }

            if (!BoardEngine.IsOnBoard(startLoc.pos, startLoc.board)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PosOutsideBoard);
            }

            var possMoves = new List<MoveInfo>();
            foreach (var cell in movePath) {
                var move = Move.Mk(startLoc.pos, cell);
                var moveInfo = new MoveInfo();

                if (ChessEngine.IsPossibleMove(move, startLoc.board)) {
                    var fig = startLoc.board[move.from.x, move.from.y].Peel();
                    if (fig.type == FigureType.Pawn) {
                        if (move.to.x == 0 || move.to.x == 7) {
                            moveInfo.promote = move.to;
                        }

                        if (fig.color == FigColor.Black && move.to.x == 3 && fig.counter == 0) {
                            moveInfo.shadow = new Vector2Int(move.to.x - 1, move.from.y);
                        }

                        if (fig.color == FigColor.Black && move.to.x == 4 && fig.counter == 0) {
                            moveInfo.shadow = new Vector2Int(move.to.x + 1, move.from.y);
                        }

                        if (lastMove.shadow.HasValue && move.to == lastMove.shadow.Value) {
                            moveInfo.sentenced = lastMove.move.first.Value.to;
                        }
                    }

                    if (startLoc.board[move.to.x, move.to.y].IsSome()) {
                        moveInfo.sentenced = move.to;
                    }

                    moveInfo.move = DoubleMove.Mk(move, null);
                    possMoves.Add(moveInfo);
                }
            }

            return Result<List<MoveInfo>, MoveError>.Ok(possMoves);
        }

        public static Result<List<MoveInfo>, MoveError> GetPossibleLinearMoves(
            FigLoc startLoc,
            FixedMovement fixedMovement,
            MoveInfo lastMove
        ) {
            if (startLoc.board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(startLoc.pos, startLoc.board)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PathIsNull);
            }
            var start = fixedMovement.start;
            var dir = fixedMovement.movement.linear.Value.dir;
            var length = fixedMovement.movement.linear.Value.length;

            if (length < 0) {
                length = BoardEngine.GetLinearLength(start, dir, startLoc.board);
            }
            var linear = fixedMovement.movement.linear.Value;
            var linearPath = BoardEngine.GetLinearPathToFigure<Fig>(
                fixedMovement.start,
                linear,
                startLoc.board
            );

            var lastPos = BoardEngine.GetLastOnPathPos(start, linear, startLoc.board);
            var movePath = new List<Vector2Int>();
            var resultPath = new List<Vector2Int>();
            movePath = linearPath;

            if (fixedMovement.movement.moveType == MoveType.Move) {
                if (lastPos != null) {
                    if (startLoc.board[lastPos.Value.x, lastPos.Value.y].IsSome()){
                        movePath.Remove(lastPos.Value);
                    }
                }

                resultPath.AddRange(movePath);
            }

            if (fixedMovement.movement.moveType == MoveType.Attack) {
                if (lastPos != null) {
                    if (startLoc.board[lastPos.Value.x, lastPos.Value.y].IsSome()) {
                        resultPath.Add(lastPos.Value);
                    }
                }

                var figOpt = startLoc.board[startLoc.pos.x, startLoc.pos.y];
                if (figOpt.IsSome()) {
                    var fig = figOpt.Peel();

                    if (fig.type == FigureType.Pawn) {
                        if (lastMove.shadow.HasValue && lastPos == lastMove.shadow.Value) {
                            resultPath.Add(lastMove.shadow.Value);
                        }
                    }
                }
            }

            var linearMoves = GetPathMoves(startLoc, resultPath, lastMove).AsOk();
            return Result<List<MoveInfo>, MoveError>.Ok(linearMoves); 
        }

        public static Result<List<MoveInfo>, MoveError> GetMoves(
            FigLoc figLoc,
            List<Movement> movements,
            MoveInfo lastMove
        ) {
            if (figLoc.board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(figLoc.pos, figLoc.board)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PosOutsideBoard);
            }

            var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            if (figOpt.IsNone()) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.NoFigureOnPos);
            }

            var figMoves = new List<MoveInfo>();
            var fig = figOpt.Peel();
            foreach (Movement movement in movements) {
                if (movement.square.HasValue) {
                    var squarePath = BoardEngine.GetSquarePath(
                        figLoc.pos,
                        movement.square.Value
                    );

                    var square = new List<Vector2Int>();
                    if (fig.type == FigureType.Knight) {
                        square = BoardEngine.RemoveSquareParts(squarePath, 0, 1);
                    }

                    if (fig.type == FigureType.King) {
                        square = BoardEngine.RemoveSquareParts(squarePath, 0, 0);
                    }

                    var pathMovesRes = GetPathMoves(figLoc, square, lastMove);
                    if (pathMovesRes.IsErr()) {
                        return Result<List<MoveInfo>, MoveError>.Err(pathMovesRes.AsErr());
                    }

                    figMoves.AddRange(pathMovesRes.AsOk());
                } else {
                    var fixedMovement = FixedMovement.Mk(figLoc.pos, movement);

                    var possLinearMovesRes = GetPossibleLinearMoves(
                        figLoc,
                        fixedMovement,
                        lastMove
                    );

                    if (possLinearMovesRes.IsErr()) {
                        return Result<List<MoveInfo>, MoveError>.Err(possLinearMovesRes.AsErr());
                    }

                    figMoves.AddRange(possLinearMovesRes.AsOk());
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

        public static Result<bool, MoveError> IsUnderAttackPos(
            FigLoc figLoc,
            FigColor color,
            MoveInfo lastMove
        ) {
            if (figLoc.board == null) {
                return Result<bool, MoveError>.Err(MoveError.BoardIsNull);
            }

            var figMoves = new List<MoveInfo>();
            var hasFig = true;
            var movements = Movements.movements;
            var currentFigOpt = new Option<Fig>();

            var fig = Fig.CreateFig(color, FigureType.Knight);

            if (figLoc.board[figLoc.pos.x, figLoc.pos.y].IsNone()) {
                hasFig = false;
            } else {
                currentFigOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
            }
            figLoc.board[figLoc.pos.x, figLoc.pos.y] = Option<Fig>.Some(fig);

            var queen = movements[FigureType.Queen];
            var knight = movements[FigureType.Knight];

            var queenMovesRes = MoveEngine.GetMoves(figLoc, queen, lastMove);
            if (queenMovesRes.IsErr()) {
                return Result<bool, MoveError>.Err(queenMovesRes.AsErr());
            }
            var moves = new List<MoveInfo>();

            moves.AddRange(queenMovesRes.AsOk());

            var knightMovesRes = MoveEngine.GetMoves(figLoc, knight, lastMove);
            if (knightMovesRes.IsErr()) {
                return Result<bool, MoveError>.Err(knightMovesRes.AsErr());
            }
            moves.AddRange(knightMovesRes.AsOk());

            foreach (var move in moves) {
                var to = move.move.first.Value.to;
                if (BoardEngine.IsOnBoard(new Vector2Int(to.x, to.y), figLoc.board)) {
                    var figOpt = figLoc.board[to.x, to.y];

                    if (figOpt.IsSome()) {
                        var figure = figOpt.Peel();
                        var dFigLoc = FigLoc.Mk(move.move.first.Value.to, figLoc.board);
                        var dmovesRes = MoveEngine.GetMoves(
                            dFigLoc,
                            movements[figOpt.Peel().type],
                            lastMove
                        );

                        if (dmovesRes.IsErr()) {
                            return Result<bool, MoveError>.Err(dmovesRes.AsErr());
                        }

                        var dmoves = dmovesRes.AsOk();
                        figMoves.AddRange(dmoves);
                    }
                }
            }

            if (!hasFig) {
                figLoc.board[figLoc.pos.x, figLoc.pos.y] = Option<Fig>.None();
            } else {
                figLoc.board[figLoc.pos.x, figLoc.pos.y] = currentFigOpt;
            }

            foreach (var move in figMoves) {
                if (move.move.first.Value.to == figLoc.pos) {
                    return Result<bool, MoveError>.Ok(true);
                }
            }

            return Result<bool, MoveError>.Ok(false);
        }
    }
}