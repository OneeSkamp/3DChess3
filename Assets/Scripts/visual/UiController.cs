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
        public Position activeFigurePos;
        public List<PossibleMove> possibleMovesList = new List<PossibleMove>();
        public List<GameObject> activeCellObjList = new List<GameObject>();
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

                    yPos-=1f;
                }
                yPos=3.5f;
                xPos-=1f;
            }
        }
        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                CleaningActiveCellObjList();
                possibleMovesList.Clear();
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if(Physics.Raycast(ray, out hit)) {
                    x = Mathf.Abs((int)(hit.point.x - 4));
                    y = Mathf.Abs((int)(hit.point.z - 4));

                    if (figuresMap[x, y] != null) {
                        activeFigurePos = SelectFigure(x, y);
                        GetPossibleMoveList(activeFigurePos);
                        CreatingPossibleMoves();
                    } else {
                        MoveFigure();
                    }
                }
            }
        }

        private void CreatingPossibleMoves() {
            foreach (PossibleMove move in possibleMovesList) {
                activeCellObjList.Add(Instantiate(blueBacklight,
                        new Vector3(3.5f - move.movePosition.x,0.5f, 
                                        3.5f - move.movePosition.y),Quaternion.identity));
            }
        } 

        private Position SelectFigure(int x, int y) {
            
            return ChessEngine.GetPosition(x, y);
        }

        private List<PossibleMove> GetPossibleMoveList(Position position) {
            possibleMovesList.AddRange(ChessEngine.GetPossibleMoves(position, 
                                                                    board.boardMap));
            return possibleMovesList;
        }

        private void CleaningActiveCellObjList() {
            foreach(GameObject cell in activeCellObjList) {
                Destroy(cell);
            }
        }

        private void MoveFigure() {
            
            Debug.Log(activeCellObjList);
            Debug.Log("move");
        }
        private void Start() {
            CreatingFiguresOnBoard(board.boardMap);
            // Position start = ChessEngine.GetPosition(0, 1);
            // Position to = ChessEngine.GetPosition(2, 2);
            
            // Debug.Log(ChessEngine.MoveFigure(start, to, board.boardMap));       
        }
    }
}