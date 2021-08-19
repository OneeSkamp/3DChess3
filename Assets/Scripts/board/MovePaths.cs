using UnityEngine;
using option;

namespace board {
    public static class MovePaths {
        public static int CalcLinearMoveLength<T>(
            Vector2Int pos,
            LinearMovement lineMove,
            int maxLength,
            Option<T>[,] board
        ) {
            var length = 0;

            for (int i = 1; i <= maxLength; i++) {
                var posX = pos.x + i * lineMove.dir.x;
                var posY = pos.y + i * lineMove.dir.y;

                if (!IsOnBoard(
                    new Vector2Int(posX, posY), board.GetLength(0), board.GetLength(1))) {
                    break;
                }

                if (board[posX, posY].IsNone()) {
                    length++;
                } else {
                    length++;
                    break;
                }
            }
            return length;
        }

        public static int CalcCircularMoveRadius() {
            var radius = 0;

            return radius;
        }

        public static bool IsOnBoard(Vector2Int pos, int width, int height) {

            if (pos.x < 0 || pos.y < 0 || pos.x >= height || pos.y >= width) {
                return false;
            }

            return true;
        }
    }
}

