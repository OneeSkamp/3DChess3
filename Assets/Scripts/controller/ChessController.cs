using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;

namespace visual {
    public class ChessController : MonoBehaviour {

        public static Option<Fig>[,] boardMap; 

        public GameObject[,] figuresMap = new GameObject[8,8];

        public Board<Fig> board = new Board<Fig>(true, CreateBoard());

        public FigureSpawner figureSpawner;
        public FigureResurses figCont;

        private int x;
        private int y;

        private Position figPos;
        private List<Position> possibleMoves;

        private const float CONST = 5.25f;

        private Ray ray;
        private RaycastHit hit;

        private List<GameObject> possibleMoveList;

        private void Update() {

            if (Input.GetMouseButtonDown(0)) {

                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit)) {
                    var localHit = figureSpawner.boardTransform.InverseTransformPoint(hit.point);

                    x = Mathf.Abs((int)((localHit.x - 6f) / 1.5f));
                    y = Mathf.Abs((int)((localHit.z - 6f) / 1.5f));

                    var fig = board.boardMap[x, y].Peel();

                    if (possibleMoveList != null) {
                        foreach (GameObject cell in possibleMoveList) {
                            Destroy(cell);
                        }
                    }

                    if (figuresMap[x, y] != null && fig.white == board.whiteMove) {

                        figPos = new Position(x, y);

                        var figPaths = ChessEngine.CalcFigurePaths(figPos, boardMap, board);
                        possibleMoves = null;
                        possibleMoves = ChessEngine.CalcPossibleMoves(figPaths);

                        foreach (MovePath path in figPaths) {
                            var onWay = board.boardMap[path.onWay.x, path.onWay.y].Peel();
                            //Debug.Log(path.onWay.x + "sdad" + path.onWay.y);
                            if (onWay.white != board.whiteMove) {

                                possibleMoves.Add(new Position(path.onWay.x, path.onWay.y));
                            }
                        }
                        foreach (Position pos in possibleMoves) {
                           // Debug.Log(pos.x + "s" + pos.y);
                        }

                        possibleMoveList = CreatingPossibleMoves(possibleMoves);

                    } else if (possibleMoves != null) {

                        var from = figPos;
                        var to = new Position(x, y);

                        foreach (Position pos in possibleMoves) {
                            if (pos.x == to.x && pos.y == to.y) {
                                MoveFigure(from, to);
                                var figure = board.boardMap[to.x, to.y].Peel();
                                figure.firstMove = false;
                                board.boardMap[to.x, to.y] = Option<Fig>.Some(figure);
                            }
                        }

                        possibleMoves.Clear();
                    }
                }
            }
        }

        private List<GameObject> CreatingPossibleMoves(List<Position> possibleMoves) {
            var possibleMovesObj = new List<GameObject>();

            foreach (Position pos in possibleMoves) {
               // Debug.Log(pos.x + "ss" + pos.y);
                var objPos = new Vector3(CONST - pos.x * 1.5f, 0.01f, CONST - pos.y * 1.5f);

                var obj = Instantiate(
                    figCont.blueBacklight,
                    objPos, 
                    Quaternion.Euler(90, 0, 0),
                    figureSpawner.boardTransform
                );

                obj.transform.localPosition = objPos;
                possibleMovesObj.Add(obj);
            }

            return possibleMovesObj;
        }

        private void MoveFigure(Position from, Position to) {
            board.boardMap[to.x, to.y] = board.boardMap[from.x, from.y];
            board.boardMap[from.x, from.y] = Option<Fig>.None();

            var figForMove = figuresMap[from.x, from.y];

            Destroy(figuresMap[to.x, to.y]);

            figuresMap[to.x, to.y] = figuresMap[from.x, from.y];
            figuresMap[from.x, from.y] = null;

            var newPos = new Vector3(CONST - to.x * 1.5f, 0.0f, CONST - to.y * 1.5f);

            if (figForMove != null) {
                figForMove.transform.localPosition = newPos;
            }

            board.whiteMove = !board.whiteMove;
        }

        private bool CheckKing() {
            Option<Fig> king = Option<Fig>.None();
            var kingPos = new Position();

            for (int i = 0; i < 8; i++) {
                for (int j = 0; j <8; j++) {
                    if (board.boardMap[i, j].Peel().type == FigureType.King) {
                        king = board.boardMap[i, j];
                        kingPos = new Position(i, j);
                    }
                }
            }

            var figure = king.Peel();
            figure.type = FigureType.Bishop;

            return false;
        }

        private static Option<Fig>[,] CreateBoard() {
            boardMap = new Option<Fig>[8, 8];
            boardMap[0, 0] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Rook));
            boardMap[0, 7] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Rook));

            boardMap[0, 1] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Knight));
            boardMap[0, 6] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Knight));

            boardMap[0, 2] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Bishop));
            boardMap[0, 5] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Bishop));

            boardMap[0, 3] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Queen));
            boardMap[0, 4] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.King));

            for (int x = 0; x <= 7; x++) {
                boardMap[1, x] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Pawn));
            }

            boardMap[7, 0] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Rook));
            boardMap[7, 7] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Rook));

            boardMap[7, 1] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Knight));
            boardMap[7, 6] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Knight));

            boardMap[7, 2] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Bishop));
            boardMap[7, 5] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Bishop));

            boardMap[7, 3] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Queen));
            boardMap[7, 4] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.King));

            for (int x = 0; x <= 7; x++) {
                boardMap[6, x] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Pawn));
            }

            return boardMap;
        }
    }
}