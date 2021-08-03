using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using chess;

namespace visual {
    public class ChessController : MonoBehaviour {
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
        public Text endGameText;
        public string checkmate;
        public string stalemate;
        public Button queenBut;
        public Button bishopBut;
        public Button rookBut;
        public Button knightBut;
        public Button newGame;
        public GameObject CheckMateUi;
        public GameObject ChangePawnUi;
        public Position activeFigurePos;
        public List<PossibleMove> possibleMovesList = new List<PossibleMove>();
        public List<GameObject> activeCellObjList = new List<GameObject>();
        public Board board = new Board(true);
        private Fig pawnForChange;
        private GameObject pawnForChangeObj;
         
        public int x;
        public int y;

        private GameObject CheckCell;
        private Ray ray;
        private RaycastHit hit;

        public GameObject [,]  figuresMap = new GameObject[8,8];

        private void CreatingFiguresOnBoard(Fig [,] board) {
                float xPos=3.5f;
                float yPos=3.5f;
            for (int i = 0; i < board.GetLength(0); i++) {

                for (int j = 0; j < board.GetLength(1); j++) {
                    var pos = new Vector3(xPos, 0.5f, yPos);

                    if (board[i, j].type == FigureType.Pawn && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackPawn, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Queen && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackQueen, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.King && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackKing, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Knight && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackKnight, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Bishop && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackBishop, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Rook && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(blackRook, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Pawn && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whitePawn, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Queen && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteQueen, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.King && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteKing, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Knight && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteKnight, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Bishop && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteBishop, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Rook && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(whiteRook, pos, Quaternion.identity);
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
                
                if (Physics.Raycast(ray, out hit)) {
                    x = Mathf.Abs((int)(hit.point.x - 4));
                    y = Mathf.Abs((int)(hit.point.z - 4));

                    if (figuresMap[x, y] != null 
                        && board.boardMap[x, y].white == board.whiteMove) {
                        activeFigurePos = SelectFigure(x, y);
                        GetPossibleMoveList(activeFigurePos);
                        CreatingPossibleMoves();

                    } else {
                        Move move = new Move();
                        move.from = activeFigurePos;
                        x = Mathf.Abs((int)(hit.point.x - 4));
                        y = Mathf.Abs((int)(hit.point.z - 4));
                        move.to = SelectFigure(x, y);

                        MoveFigure(move, board.boardMap);
                    }
                }
            }
        }

        private void CreatingPossibleMoves() {

            foreach (PossibleMove move in possibleMovesList) {
                activeCellObjList.Add(
                    Instantiate(
                        blueBacklight,
                        new Vector3(3.5f - move.movePosition.x,0.515f, 3.5f - move.movePosition.y),
                        Quaternion.Euler(90, 0, 0)
                    )
                );
            }
        } 

        private Position SelectFigure(int x, int y) {

            return ChessEngine.GetPosition(x, y);
        }

        private List<PossibleMove> GetPossibleMoveList(Position position) {
            possibleMovesList.AddRange(
                ChessEngine.CheckingPossibleMoves(position, board.whiteMove, board.boardMap)
                );
            return possibleMovesList;
        }
        private void CleaningCheckCell() {
            if (CheckCell != null) {
                Destroy(CheckCell);
            }
        }

        private void CleaningActiveCellObjList() {
            foreach(GameObject cell in activeCellObjList) {
                Destroy(cell);
            }
        }

        private void MoveFigure(Move move, Fig[,] boardMap) {
            GameObject figureForMove = figuresMap[move.from.x, move.from.y];

            if (ChessEngine.Castling(move, boardMap)) {
                figuresMap[move.to.x, move.to.y] = figuresMap[move.from.x, move.from.y];
                figuresMap[move.from.x, move.from.y] = null;
                Vector3 newPos = new Vector3(3.5f - move.to.x, 0.5f, 3.5f - move.to.y);

                if (figureForMove != null) {
                figureForMove.transform.position = newPos;
                }

                if (move.to.y == 2) {
                    var newRookPos = new Vector3(3.5f - move.to.x, 0.5f, 3.5f - move.to.y -1);
                    figuresMap[move.to.x, move.to.y + 1] = figuresMap[move.to.x, move.to.y - 2];
                    figuresMap[move.to.x, move.to.y + 1].transform.position = newRookPos;
                    figuresMap[move.to.x, move.to.y - 2] = null;
                }

                if (move.to.y == 6) {
                    var newRookPos = new Vector3(3.5f - move.to.x, 0.5f, 3.5f - move.to.y + 1);
                    figuresMap[move.to.x, move.to.y - 1] = figuresMap[move.to.x, move.to.y + 1];
                    figuresMap[move.to.x, move.to.y - 1].transform.position = newRookPos;
                    figuresMap[move.to.x, move.to.y + 1] = null;
                }
                board.whiteMove = !board.whiteMove;
                Debug.Log("castling");
            }

            if (ChessEngine.EnPassant(move, boardMap)) {
                figuresMap[move.to.x, move.to.y] = figuresMap[move.from.x, move.from.y];
                figuresMap[move.from.x, move.from.y] = null;
                Vector3 newPos = new Vector3(3.5f - move.to.x, 0.5f, 3.5f - move.to.y);

                if (figureForMove != null) {
                    figureForMove.transform.position = newPos;
                }

                if (move.to.x == 5) {
                    Destroy(figuresMap[move.to.x - 1, move.to.y]);
                }

                if (move.to.x == 2) {
                    Destroy(figuresMap[move.to.x + 1, move.to.y]);
                }
                board.whiteMove = !board.whiteMove;
                Debug.Log("enPassant");
            }

            if (ChessEngine.MoveFigure(move, board.whiteMove, boardMap)) {
                CleaningCheckCell();

                if (figuresMap[move.to.x, move.to.y] != null) {
                    Destroy(figuresMap[move.to.x, move.to.y]);
                }

                figuresMap[move.to.x, move.to.y] = figuresMap[move.from.x, move.from.y];
                figuresMap[move.from.x, move.from.y] = null;

                Vector3 newPos = new Vector3(3.5f - move.to.x, 0.5f, 3.5f - move.to.y);
                if (figureForMove != null) {
                    figureForMove.transform.position = newPos;
                }

                board.whiteMove = !board.whiteMove;

                if (ChessEngine.CheckKing(board.whiteMove, boardMap)) {

                    List<PossibleMove> list = new List<PossibleMove>();
                    list.AddRange(ChessEngine.Сheckmate(
                            board.whiteMove, 
                            boardMap, 
                            move.to.x, 
                            move.to.y)
                        );

                    if (list.Count == 0 && ChessEngine.CheckKing(board.whiteMove, boardMap)) {
                        endGameText.text = checkmate;
                        CheckMateUi.SetActive(true);
                        Debug.Log("shahimat");
                    }

                    if (list.Count == 0 && !ChessEngine.CheckKing(board.whiteMove, boardMap)) {
                        endGameText.text = stalemate;
                        CheckMateUi.SetActive(true);
                        Debug.Log("pat");
                    }

                    for (int i = 0; i < 8; i++) {
                        for (int j = 0; j < 8; j++) {

                            if (boardMap[i, j].check) {
                                CheckCell = Instantiate(
                                    redBacklight,
                                    new Vector3(3.5f - i, 0.515f, 3.5f - j), 
                                    Quaternion.Euler(90, 0, 0)
                                );
                            }
                        }
                    }
                }
            }
        }

        private void Start() {
            CreatingFiguresOnBoard(board.boardMap);
        }
    }
}