using System;
using UnityEngine;

namespace chess {
    public class Board : MonoBehaviour{
        public string fen;
        public static char[,] boardMap;

        public Board(string fen){
            this.fen = fen;
            boardMap = new char[8, 8];
            int fenCount = 0;

            for (int i = 0; i < boardMap.GetLength(0); i++) {
                for (int j = 0; j < boardMap.GetLength(1); j++) {
                    
                    if (fen[fenCount] == '/') {
                        Debug.Log($"+ {fen[fenCount]}");
                        fenCount++;
                    } 
                    
                    if (Char.IsDigit(fen[fenCount])) {
                        
                        for(int k = j; k < Int32.Parse(fen[fenCount].ToString()); k++) {
                            boardMap[i, k] = '.';
                            j = k;
                        }
                        fenCount++;
                    } 

                    if (!Char.IsDigit(fen[fenCount])&&fen[fenCount] != '/') {
                        boardMap[i, j] = fen[fenCount];
                        fenCount++;
                        
                    }
                }
            }
        }

        public static char[, ] GetBoardMap() {
            return boardMap;
        }

        public static int GetBoardEdgeLen() {
            return (int)Math.Sqrt(boardMap.Length);
        }

        public static bool OnBoard(ChessEngine.Position pos) {
            int lenBoard = GetBoardEdgeLen();

            if (pos.x < 0 || pos.y < 0 || pos.x >= lenBoard || pos.y >= lenBoard) {
                return false;
            }

            return true;
        }
    }
}

