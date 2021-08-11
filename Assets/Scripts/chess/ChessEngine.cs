using System.Collections.Generic;
using UnityEngine;

namespace chess {
    public static class ChessEngine {
        public static MoveTypes CalcMoveType(Fig fig) {
            var moveType = new MoveTypes();

            switch(fig.type) {
                case FigureType.Pawn:
                    break;

                case FigureType.Bishop:
                    moveType.DiagonalMove = true;
                    break;

                case FigureType.Knight:
                    break;

                case FigureType.Rook:
                    moveType.LineMove = true;
                    break;

                case FigureType.Queen:
                    moveType.DiagonalMove = true;
                    moveType.LineMove = true;
                    break;

                case FigureType.King:
                    break;
            }

            return moveType;
        }

        public static List<MovePath> CalcMovePaths(
            Position pos,
            MoveTypes moveType, 
            Fig[,] board, 
            bool whiteMove
        ) {
            var movePaths = new List<MovePath>();

            if (moveType.DiagonalMove) {
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, 1), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, -1), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, 1), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, -1), board, whiteMove));
            }

            if (moveType.LineMove) {
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, 0), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(0, 1), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(0, -1), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, 0), board, whiteMove));
            }

            return movePaths;
        }

        public static List<Position> CalcPossibleMoves(List<MovePath> movePaths) {
            List<Position> possibleMoves = new List<Position>();

            foreach (MovePath path in movePaths) {
                for (int i = 1; i <= path.Lenght; i++) {
                    var posX = path.pos.x + i * path.dir.x;
                    var posY = path.pos.y + i * path.dir.y;

                    possibleMoves.Add(new Position(posX, posY));
                }
            }

            return possibleMoves;
        }

        private static MovePath CalcMovePath(Position pos, Dir dir, Fig[,] board, bool whiteMove) {
            var myFig = board[pos.x, pos.y];
            var movePath = new MovePath();
            int lenght = 0;
            movePath.dir = dir;
            movePath.pos = pos;
            movePath.Lenght = 0;

            switch (board[pos.x, pos.y].type) {
                case FigureType.King:
                    lenght = 1;
                    break;
                case FigureType.Bishop:
                    lenght = 8;
                    break;
                case FigureType.Rook:
                    lenght = 8;
                    break;
            }

            for (int i = 1; i < lenght; i++) {

                if (!OnBoard(new Position(pos.x + i * dir.x, pos.y + i * dir.y))) {
                    break;
                }

                Fig fig = board[pos.x + i * dir.x, pos.y + i * dir.y];

                if (fig.type != FigureType.None && fig.white == whiteMove) {
                    break;
                    
                } else if (fig.type == FigureType.None) {
                    movePath.Lenght++;

                } else if (fig.type != FigureType.None && fig.white != whiteMove) {
                    movePath.Lenght++;
                    break;
                }
            }
            return movePath;
        }

        private static bool OnBoard(Position pos) {

            if (pos.x < 0 || pos.y < 0 || pos.x >= 8 || pos.y >= 8) {
                return false;
            }

            return true;
        }
    }
}

