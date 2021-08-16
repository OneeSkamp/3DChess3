using System.Collections.Generic;
using board;
using option;
using UnityEngine;

namespace chess {
    public static class ChessEngine {
        public static List<MovePath> CalcFigurePaths(Position pos, Option<Fig>[,] boardMap, Board<Fig> board) {
            var figure = boardMap[pos.x, pos.y].Peel();
            var figurePaths = new List<MovePath>();
            var dirs = new List<Dir>();
            var moveType = MoveTypes.moveTypes[figure.type];

            if (moveType.lineMove) {
                dirs.AddRange(board.lineDir);
            }

            if (moveType.diagonalMove) {
                dirs.AddRange(board.diagonalDir);
            }

            if (moveType.circularMove) {
                dirs.AddRange(board.circularDir);
            }

            figurePaths.AddRange(MovePaths.CalcMovePaths<Fig>(pos, dirs, boardMap));

            return figurePaths;
        }

        public static List<Position> CalcPossibleMoves(List<MovePath> movePaths) {
            List<Position> possibleMoves = new List<Position>();

            foreach (MovePath path in movePaths) {
                for (int i = 1; i <= path.Length; i++) {
                    var posX = path.pos.x + i * path.dir.x;
                    var posY = path.pos.y + i * path.dir.y;

                    possibleMoves.Add(new Position(posX, posY));
                }
            }

            return possibleMoves;
        }
    }
}

