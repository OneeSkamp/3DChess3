using System.Collections.Generic;
using UnityEngine;
using chess;

namespace visual {
    public class UiController : MonoBehaviour {
        public GameObject blueBacklight;
        public GameObject redBacklight;
        public GameObject blackPawn; 
        public GameObject blackKing; 
        public GameObject blackRook; 
        public GameObject blackKnight; 
        public GameObject blackBishop; 
        public GameObject blackQueen; 
        public GameObject whitePawn; 
        public GameObject whiteKing;
        public GameObject whiteRook; 
        public GameObject whiteKnight; 
        public GameObject whiteBishop; 
        public GameObject whiteQueen;
        public Board board = new Board();
        //public Board board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"); 
        public int x;
        public int y;
        private Ray ray;
        private RaycastHit hit;

        public GameObject [,]  figuresMap = new GameObject[8,8];

        private void CreatingFiguresOnBoard(Fig [,] board) {
                float xPos=3.5f;
                float yPos=3.5f;
            for (int i = 0; i < board.GetLength(0); i++) {

                for (int j = 0; j < board.GetLength(1); j++) {
                    if (board[i, j].type == figureType.Pawn && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackPawn,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.Queen && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackQueen,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.King && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackKing,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.Knight && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackKnight,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.Bishop && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackBishop,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.Rook && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackRook,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    
                    if (board[i, j].type == figureType.Pawn && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whitePawn,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.Queen && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteQueen,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.King && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteKing,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.Knight && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteKnight,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.Bishop && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteBishop,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    if (board[i, j].type == figureType.Rook && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteRook,
                        new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    }
                    // switch (board[i, j]) {
                    //     case 'p':
                    //         figuresMap[i, j] = Instantiate(blackPawn,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'q':
                    //         figuresMap[i, j] = Instantiate(blackQueen,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'k':
                    //         figuresMap[i, j] = Instantiate(blackKing,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'n':
                    //         figuresMap[i, j] = Instantiate(blackKnight,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'r':
                    //         figuresMap[i, j] = Instantiate(blackRook,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'b':
                    //         figuresMap[i, j] = Instantiate(blackBishop,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'P':
                    //         figuresMap[i, j] = Instantiate(whitePawn,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'Q':
                    //         figuresMap[i, j] = Instantiate(whiteQueen,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'K':
                    //         figuresMap[i, j] = Instantiate(whiteKing,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'N':
                    //         figuresMap[i, j] = Instantiate(whiteKnight,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'R':
                    //         figuresMap[i, j] = Instantiate(whiteRook,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    //     case 'B':
                    //         figuresMap[i, j] = Instantiate(whiteBishop,
                    //         new Vector3(xPos,0.5f,yPos),Quaternion.identity);
                    //         break;
                    // }
                    yPos-=1f;
                }
                yPos=3.5f;
                xPos-=1f;
            }
        }
        private void Update() {
            if (Input.GetMouseButtonDown(0))
            Debug.Log("Pressed primary button.");
        }

        private void Start() {
            CreatingFiguresOnBoard(board.boardMap);
            Position start = ChessEngine.GetPosition(0, 1);
            Position to = ChessEngine.GetPosition(2, 2);
            
            Debug.Log(ChessEngine.MoveFigure(start, to, board.boardMap));       
        }
    }
}