using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;
using collections;
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
                    }

                    if (startLoc.board[move.to.x, move.to.y].IsSome()) {
                        moveInfo.sentenced = move.to;
                    }

                    moveInfo.move = DoubleMove.Mk(move, null);
                    possMoves.Add(moveInfo);
                }
            }
            if (GetCastlingMoves(startLoc.pos, lastMove, startLoc.board).AsOk() != null) {
                possMoves.AddRange(GetCastlingMoves(startLoc.pos, lastMove, startLoc.board).AsOk());
            }

            return Result<List<MoveInfo>, MoveError>.Ok(possMoves);
        }

        public static Result<List<MoveInfo>, MoveError> GetPossibleLinearMoves(
            FigLoc startLoc,
            LinearMovement linear,
            MoveInfo lastMove,
            int length
        ) {
            if (startLoc.board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            if (length < 0) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.IncorrectLength);
            }

            if (!BoardEngine.IsOnBoard(startLoc.pos, startLoc.board)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PathIsNull);
            }

            var linearPath = BoardEngine.GetLinearPath<Fig>(
                startLoc.pos,
                linear.dir,
                length,
                startLoc.board
            );

            var linearMoves = GetPathMoves(startLoc, linearPath, lastMove).AsOk();
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
            foreach (Movement type in movements) {
                if (type.square.HasValue) {
                    var squarePath = BoardEngine.GetSquarePath(figLoc.pos, type.square.Value.side);
                    var square = new BindableList<Vector2Int>();

                    if (fig.type == FigureType.Knight) {
                        square = BoardEngine.RemoveSquareParts(squarePath, 0, 1);
                    }

                    if (fig.type == FigureType.King) {
                        square = BoardEngine.RemoveSquareParts(squarePath, 0, 0);
                    }

                    var list = new List<Vector2Int>();
                    if (list != null) {
                        foreach (var i in square) {
                            if (i != null) {
                                list.Add(i.value);
                            }
                        }
                    }

                    figMoves.AddRange(GetPathMoves(figLoc, list, lastMove).AsOk());

                } else {
                    var linear = type.linear.Value;
                    var length = BoardEngine.GetLinearLength(figLoc.pos, linear.dir, figLoc.board);
                    figMoves.AddRange(GetPossibleLinearMoves(
                        figLoc,
                        linear,
                        lastMove,
                        length
                    ).AsOk());
                }
            }

            if (figOpt.Peel().type == FigureType.Pawn) {
                figMoves = GetPawnMoves(figLoc, figMoves, lastMove).AsOk();
            }

            return Result<List<MoveInfo>, MoveError>.Ok(figMoves);
        }

        public static Result<List<MoveInfo>, MoveError> GetPawnMoves(
            FigLoc figLoc,
            List<MoveInfo> moves,
            MoveInfo lastMove
        ) {
            if (figLoc.board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            if (!BoardEngine.IsOnBoard(figLoc.pos, figLoc.board)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PosOutsideBoard);
            }

            var pawnPath = new List<MoveInfo>();
            var pawn = figLoc.board[figLoc.pos.x, figLoc.pos.y].Peel();
            int prop = 1;
            var length = 1;

            if (pawn.color == FigColor.White) {
                prop = -1;
            }

            var leftPos = new Vector2Int(figLoc.pos.x + prop, figLoc.pos.y - prop);
            var rightPos = new Vector2Int(figLoc.pos.x + prop, figLoc.pos.y + prop);

            if (pawn.counter == 0) {
                length = 2;
            }
            var forwardPos = new Vector2Int(figLoc.pos.x + 1 * prop, figLoc.pos.y);
            var nextForwardPos = new Vector2Int(figLoc.pos.x + 2 * prop, figLoc.pos.y);

            var forwardPath = BoardEngine.GetLinearPath(
                figLoc.pos,
                forwardPos,
                length,
                figLoc.board
            );

            foreach (var cell in moves) {
                var nextCell = cell.move.first.Value.to;
                if (!BoardEngine.IsOnBoard(nextCell, figLoc.board)) {
                    continue;
                }

                if (figLoc.board[forwardPos.x, forwardPos.y].IsSome() && nextCell == forwardPos) {
                    continue;
                }

                if (pawn.counter == 0 && figLoc.board[forwardPos.x + prop, forwardPos.y].IsNone()
                    && figLoc.board[forwardPos.x, forwardPos.y].IsNone()) {

                    if (nextCell == new Vector2Int(forwardPos.x + prop, forwardPos.y)) {
                        pawnPath.Add(cell);
                    }
                }

                if (figLoc.board[forwardPos.x, forwardPos.y].IsNone() && nextCell == forwardPos) {
                    pawnPath.Add(cell);
                }

                if (rightPos == nextCell && figLoc.board[rightPos.x, rightPos.y].IsSome()) {
                    pawnPath.Add(cell);
                }

                if (leftPos == nextCell && figLoc.board[leftPos.x, leftPos.y].IsSome()) {
                    pawnPath.Add(cell);
                }
            }
            pawnPath.AddRange(GetEnPassantMoves(figLoc.pos, lastMove, figLoc.board).AsOk());
            return Result<List<MoveInfo>, MoveError>.Ok(pawnPath);
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

        public static Result<List<MoveInfo>, MoveError> GetCastlingMoves(
            Vector2Int kingPos,
            MoveInfo lastMove,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            var figOpt = board[kingPos.x, kingPos.y];
            if (figOpt.IsNone()) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.NoFigureOnPos);
            }

            var fig = figOpt.Peel();
            if (fig.type != FigureType.King) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.FigureIsNotKing);
            }

            var castlingMoves = new List<MoveInfo>();

            var leftDir = new Vector2Int(0, -1);
            var rightDir = new Vector2Int(0, 1);

            var leftLength = BoardEngine.GetLinearLength(kingPos, leftDir, board);
            var rightLength = BoardEngine.GetLinearLength(kingPos, rightDir, board);

            var leftPath = BoardEngine.GetLinearPath(kingPos, leftDir, leftLength, board);
            var rightPath = BoardEngine.GetLinearPath(kingPos, rightDir, rightLength, board);

            var leftPos = new Vector2Int();
            var rightPos = new Vector2Int();

            if (leftPath.Count > 0) {
                leftPos = leftPath[leftPath.Count - 1];
            }

            if (rightPath.Count > 0) {
                rightPos = rightPath[rightPath.Count - 1];
            }

            if (fig.type == FigureType.King && fig.counter == 0) {
                var leftFig = board[leftPos.x, leftPos.y].Peel();
                var rightFig = board[rightPos.x, rightPos.y].Peel();

                var move = new DoubleMove();
                if (leftFig.type == FigureType.Rook && leftFig.counter == 0) {
                    move = DoubleMove.Mk(
                        Move.Mk(kingPos, new Vector2Int(kingPos.x, kingPos.y - 2)),
                        Move.Mk(leftPos, new Vector2Int(kingPos.x, kingPos.y - 1))
                    );

                    var firstTo = move.first.Value.to;
                    var secondTo = move.second.Value.to;
                    var firstToLoc = FigLoc.Mk(firstTo, board);
                    var secondToLoc = FigLoc.Mk(secondTo, board);
                    var firstToIsUnderAttack = IsUnderAttackPos(firstToLoc, fig.color, lastMove);
                    if (firstToIsUnderAttack.IsErr()) {
                        return Result<List<MoveInfo>, MoveError>.Err(firstToIsUnderAttack.AsErr());
                    }

                    var secondToIsUnderAttack = IsUnderAttackPos(secondToLoc, fig.color, lastMove);
                    if (secondToIsUnderAttack.IsErr()) {
                        return Result<List<MoveInfo>, MoveError>.Err(secondToIsUnderAttack.AsErr());
                    }
                    if (!firstToIsUnderAttack.AsOk() && !secondToIsUnderAttack.AsOk()) {
                        castlingMoves.Add(new MoveInfo { move = move });
                    }
                }

                if (rightFig.type == FigureType.Rook && rightFig.counter == 0) {
                    move = DoubleMove.Mk(
                        Move.Mk(kingPos, new Vector2Int(kingPos.x, kingPos.y + 2)),
                        Move.Mk(rightPos, new Vector2Int(kingPos.x, kingPos.y + 1))
                    );

                    var firstTo = move.first.Value.to;
                    var secondTo = move.second.Value.to;
                    var firstToLoc = FigLoc.Mk(firstTo, board);
                    var secondToLoc = FigLoc.Mk(secondTo, board);
                    var firstToIsUnderAttack = IsUnderAttackPos(firstToLoc, fig.color, lastMove);
                    if (firstToIsUnderAttack.IsErr()) {
                        return Result<List<MoveInfo>, MoveError>.Err(firstToIsUnderAttack.AsErr());
                    }

                    var secondToIsUnderAttack = IsUnderAttackPos(secondToLoc, fig.color, lastMove);
                    if (secondToIsUnderAttack.IsErr()) {
                        return Result<List<MoveInfo>, MoveError>.Err(secondToIsUnderAttack.AsErr());
                    }
                    if (!firstToIsUnderAttack.AsOk() && !secondToIsUnderAttack.AsOk()) {
                            castlingMoves.Add(new MoveInfo { move = move });
                    }
                }
            }

            return Result<List<MoveInfo>, MoveError>.Ok(castlingMoves);
        }

        public static Result<List<MoveInfo>, MoveError> GetEnPassantMoves(
        Vector2Int pawnPos,
        MoveInfo lastMove,
        Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            var figOpt = board[pawnPos.x, pawnPos.y];
            if (figOpt.IsNone()) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.NoFigureOnPos);
            }

            var fig = figOpt.Peel();
            if (fig.type != FigureType.Pawn) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.FigureIsNotPawn);
            }

            var enPassantMoves = new List<MoveInfo>();
            var leftDir = new Vector2Int(0, -1);
            var rightDir = new Vector2Int(0, 1);

            var leftLength = BoardEngine.GetLinearLength(pawnPos, leftDir, board);
            var rightLength = BoardEngine.GetLinearLength(pawnPos, rightDir, board);

            var leftPath = BoardEngine.GetLinearPath(pawnPos, leftDir, leftLength, board);
            var rightPath = BoardEngine.GetLinearPath(pawnPos, rightDir, rightLength, board);

            var leftPos = new Vector2Int();
            var rightPos = new Vector2Int();
            var prop = 0;

            if (leftPath.Count == 1) {
                leftPos = leftPath[0];
            }

            if (rightPath.Count == 1) {
                rightPos = rightPath[0];
            }

            if (fig.type == FigureType.Pawn) {
                if (fig.color == FigColor.White && pawnPos.x == 3) {
                    prop = -1;
                }

                if (fig.color == FigColor.Black && pawnPos.x == 4) {
                    prop = 1;
                }

                if (prop == 1 || prop == -1) {
                    var leftFig = board[leftPos.x, leftPos.y].Peel();
                    var rightFig = board[rightPos.x, rightPos.y].Peel();
                    if (leftFig.type == FigureType.Pawn && leftFig.color != fig.color
                        && leftFig.counter == 1 && lastMove.move.first.Value.to == leftPos
                    ) {
                        var newPos = new Vector2Int(leftPos.x + prop, leftPos.y);
                        var move = DoubleMove.Mk(Move.Mk(pawnPos, newPos), null);

                        enPassantMoves.Add(new MoveInfo { move = move, sentenced = leftPos });
                    }

                    if (rightFig.type == FigureType.Pawn && rightFig.color != fig.color
                        && rightFig.counter == 1 && lastMove.move.first.Value.to == rightPos
                    ) {
                        var newPos = new Vector2Int(rightPos.x + prop, rightPos.y);
                        var move = DoubleMove.Mk(Move.Mk(pawnPos, newPos), null);

                        enPassantMoves.Add(new MoveInfo { move = move, sentenced = rightPos });
                    }
                }
            }

            return Result<List<MoveInfo>, MoveError>.Ok(enPassantMoves);
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
                        var dmoves = MoveEngine.GetMoves(
                            dFigLoc,
                            movements[figLoc.board[to.x, to.y].Peel().type],
                            lastMove
                        ).AsOk();
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