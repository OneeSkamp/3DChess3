using UnityEngine;
using option;
using chess;
using board;

namespace visual {
    public class FigureSpawner : MonoBehaviour {
        public Transform boardTransform;

        public const float CONST = 5.3f;

        private FigureResurses figCont;
        private ChessController chessController;

        private float changedX;
        private float changedY;
        private float changedZ; 

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

        private void CreatingFiguresOnBoard(Option<Fig>[,] board) {
            float xPos = CONST;
            float yPos = CONST;

            for (int i = 0; i < board.GetLength(0); i++) {

                for (int j = 0; j < board.GetLength(1); j++) {
                    var pos = new Vector3(xPos + changedX, 0.172f , yPos + changedZ);
                    var pawnPos = new Vector3(xPos + changedX, 0f , yPos + changedZ);
                    var fig = board[i, j].Peel();

                    if (fig.type == FigureType.Pawn && !fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bPawn, 
                            pawnPos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Queen && !fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bQueen, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.King && !fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bKing, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Knight && !fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bKnight, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Bishop && !fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bBishop, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Rook && !fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.bRook, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Pawn && fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wPawn, 
                            pawnPos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Queen && fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wQueen, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.King && fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wKing, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Knight && fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wKnight, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Bishop && fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wBishop, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (fig.type == FigureType.Rook && fig.white) {
                        chessController.figuresMap[i, j] = Instantiate(
                            figCont.wRook, 
                            pos, 
                            Quaternion.Euler(0, 90, 0),
                            boardTransform
                        );
                    }

                    if (chessController.figuresMap[i, j] != null) {
                        chessController.figuresMap[i, j].transform.position = pos;
                    }

                    yPos -= 1.5f;
                }
                yPos = CONST;
                xPos -= 1.5f;
            }
        }
    }
}
