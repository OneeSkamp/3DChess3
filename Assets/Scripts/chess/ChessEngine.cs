using System.Collections.Generic;
using board;
using UnityEngine;
using option;

namespace chess {
    public enum FigColor {
        White,
        Black
    }

    public enum FigureType {
        Pawn,
        Knight,
        Bishop,
        Rook,
        Queen,
        King
    }

    public enum MoveType {
        Attack,
        Move
    }

    public enum ChessErr {
        None,
        CantInterpMoveErr,
        PosOutsideBoard,
        BoardIsNull,
        NoFigureOnPos,
        NoSquareFig,
        MaxLengthErr,
        LastLinearPosErr,
        SquarePointsErr,
        FigMovementHasSquareMovement,
    }

    public struct FigMovement {
        public MoveType type;
        public Movement movement;

        public static FigMovement Mk(MoveType type, Movement movement) {
            return new FigMovement { type = type, movement = movement };
        }

        public static FigMovement Linear(MoveType type, Vector2Int dir, int length) {
            var movement = Movement.Linear(LinearMovement.Mk(length, dir));
            return new FigMovement { type = type, movement = movement };
        }

        public static FigMovement Square(MoveType type, int side, int mod) {
            var movement = Movement.Square(
                SquareMovement.Mk(side, new SquareHoles { mod = mod })
            );
            return new FigMovement { type = type, movement = movement };
        }
    }

    public struct FixedMovement {
        public Vector2Int start;
        public FigMovement figMovement;

        public static FixedMovement Mk(Vector2Int start, FigMovement figMovement) {
            return new FixedMovement { start = start, figMovement = figMovement };
        }
    }

    public struct FigLoc {
        public Vector2Int pos;
        public Option<Fig>[,] board;

        public static FigLoc Mk(Vector2Int pos, Option<Fig>[,] board) {
            return new FigLoc { pos = pos, board = board };
        }
    }

    public struct Fig {
        public FigColor color;
        public int counter;
        public FigureType type;

        public static Fig CreateFig(FigColor color, FigureType type) {
            return new Fig {
                color = color,
                type = type
            };
        }
    }

    public struct MoveInfo {
        public DoubleMove move;
        public Vector2Int? sentenced;
        public Vector2Int? promote;
    }

    public struct DoubleMove {
        public Move? first;
        public Move? second;

        public static DoubleMove Mk(Move? first, Move? second) {
            return new DoubleMove { first = first, second = second };
        }
    }

    public struct Move {
        public Vector2Int from;
        public Vector2Int to;

        public static Move Mk(Vector2Int from, Vector2Int to) {
            return new Move { from = from, to = to };
        }
    }

    public static class ChessEngine {
        public static List<FigMovement> CorrectFigMovementsLength(
            FigLoc figLoc,
            List<FigMovement> baseFigMovements
        ) {
            var figMovements = new List<FigMovement>();
            foreach (var baseFigMovement in baseFigMovements) {
                if (baseFigMovement.movement.square.HasValue) {
                    return baseFigMovements;
                }

                if (baseFigMovement.movement.linear.HasValue) {
                    var figMovement = new FigMovement();
                    var linear = baseFigMovement.movement.linear.Value;
                    var length = linear.length;

                    var (reslength, err) = BoardEngine.GetLenUntilFig(
                        figLoc.pos,
                        linear,
                        figLoc.board,
                        length
                    );

                    if (reslength == 0) {
                        continue;
                    }

                    var lastLinearPoint = BoardEngine.GetLinearPoint(
                        figLoc.pos,
                        linear,
                        reslength
                    );

                    if (baseFigMovement.type == MoveType.Move) {
                        if (BoardEngine.IsOnBoard(lastLinearPoint, figLoc.board)) {
                            if (figLoc.board[lastLinearPoint.x, lastLinearPoint.y].IsSome()) {
                                reslength--;
                            }
                        }
                    }

                    if (baseFigMovement.type == MoveType.Attack) {
                        if (BoardEngine.IsOnBoard(lastLinearPoint, figLoc.board)) {
                            if (figLoc.board[lastLinearPoint.x, lastLinearPoint.y].IsNone()) {
                                reslength--;
                            }
                        }
                    }

                    figMovement = FigMovement.Mk(
                        baseFigMovement.type,
                        Movement.Linear(LinearMovement.Mk(reslength, linear.dir))
                    );

                    figMovements.Add(figMovement);
                }
            }

            return figMovements;
        }

        public static bool IsPossibleMove(Move move, Option<Fig>[,] board) {
            var fromPos = move.from;
            var figOpt = board[fromPos.x, fromPos.y];

            if (figOpt.IsNone()) {
                return false;
            }

            var fig = figOpt.Peel();

            if (BoardEngine.IsOnBoard(move.to, board)) {
                var nextFigOpt = board[move.to.x, move.to.y];

                if (nextFigOpt.IsNone()) {
                    return true;
                }

                var nextFig = nextFigOpt.Peel();
                if (fig.color != nextFig.color) {
                    return true;
                }
            }
            return false;
        }
    }
}