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
            Vector2Int start,
            List<Vector2Int> movePath,
            MoveInfo lastMove,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            if (movePath == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PathIsNull);
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (!BoardEngine.IsOnBoard(start, size)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PosOutsideBoard);
            }

            var possMoves = new List<MoveInfo>();
            foreach (var cell in movePath) {
                var move = Move.Mk(start, cell);
                var moveInfo = new MoveInfo();

                if (ChessEngine.IsPossibleMove(move, board)) {
                    var fig = board[move.from.x, move.from.y].Peel();
                    if (fig.type == FigureType.Pawn) {
                        if (move.to.x == 0 || move.to.x == 7) {
                            moveInfo.promote = move.to;
                        }
                    }

                    if (board[move.to.x, move.to.y].IsSome()) {
                        moveInfo.sentenced = move.to;
                    }

                    moveInfo.move = DoubleMove.Mk(move, null);
                    possMoves.Add(moveInfo);
                }
            }
            if (GetCastlingMoves(start, lastMove, board).AsOk() != null) {
                possMoves.AddRange(GetCastlingMoves(start, lastMove, board).AsOk());
            }

            return Result<List<MoveInfo>, MoveError>.Ok(possMoves);
        }

        public static Result<List<MoveInfo>, MoveError> GetPossibleLinearMoves(
            Vector2Int start,
            LinearMovement linear,
            MoveInfo lastMove,
            int length,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            if (length < 0) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.IncorrectLength);
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (!BoardEngine.IsOnBoard(start, size)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PathIsNull);
            }

            var linearPath = BoardEngine.GetLinearPath<Fig>(start, linear.dir, length, board);

            var linearMoves = GetPathMoves(start, linearPath, lastMove, board).AsOk();
            return Result<List<MoveInfo>, MoveError>.Ok(linearMoves); 
        }

        public static Result<List<MoveInfo>, MoveError> GetMoves(
            Vector2Int pos,
            List<Movement> movements,
            MoveInfo lastMove,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (!BoardEngine.IsOnBoard(pos, size)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PosOutsideBoard);
            }

            var figOpt = board[pos.x, pos.y];
            if (figOpt.IsNone()) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.NoFigureOnPos);
            }

            var figMoves = new List<MoveInfo>();
            var fig = figOpt.Peel();
            foreach (Movement type in movements) {
                if (type.square.HasValue) {
                    var squarePath = BoardEngine.GetSquarePath(pos, type.square.Value.side);
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

                    figMoves.AddRange(GetPathMoves(pos, list, lastMove, board).AsOk());

                } else {
                    var linear = type.linear.Value;
                    var length = BoardEngine.GetLinearLength(pos, linear.dir, board);
                    figMoves.AddRange(GetPossibleLinearMoves(
                        pos,
                        linear,
                        lastMove,
                        length,
                        board
                    ).AsOk());
                }
            }

            if (figOpt.Peel().type == FigureType.Pawn) {
                figMoves = GetPawnMoves(pos, figMoves, lastMove, board).AsOk();
            }

            return Result<List<MoveInfo>, MoveError>.Ok(figMoves);
        }

        public static Result<List<MoveInfo>, MoveError> GetPawnMoves(
            Vector2Int pos,
            List<MoveInfo> moves,
            MoveInfo lastMove,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.BoardIsNull);
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (!BoardEngine.IsOnBoard(pos, size)) {
                return Result<List<MoveInfo>, MoveError>.Err(MoveError.PosOutsideBoard);
            }

            var pawnPath = new List<MoveInfo>();
            var pawn = board[pos.x, pos.y].Peel();
            int prop = 1;
            var length = 1;

            if (pawn.white) {
                prop = -1;
            }

            var leftPos = new Vector2Int(pos.x + prop, pos.y - prop);
            var rightPos = new Vector2Int(pos.x + prop, pos.y + prop);

            if (pawn.counter == 0) {
                length = 2;
            }
            var forwardPos = new Vector2Int(pos.x + 1 * prop, pos.y);
            var nextForwardPos = new Vector2Int(pos.x + 2 * prop, pos.y);

            var forwardPath = BoardEngine.GetLinearPath(pos, forwardPos, length, board);

            foreach (var cell in moves) {
                var nextCell = cell.move.first.Value.to;
                if (!BoardEngine.IsOnBoard(nextCell, size)) {
                    continue;
                }

                if (board[forwardPos.x, forwardPos.y].IsSome() && nextCell == forwardPos) {
                    continue;
                }

                if (pawn.counter == 0 && board[forwardPos.x + prop, forwardPos.y].IsNone()
                    && board[forwardPos.x, forwardPos.y].IsNone()) {

                    if (nextCell == new Vector2Int(forwardPos.x + prop, forwardPos.y)) {
                        pawnPath.Add(cell);
                    }
                }

                if (board[forwardPos.x, forwardPos.y].IsNone() && nextCell == forwardPos) {
                    pawnPath.Add(cell);
                }

                if (rightPos == nextCell && board[rightPos.x, rightPos.y].IsSome()) {
                    pawnPath.Add(cell);
                }

                if (leftPos == nextCell && board[leftPos.x, leftPos.y].IsSome()) {
                    pawnPath.Add(cell);
                }
            }
            pawnPath.AddRange(GetEnPassantMoves(pos, lastMove, board).AsOk());
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

                if (leftFig.type == FigureType.Rook && leftFig.counter == 0) {
                    var move = DoubleMove.Mk(
                        Move.Mk(kingPos, new Vector2Int(kingPos.x, kingPos.y - 2)),
                        Move.Mk(leftPos, new Vector2Int(kingPos.x, kingPos.y - 1))
                    );

                    var firstTo = move.first.Value.to;
                    var secondTo = move.second.Value.to;
                    if (!IsUnderAttackPos(firstTo, fig.white, lastMove, board).AsOk()
                        && !IsUnderAttackPos(secondTo, fig.white, lastMove, board).AsOk()) {
                            castlingMoves.Add(new MoveInfo { move = move });
                        }
                }

                if (rightFig.type == FigureType.Rook && rightFig.counter == 0) {
                    var move = DoubleMove.Mk(
                        Move.Mk(kingPos, new Vector2Int(kingPos.x, kingPos.y + 2)),
                        Move.Mk(rightPos, new Vector2Int(kingPos.x, kingPos.y + 1))
                    );

                    var firstTo = move.first.Value.to;
                    var secondTo = move.second.Value.to;
                    if (!IsUnderAttackPos(firstTo, fig.white, lastMove, board).AsOk() 
                        && !IsUnderAttackPos(secondTo, fig.white, lastMove, board).AsOk()) {
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
                if (fig.white && pawnPos.x == 3) {
                    prop = -1;
                }

                if (!fig.white && pawnPos.x == 4) {
                    prop = 1;
                }

                if (prop == 1 || prop == -1) {
                    var leftFig = board[leftPos.x, leftPos.y].Peel();
                    var rightFig = board[rightPos.x, rightPos.y].Peel();
                    if (leftFig.type == FigureType.Pawn && leftFig.white != fig.white
                        && leftFig.counter == 1 && lastMove.move.first.Value.to == leftPos
                    ) {
                        var newPos = new Vector2Int(leftPos.x + prop, leftPos.y);
                        var move = DoubleMove.Mk(Move.Mk(pawnPos, newPos), null);

                        enPassantMoves.Add(new MoveInfo { move = move, sentenced = leftPos });
                    }

                    if (rightFig.type == FigureType.Pawn && rightFig.white != fig.white
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
            Vector2Int pos,
            bool white,
            MoveInfo lastMove,
            Option<Fig>[,] board
        ) {
            if (board == null) {
                return Result<bool, MoveError>.Err(MoveError.BoardIsNull);
            }

            var figMoves = new List<MoveInfo>();
            var hasFig = true;
            var movements = Movements.movements;
            var currentFigOpt = new Option<Fig>();

            var fig = Fig.CreateFig(white, FigureType.Knight);

            if (board[pos.x, pos.y].IsNone()) {
                hasFig = false;
            } else {
                currentFigOpt = board[pos.x, pos.y];
            }
            board[pos.x, pos.y] = Option<Fig>.Some(fig);

            var queen = movements[FigureType.Queen];
            var knight = movements[FigureType.Knight];
            var moves = MoveEngine.GetMoves(pos, queen, lastMove, board).AsOk();
            moves.AddRange(MoveEngine.GetMoves(pos, knight, lastMove, board).AsOk());

            foreach (var move in moves) {
                var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
                var to = move.move.first.Value.to;
                if (BoardEngine.IsOnBoard(new Vector2Int(to.x, to.y), size)) {
                    var figOpt = board[to.x, to.y];

                    if (figOpt.IsSome()) {
                        var figure = figOpt.Peel();
                        var dmoves = MoveEngine.GetMoves(
                            move.move.first.Value.to,
                            movements[board[to.x, to.y].Peel().type],
                            lastMove,
                            board
                        ).AsOk();
                        figMoves.AddRange(dmoves);
                    }
                }
            }

            if (!hasFig) {
                board[pos.x, pos.y] = Option<Fig>.None();
            } else {
                board[pos.x, pos.y] = currentFigOpt;
            }

            foreach (var move in figMoves) {
                if (move.move.first.Value.to == pos) {
                    return Result<bool, MoveError>.Ok(true);
                }
            }

            return Result<bool, MoveError>.Ok(false);
        }
    }
}