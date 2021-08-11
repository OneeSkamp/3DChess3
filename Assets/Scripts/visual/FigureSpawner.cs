using UnityEngine;
using chess;

namespace visual {
    public class FigureSpawner : MonoBehaviour {
        public Transform boardTransform;

        private FigureResurses figCont;
        private ChessController chessController;

        private float changedX;
        private float changedY;
        private float changedZ;

        private const float CONST = 3.5f;

        private void Awake() {
            changedX = boardTransform.position.x;
            changedY = boardTransform.position.y;
            changedZ = boardTransform.position.z;

            figCont = gameObject.GetComponent<FigureResurses>();
            chessController = gameObject.GetComponent<ChessController>();
        }

        private void Start() {
            CreatingFiguresOnBoard(chessController.board.boardMap);
        }

        private void CreatingFiguresOnBoard(Fig[,] board) {
            float xPos = CONST;
            float yPos = CONST;

            for (int i = 0; i < board.GetLength(0); i++) {

                for (int j = 0; j < board.GetLength(1); j++) {
                    var pos = new Vector3(xPos, 0.172f , yPos);
                    var pawnPos = new Vector3(xPos, 0f , yPos);

                    if (board[i, j].type == FigureType.Pawn && !board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bPawn, 
                            pawnPos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Queen && !board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bQueen, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.King && !board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bKing, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Knight && !board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bKnight, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Bishop && !board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bBishop, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Rook && !board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bRook, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Pawn && board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wPawn, 
                            pawnPos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Queen && board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wQueen, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.King && board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wKing, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Knight && board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wKnight, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Bishop && board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wBishop, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (board[i, j].type == FigureType.Rook && board[i, j].white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wRook, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    yPos -= 1f;
                }
                yPos = CONST;
                xPos -= 1f;
            }
        }
    }
}
