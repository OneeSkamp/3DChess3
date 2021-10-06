using System.Collections.Generic;
using UnityEngine;
using option;
using board;
using chess;

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

    public struct SegmentInfo {
        public Vector2Int start;
        public Vector2Int normal;

        public static SegmentInfo Mk(Vector2Int start, Vector2Int normal) {
            return new SegmentInfo { start = start, normal = normal };
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

        public static SegmentInfo GetSegmentInfo(Vector2Int point1, Vector2Int point2) {
            return SegmentInfo.Mk(
                point1,
                new Vector2Int(point2.y - point1.y, point2.x - point1.x)
            );
        }

        public static Option<Vector2Int> GetIntersectionPoint(SegmentInfo segment1, SegmentInfo segment2) {
            var A1 = segment1.normal.x;
            var A2 = segment2.normal.x;
            var B1 = segment1.normal.y;
            var B2 = segment2.normal.y;
            var x1 = segment1.start.x;
            var y1 = segment1.start.y;
            var x2 = segment2.start.x;
            var y2 = segment2.start.y;

            if (segment1.normal == segment2.normal) {
                return Option<Vector2Int>.None();
            }

            var y = (A1*A2*x2 - A2*A1*x1 + A2*B1*y1 - A1*B2*y2) / (A2*B1 - A1*B2);
            var x = (A1*x1 + B1*y - B1*y1) / A1;

            var result = new Vector2Int(x, y);

            return Option<Vector2Int>.Some(result);
        }

        public static List<Vector2Int> GetDefPoints(FigLoc figLoc) {
            var (potencialFigMovements, err) = GetPotentialLineFigMovements(figLoc);
            var (linearAttackInfos, error) = GetLinearAttackInfo(figLoc);

            // foreach (var attackLinear )
            return null;
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

        public static (List<FigMovement>, MoveErr) GetPotentialLineFigMovements(FigLoc figLoc) {
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
                    figMovements = new List<FigMovement> {
                        FigMovement.Linear(MoveType.Move, new Vector2Int(1, 1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(-1, 1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(1, -1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(-1, -1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(1, 1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(-1, 1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(1, -1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(-1, -1), -1)
                    };
                    break;
                case FigureType.Rook:
                    figMovements = new List<FigMovement> {
                        FigMovement.Linear(MoveType.Move, new Vector2Int(0, 1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(0, -1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(1, 0), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(-1, 0), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(0, 1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(0, -1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(1, 0), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(-1, 0), -1)
                    };
                    break;
                case FigureType.Knight:
                    figMovements = new List<FigMovement> {
                        FigMovement.Square(MoveType.Attack, 5)
                    };
                    break;
                case FigureType.King:
                    figMovements = new List<FigMovement> {
                        FigMovement.Square(MoveType.Attack, 3)
                    };
                    break;
                case FigureType.Queen:
                    figMovements = new List<FigMovement> {
                        FigMovement.Linear(MoveType.Move, new Vector2Int(1, 1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(-1, 1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(1, -1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(-1, -1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(0, 1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(0, -1), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(1, 0), -1),
                        FigMovement.Linear(MoveType.Move, new Vector2Int(-1, 0), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(1, 1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(-1, 1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(1, -1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(-1, -1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(0, 1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(0, -1), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(1, 0), -1),
                        FigMovement.Linear(MoveType.Attack, new Vector2Int(-1, 0), -1)
                    };
                    break;
            }
            figMovements = ChessEngine.CorrectFigMovementsLength(figLoc, figMovements);

            return (figMovements, MoveErr.None);
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