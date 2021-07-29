using System;
using System.Collections.Generic;

namespace chess {
    public struct Position {
        public int x;
        public int y;
    }

    public struct PossibleMove {
        public Position movePosition;
    }

    public static class ChessEngine {

        public static Position GetPosition(int x, int y) {
            Position position = new Position();
            position.x = x;
            position.y = y;
            return position;
        }
        public static PossibleMove GetPossibleMove(Position to) {
            PossibleMove possibleMove = new PossibleMove();
            possibleMove.movePosition = GetPosition(to.x, to.y);

            return possibleMove;
        }

        public static List<PossibleMove> GetPawnMoves(Position from, int direction, 
                                                                            Fig[,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 
 
            Fig myElement = board[from.x, from.y];
            Fig nextElement = board[from.x + direction, from.y];
            Fig nextSecondElement = board[from.x + direction * 2, from.y];
            Fig leftDiagonalElement = board[from.x + direction, from.y - direction];
            Fig rightDiagonalElement = board[from.x + direction, from.y + direction];

            Position leftDiagonalPos = GetPosition(from.x + direction, from.y - direction);
            Position rightDiagonalPos = GetPosition(from.x + direction, from.y + direction);
            Position nextElementPos = GetPosition(from.x + direction, from.y);
            Position nextSecondElementPos = GetPosition(from.x + direction * 2, from.y);

            if (nextElement.type == figureType.None) {
                possibleMoves.Add(GetPossibleMove(nextElementPos)); 
                if (board[from.x + direction * 2, from.y].type == figureType.None) {
                    possibleMoves.Add(GetPossibleMove(nextSecondElementPos));
                }
            }

            if (IsColorful(myElement, leftDiagonalElement)) {
                possibleMoves.Add(GetPossibleMove(leftDiagonalPos));
            }

            if (IsColorful(myElement, rightDiagonalElement)) {
                possibleMoves.Add(GetPossibleMove(rightDiagonalPos));
            }

            return possibleMoves;
        }

        public static List<PossibleMove> GetOnePosMoves(Position from, int dirX, 
                                                            int dirY, Fig [,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            Fig myElement = board[from.x, from.y];
            Fig nextElement = board[from.x + dirX, from.y + dirY];
            Position nextElementPos = GetPosition(from.x + dirX, from.y + dirY);

            if (nextElement.type == figureType.None) {
                possibleMoves.Add(GetPossibleMove(nextElementPos));
            }

            if (IsColorful(myElement, nextElement)) {
                possibleMoves.Add(GetPossibleMove(nextElementPos));
            }

            return possibleMoves;
        }

        public static List<PossibleMove> GetLineMoves(Position from, int dirX, 
                                                        int dirY, Fig [,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            Fig myElement = board[from.x, from.y];
            for (int i = 1; i < 8; i++) {

                int x = from.x + i * dirX;
                int y = from.y + i * dirY;
                
                if (!OnBoard(GetPosition(x, y))) {
                    break;
                }

                Fig nextElement = board[x, y];
                Position nextElementPos = GetPosition(x, y);

                if (nextElement.type == figureType.None) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                }

                if (nextElement.type != figureType.None 
                    && !IsColorful(myElement, nextElement)) {
                    break;
                }

                if (IsColorful(myElement, nextElement)) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                    break;
                }
            }
            return possibleMoves;
        }

        private static bool IsColorful(Fig figure1, Fig figure2) {
            if (figure1.white != figure2.white) {
                return true;
            }

            return false;
        }

        private static bool OnBoard(Position pos) {

            if (pos.x < 0 || pos.y < 0 || pos.x >= 7 || pos.y >= 7) {
                return false;
            }

            return true;
        }

        public static List<PossibleMove> GetPossibleMoves(Position startPos, Fig [,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            Fig figure = board[startPos.x, startPos.y];
            switch(figure.type) {
                case figureType.Bishop :
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, -1, board));
                    return possibleMoves;

                case figureType.Rook :
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 0, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 0, -1, board));
                    return possibleMoves;

                case figureType.Knight :
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 2, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 2, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -2, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -2, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, 2, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, 2, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, -2, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, -2, board));
                    return possibleMoves;

                case figureType.Queen :
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 0, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 0, -1, board));
                    return possibleMoves;

                case figureType.King :
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, 0, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 0, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, 0, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 0, -1, board));
                    return possibleMoves;

                // case (figure.type == figureType.Pawn && figure.white) :
                //     possibleMoves.AddRange(GetPawnMoves(figure.position, -1, board));
                //     return possibleMoves;

                // case (figure.type == figureType.Pawn && !figure.white):
                //     possibleMoves.AddRange(GetPawnMoves(figure.position, 1, board));
                //     return possibleMoves;   
            }

            if (figure.type == figureType.Pawn && !figure.white) {
                possibleMoves.AddRange(GetPawnMoves(startPos, + 1, board));
                return possibleMoves;
            }

            if (figure.type == figureType.Pawn && figure.white) {
                possibleMoves.AddRange(GetPawnMoves(startPos, - 1, board));
                return possibleMoves;
            }

            return possibleMoves;
        }

        public static bool MoveFigure(Position from, Position to, Fig[,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            possibleMoves.AddRange(GetPossibleMoves(from, board));

            foreach (PossibleMove move in possibleMoves) {
                if (to.x == move.movePosition.x && to.y == move.movePosition.y) {
                    board[to.x, to.y] = board[from.x, from.y];
                    
                    return true;
                }
            }

            return false;
        }
        // public static bool ComparePosition(Position position1, Position position2) {
        //     if (position1 == position2) {
        //         return true;
        //     }
        //     return false;
        // }

        // public static bool MoveFigure(Figure figure, Position to, char [,] board) {
        //     List<PossibleMove> possibleMoves = GetPossibleMoves(figure, board);
        //     Position currentPos = figure.position; 
            
        //     foreach(PossibleMove move in possibleMoves) {
        //         if (ComparePosition(move, to)) {
        //             board[currentPos.x, currentPos.y] = board[to.x, to.y];
        //             return true;
        //         }
        //     }
        //     return false;
        // }
    }
}

