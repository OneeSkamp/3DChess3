using UnityEngine;
using option;
using chess;

namespace controller {
    public class FigureSpawner : MonoBehaviour {
        public Transform boardTransform;


        public FigureResourses figCont;
        public ChessController chessController;

        private float offset;
        private float cellSize;

        public void CreatingFiguresOnBoard(Option<Fig>[,] board) {
            float xPos = offset;
            float yPos = offset;

            for (int i = 0; i < board.GetLength(0); i++) {

                for (int j = 0; j < board.GetLength(1); j++) {
                    var pos = new Vector3(xPos, 0.172f , yPos);
                    var pawnPos = new Vector3(xPos, 0f , yPos);
                    var fig = board[i, j].Peel();
                    if (board[i, j].IsSome()) {
                        if (fig.type == FigureType.Pawn && !fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.bPawn,
                                pawnPos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Queen && !fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.bQueen,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.King && !fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.bKing,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Knight && !fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.bKnight,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Bishop && !fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.bBishop,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Rook && !fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.bRook,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Pawn && fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.wPawn,
                                pawnPos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Queen && fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.wQueen,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.King && fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.wKing,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Knight && fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.wKnight,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Bishop && fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.wBishop,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (fig.type == FigureType.Rook && fig.white) {
                            chessController.map.figures[i, j] = Instantiate(
                                figCont.wRook,
                                pos,
                                Quaternion.Euler(0, 90, 0),
                                boardTransform
                            );
                        }

                        if (chessController.map.figures[i, j] != null) {
                            chessController.map.figures[i, j].transform.localPosition = pos;
                        }

                        yPos -= cellSize;
                    }

                }
                yPos = offset;
                xPos -= cellSize;
            }
        }

        private void Start() {
            offset = chessController.cell.offset;
            cellSize = chessController.cell.size;

            CreatingFiguresOnBoard(chessController.map.board);
        }
    }
}