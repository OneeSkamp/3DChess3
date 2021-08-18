using System.Collections.Generic;
using option;

namespace board {
    public static class MovePaths {
        public static MovePath CalcMovePath<T>(
            Position pos, 
            Dir dir, 
            int maxLength, 
            Option<T>[,] board
        ) {
            var movePath = new MovePath();
            var length = 0;

            for (int i = 1; i <= maxLength; i++) {
                var posX = pos.x + i * dir.x;
                var posY = pos.y + i * dir.y;

                if (!IsOnBoard(new Position(posX, posY), 8, 8)) {
                    movePath.onWay = new Position(posX - dir.x, posY - dir.y);
                    break;
                }

                if (board[posX, posY].IsNone()) {
                    length++;
                }

                if (!board[posX, posY].IsNone()) {
                    movePath.onWay = new Position(posX, posY);
                    break;
                }
            }
            movePath.pos = pos;
            movePath.dir = dir;
            movePath.Length = length;

            return movePath;
        }

        public static List<MovePath> CalcMovePaths<T>(
            Position pos,
            List<Dir> directions,
            int maxLength,
            Option<T>[,] board
        ) {
            List<MovePath> movePaths = new List<MovePath>();

            foreach (Dir dir in directions) {
                movePaths.Add(CalcMovePath(pos, dir, maxLength, board));
            }

            return movePaths;
        }

        public static bool IsOnBoard(Position pos, int width, int height) {

            if (pos.x < 0 || pos.y < 0 || pos.x >= height || pos.y >= width) {
                return false;
            }

            return true;
        }
    }
}

