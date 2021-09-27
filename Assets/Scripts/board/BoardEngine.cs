using System.Collections.Generic;
using UnityEngine;
using option;
using collections;

namespace board {
    public enum MoveType {
        Attack,
        Move
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
        public MoveType moveType;
        public LinearMovement? linear;
        public SquareMovement? square;

        public static Movement Linear(LinearMovement linear, MoveType moveType) {
            return new Movement { linear = linear, moveType = moveType };
        }

        public static Movement Square(SquareMovement square, MoveType moveType) {
            return new Movement { square = square, moveType = moveType };
        }
    }

    public struct FixedMovement {
        public Vector2Int start;
        public Movement movement;

        public static FixedMovement Mk(Vector2Int start, Movement movement) {
            return new FixedMovement { start = start, movement = movement };
        }
    }

    public static class BoardEngine {
        public static int GetLinearLength<T>(Vector2Int pos, Vector2Int dir, Option<T>[,] board) {
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));

            var length = 0;
            for (int i = 1; i <= size.x; i++) {
                var nextPos = pos + i * dir;

                if (!IsOnBoard(nextPos, board)) {
                    break;
                }

                length++;
                if (board[nextPos.x, nextPos.y].IsSome()) {
                    break;
                }
            }
            return length;
        }

        public static List<Vector2Int> GetLinearPath<T>(
            Vector2Int pos,
            Vector2Int dir,
            int length,
            Option<T>[,] board
        ) {
            var linearMoves = new List<Vector2Int>();

            for (int i = 1; i <= length; i++) {
                var nextPos = pos + i * dir;
                linearMoves.Add(nextPos);
                if (!IsOnBoard(nextPos, board)) {
                    continue;
                }

                if (board[nextPos.x, nextPos.y].IsSome()){
                    break;
                }
            }
            return linearMoves;
        }

        public static BindableList<Vector2Int> GetSquarePath(Vector2Int pos, int side) {
            var squareMoves = new BindableList<Vector2Int>();
            var startPos = new Vector2Int(pos.x - side/2, pos.y - side/2);
            var nextPos = new Vector2Int();
            var dir = new Vector2Int(1, 0);

            for (int i = 1; i < side; i++) {
                nextPos = new Vector2Int(startPos.x, startPos.y + i);
                squareMoves.Add(nextPos);
            }
            startPos = nextPos;

            for (int i = 1; i < side; i++) {
                nextPos = new Vector2Int(startPos.x + i, startPos.y);
                squareMoves.Add(nextPos);
            }
            startPos = nextPos;

            for (int i = 1; i < side; i++) {
                nextPos = new Vector2Int(startPos.x, startPos.y - i);
                squareMoves.Add(nextPos);
            }
            startPos = nextPos;

            for (int i = 1; i < side; i++) {
                nextPos = new Vector2Int(startPos.x - i, startPos.y);
                squareMoves.Add(nextPos);
            }

            return squareMoves;
        }

        public static BindableList<Vector2Int> RemoveSquareParts(
            BindableList<Vector2Int> square,
            int start,
            int skipValue
        ) {
            var pointer = square.head;

            for (int i = 0; i < start; i++) {
                pointer = pointer.next;
            }

            square.head = pointer;
            var count = 0;

            foreach (var i in square) {
                if (count == 0) {
                    count = skipValue;
                    continue;
                }
                square.Remove(i);

                count--;
            }

            return square;
        }

        public static Vector2Int? GetLastOnPathPos<T>(
            FixedMovement fixedMovement,
            Option<T>[,] board
        ) {
            var dir = fixedMovement.movement.linear.Value.dir;
            var startPos = fixedMovement.start;
            var length = fixedMovement.movement.linear.Value.length;
            var linearPath = BoardEngine.GetLinearPath(startPos, dir, length, board);

            if (linearPath.Count == 0) {
                return null;
            }

            var figPos = linearPath[linearPath.Count - 1];
            if (!BoardEngine.IsOnBoard(figPos, board)) {
                return null;
            }

            var figOpt = board[figPos.x, figPos.y];
            if (figOpt.IsNone()) {
                return null;
            }

            return figPos;
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
            if (IsOnPlace(pos, size)) {
                return true;
            }

            return false;
        }

        public static bool IsOnPlace(Vector2Int pos, Vector2Int size) {
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }
    }
}