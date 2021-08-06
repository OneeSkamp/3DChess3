using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using chess;

namespace visual {
    public class ChessController : MonoBehaviour {
        private FigureContainer figCont;
        private UiController uiController;
        public Transform boardTransform;
        public Position activeFigurePos;
        public List<Position> possibleMovesList = new List<Position>();
        private List<GameObject> activeCellObjList = new List<GameObject>();
        public Board board = new Board(true);

        private const float CONST = 3.5f;
        private float changedX;
        private float changedY;
        private float changedZ;
        private int x;
        private int y;

        private string[,] jsonBoardMap;
        private string jsonBoard;

        private GameObject CheckCell;
        private Ray ray;
        private RaycastHit hit;

        public GameObject[,]  figuresMap = new GameObject[8,8];

        private void CreatingFiguresOnBoard(Fig[,] board) {
            float xPos = CONST;
            float yPos = CONST;

            for (int i = 0; i < board.GetLength(0); i++) {

                for (int j = 0; j < board.GetLength(1); j++) {
                    var pos = new Vector3(xPos + changedX, 0.5f + changedY, yPos + changedZ);

                    if (board[i, j].type == FigureType.Pawn && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.bPawn, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Queen && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.bQueen, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.King && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.bKing, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Knight && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.bKnight, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Bishop && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.bBishop, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Rook && !board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.bRook, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Pawn && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.wPawn, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Queen && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.wQueen, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.King && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.wKing, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Knight && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.wKnight, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Bishop && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.wBishop, pos, Quaternion.identity);
                    }

                    if (board[i, j].type == FigureType.Rook && board[i, j].white) {
                        figuresMap[i, j] = Instantiate(figCont.wRook, pos, Quaternion.identity);
                    }

                    yPos -= 1f;
                }
                yPos = CONST;
                xPos -= 1f;
            }
        }

