using System.Collections.Generic;
using UnityEngine;
using option;

namespace board {
    public struct LinearMovement {
        public Vector2Int dir;
    }

    public struct CircularMovement {
        public int radius;
    }

    public struct MovementType {
        public List<LinearMovement?> linear;
        public CircularMovement? circular;
        public int maxLength;
    }

    public static class BoardCalculator {
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
                    new Vector2Int(posX, posY), board.GetLength(0), board.GetLength(1)
                )) {
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

        public static List<Vector2Int> CalcCircularMoves(Vector2Int pos, int radius) {
            var CircularMoves = new List<Vector2Int>();
            CircularMoves.Add(new Vector2Int(pos.x + radius, pos.y + radius/2));
            CircularMoves.Add(new Vector2Int(pos.x + radius, pos.y - radius/2));
            CircularMoves.Add(new Vector2Int(pos.x - radius, pos.y - radius/2));
            CircularMoves.Add(new Vector2Int(pos.x - radius, pos.y + radius/2));
            CircularMoves.Add(new Vector2Int(pos.x + radius/2, pos.y + radius));
            CircularMoves.Add(new Vector2Int(pos.x - radius/2, pos.y - radius));
            CircularMoves.Add(new Vector2Int(pos.x + radius/2, pos.y - radius));
            CircularMoves.Add(new Vector2Int(pos.x - radius/2, pos.y + radius));

            return CircularMoves;
        }

        public static bool IsOnBoard(Vector2Int pos, int width, int height) {

            if (pos.x < 0 || pos.y < 0 || pos.x >= height || pos.y >= width) {
                return false;
            }

            return true;
        }
    }
}

