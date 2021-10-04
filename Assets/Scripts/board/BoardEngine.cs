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
        public static (MovementLoc, BoardErr) GetLastLinearPoint<T>(
            Vector2Int pos,
            LinearMovement linear,
            Option<T>[,] board
        ) {
            if (board == null) {
                return (new MovementLoc(), BoardErr.BoardIsNull);
            }

            if (!IsOnBoard(pos, board)) {
                return (new MovementLoc(), BoardErr.PosOutsideBoard);
            }

            var movementLoc = MovementLoc.Mk(linear.length, Option<Vector2Int>.None());
            for (int i = 1; i <= linear.length; i++) {
                var nextPos = pos + i * linear.dir;
                if (!IsOnBoard(nextPos, board)) {
                    movementLoc = MovementLoc.Mk(i - 1, Option<Vector2Int>.None());
                    return (movementLoc, BoardErr.None);
                }

                if (board[nextPos.x, nextPos.y].IsSome()) {
                    movementLoc = MovementLoc.Mk(i, Option<Vector2Int>.Some(nextPos));
                    return (movementLoc, BoardErr.None);
                }
            }

            return (movementLoc, BoardErr.None);
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

        public static Option<Vector2Int> GetSquarePoint(Vector2Int pos, SquareMovement square, int index) {
            var point = new Vector2Int();
            var maxIndex = (square.side - 1) * 4;
            if (index < square.side - 1) {
                point.x = pos.x - (square.side - 1) / 2;
                point.y = pos.y - (square.side - 1) / 2 + index;
            } else if (index < (square.side - 1) * 2) {
                point.x = pos.x - 3 * (square.side - 1) / 2 + index;
                point.y = pos.y + (square.side - 1) / 2;
            } else if (index < (square.side - 1) * 3) {
                point.x = pos.x + (square.side - 1) / 2;
                point.y = pos.y + 5 * (square.side - 1) /2 - index;
            } else if (index < maxIndex) {
                point.x = pos.x + 7 * (square.side - 1) / 2 - index;
                point.y = pos.y - (square.side - 1) / 2;
            } else {
                return Option<Vector2Int>.None();
            }

            return Option<Vector2Int>.Some(point);
        }

        public static List<Vector2Int> GetSquarePoints2<T>(
            Vector2Int pos,
            SquareMovement square,
            Option<T>[,] board,
            int skipValue
        ) {
            var maxIndex = (square.side - 1) * 4;
            var points = new List<Vector2Int>();
            for (int i = skipValue; i < maxIndex; i += 1 + skipValue) {
                var point = GetSquarePoint(pos, square, i);
                if (point.IsSome()) {
                    if (IsOnBoard(point.Peel(), board)) {
                        points.Add(point.Peel());
                    }
                }
            }

            return points;
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