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
            var height = board.GetLength(0);
            var width = board.GetLength(1);

            for (int i = 1; i <= height; i++) {
                var nextPos = pos + i * dir;
                if (!IsOnBoard(nextPos, height, width)) {
                    break;
                }

                length++;
                if (board[nextPos.x, nextPos.y].IsSome()) {
                    break;
                }
            }
            Debug.Log(length);
            return length;
        }

        public static List<Vector2Int> CalcLinearMoves<T>(
            Vector2Int pos,
            Vector2Int dir,
            Option<T>[,] board
        ) {
            var linearMoves = new List<Vector2Int>();
            var length = CalcLinearLength(pos, dir, board);

            for (int i = 1; i <= length; i++) {
                var nextPos = pos + i * dir;

                linearMoves.Add(nextPos);
            }

            return linearMoves;
        }

        public static List<Vector2Int> CalcSquareMoves(Vector2Int pos, int side) {
            var squareMoves = new List<Vector2Int>();
            var startPos = new Vector2Int(pos.x - side/2, pos.y - side/2);
            var nextPos = new Vector2Int();
            Debug.Log(startPos.x + " " + startPos.y);

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

        public static bool IsOnBoard(Vector2Int pos, int width, int height) {
            if (pos.x < 0 || pos.y < 0 || pos.x >= height || pos.y >= width) {
                return false;
            }

            return true;
        }
    }
}

