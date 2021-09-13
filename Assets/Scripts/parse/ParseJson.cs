using UnityEngine;
using chess;
using option;
using jonson;
using jonson.reflect;

namespace parse {
    public struct Board {
        public Option<Fig> board;
        public bool whiteMove;

        public static Board Mk(Option<Fig> board, bool whiteMove) {
            return new Board { board = board, whiteMove = whiteMove };
        }
    }
    public class ParseJson : MonoBehaviour{
        // private void Awake() {


        //     // JSONType personType = Reflect.ToJSON(person, false);
        //     // string output = Jonson.Generate(personType);

        // }

        // public string filepathBoard;
        // public string filepathBoardMap;

        // private string[,] jsonBoardMap;
        // private string jsonBoard;
        // private string jsonAll;

        // public static void ToJson(bool whiteMove, Option<Fig>[,] board) {

        //     var b = Board.Mk(board[1,1], whiteMove);
        //     JSONType personType = Reflect.ToJSON(b, false);
        //     string output = Jonson.Generate(personType);

        //     Debug.Log(output);
        // }

        // public void ToJson(Option<Fig>[,] boardmap) {
        //     jsonBoardMap = new string[8,8];
        //     jsonBoard = JsonUtility.ToJson(chessController.board);
        //     jsonAll = null;

        //     for (int i = 0; i < 8; i++) {
        //         for (int j = 0; j < 8; j++) {
        //             string jsonFig = JsonUtility.ToJson(boardmap[i, j]);

        //             jsonAll += jsonFig;
        //         }
        //     }
        // }

/*        public void FromJson(Option<Fig>[,] boardmap) {
            jsonBoardMap = new string[8,8];
            jsonAll = File.ReadAllText(filepathBoardMap);
            jsonBoard = File.ReadAllText(filepathBoard);

            if (jsonAll != null) {
                chessController.board.whiteMove = JsonUtility.FromJson<Board>(jsonBoard).whiteMove;
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

                        boardmap[i, j] = JsonUtility.FromJson<Option<Fig>>(jsonBoardMap[i, j]);
                    }
                }
            }
        }*/
    }
}

