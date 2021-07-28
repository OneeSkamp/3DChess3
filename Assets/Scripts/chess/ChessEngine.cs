using System;
using System.Collections.Generic;

namespace chess {
    public struct Position {
        public int x;
        public int y;
    }

    public struct Figure {
        public Position position;
        public char type;
    }

    public struct PossibleMove {
        public Position movePosition;
    }

    public static class ChessEngine {
        public static Figure GetFigure (Position pos, char[,] board) {
            Figure figure = new Figure();

            figure.position = pos;
            figure.type = board[pos.x, pos.y];

            return figure;
        }

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

        public static List<PossibleMove> GetPawnMoves(Position from, int direction, char[,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 
 
            char myElement = board[from.x, from.y];
            char nextElement = board[from.x + direction, from.y];
            char nextSecondElement = board[from.x + direction * 2, from.y];
            char leftDiagonalElement = board[from.x + direction, from.y - direction];
            char rightDiagonalElement = board[from.x + direction, from.y + direction];

            Position leftDiagonalPos = GetPosition(from.x + direction, from.y - direction);
            Position rightDiagonalPos = GetPosition(from.x + direction, from.y + direction);
            Position nextElementPos = GetPosition(from.x + direction, from.y);
            Position nextSecondElementPos = GetPosition(from.x + direction * 2, from.y);

            if (nextElement == '.') {
                possibleMoves.Add(GetPossibleMove(nextElementPos)); 
                if (board[from.x + direction * 2, from.y] == '.') {
                    possibleMoves.Add(GetPossibleMove(nextSecondElementPos));
                }
            }

            if (leftDiagonalElement != '.' && IsColorful(myElement, leftDiagonalElement)) {
                possibleMoves.Add(GetPossibleMove(leftDiagonalPos));
            }

            if (rightDiagonalElement != '.' && IsColorful(myElement, rightDiagonalElement)) {
                possibleMoves.Add(GetPossibleMove(rightDiagonalPos));
            }

            return possibleMoves;
        }

        public static List<PossibleMove> GetOnePosMoves(Position from, int dirX, int dirY, char [, ] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            char myElement = board[from.x, from.y];
            char nextElement = board[from.x + dirX, from.y + dirY];
            Position nextElementPos = GetPosition(from.x + dirX, from.y + dirY);

            if (nextElement == '.') {
                possibleMoves.Add(GetPossibleMove(nextElementPos));
            }

            if (nextElement != '.' && IsColorful(myElement, nextElement)) {
                possibleMoves.Add(GetPossibleMove(nextElementPos));
            }

            return possibleMoves;
        }

        public static List<PossibleMove> GetLineMoves(Position from, int dirX, int dirY, char [, ] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            char myElement = board[from.x, from.y];
            for (int i = 1; i < 8; i++) {

                int x = from.x + i * dirX;
                int y = from.y + i * dirY;
                
                if (!OnBoard(GetPosition(x, y))) {
                    break;
                }

                char nextElement = board[x, y];
                Position nextElementPos = GetPosition(x, y);

                if (nextElement == '.') {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                }

                if (nextElement != '.' && !IsColorful(myElement, nextElement)) {
                    break;
                }

                if (nextElement != '.' && IsColorful(myElement, nextElement)) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                    break;
                }
            }
            return possibleMoves;
        }

        public static bool IsColorful(char figure1, char figure2) {
            if (Char.IsUpper(figure1) != Char.IsUpper(figure2)) {
                return true;
            }

            return false;
        }

        public static bool OnBoard(Position pos) {

            if (pos.x < 0 || pos.y < 0 || pos.x >= 7 || pos.y >= 7) {
                return false;
            }

            return true;
        }

        public static List<PossibleMove> GetPossibleMoves(Figure figure, char [,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            switch(figure.type) {
                case 'B':
                case 'b':
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, -1, board));
                    return possibleMoves;
                case 'R':
                case 'r':
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 0, 1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 0, -1, board));
                    return possibleMoves;
                case 'N':
                case 'n':
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 2, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 2, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -2, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -2, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, 2, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, 2, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, -2, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, -2, board));
                    return possibleMoves;
                case 'Q':
                case 'q':
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 0, 1, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 0, -1, board));
                    return possibleMoves;
                case 'K':
                case 'k':
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, 0, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 0, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, 0, board));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 0, -1, board));
                    return possibleMoves;
                case 'P':
                    possibleMoves.AddRange(GetPawnMoves(figure.position, -1, board));
                    return possibleMoves;
                case 'p':
                    possibleMoves.AddRange(GetPawnMoves(figure.position, 1, board));
                    return possibleMoves;   
            }
            return possibleMoves;
        }
    }
}

