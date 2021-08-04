using System;
using System.Collections.Generic;
using UnityEngine;

namespace chess {
    public struct Position {
        public int x;
        public int y;
    }

    public struct PossibleMove {
        public Position movePos;
    }

    public static class ChessEngine {
        private static Position GetPosition(int x, int y) {
            Position position = new Position();
            position.x = x;
            position.y = y;
            return position;
        }

        private static PossibleMove GetPossibleMove(Position to) {
            PossibleMove possibleMove = new PossibleMove();
            possibleMove.movePos = GetPosition(to.x, to.y);

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

        private static List<PossibleMove> GetOnePosMoves(Position from, Dir dir, Fig[,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            if (OnBoard(GetPosition(from.x + dir.x, from.y + dir.y))) {
                Fig myElement = board[from.x, from.y];
                Fig nextElement = board[from.x + dir.x, from.y + dir.y];
                Position nextElementPos = GetPosition(from.x + dir.x, from.y + dir.y);

                if (nextElement.type == FigureType.None) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                }

                if (IsColorful(myElement, nextElement)) {
                    possibleMoves.Add(GetPossibleMove(nextElementPos));
                }
            }

            return possibleMoves;
        }

        private static List<PossibleMove> GetCastlingMoves(Position from, Fig[,] board) {

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

        private static List<PossibleMove> GetEnPassantMoves(Position from, Fig[,] board) {
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

        private static List<PossibleMove> GetLineMoves(Position from, Dir dir, Fig [,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>(); 

            Fig myElement = board[from.x, from.y];
            for (int i = 1; i < 8; i++) {

                int x = from.x + i * dir.x;
                int y = from.y + i * dir.y;

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

        private static List<PossibleMove> CheckingPossibleMoves(Position startPos, Fig[,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            Fig figure = board[startPos.x, startPos.y];

            switch(figure.type) {
                case FigureType.Bishop :
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(1, 1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(1, -1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(-1, 1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(-1, -1), board));
                    return possibleMoves;

                case FigureType.Rook :
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(1, 0), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(0, 1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(-1, 0), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(0, -1), board));
                    return possibleMoves;

                case FigureType.Knight :
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(2, 1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(2, -1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(-2, 1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(-2, -1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(1, 2), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(-1, 2), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(1, -2), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(-1, -2), board));
                    return possibleMoves;

                case FigureType.Queen :
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(1, 1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(1, -1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(-1, 1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(-1, -1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(1, 0), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(0, 1), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(-1, 0), board));
                    possibleMoves.AddRange(GetLineMoves(startPos, Dir.NewDir(0, -1), board));
                    return possibleMoves;

                case FigureType.King :
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(1, 0), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(0, 1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(1, 1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(-1, -1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(1, -1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(-1, 1), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(-1, 0), board));
                    possibleMoves.AddRange(GetOnePosMoves(startPos, Dir.NewDir(0, -1), board));
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

        public static List<PossibleMove> GetPossibleMoves(
                Position startPos, 
                bool whiteMove, 
                Fig[,] board
                )
            {
            List<PossibleMove>  possibleMoves = CheckingPossibleMoves(startPos, board);
            List<PossibleMove>  test = new List<PossibleMove>();

                foreach (PossibleMove move in possibleMoves) {
                    Fig[,] boardForCheck = (Fig[,])board.Clone();
                    boardForCheck[startPos.x, startPos.y].firstMove = false;
                    boardForCheck[
                        move.movePos.x, 
                        move.movePos.y
                    ] = boardForCheck[startPos.x, startPos.y];

                    boardForCheck[startPos.x, startPos.y].type = FigureType.None;

                    if (!IsCheckKing(whiteMove, boardForCheck)) {
                        test.Add(move);
                    }

                }
            return test;
        }

        private static List<PossibleMove> GetAttackedKingMoves(bool whiteMove, Fig[,] board) {
            List<PossibleMove> attackedKingMoves = new List<PossibleMove>();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j].white != whiteMove) {
                        attackedKingMoves.AddRange(CheckingPossibleMoves(GetPosition(i, j), board));
                    }
                }
            }
            return attackedKingMoves;
        }

        public static List<PossibleMove> GetDefenceMoves(bool whiteMove, Fig[,] board, Dir dir) {
            List<PossibleMove> defenceKingMoves = new List<PossibleMove>();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    if (board[i, j].type != FigureType.None 
                        && board[i, j].white == whiteMove && i != dir.x && j != dir.y) {

                        defenceKingMoves.AddRange(GetPossibleMoves(
                            GetPosition(i, j), whiteMove, board
                            ));
                    }
                }
            }

            return defenceKingMoves;
        }

        public static bool IsCheckKing(bool whiteMove, Fig[,] board) {
            List<PossibleMove> attackedKingMoves = GetAttackedKingMoves(whiteMove, board);
            foreach (PossibleMove attackMove in attackedKingMoves) {

                if (board[attackMove.movePos.x, attackMove.movePos.y].
                    type == FigureType.King 
                    && board[attackMove.movePos.x, attackMove.movePos.y].
                    white == whiteMove) {

                    board[attackMove.movePos.x, attackMove.movePos.y].check = true;
                    return true;
                }
            }
            return false;
        }

        public static bool IsEnPassant(Move move, Fig[,] board) {
            var myFigType = board[move.from.x, move.from.y].type;

            if (myFigType == FigureType.Pawn && board[move.to.x, move.to.y].enPassant) {
                return true;
            }

            return false;
        }

        public static EnPassantMove EnPassantMove(Move move, Fig[,] board) {
            var enPassantMove = new EnPassantMove();

            if (IsEnPassant(move, board)) {
                board[move.to.x, move.to.y] = board[move.from.x, move.from.y];

                if (move.to.x == 5) {
                    enPassantMove.pawnPos = GetPosition(move.to.x - 1, move.to.y);
                    board[move.to.x - 1, move.to.y].type = FigureType.None;
                }

                if (move.to.x == 2) {
                    enPassantMove.pawnPos = GetPosition(move.to.x + 1, move.to.y);
                    board[move.to.x + 1, move.to.y].type = FigureType.None;
                }
            }
            return enPassantMove;
        }

        public static bool IsCastling(Move move, Fig[,] board) {
            var myFigType = board[move.from.x, move.from.y].type;

            if (myFigType == FigureType.King && board[move.to.x, move.to.y].castling) {
                return true;
            }
            return false;
        }

        public static CastlingMove CastlingMove(Move move, Fig[,] board) {
            var castlingMove = new CastlingMove();

            if (IsCastling(move, board)) {

                board[move.to.x, move.to.y] = board[move.from.x, move.from.y];
                board[move.from.x, move.from.y].type = FigureType.None;

                if (move.to.y == 2) {
                    castlingMove.oldRookPos = GetPosition(move.to.x, move.to.y - 2);
                    castlingMove.newRookPos = GetPosition(move.to.x, move.to.y + 1);

                    board[move.to.x, move.to.y + 1] = board[move.to.x, move.to.y - 2];
                    board[move.to.x, move.to.y - 2].type = FigureType.None;
                }

                if (move.to.y == 6) {
                    castlingMove.oldRookPos = GetPosition(move.to.x, move.to.y + 1);
                    castlingMove.newRookPos = GetPosition(move.to.x, move.to.y - 1);

                    board[move.to.x, move.to.y - 1] = board[move.to.x, move.to.y + 1];
                    board[move.to.x, move.to.y + 1].type = FigureType.None;
                }
            }
            return castlingMove;
        }

        public static Nullable<Position> FindChangePawn(Fig[,] board) {

            Nullable<Position> pos = new Nullable<Position>();
            pos = null;

            for (int i = 0; i < 8; i++) {
                if (board[0, i].type == FigureType.Pawn) {
                    pos = GetPosition(0, i);
                }
            }

            for (int i = 0; i < 8; i++) {
                if (board[7, i].type == FigureType.Pawn) {
                    pos = GetPosition(7, i);
                }
            }

            return pos;
        }

        public static void ChangePawn(FigureType type, Position pawnPos, Fig[,] board) {

            switch(type) {
                case FigureType.Queen:
                    board[pawnPos.x, pawnPos.y].type = FigureType.Queen;
                    break;
                case FigureType.Bishop:
                    board[pawnPos.x, pawnPos.y].type = FigureType.Bishop;
                    break;
                case FigureType.Rook:
                    board[pawnPos.x, pawnPos.y].type = FigureType.Rook;
                    break;
                case FigureType.Knight:
                    board[pawnPos.x, pawnPos.y].type = FigureType.Knight;
                    break;
            }
        }

        public static bool MoveFigure(Move move, bool whiteMove, Fig[,] board) {
            List<PossibleMove> possibleMoves = new List<PossibleMove>();
            possibleMoves.AddRange(CheckingPossibleMoves(move.from, board));

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    board[i, j].enPassant = false;
                    board[i, j].check = false;
                }
            }

            foreach (PossibleMove posMove in possibleMoves) {
                if (move.to.x == posMove.movePos.x && move.to.y == posMove.movePos.y) {

                    if (board[move.from.x, move.from.y].type == FigureType.Pawn) {

                        if (board[move.from.x, move.from.y].firstMove) {

                            if (move.to.x == move.from.x + 2) {
                                board[move.from.x + 1, move.from.y].enPassant = true;
                            }

                            if (move.to.x == move.from.x - 2) {
                                board[move.from.x - 1, move.from.y].enPassant = true;
                            }
                        }
                    }

                    board[move.from.x, move.from.y].firstMove = false;
                    board[move.to.x, move.to.y] = board[move.from.x, move.from.y];
                    board[move.from.x, move.from.y].type = FigureType.None;
                    return true;
                }
            }
            return false;
        }
    }
}

