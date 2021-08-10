using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using chess;
using visual;

namespace parse {
    public class ParseJson {
        public string filepathBoard;
        public string filepathBoardMap;

        private string[,] jsonBoardMap;
        private string jsonBoard;
        private string jsonAll;

        private void ToJson(Fig[,] boardmap) {
            jsonBoardMap = new string[8,8];
            // jsonBoard = JsonUtility.ToJson(ChessController.board);
            jsonAll = null;

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    string jsonFig = JsonUtility.ToJson(boardmap[i, j]);

                    jsonAll += jsonFig;
                }
            }
        }

        private void FromJson(Fig[,] boardmap) {
            jsonBoardMap = new string[8,8];
            jsonAll = File.ReadAllText(filepathBoardMap);
            jsonBoard = File.ReadAllText(filepathBoard);

            if (jsonAll != null) {
                // board.whiteMove = JsonUtility.FromJson<Board>(jsonBoard).whiteMove;
                int count = 0;
                for (int i = 0; i < 8; i++) {
                    for (int j = 0; j < 8; j++) {

                        for (int x = count; x < jsonAll.Length; x++) {
                            jsonBoardMap[i, j] += jsonAll[x];

                            if (jsonAll[x] == '}') {
                                count = x + 1;
                                break; 
                            }
                        }

                        boardmap[i, j] = JsonUtility.FromJson<Fig>(jsonBoardMap[i, j]);
                    }
                }
            }
        }
    }
}