        private void Update() {
            if (!uiController.ChangePawnUi.activeSelf && !uiController.MenuUi.activeSelf) {
                if (Input.GetMouseButtonDown(0)) {

                    foreach(GameObject cell in activeCellObjList) {
                        Destroy(cell);
                    }

                    possibleMovesList.Clear();
                    ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit)) {
                        x = Mathf.Abs((int)(hit.point.x - changedX - 4));
                        y = Mathf.Abs((int)(hit.point.z - changedZ - 4));

                        if (ChessEngine.OnBoard(new Position(x, y))  
                            && figuresMap[x, y] != null 
                            && board.boardMap[x, y].white == board.whiteMove) {

                            activeFigurePos = SelectFigure(x, y);
                            GetPossibleMoveList(activeFigurePos);
                            CreatingPossibleMoves();

                        } else {
                            Move move = new Move();
                            move.from = activeFigurePos;
                            x = Mathf.Abs((int)(hit.point.x - changedX - 4));
                            y = Mathf.Abs((int)(hit.point.z - changedZ - 4));
                            move.to = SelectFigure(x, y);

                            MoveFigure(move, board.boardMap);
                        }
                    }
                }
            }
        }

        private void CreatingPossibleMoves() {

            foreach (Position move in possibleMovesList) {
                activeCellObjList.Add(Instantiate(
                        figCont.blueBacklight,
                        new Vector3(
                            CONST - move.x + changedX, 
                            0.515f + changedY, 
                            CONST - move.y + changedZ
                        ),
                        Quaternion.Euler(90, 0, 0)
                    ));
            }
        }

        private Position SelectFigure(int x, int y) {
            Position figPos = new Position();
            figPos.x = x;
            figPos.y = y;
            return figPos;
        }

        private List<Position> GetPossibleMoveList(Position position) {
            possibleMovesList.AddRange(
                ChessEngine.GetPossibleMoves(position, board.whiteMove, board.boardMap)
                );
            return possibleMovesList;
        }

        private void Relocation (Move move) {
            GameObject figureForMove = figuresMap[move.from.x, move.from.y];
            figuresMap[move.to.x, move.to.y] = figuresMap[move.from.x, move.from.y];
            figuresMap[move.from.x, move.from.y] = null;
            Vector3 newPos = new Vector3(
                    CONST - move.to.x + changedX, 
                    0.5f + changedY, 
                    CONST - move.to.y + changedZ
                );

            if (figureForMove != null) {
                figureForMove.transform.position = newPos;
            }
        }

        private void MoveFigure(Move move, Fig[,] boardMap) {
            FigureType figureType = boardMap[move.from.x, move.from.y].type;

            if (ChessEngine.IsCastling(move, boardMap)) {
                var castlingMove = ChessEngine.CastlingMove(move, boardMap);
                var oldPos = castlingMove.oldRookPos;
                var newPos = castlingMove.newRookPos;

                Relocation(move);

                var newRookPos = new Vector3(
                        CONST - newPos.x + changedX, 
                        0.5f + changedY, 
                        CONST - newPos.y + changedZ
                    );

                figuresMap[newPos.x, newPos.y] = figuresMap[oldPos.x, oldPos.y];
                figuresMap[newPos.x, newPos.y].transform.position = newRookPos;
                figuresMap[oldPos.x, oldPos.y] = null;

                board.whiteMove = !board.whiteMove;
            }

            if (ChessEngine.IsEnPassant(move, boardMap)) {
                var pawnForDesroyPos = ChessEngine.EnPassantMove(move, boardMap);

                Relocation(move);

                Destroy(figuresMap[pawnForDesroyPos.x, pawnForDesroyPos.y]);

                board.whiteMove = !board.whiteMove;
            }

            var moveRes = ChessEngine.MoveFigure(move, board.whiteMove, boardMap);

            if (moveRes.error != MoveError.ImpossibleMove) {

                if (CheckCell != null) {
                    Destroy(CheckCell);
                }

                if (ChessEngine.FindChangePawn(boardMap) != null) {
                    uiController.ChangePawnUi.SetActive(true);
                }

                if (figuresMap[moveRes.position.x, moveRes.position.y] != null) {
                    Destroy(figuresMap[moveRes.position.x, moveRes.position.y]);
                }

                Relocation(move);

                board.whiteMove = !board.whiteMove;

                if (ChessEngine.IsCheckKing(board.whiteMove, boardMap)) {
                    var defenceKingMoves = new List<Position>();
                    var dir = Dir.NewDir(moveRes.position.x, moveRes.position.y);
                    var defenceList = ChessEngine.GetDefenceMoves(board.whiteMove, boardMap, dir);
                    var checkKing = ChessEngine.IsCheckKing(board.whiteMove, boardMap);

                    defenceKingMoves.AddRange(defenceList);

                    if (defenceKingMoves.Count == 0 && checkKing) {
                        uiController.endGameText.text = uiController.checkmate;
                        uiController.CheckMateUi.SetActive(true);
                    }

                    if (defenceKingMoves.Count == 0 && !checkKing) {
                        uiController.endGameText.text = uiController.stalemate;
                        uiController.CheckMateUi.SetActive(true);
                    }

                    InstantiateCheckKingCell();

                }
            }
        }

        private void InstantiateCheckKingCell() {
            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {

                    if (board.boardMap[i, j].check) {
                        CheckCell = Instantiate(
                            figCont.redBacklight,
                            new Vector3(
                                CONST - i + changedX, 
                                0.515f + changedY, 
                                CONST - j + changedZ
                            ), 
                            Quaternion.Euler(90, 0, 0)
                        );
                    }
                }
            }
        }

        public void ChangePawn(GameObject wFig, GameObject bFig, FigureType type) {
            var pawnPos = (Position)ChessEngine.FindChangePawn(board.boardMap);

            Destroy(figuresMap[pawnPos.x, pawnPos.y]);

            if (pawnPos.x == 0) {
                var newPos = new Vector3 (
                        CONST - pawnPos.x + changedX, 
                        0.5f + changedY, 
                        CONST - pawnPos.y + changedZ
                    );
                figuresMap[pawnPos.x, pawnPos.y] = Instantiate(wFig, newPos, Quaternion.identity);
            }

            if (pawnPos.x == 7) {
                var newPos = new Vector3 (
                        CONST - pawnPos.x + changedX, 
                        0.5f + changedY, 
                        CONST - pawnPos.y + changedZ
                    );
                figuresMap[pawnPos.x, pawnPos.y] = Instantiate(bFig, newPos, Quaternion.identity);
            }

            ChessEngine.ChangePawn(type, pawnPos, board.boardMap);
            uiController.ChangePawnUi.SetActive(false);

            ChessEngine.IsCheckKing(board.whiteMove, board.boardMap);
            InstantiateCheckKingCell();
        }

        private void ToJson(Fig[,] boardmap) {
            jsonBoardMap = new string[8,8];
            jsonBoard = JsonUtility.ToJson(board);

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j < 8; j++) {
                    string jsonFig = JsonUtility.ToJson(boardmap[i, j]);
                    jsonBoardMap[i, j] = jsonFig;
                }
            }

        }

        private void FromJson(Fig[,] boardmap) {
            if (jsonBoardMap != null) {
                board.whiteMove = JsonUtility.FromJson<Board>(jsonBoard).whiteMove;

                for (int i = 0; i < 8; i++) {
                    for (int j = 0; j < 8; j++) {
                        boardmap[i, j] = JsonUtility.FromJson<Fig>(jsonBoardMap[i, j]);
                    }
                }

            }
        }

        public void OpenMenu() {
            uiController.MenuUi.SetActive(!uiController.MenuUi.activeSelf);
        }

        public void SaveGame() {
            ToJson(board.boardMap);
            OpenMenu();
        }

        public void LoadGame() {
            FromJson(board.boardMap);

            foreach (GameObject figure in figuresMap) {
                Destroy(figure);
            }

            CreatingFiguresOnBoard(board.boardMap);
            OpenMenu();
        }

        public void NewGame() {
            board = new Board(true);

            foreach (GameObject figure in figuresMap) {
                Destroy(figure);
            }

            Destroy(CheckCell);
            CreatingFiguresOnBoard(board.boardMap);

            if (uiController.MenuUi.activeSelf) {
                uiController.MenuUi.SetActive(false);
            }

            uiController.CheckMateUi.SetActive(false);
        }

        private void Start() {

            figCont = gameObject.GetComponent<FigureContainer>();
            uiController = gameObject.GetComponent<UiController>();

            changedX = boardTransform.position.x;
            changedY = boardTransform.position.y;
            changedZ = boardTransform.position.z;

            CreatingFiguresOnBoard(board.boardMap);
        }
    }
}