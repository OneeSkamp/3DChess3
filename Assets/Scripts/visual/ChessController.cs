using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using chess;

namespace visual {
    public class ChessController : MonoBehaviour {
        FigureContainer figCont;
        public Transform boardTransform;
        public Text endGameText;
        public string checkmate;
        public string stalemate;
        public Button queenBut;
        public Button bishopBut;
        public Button rookBut;
        public Button knightBut;
        public GameObject CheckMateUi;
        public GameObject ChangePawnUi;
        public Position activeFigurePos;
        public List<PossibleMove> possibleMovesList = new List<PossibleMove>();
        public List<GameObject> activeCellObjList = new List<GameObject>();
        public Board board = new Board(true);
        const float CONST = 3.5f;
        public int x;
        public int y;

        private GameObject CheckCell;
        private Ray ray;
        private RaycastHit hit;

        public GameObject[,]  figuresMap = new GameObject[8,8];

        private void CreatingFiguresOnBoard(Fig[,] board) {
                float xPos = CONST;
                float yPos = CONST;
            for (int i = 0; i < board.GetLength(0); i++) {

                for (int j = 0; j < board.GetLength(1); j++) {
                    var pos = new Vector3(xPos, 0.5f, yPos);

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
            if (!ChangePawnUi.activeSelf) {
                if (Input.GetMouseButtonDown(0)) {

                    foreach(GameObject cell in activeCellObjList) {
                        Destroy(cell);
                    }

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
        }

        private void CreatingPossibleMoves() {

            foreach (PossibleMove move in possibleMovesList) {
                activeCellObjList.Add(
                    Instantiate(
                        figCont.blueBacklight,
                        new Vector3(CONST - move.movePos.x, 0.515f, CONST - move.movePos.y),
                        Quaternion.Euler(90, 0, 0)
                    )
                );
            }
        }

        private Position SelectFigure(int x, int y) {
            Position figPos = new Position();
            figPos.x = x;
            figPos.y = y;
            return figPos;
        }

        private List<PossibleMove> GetPossibleMoveList(Position position) {
            possibleMovesList.AddRange(
                ChessEngine.GetPossibleMoves(position, board.whiteMove, board.boardMap)
                );
            return possibleMovesList;
        }

        private void Relocation (Move move) {
            GameObject figureForMove = figuresMap[move.from.x, move.from.y];
            figuresMap[move.to.x, move.to.y] = figuresMap[move.from.x, move.from.y];
            figuresMap[move.from.x, move.from.y] = null;
            Vector3 newPos = new Vector3(CONST - move.to.x, 0.5f, CONST - move.to.y);

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

                var newRookPos = new Vector3(CONST - newPos.x, 0.5f, CONST - newPos.y);
                figuresMap[newPos.x, newPos.y] = figuresMap[oldPos.x, oldPos.y];
                figuresMap[newPos.x, newPos.y].transform.position = newRookPos;
                figuresMap[oldPos.x, oldPos.y] = null;

                board.whiteMove = !board.whiteMove;
            }

            if (ChessEngine.IsEnPassant(move, boardMap)) {
                var enPassantMove = ChessEngine.EnPassantMove(move, boardMap);
                var pawnForDesroyPos = enPassantMove.pawnPos;

                Relocation(move);

                Destroy(figuresMap[pawnForDesroyPos.x, pawnForDesroyPos.y]);

                board.whiteMove = !board.whiteMove;
            }

            if (ChessEngine.MoveFigure(move, board.whiteMove, boardMap)) {

                if (CheckCell != null) {
                    Destroy(CheckCell);
                }

                if (ChessEngine.FindChangePawn(boardMap) != null) {
                    ChangePawnUi.SetActive(true);
                }

                if (figuresMap[move.to.x, move.to.y] != null) {
                    Destroy(figuresMap[move.to.x, move.to.y]);
                }

                Relocation(move);

                board.whiteMove = !board.whiteMove;

                if (ChessEngine.IsCheckKing(board.whiteMove, boardMap)) {
                    var defenceKingMoves = new List<PossibleMove>();
                    var dir = Dir.NewDir(move.to.x, move.to.y);
                    var defenceList = ChessEngine.GetDefenceMoves(board.whiteMove, boardMap, dir);
                    var checkKing = ChessEngine.IsCheckKing(board.whiteMove, boardMap);

                    defenceKingMoves.AddRange(defenceList);

                    if (defenceKingMoves.Count == 0 && checkKing) {
                        endGameText.text = checkmate;
                        CheckMateUi.SetActive(true);
                    }

                    if (defenceKingMoves.Count == 0 && !checkKing) {
                        endGameText.text = stalemate;
                        CheckMateUi.SetActive(true);
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
                            new Vector3(CONST - i, 0.515f, CONST - j), 
                            Quaternion.Euler(90, 0, 0)
                        );
                    }
                }
            }
        }

        private void ChangePawn(GameObject wFig, GameObject bFig, FigureType type) {
            var pawnPos = (Position)ChessEngine.FindChangePawn(board.boardMap);

            Destroy(figuresMap[pawnPos.x, pawnPos.y]);

            if (pawnPos.x == 0) {
                var newPos = new Vector3 (CONST - pawnPos.x, 0.5f, CONST - pawnPos.y);
                figuresMap[pawnPos.x, pawnPos.y] = Instantiate(wFig, newPos, Quaternion.identity);
            }

            if (pawnPos.x == 7) {
                var newPos = new Vector3 (CONST - pawnPos.x, 0.5f, CONST - pawnPos.y);
                figuresMap[pawnPos.x, pawnPos.y] = Instantiate(bFig, newPos, Quaternion.identity);
            }

            ChessEngine.ChangePawn(type, pawnPos, board.boardMap);
            ChangePawnUi.SetActive(false);

            ChessEngine.IsCheckKing(board.whiteMove, board.boardMap);
            InstantiateCheckKingCell();
        }

        private void Start() {
            figCont = gameObject.GetComponent<FigureContainer>();
            var queen = FigureType.Queen;
            var bishop = FigureType.Bishop;
            var rook = FigureType.Rook;
            var knight = FigureType.Knight;

            CreatingFiguresOnBoard(board.boardMap);
            queenBut.onClick.AddListener(() => ChangePawn(figCont.wQueen, figCont.bQueen, queen));

            bishopBut.onClick.AddListener(() => ChangePawn(
                    figCont.wBishop, 
                    figCont.bBishop, 
                    bishop)
                );

            rookBut.onClick.AddListener(() => ChangePawn(figCont.wRook, figCont.bRook, rook));

            knightBut.onClick.AddListener(() => ChangePawn(
                    figCont.wKnight, 
                    figCont.bKnight, 
                    knight)
                );
        }
    }
}