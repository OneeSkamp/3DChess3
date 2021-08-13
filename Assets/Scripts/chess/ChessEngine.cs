using System.Collections.Generic;
using UnityEngine;
using board;
using option;

namespace chess {
    public static class ChessEngine {

        public static MoveType GetMoveType(Fig fig) {
            var moveTypes = MoveTypes.MakeFigMoveTypes();
            var moveType = moveTypes[fig.type];
            return moveType;
        }

        public static List<MovePath> CalcMovePaths(
            Position pos, 
            MoveType moveType, 
            Option<Fig>[,] board
        ) {
            var movePaths = new List<MovePath>();

            if (moveType.diagonalMove) {
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, 1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, -1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, 1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, -1), board));
            }

            if (moveType.lineMove) {
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, 0), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(0, 1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(0, -1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, 0), board));
            }

            if (moveType.circularMove) {
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(2, 1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(2, -1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-2, 1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-2, -1), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, 2), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, 2), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, -2), board));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, -2), board));
            }

            return movePaths;
        }

        public static MovePath CalcMovePath(Position pos, Dir dir, Option<Fig>[,] board) {
            var myFig = board[pos.x, pos.y].Peel();
            var movePath = new MovePath();
            int length = 0;

            movePath.dir = dir;
            movePath.pos = pos;
            movePath.Length = 0;

            switch (myFig.type) {
                case FigureType.King:
                    length = 1;
                    break;
                case FigureType.Knight:
                    length = 1;
                    break;
                case FigureType.Bishop:
                    length = 8;
                    break;
                case FigureType.Rook:
                    length = 8;
                    break;
                case FigureType.Queen:
                    length = 8;
                    break;
            }

            if (myFig.type == FigureType.Pawn) {
                if (myFig.firstMove) {
                    length = 2;
                } else {
                    length = 1;
                }
            }

            for (int i = 1; i <= length; i++) {

                if (!OnBoard(new Position(pos.x + i * dir.x, pos.y + i * dir.y))) {
                    break;
                }

                var fig = board[pos.x + i * dir.x, pos.y + i * dir.y].Peel();

                if (fig.type != FigureType.None && fig.white == myFig.white) {
                    break;

                } else if (fig.type == FigureType.None) {
                    movePath.Length++;

                } else if (fig.type != FigureType.None && fig.white != myFig.white) {
                    movePath.Length++;
                    break;
                }
            }
            return movePath;
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

        private static bool OnBoard(Position pos) {

            if (pos.x < 0 || pos.y < 0 || pos.x >= 8 || pos.y >= 8) {
                return false;
            }

            return true;
        }
    }
}

