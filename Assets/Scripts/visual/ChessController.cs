using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;

namespace visual {
    public class ChessController : MonoBehaviour {
        public Board board = new Board(true);
        public GameObject[,] figuresMap = new GameObject[8,8];

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
                        var movePaths = ChessEngine.CalcMovePaths(
                            new Position(x, y),
                            ChessEngine.GetMoveType(fig),
                            board.boardMap
                        );
                        if (fig.type == FigureType.Pawn) {
                            movePaths = CalcPawnPath(movePaths, fig);
                        }
                        possibleMoves = ChessEngine.CalcPossibleMoves(movePaths);
                        figPos = new Position(x, y);
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

        private List<MovePath> CalcPawnPath(List<MovePath> movePaths, Fig figure) {
            var PawnPath = new List<MovePath>();
            int colorDir = 0;

            if (figure.white) {
                colorDir = -1;
            } else {
                colorDir = 1;
            }

            foreach (MovePath path in movePaths) {
                var myFig = board.boardMap[path.pos.x, path.pos.y];
                var nextFig = board.boardMap[path.pos.x + path.dir.x, path.pos.y + path.dir.y];

                if (path.dir.x  == colorDir && path.dir.y == 0 && nextFig.IsNone()) {
                    PawnPath.Add(path);
                    continue;
                }

                if (path.dir.x == colorDir && path.dir.y == -1 && !nextFig.IsNone() 
                    && myFig.Peel().white != nextFig.Peel().white) {

                    PawnPath.Add(path);
                    continue;
                }

                if (path.dir.x == colorDir && path.dir.y == 1 && !nextFig.IsNone() 
                    && myFig.Peel().white != nextFig.Peel().white) {

                    PawnPath.Add(path);
                    continue;
                }
            }

            return PawnPath;
        }

        private bool CheckKing() {
            Option<Fig> king;
            var moveTypes = MoveTypes.MakeFigMoveTypes();

            foreach (Option<Fig> fig in board.boardMap) {
                if (fig.Peel().type == FigureType.King) {
                    king = fig;
                }
            }

            return false;
        }
    }
}