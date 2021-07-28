using System;
using System.Collections.Generic;
using UnityEngine;

namespace chess {
    public static class ChessEngine {
        
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

        public static Figure GetFigure (Position pos) {
            Figure figure = new Figure();
            char [, ] board = Board.GetBoardMap();

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

        public static List<PossibleMove> GetPawnMoves(Position from, int direction) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            char [, ] board = Board.GetBoardMap();
 
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

        public static List<PossibleMove> GetOnePosMoves(Position from, int dirX, int dirY) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            char [, ] board = Board.GetBoardMap();
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

        public static List<PossibleMove> GetLineMoves(Position from, int dirX, int dirY) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 
            
            char [, ] board = Board.GetBoardMap();

            char myElement = board[from.x, from.y];
            for (int i = 1; i < Board.GetBoardEdgeLen(); i++) {
                int x = from.x + i * dirX;
                int y = from.y + i * dirY;

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

        public static List<PossibleMove> GetPossibleMoves(Figure figure) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            switch(figure.type) {
                case 'B':
                case 'b':
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, 1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, -1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, 1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, -1));
                    return possibleMoves;
                case 'R':
                case 'r':
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, 0));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 0, 1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, 0));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 0, -1));
                    return possibleMoves;
                case 'N':
                case 'n':
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 2, 1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 2, -1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -2, 1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -2, -1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, 2));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, 2));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, -2));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, -2));
                    return possibleMoves;
                case 'Q':
                case 'q':
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, 1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, -1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, 1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, -1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 1, 0));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 0, 1));
                    possibleMoves.AddRange(GetLineMoves(figure.position, -1, 0));
                    possibleMoves.AddRange(GetLineMoves(figure.position, 0, -1));
                    return possibleMoves;
                case 'K':
                case 'k':
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, 0));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 0, 1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, 1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, -1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 1, -1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, 1));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, -1, 0));
                    possibleMoves.AddRange(GetOnePosMoves(figure.position, 0, -1));
                    return possibleMoves;
                case 'P':
                    possibleMoves.AddRange(GetPawnMoves(figure.position, -1));
                    return possibleMoves;
                case 'p':
                    possibleMoves.AddRange(GetPawnMoves(figure.position, 1));
                    return possibleMoves;   
            }
            return possibleMoves;
        }


        // char [, ] boardMap;
        
        // void Start()
        // {
        //     Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        //     boardMap = Board.GetBoardMap();

        //     for (int i = 0; i < 8; i++) {
        //         Debug.Log($"{boardMap[i, 0]} {boardMap[i, 1]} {boardMap[i, 2]} {boardMap[i, 3]} {boardMap[i, 4]} {boardMap[i, 5]} {boardMap[i, 6]} {boardMap[i, 7]}");
        //     }
            
        // }
    }
}

