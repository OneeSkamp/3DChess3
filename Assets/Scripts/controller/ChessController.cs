using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;

namespace controller {
    public class ChessController : MonoBehaviour {
        public Option<Fig>[,] boardMap = new Option<Fig>[8, 8];

        public bool whiteMove = true;

        public GameObject[,] figuresMap = new GameObject[8, 8];

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
        private List<Vector2Int> diagonal = new List<Vector2Int> {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        private List<Vector2Int> straight = new List<Vector2Int> {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        };

        public Dictionary<FigureType, MovementType> moveTypes;

        private void Awake() {
            moveTypes =  new Dictionary<FigureType, MovementType> {
                {
                    FigureType.Bishop,
                    new MovementType {
                        linear = new LinearMovement {
                            diagonal = new Diagonal {
                                diagonalDirs = diagonal
                            }
                        }
                    }
                },
                {
                    FigureType.Rook,
                    new MovementType {
                        linear = new LinearMovement {
                            straight = new Straight {
                                straightDirs = straight
                            }
                        }
                    }
                },
                {
                    FigureType.Queen,
                    new MovementType {
                        linear = new LinearMovement {
                            straight = new Straight {
                                straightDirs = straight
                            },
                            diagonal = new Diagonal {
                                diagonalDirs = diagonal
                            }
                        }
                    }
                },
                {
                    FigureType.King,
                    new MovementType {
                        square = new SquareMovement {
                                side = 3
                        }
                    }
                },
                {
                    FigureType.Knight,
                    new MovementType {
                        square = new SquareMovement {
                                side = 5
                        }
                    }
                },
                {
                    FigureType.Pawn,
                    new MovementType {
                        linear = new LinearMovement {
                            straight = new Straight {
                                straightDirs = straight
                            },
                            diagonal = new Diagonal {
                                diagonalDirs = diagonal
                            }
                        }
                    }
                },
            };

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
        }

        private void Update() {

            if (Input.GetMouseButtonDown(0)) {

                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit)) {
                    var localHit = figureSpawner.boardTransform.InverseTransformPoint(hit.point);

                    x = Mathf.Abs((int)((localHit.x - 6f) / 1.5f));
                    y = Mathf.Abs((int)((localHit.z - 6f) / 1.5f));
                    var width = boardMap.GetLength(0);
                    var height = boardMap.GetLength(1);

                    if (BoardEngine.IsOnBoard(new Vector2Int(x, y), width, height)) {
                        var fig = boardMap[x, y].Peel();

                        if (possibleMoveList != null) {
                            foreach (GameObject cell in possibleMoveList) {
                                Destroy(cell);
                            }
                        }

                        if (figuresMap[x, y] != null && fig.white == whiteMove) {

                            var moveType = moveTypes[fig.type];
                            
                            possibleMoves.Clear();
                            figPos = new Vector2Int(x, y);



                            if (moveType.square != null) {

                                var square = BoardEngine.CalcSquarePath(
                                    figPos,
                                    moveType.square.Value.side
                                );

                                if (fig.type == FigureType.Knight) {
                                    square = ChangeKnightPath(square);
                                }

                                possibleMoves = ChessEngine.CalcSquareMoves(
                                    figPos,
                                    square,
                                    boardMap
                                );
                            } else {
                                possibleMoves = ChessEngine.CalcLinearMoves(
                                    figPos,
                                    moveType.linear.Value,
                                    boardMap
                                );
                            }

                            if (fig.type == FigureType.Pawn) {
                                possibleMoves = ChangePawnMoves(figPos, possibleMoves, boardMap);
                            }

                            possibleMoveList = CreatingPossibleMoves(possibleMoves);

                        } else if (possibleMoves != null) {
                            MoveFigure(figPos, new Vector2Int(x,y));
                            possibleMoves.Clear();
                        }
                    }
                }
            }
        }

        private List<Vector2Int> ChangePawnMoves(
            Vector2Int pos,
            List<Vector2Int> moves,
            Option<Fig>[,] board
        ) {
            var newMoves = new List<Vector2Int>();
            var fig = board[pos.x, pos.y].Peel();
            var prop = 1;

            if (fig.white) {
                prop = -1;
            }

            var width = boardMap.GetLength(0);
            var height = boardMap.GetLength(1);
            var nextFig = board[pos.x + prop, pos.y];
            var leftPos = new Vector2Int(pos.x + prop, pos.y + prop);
            var rightPos = new Vector2Int(pos.x + prop, pos.y - prop);
            var leftOnBoard = BoardEngine.IsOnBoard(leftPos, width, height);
            var rightOnBoard = BoardEngine.IsOnBoard(rightPos, width, height);

            foreach (Vector2Int move in moves) {
                if (pos.x == 6 && prop == -1 || pos.x == 1 && prop == 1) {
                    if (Equals(new Vector2Int(pos.x + prop * 2, pos.y), move) && nextFig.IsNone()) {
                        newMoves.Add(move);
                    }
                }

                if (Equals(new Vector2Int(pos.x + prop, pos.y), move) && nextFig.IsNone()) {
                    newMoves.Add(move);
                }

                if (leftOnBoard && Equals(leftPos, move) 
                    && board[pos.x + prop, pos.y + prop].IsSome()) {

                    newMoves.Add(move);
                }

                if (rightOnBoard && Equals(rightPos, move) 
                    && board[pos.x + prop, pos.y - prop].IsSome()) {

                    newMoves.Add(move);
                }
            }
            return newMoves;
        }

        private List<Vector2Int> ChangeKnightPath(List<Vector2Int> square) {
            var newPath = new List<Vector2Int>();
            var count = 0;

            foreach (Vector2Int move in square) {
                if (count < square.Count) {
                    newPath.Add(square[count]);
                    count += 2;
                }
            }

            return newPath;
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
                    boardMap[to.x, to.y] = Option<Fig>.Some(figure);
                    return true;
                }
            }
            return false;
        }
    }
}