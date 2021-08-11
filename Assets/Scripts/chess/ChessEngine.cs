using System;
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

        public static List<MovePath> CalcMovePath(
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

            if (moveType.DiagonalMove) {
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(1, 0), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(0, 1), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(0, -1), board, whiteMove));
                movePaths.Add(CalcMovePath(pos, Dir.NewDir(-1, 0), board, whiteMove));
            }

            return movePaths;
        }

        public static List<Position> GetPossibleMoves(MovePath MovePath) {
            var posssibleMoves = new List<Position>();

            return posssibleMoves;
        }

        private static MovePath CalcMovePath(Position pos, Dir dir, Fig[,] board, bool whiteMove) {
            var myFig = board[pos.x, pos.y];
            var movePath = new MovePath();
            int lenght = 0;
            movePath.dir = dir;

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

            for (int i = 0; i < lenght; i++) {
                for(int j = 0; j < lenght; j++) {
                    Fig fig = board[pos.x + i * dir.x, pos.y + j * dir.y];

                    if (fig.type != FigureType.None && fig.white == whiteMove) {
                        break;
                        
                    } else if (fig.type == FigureType.None) {
                        movePath.Lenght++;

                    } else if (fig.type != FigureType.None && fig.white != whiteMove) {

                        movePath.Lenght++;
                        break;
                    }
                }
            }
            return movePath;
        }
    }
}

