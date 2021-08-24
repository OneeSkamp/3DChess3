using System.Collections.Generic;
using UnityEngine;
using option;

namespace board {
    public struct Diagonal {
        public List<Vector2Int> diagonalDirs;
    }

    public struct Straight {
        public List<Vector2Int> straightDirs;
    }

    public struct LinearMovement {
        public Diagonal? diagonal;
        public Straight? straight;
    }

    public struct SquareMovement {
        public int side;
    }

    public struct MovementType {
        public LinearMovement? linear;
        public SquareMovement? square;
    }

    public static class BoardEngine {
        public static int CalcLinearLength<T>(
            Vector2Int pos,
            Vector2Int dir,
            Option<T>[,] board
        ) {
            var length = 0;
            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));

            for (int i = 1; i <= size.x; i++) {
                var nextPos = pos + i * dir;
                if (!IsOnBoard(nextPos, size)) {
                    break;
                }

                length++;
                if (board[nextPos.x, nextPos.y].IsSome()) {
                    break;
                }
            }
            return length;
        }

        public static List<Vector2Int> CalcLinearPath<T>(
            Vector2Int pos,
            Vector2Int dir,
            int length,
            Option<T>[,] board
        ) {
            var linearMoves = new List<Vector2Int>();

            for (int i = 1; i <= length; i++) {
                var nextPos = pos + i * dir;

                linearMoves.Add(nextPos);
            }
            return linearMoves;
        }

        public static List<Vector2Int> CalcSquarePath(Vector2Int pos, int side) {
            var squareMoves = new List<Vector2Int>();
            var startPos = new Vector2Int(pos.x - side/2, pos.y - side/2);
            var nextPos = new Vector2Int();

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

        public static bool IsOnBoard(Vector2Int pos, Vector2Int size) {
            if (pos.x < 0 || pos.y < 0 || pos.x >= size.x || pos.y >= size.y) {
                return false;
            }

            return true;
        }
    }
}

