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
        LinearMovesErr,
        SquareMovesErr,
        SquarePointsErr
    }

    public struct KingsPos {
        public Vector2Int black;
        public Vector2Int white;
    }

    public struct AttackInfo {
        public Option<Vector2Int> defPos;
        public Movement attack;
    }

    public class MoveEngine {
        public static Result<Option<Fig>[,], MoveErr> GetBoardWithoutColor(
            Option<Fig>[,] board,
            FigColor color
        ) {
            if (board == null) {
                return Result<Option<Fig>[,], MoveErr>.Err(MoveErr.BoardIsNull);
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

            return Result<Option<Fig>[,], MoveErr>.Ok(boardClone);
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

        // public static Result<List<AttackInfo>, MoveErr> GetAttackInfos(FigLoc figLoc) {
        //     if (figLoc.board == null) {
        //         return Result<List<AttackInfo>, MoveErr>.Err(MoveErr.BoardIsNull);
        //     }

        //     var figOpt = figLoc.board[figLoc.pos.x, figLoc.pos.y];
        //     if (figOpt.IsNone()) {
        //         return Result<List<AttackInfo>, MoveErr>.Err(MoveErr.NoFigureOnPos);
        //     }

        //     var color = figOpt.Peel().color;
        //     var boardCloneRes = GetBoardWithoutColor(figLoc.board, color);
        //     if (boardCloneRes.IsErr()) {
        //         return Result<List<AttackInfo>, MoveErr>.Err(boardCloneRes.AsErr());
        //     }
        //     var boardClone = boardCloneRes.AsOk();

        //     var kingsPosRes = FindKingsPos(figLoc.board);
        //     if (kingsPosRes.IsErr()) {
        //         return Result<List<AttackInfo>, MoveErr>.Err(kingsPosRes.AsErr());
        //     }

        //     var kingPos = kingsPosRes.AsOk().black;
        //     if (color == FigColor.White) {
        //         kingPos = kingsPosRes.AsOk().white;
        //     }


        // }

        public static (List<FigMovement>, MoveErr) GetFigMovements(FigLoc figLoc) {
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
                var (movelengthRes, err) = ChessEngine.GetMoveLength(
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
            FixedMovement fixedMovement,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            if (fixedMovement.figMovement.movement.square.HasValue) {
                return (null, MoveErr.MovementNotСontainLinear);
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

            return (linearMoves, MoveErr.None);
        }

        public static (List<MoveInfo>, MoveErr) GetSquareMoves(
            FixedMovement fixedMovement,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return (null, MoveErr.BoardIsNull);
            }

            if (fixedMovement.figMovement.movement.linear.HasValue) {
                return (null, MoveErr.MovementNotСontainSquare);
            }

            var (squarePointsRes, err) = ChessEngine.GetRealSquarePoints(fixedMovement, board);
            if (err != ChessErr.None) {
                return (null, MoveErr.SquareMovesErr);
            }

            var squarePoints = squarePointsRes;
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
            var (figMovementsRes, err) = GetFigMovements(figLoc);
            if (err != MoveErr.None) {
                return (null, MoveErr.FigMovementsErr);
            }

            var figMovements = figMovementsRes;
            foreach (var figMovement in figMovements) {
                var fixedMovement = FixedMovement.Mk(figLoc.pos, figMovement);
                if (figMovement.movement.linear.HasValue) {
                    var (linearMovesRes, error) = GetLinearMoves(fixedMovement, figLoc.board);
                    if (error != MoveErr.None) {
                        return (null, MoveErr.LinearMovesErr);
                    }

                    var linearMoves = linearMovesRes;
                    figMoves.AddRange(linearMoves);
                } else if (figMovement.movement.square.HasValue) {
                    var (squareMovesRes, error) = GetSquareMoves(fixedMovement, figLoc.board);
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