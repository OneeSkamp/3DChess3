using System.Collections.Generic;
using UnityEngine;
using option;

namespace board {
    public enum BoardErr {
        None,
        PosOutsideBoard,
        BoardIsNull,
        SquarePointErr
    }

    public struct LinearMovement {
        public int length;
        public Vector2Int dir;

        public static LinearMovement Mk(int length, Vector2Int dir) {
            return new LinearMovement { length = length, dir = dir };
        }
    }

    public struct SquareHoles {
        public int mod;
    }

    public struct SquareMovement {
        public int halfSide;
        public SquareHoles squareHoles;
        public static SquareMovement Mk(int halfSide, SquareHoles squareHoles) {
            return new SquareMovement { halfSide = halfSide, squareHoles = squareHoles };
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

    public struct LastLinearPos {
        public Option<Vector2Int> pos;

        public static LastLinearPos Mk(Option<Vector2Int> pos) {
            return new LastLinearPos { pos = pos };
        }
    }

    public static class BoardEngine {
        public static Vector2Int GetLinearPoint(
            Vector2Int start,
            LinearMovement linear,
            int index
        ) {

            return start + linear.dir * index;
        }

        public static (int, BoardErr) GetLenUntilFig<T>(
            Vector2Int pos,
            LinearMovement linear,
            Option<T>[,] board,
            int maxLength
        ) {
            if (board == null) {
                return (-1, BoardErr.BoardIsNull);
            }

            if (!IsOnBoard(pos, board)) {
                return (-1, BoardErr.PosOutsideBoard);
            }

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

        public static (Option<Vector2Int>, BoardErr) GetSquarePoint(
            Vector2Int center,
            SquareMovement square,
            int index
        ) {
            var point = new Vector2Int();
            var maxIndex = square.halfSide * 8;
            if (index < square.halfSide * 2) {
                point.x = center.x - square.halfSide;
                point.y = center.y - square.halfSide + index;
            } else if (index < square.halfSide * 4) {
                point.x = center.x - 3 * square.halfSide + index;
                point.y = center.y + square.halfSide;
            } else if (index < square.halfSide * 6) {
                point.x = center.x + square.halfSide;
                point.y = center.y + 5 * square.halfSide - index;
            } else if (index < maxIndex) {
                point.x = center.x + 7 * square.halfSide - index;
                point.y = center.y - square.halfSide;
            } else {
                return (Option<Vector2Int>.None(), BoardErr.None);
            }

            return (Option<Vector2Int>.Some(point), BoardErr.None);
        }

        public static (List<Vector2Int>, BoardErr) GetSquarePoints<T>(
            Vector2Int center,
            SquareMovement square,
            Option<T>[,] board
        ) {
            if (board == null) {
                return (null, BoardErr.BoardIsNull);
            }

            if (!IsOnBoard(center, board)) {
                return (null, BoardErr.PosOutsideBoard);
            }

            var maxIndex = square.halfSide * 8;
            var points = new List<Vector2Int>();
            for (int i = 0; i < maxIndex; i ++) {
                if (i % square.squareHoles.mod == 0) {
                    continue;
                }
                var (point, err) = GetSquarePoint(center, square, i);
                if (err != BoardErr.None) {
                    return (null, BoardErr.SquarePointErr);
                }
                if (point.IsSome()) {
                    if (IsOnBoard(point.Peel(), board)) {
                        points.Add(point.Peel());
                    }
                }
            }

            return (points, BoardErr.None);
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