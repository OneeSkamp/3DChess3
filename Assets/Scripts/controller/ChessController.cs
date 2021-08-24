using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;
using master;

namespace controller {
    public class ChessController : MonoBehaviour {
        public Option<Fig>[,] boardMap = new Option<Fig>[8, 8];

        public bool whiteMove = true;

        public GameObject[,] figuresMap = new GameObject[8, 8];

        public FigureSpawner figureSpawner;
        public FigureResurses figCont;

        private int x;
        private int y;
 
        private List<Move> possibleMoves = new List<Move>();
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
                    var size = new Vector2Int(boardMap.GetLength(0), boardMap.GetLength(1));

                    if (BoardEngine.IsOnBoard(new Vector2Int(x, y), size)) {
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

                                possibleMoves = ChessEngine.PossibleSquareMoves(
                                    figPos,
                                    square,
                                    boardMap
                                );
                            } else {
                                possibleMoves = ChessEngine.CalcLinearMoves(
                                    figPos,
                                    moveType.linear.Value,
                                    boardMap.GetLength(0),
                                    boardMap
                                );
                            }

                            if (fig.type == FigureType.Pawn) {
                                possibleMoves = ChangePawnMoves(figPos, possibleMoves, boardMap);
                            }

                            possibleMoveList = CreatingPossibleMoves(possibleMoves);

                        } else if (possibleMoves != null) {
                            Move move = new Move {
                                from = figPos,
                                to = new Vector2Int(x, y)
                            };

                            relocation(move, boardMap);

                            possibleMoves.Clear();
                            whiteMove = !whiteMove;
                        }
                    }
                }
            }
        }

        private void relocation (Move move, Option<Fig>[,] board) {
            foreach (Move possMove in possibleMoves) {
                if (Equals(move, possMove)) {
                    var moveRes = Master.MoveFigure(move, board);
                    var posFrom = move.from;
                    var posTo = move.to;

                    if (moveRes.error == MoveError.MoveOnFigure) {
                        Destroy(figuresMap[moveRes.pos.x, moveRes.pos.y]);
                    }

                    figuresMap[posTo.x, posTo.y] = figuresMap[posFrom.x, posFrom.y];
                    figuresMap[posFrom.x, posFrom.y] = null;
                    var newPos = new Vector3(CONST - posTo.x * 1.5f, 0.0f, CONST - posTo.y * 1.5f);
                    figuresMap[posTo.x, posTo.y].transform.position = newPos;

                }
            }
        }

        private List<Move> ChangePawnMoves(
            Vector2Int pos,
            List<Move> moves,
            Option<Fig>[,] board
        ) {
            var newMoves = new List<Move>();
            var fig = board[pos.x, pos.y].Peel();
            var prop = 1;

            if (fig.white) {
                prop = -1;
            }

            var size = new Vector2Int(boardMap.GetLength(0), boardMap.GetLength(1));
            var nextFig = board[pos.x + prop, pos.y];
            var leftPos = new Vector2Int(pos.x + prop, pos.y + prop);
            var rightPos = new Vector2Int(pos.x + prop, pos.y - prop);
            var leftOnBoard = BoardEngine.IsOnBoard(leftPos, size);
            var rightOnBoard = BoardEngine.IsOnBoard(rightPos, size);


            foreach (Move move in moves) {
                if (pos.x == 6 && prop == -1 || pos.x == 1 && prop == 1) {
                    if (Equals(new Vector2Int(pos.x + prop * 2, pos.y), move.to) && nextFig.IsNone()) {
                        newMoves.Add(move);
                    }
                }

                if (Equals(new Vector2Int(pos.x + prop, pos.y), move.to) && nextFig.IsNone()) {
                    newMoves.Add(move);
                }

                if (leftOnBoard && Equals(leftPos, move.to) 
                    && board[pos.x + prop, pos.y + prop].IsSome()) {

                    newMoves.Add(move);
                }

                if (rightOnBoard && Equals(rightPos, move.to) 
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

        private List<GameObject> CreatingPossibleMoves(List<Move> possibleMoves) {
            var possibleMovesObj = new List<GameObject>();

            foreach (Move move in possibleMoves) {
                var posX = move.to.x;
                var posY = move.to.y;

                var objPos = new Vector3(CONST - posX * 1.5f, 0.01f, CONST - posY * 1.5f);

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
    }
}