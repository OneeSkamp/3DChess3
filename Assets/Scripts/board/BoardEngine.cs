using System.Collections.Generic;
using UnityEngine;
using option;

namespace board {
    public enum BoardErr {
        None,
        PosOutsideBoard,
        BoardIsNull,
    }

    public struct LinearMovement {
        public int length;
        public Vector2Int dir;

        public static LinearMovement Mk(int length, Vector2Int dir) {
            return new LinearMovement { length = length, dir = dir };
        }
    }

    public struct SquareMovement {
        public int side;

        public static SquareMovement Mk(int side) {
            return new SquareMovement { side = side };
        }
    }

    public struct Movement {
        public LinearMovement? linear;
        public SquareMovement? square;

        public static Movement Linear(LinearMovement linear) {
            return new Movement { linear = linear };
        }

        public static Movement Square(SquareMovement square) {
            return new Movement { square = square };
        }
    }

    public struct MovementLoc {
        public int index;
        public Option<Vector2Int> pos;

        public static MovementLoc Mk(int index, Option<Vector2Int> pos) {
            return new MovementLoc { index = index, pos = pos };
        }
    }

    public static class BoardEngine {
        public static Result<MovementLoc, BoardErr> GetMovementLoc<T>(
            Vector2Int pos,
            LinearMovement linear,
            Option<T>[,] board
        ) {
            if (board == null) {
                return Result<MovementLoc, BoardErr>.Err(BoardErr.BoardIsNull);
            }

            if (!IsOnBoard(pos, board)) {
                return Result<MovementLoc, BoardErr>.Err(BoardErr.PosOutsideBoard);
            }

            var movementLoc = MovementLoc.Mk(linear.length, Option<Vector2Int>.None());
            for (int i = 1; i <= linear.length; i++) {
                var nextPos = pos + i * linear.dir;
                if (!IsOnBoard(nextPos, board)) {
                    movementLoc = MovementLoc.Mk(i - 1, Option<Vector2Int>.None());
                    return Result<MovementLoc, BoardErr>.Ok(movementLoc);
                }

                if (board[nextPos.x, nextPos.y].IsSome()) {
                    movementLoc = MovementLoc.Mk(i, Option<Vector2Int>.Some(nextPos));
                    return Result<MovementLoc, BoardErr>.Ok(movementLoc);
                }
            }

            return Result<MovementLoc, BoardErr>.Ok(movementLoc);
        }

        public static (int, BoardErr) GetLenUntilFig<T>(
            Vector2Int pos,
            LinearMovement linear,
            Option<T>[,] board
        ) {
            if (board == null) {
                return (-1, BoardErr.BoardIsNull);
            }

            if (!IsOnBoard(pos, board)) {
                return (-1, BoardErr.PosOutsideBoard);
            }

            var maxLength = Mathf.Max(board.GetLength(0), board.GetLength(1));
            var length = 0;

            for (int i = 1; i <= maxLength; i++) {
                var nextPos = pos + i * linear.dir;

                if (!IsOnBoard(nextPos, board)) {
                    break;
                }

                length++;
                if (board[nextPos.x, nextPos.y].IsSome()) {
                    break;
                }
            }

            return (length, BoardErr.None);
        }

        public static List<Vector2Int> GetSquarePoints(Vector2Int pos, SquareMovement square) {
            var squareMoves = new List<Vector2Int>();
            var startPos = new Vector2Int(pos.x - square.side/2, pos.y - square.side/2);
            var nextPos = new Vector2Int();
            var dir = new Vector2Int(1, 0);

            for (int i = 1; i < square.side; i++) {
                nextPos = new Vector2Int(startPos.x, startPos.y + i);
                squareMoves.Add(nextPos);
            }
            startPos = nextPos;

            for (int i = 1; i < square.side; i++) {
                nextPos = new Vector2Int(startPos.x + i, startPos.y);
                squareMoves.Add(nextPos);
            }
            startPos = nextPos;

            for (int i = 1; i < square.side; i++) {
                nextPos = new Vector2Int(startPos.x, startPos.y - i);
                squareMoves.Add(nextPos);
            }
            startPos = nextPos;

            for (int i = 1; i < square.side; i++) {
                nextPos = new Vector2Int(startPos.x - i, startPos.y);
                squareMoves.Add(nextPos);
            }

            return squareMoves;
        }

        public static List<Vector2Int> RemoveSquareParts<T>(
            List<Vector2Int> square,
            int start,
            int skipValue,
            Option<T>[,] board
        ) {
            var newSquare = new List<Vector2Int>();
            for (int i = start; i < square.Count; i = i + 1 + skipValue) {
                if (IsOnBoard(square[i], board)){
                    newSquare.Add(square[i]);
                }
            }

            return newSquare;
        }

        public static Option<T>[,] CopyBoard<T>(Option<T>[,] board) {
            var clone = new Option<T>[board.GetLength(0), board.GetLength(1)];
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    clone[i, j] = board[i, j];
                }
            }

            return clone;
        }

        public static bool IsOnBoard<T>(Vector2Int pos, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            if (IsOnBoard(pos, size)) {
                return true;
            }

            return false;
        }

        public static bool IsOnBoard(Vector2Int pos, Vector2Int size) {
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }
    }
}