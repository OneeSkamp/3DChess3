using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;

namespace controller {
    public class ChessController : MonoBehaviour {
        public static Option<Fig>[,] boardMap = CreateBoard();

        public bool whiteMove = true;

        public GameObject[,] figuresMap = new GameObject[8,8];

        public FigureSpawner figureSpawner;
        public FigureResurses figCont;

        private int x;
        private int y;
 
        private List<Vector2Int> possibleMoves = new List<Vector2Int>();
        private Vector2Int figPos;

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

                    var fig = boardMap[x, y].Peel();

                    if (possibleMoveList != null) {
                        foreach (GameObject cell in possibleMoveList) {
                            Destroy(cell);
                        }
                    }

                    if (figuresMap[x, y] != null && fig.white == whiteMove) {

                        var moveType = ChessEngine.GetMovementType(fig.type);
                        possibleMoves.Clear();
                        figPos = new Vector2Int(x, y);

                        if (Equals(moveType, ChessEngine.circularMovementType)) {
                            possibleMoves = CalcCircularPossibleMoves(
                                fig,
                                figPos,
                                moveType.circular.Value.radius
                            );
                        } else {
                            possibleMoves = CalcLinearPossibleMoves(fig, figPos, moveType);
                        }

                        if (fig.type == FigureType.Pawn) {
                            possibleMoves = ChangePawnMoves(fig);
                        }

                        possibleMoveList = CreatingPossibleMoves(possibleMoves);

                    } else if (possibleMoves != null) {
                        MoveFigure(figPos, new Vector2Int(x,y));
                        CheckKing();
                        possibleMoves.Clear();
                    }
                }
            }
        }

        private List<Vector2Int> CalcLinearPossibleMoves(
            Fig fig,
            Vector2Int figPos,
            MovementType moveType
        ) {
            var moves = new List<Vector2Int>();
            foreach (LinearMovement linear in moveType.linear) {
                var length = BoardCalculator.CalcLinearMoveLength(
                    figPos,
                    linear,
                    moveType.maxLength,
                    boardMap
                );

                var linearMoves = ChessEngine.CalcLinearMoves(
                    figPos,
                    linear,
                    length,
                    boardMap
                );

                var lastPos = ChessEngine.GetLastLinearPosition(linearMoves);

                if (lastPos != null) {
                    var lastFig = boardMap[lastPos.Value.x, lastPos.Value.y].Peel();

                    if (lastFig.type != FigureType.None && lastFig.white == fig.white) {
                        linearMoves.Remove(lastPos.Value);
                    }
                }

                moves.AddRange(linearMoves);
            }
            return moves;
        }

        private List<Vector2Int> CalcCircularPossibleMoves(Fig fig, Vector2Int pos, int radius) {
            var possMoves = new List<Vector2Int>();
            var circularMoves = BoardCalculator.CalcCircularMoves(pos, radius);

            foreach (var move in circularMoves) {
                if (BoardCalculator.IsOnBoard(move, boardMap.GetLength(0), boardMap.GetLength(1))) {
                    var figure = boardMap[move.x, move.y];

                    if (figure.IsSome() && figure.Peel().white == fig.white) {
                        continue;
                    }

                    possMoves.Add(move);
                }
            }
            return possMoves;
        }

        private List<Vector2Int> ChangePawnMoves(Fig fig) {
            var pawnMoves = new List<Vector2Int>();
            var length = 0;
            var prop = 0;

            if (fig.firstMove) {
                length = 2;
            } else {
                length = 1;
            }

            if (fig.white) {
                prop = -1;
            } else {
                prop = 1;
            }

            var forwardMove = new LinearMovement {dir = new Vector2Int(prop, 0)};
            var leftDiagonalMove = new LinearMovement {dir = new Vector2Int(prop, 1)};
            var rightDiagonalMove = new LinearMovement {dir = new Vector2Int(prop, -1)};

            var forward = ChessEngine.CalcLinearMoves(figPos, forwardMove, length, boardMap);
            var leftDiagonal = ChessEngine.CalcLinearMoves(
                figPos,
                leftDiagonalMove,
                length,
                boardMap
            );

            var rightDiagonal = ChessEngine.CalcLinearMoves(
                figPos,
                rightDiagonalMove,
                length,
                boardMap
            );

            foreach (Vector2Int pos in forward) {
                if (boardMap[pos.x, pos.y].IsNone()) {
                    pawnMoves.Add(pos);
                }
            }

            if (BoardCalculator.IsOnBoard(
                new Vector2Int(leftDiagonal[0].x, leftDiagonal[0].y), 8, 8
                ) && boardMap[leftDiagonal[0].x, leftDiagonal[0].y].IsSome()
            ) {
                pawnMoves.Add(leftDiagonal[0]);
            }

            if (BoardCalculator.IsOnBoard(
                new Vector2Int(rightDiagonal[0].x, rightDiagonal[0].y), 8, 8
                ) &&boardMap[rightDiagonal[0].x, rightDiagonal[0].y].IsSome()) {
                pawnMoves.Add(rightDiagonal[0]);
            }

            return pawnMoves;
        }

        private bool CheckKing() {
            var kingPos = new Vector2Int();

            for (int i = 0; i < boardMap.GetLength(0); i++) {
                for (int j = 0; j < boardMap.GetLength(1); j++) {
                    if (boardMap[i, j].IsSome()) {
                        var figure = boardMap[i, j].Peel();

                        if (figure.type == FigureType.King && figure.white == whiteMove) {
                            kingPos = new Vector2Int(i, j);
                        }
                    }
                }
            }

            var king = boardMap[kingPos.x, kingPos.y].Peel();
            var linearType = ChessEngine.mixedMovementType;
            Fig fig;
            MovementType figMoveType;

            foreach (LinearMovement linearMove in linearType) {
                var linear = ChessEngine.CalcLinearMoves(
                    kingPos,
                    linearMove,
                    boardMap.GetLength(0),
                    boardMap
                );

                var lastLinearPos = ChessEngine.GetLastLinearPosition(linear);

                if (BoardCalculator.IsOnBoard(
                    new Vector2Int(lastLinearPos.Value.x, lastLinearPos.Value.y),
                    boardMap.GetLength(0),
                    boardMap.GetLength(1)
                ) && boardMap[lastLinearPos.Value.x, lastLinearPos.Value.y].IsSome()) {
                    fig = boardMap[lastLinearPos.Value.x, lastLinearPos.Value.y].Peel();
                    figMoveType = ChessEngine.GetMovementType(fig.type);

                } else {
                    continue;
                }

                foreach (LinearMovement linMove in figMoveType.linear) {
                    if (boardMap[lastLinearPos.Value.x, lastLinearPos.Value.y].IsSome() 
                        && fig.white != king.white) {

                        if (Equals(linMove.dir, linearMove.dir * -1)) {
                            Debug.Log("check");
                        }
                    }
                }
            }

            return false;
        }

        private List<GameObject> CreatingPossibleMoves(List<Vector2Int> possibleMoves) {
            var possibleMovesObj = new List<GameObject>();

            foreach (Vector2Int pos in possibleMoves) {

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

        private bool MoveFigure(Vector2Int from, Vector2Int to) {
            foreach (var move in possibleMoves) {
                if (move.x == to.x && move.y == to.y) {

                    boardMap[to.x, to.y] = boardMap[from.x, from.y];
                    boardMap[from.x, from.y] = Option<Fig>.None();

                    Destroy(figuresMap[to.x, to.y]);
                    figuresMap[to.x, to.y] = figuresMap[from.x, from.y];

                    var newPos = new Vector3(CONST - to.x * 1.5f, 0.0f, CONST - to.y * 1.5f);
                    figuresMap[from.x, from.y].transform.localPosition = newPos;

                    figuresMap[from.x, from.y] = null;
                    whiteMove = !whiteMove;

                    var figure = boardMap[to.x, to.y].Peel();
                    figure.firstMove = false;
                    boardMap[to.x, to.y] = Option<Fig>.Some(figure);
                    return true;
                }
            }
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