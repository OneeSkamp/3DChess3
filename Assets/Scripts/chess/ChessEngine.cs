using System.Globalization;
using System.Reflection;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace chess {
    public struct Move {
        public Position from;
        public Position to;
    }
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
        private static PossibleMove GetPossibleMove(Position to) {
            PossibleMove possibleMove = new PossibleMove();
            possibleMove.movePosition = GetPosition(to.x, to.y);

            return possibleMove;
        }

        private static List<PossibleMove> GetPawnMoves(
                Position from, 
                int direction, 
                Fig[,] board
                )
            {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 
 
            Fig myElement = board[from.x, from.y];

            if (OnBoard(GetPosition(from.x + direction, from.y))) {
                Fig nextElement = board[from.x + direction, from.y];
                Position nextElementPos = GetPosition(from.x + direction, from.y);
                
                if (nextElement.type == FigureType.None) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos)); 
                }
            }

            if (OnBoard(GetPosition(from.x + direction * 2, from.y))) {
                Fig nextSecondElement = board[from.x + direction * 2, from.y];
                Position nextSecondElementPos = GetPosition(from.x + direction * 2, from.y);
                
                if (board[from.x + direction * 2, from.y].type == FigureType.None
                    && board[from.x + direction, from.y].type == FigureType.None
                    && myElement.firstMove) {

                    possibleMoves.Add(GetPossibleMove(nextSecondElementPos));
                }
            }

            if (OnBoard(GetPosition(from.x + direction, from.y - direction))) {
                Fig leftDiagElement = board[from.x + direction, from.y - direction];
                Position leftDiagPos = GetPosition(from.x + direction, from.y - direction);

                if (leftDiagElement.type != FigureType.None 
                    && IsColorful(myElement, leftDiagElement)) {

                    possibleMoves.Add(GetPossibleMove(leftDiagPos));
                }
            }

            if (OnBoard(GetPosition(from.x + direction, from.y + direction))) {
                Fig rightDiagElement = board[from.x + direction, from.y + direction];
                Position rightDiagPos = GetPosition(from.x + direction, from.y + direction);

                if (rightDiagElement.type != FigureType.None 
                    && IsColorful(myElement, rightDiagElement)) {

                    possibleMoves.Add(GetPossibleMove(rightDiagPos));
                } 
            }

            return possibleMoves;
        }

        private static List<PossibleMove> GetOnePosMoves(
                Position from, 
                int dirX, 
                int dirY, 
                Fig [,] board
                )
            {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            if (OnBoard(GetPosition(from.x + dirX, from.y + dirY))) {
                Fig myElement = board[from.x, from.y];
                Fig nextElement = board[from.x + dirX, from.y + dirY];
                Position nextElementPos = GetPosition(from.x + dirX, from.y + dirY);

                if (nextElement.type == FigureType.None) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                }

                if (IsColorful(myElement, nextElement)) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                }
            }

            return possibleMoves;
        }

        private static List<PossibleMove> GetCastlingMoves(Position from, Fig [,] board) {

            List<PossibleMove> possibleMoves = new List<PossibleMove>();

            if (OnBoard(GetPosition(from.x, from.y - 4)) 
                && board[from.x, from.y - 1].type == FigureType.None
                && board[from.x, from.y - 2].type == FigureType.None
                && board[from.x, from.y - 3].type == FigureType.None
                ) {

                if (board[from.x, from.y - 4].type == FigureType.Rook 
                    && board[from.x, from.y - 4].firstMove) {

                    board[from.x, from.y - 2].castling = true;
                    possibleMoves.Add(GetPossibleMove(GetPosition(from.x, from.y - 2)));

                } else if (OnBoard(GetPosition(from.x, from.y - 2))){
                    board[from.x, from.y - 2].castling = false;
                }
            }

            if (OnBoard(GetPosition(from.x, from.y + 3)) 
                && board[from.x, from.y + 1].type == FigureType.None
                && board[from.x, from.y + 2].type == FigureType.None) {
                
                if (board[from.x, from.y + 3].type == FigureType.Rook 
                    && board[from.x, from.y + 3].firstMove) {
                    
                    board[from.x, from.y + 2].castling = true;
                    possibleMoves.Add(GetPossibleMove(GetPosition(from.x, from.y + 2)));

                } else if (OnBoard(GetPosition(from.x, from.y + 2))){
                    board[from.x, from.y + 2].castling = false;
                }
            }

            return possibleMoves;
        }

        private static List<PossibleMove> GetEnPassantMoves(Position from, Fig [,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            Fig myFig = board[from.x, from.y];

            if (from.x == 4) {

                if (OnBoard(GetPosition(from.x, from.y - 1))) {
                    Fig leftFig = board[from.x, from.y - 1];
                    Fig forMove = board[from.x + 1, from.y - 1];
                    bool colorful = IsColorful(myFig, leftFig);

                    if (leftFig.type == FigureType.Pawn && colorful) {
                        if (forMove.type == FigureType.None && forMove.enPassant) {
                            possibleMoves.Add(
                                GetPossibleMove(GetPosition(from.x + 1, from.y - 1))
                            );
                        }
                    }
                }

                if (OnBoard(GetPosition(from.x, from.y + 1))) {
                    Fig rightFig = board[from.x, from.y + 1];
                    Fig forMove = board[from.x + 1, from.y + 1];
                    bool colorful = IsColorful(myFig, rightFig);

                    if (rightFig.type == FigureType.Pawn && colorful) {
                        if (forMove.type == FigureType.None && forMove.enPassant) {
                            possibleMoves.Add(
                                GetPossibleMove(GetPosition(from.x + 1, from.y + 1))
                            );
                        }
                    }
                }
            }

            if (from.x == 3) {

                if (OnBoard(GetPosition(from.x, from.y - 1))) {
                    Fig leftFig = board[from.x, from.y - 1];
                    Fig forMove = board[from.x - 1, from.y - 1];
                    bool colorful = IsColorful(myFig, leftFig);

                    if (leftFig.type == FigureType.Pawn && colorful) {
                        if (forMove.type == FigureType.None && forMove.enPassant) {
                            possibleMoves.Add(
                                GetPossibleMove(GetPosition(from.x - 1, from.y - 1))
                            );
                        }
                    }
                }

                if (OnBoard(GetPosition(from.x, from.y + 1))) {
                    Fig rightFig = board[from.x, from.y + 1];
                    Fig forMove = board[from.x - 1, from.y + 1];
                    bool colorful = IsColorful(myFig, rightFig);

                    if (rightFig.type == FigureType.Pawn && colorful) {
                        if (forMove.type == FigureType.None && forMove.enPassant) {
                            possibleMoves.Add(
                                GetPossibleMove(GetPosition(from.x - 1, from.y + 1))
                            );
                        }
                    }
                }
            }
            return possibleMoves;
        }

        private static List<PossibleMove> GetLineMoves(
                Position from, int dirX, 
                int dirY, Fig [,] board
                )
            {
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

                if (nextElement.type == FigureType.None) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                }

                if (nextElement.type != FigureType.None 
                    && !IsColorful(myElement, nextElement)) {
                    break;
                }

                if (nextElement.type != FigureType.None && IsColorful(myElement, nextElement)) {
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

            if (pos.x < 0 || pos.y < 0 || pos.x >= 8 || pos.y >= 8) {
                return false;
            }

            return true;
        }

        private static List<PossibleMove> GetPossibleMoves(Position startPos, Fig [,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            Fig figure = board[startPos.x, startPos.y];
            switch(figure.type) {
                case FigureType.Bishop :
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, -1, board));
                    return possibleMoves;

                case FigureType.Rook :
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 0, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 0, -1, board));
                    return possibleMoves;

                case FigureType.Knight :
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 2, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 2, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -2, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -2, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, 2, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, 2, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, -2, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, -2, board));
                    return possibleMoves;

                case FigureType.Queen :
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, -1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 0, 1, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, -1, 0, board));
                    possibleMoves.AddRange(GetLineMoves(startPos, 0, -1, board));
                    return possibleMoves;

                case FigureType.King :
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, 0, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 0, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 1, -1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, 1, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, -1, 0, board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, 0, -1, board));
                    possibleMoves.AddRange(GetCastlingMoves(startPos, board));
                    return possibleMoves;
            }

            if (figure.type == FigureType.Pawn && !figure.white) {
                possibleMoves.AddRange(GetPawnMoves(startPos, + 1, board));
                possibleMoves.AddRange(GetEnPassantMoves(startPos, board));
                return possibleMoves;
            }

            if (figure.type == FigureType.Pawn && figure.white) {
                possibleMoves.AddRange(GetPawnMoves(startPos, - 1, board));
                possibleMoves.AddRange(GetEnPassantMoves(startPos, board));
                return possibleMoves;
            }

            return possibleMoves;
        }

        public static List<PossibleMove> CheckingPossibleMoves(
                Position startPos, 
                bool whiteMove, 
                Fig [,] board
                )
            {
            List<PossibleMove>  possibleMoves = GetPossibleMoves(startPos, board);
            List<PossibleMove>  test = new List<PossibleMove>();
            
                foreach (PossibleMove move in possibleMoves) {
                    Fig [,] boardForCheck = (Fig[,])board.Clone();
                    boardForCheck[startPos.x, startPos.y].firstMove = false;
                    boardForCheck[move.movePosition.x, 
                                    move.movePosition.y] = boardForCheck[startPos.x, startPos.y];
                    boardForCheck[startPos.x, startPos.y].type = FigureType.None;

                    if (!CheckKing(whiteMove, boardForCheck)) {
                        test.Add(move);
                    }

                }
            return test;
        }

        

        public static List<PossibleMove> GetAttackedKingMoves(bool whiteMove, Fig[,] board) {
            List<PossibleMove> attackedKingMoves = new List<PossibleMove>();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j].white != whiteMove) {
                        attackedKingMoves.AddRange(GetPossibleMoves(GetPosition(i, j), board));
                    }
                }
            }
            return attackedKingMoves;
        }
        public static List<PossibleMove> Сheckmate(bool whiteMove, Fig[,] board, int x, int y) {
            List<PossibleMove> attackedKingMoves = new List<PossibleMove>();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j].type != FigureType.None 
                        && board[i, j].white == whiteMove && i != x && j != y) {

                        // if (CheckingPossibleMoves(GetPosition(i, j), whiteMove, board).Count != 0) {
                        //     Debug.Log($"{i}  {j}  {x} {y}" );
                        // }
                        attackedKingMoves.AddRange(CheckingPossibleMoves(
                            GetPosition(i, j), whiteMove, board
                            ));
                    }
                }
            }
            Debug.Log(attackedKingMoves.Count);

            return attackedKingMoves;
        }

        public static bool CheckKing(bool whiteMove, Fig[,] board) {
            List<PossibleMove> attackedKingMoves = GetAttackedKingMoves(whiteMove, board);
            foreach (PossibleMove attackMove in attackedKingMoves) {
                
                if (board[attackMove.movePosition.x, attackMove.movePosition.y].
                    type == FigureType.King 
                    && board[attackMove.movePosition.x, attackMove.movePosition.y].
                    white == whiteMove) {

                    board[attackMove.movePosition.x, attackMove.movePosition.y].check = true;
                    return true;
                }
            }
            return false;
        }

        public static bool EnPassant(Position from, Position to, Fig[,] board) {
            if (board[from.x, from.y].type == FigureType.Pawn && board[to.x, to.y].enPassant) {
                board[to.x, to.y] = board[from.x, from.y];

                if (to.x == 5) {
                    board[to.x - 1, to.y].type = FigureType.None;
                    return true;
                }

                if (to.x == 2) {
                    board[to.x + 1, to.y].type = FigureType.None;
                    return true;
                }
            }
            return false;
        }
        public static bool Castling(Position from, Position to, Fig[,] board) {

            if (board[from.x, from.y].type == FigureType.King && board[to.x, to.y].castling) {

                board[to.x, to.y] = board[from.x, from.y];
                board[from.x, from.y].type = FigureType.None;

                if (to.y == 2) {
                    board[to.x, to.y + 1] = board[to.x, to.y - 2];
                    board[to.x, to.y - 2].type = FigureType.None;
                    return true;
                }

                if (to.y == 6) {
                    board[to.x, to.y - 1] = board[to.x, to.y + 1];
                    board[to.x, to.y + 1].type = FigureType.None;
                    return true;
                }
            }
            return false;
        }

        public static bool MoveFigure(Position from, Position to, bool whiteMove, Fig[,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            possibleMoves.AddRange(CheckingPossibleMoves(from, whiteMove, board));

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    board[i, j].enPassant = false;
                    board[i, j].check = false;
                }
            }

            foreach (PossibleMove move in possibleMoves) {
                if (to.x == move.movePosition.x && to.y == move.movePosition.y) {

                    if (board[from.x, from.y].type == FigureType.Pawn) {

                        if (board[from.x, from.y].firstMove) {

                            if (to.x == from.x + 2) {
                                board[from.x + 1, from.y].enPassant = true;
                            }

                            if (to.x == from.x - 2) {
                                board[from.x - 1, from.y].enPassant = true;
                            }
                        }
                    }

                    board[from.x, from.y].firstMove = false;
                    board[to.x, to.y] = board[from.x, from.y];
                    board[from.x, from.y].type = FigureType.None;
                    return true;
                }
            }
            return false;
        }
    }
}

