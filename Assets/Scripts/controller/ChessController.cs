using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;
using master;

namespace controller {
    public struct CastlingRes {
        public Vector2Int rookPos;
        public List<Move> castlingMoves;
    }
    public class ChessController : MonoBehaviour {
        public Option<Fig>[,] boardMap = new Option<Fig>[8, 8];

        public bool whiteMove = true;

        public GameObject[,] figuresMap = new GameObject[8, 8];

        public FigureSpawner figureSpawner;
        public FigureResurses figCont;

        private int x;
        private int y;

        public bool isWShortCastling = true;
        public bool isWLongCastling = true;
        public bool isBShortCastling = true;
        public bool isBLongCastling = true;
 
        private List<Move> possibleMoves = new List<Move>();
        private Vector2Int figPos;

        private const float CONST = 5.25f;

        private Ray ray;
        private RaycastHit hit;

        private List<GameObject> possibleMoveList;
        private List<Movement> bishopMovement = new List<Movement> {
            new Movement {
                linear = new LinearMovement {
                    dir = new Vector2Int(1, 1)
                }
            },
            new Movement {
                linear = new LinearMovement {
                    dir = new Vector2Int(-1, 1)
                }
            },
            new Movement {
                linear = new LinearMovement {
                    dir = new Vector2Int(1, -1)
                }
            },
            new Movement {
                linear = new LinearMovement {
                    dir = new Vector2Int(-1, -1)
                }
            }
        };

        private List<Movement> rookMovement = new List<Movement> {
            new Movement {
                linear = new LinearMovement {
                    dir = new Vector2Int(1, 0)
                }
            },
            new Movement {
                linear = new LinearMovement {
                    dir = new Vector2Int(0, 1)
                }
            },
            new Movement {
                linear = new LinearMovement {
                    dir = new Vector2Int(0, -1)
                }
            },
            new Movement {
                linear = new LinearMovement {
                    dir = new Vector2Int(-1, 0)
                }
            }
        };

        private List<Movement> knightMovemetn = new List<Movement> {
            new Movement {
                square = new SquareMovement {
                    side = 5
                }
            }
        };

        private List<Movement> kingMovement = new List<Movement> {
            new Movement {
                square = new SquareMovement {
                    side = 3
                }
            }
        };

        private List<Movement> queenMovement = new List<Movement>();
        private List<Movement> allMovement = new List<Movement>();

        public Dictionary<FigureType, List<Movement>> moveTypes;

        private void Awake() {
            queenMovement.AddRange(bishopMovement);
            queenMovement.AddRange(rookMovement);

            allMovement.AddRange(queenMovement);
            allMovement.AddRange(knightMovemetn);

            moveTypes =  new Dictionary<FigureType, List<Movement>> {
                {FigureType.Bishop, bishopMovement},
                {FigureType.Rook, rookMovement},
                {FigureType.Queen, queenMovement},
                {FigureType.Knight, knightMovemetn},
                {FigureType.Pawn, queenMovement},
                {FigureType.King, kingMovement}
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


                            foreach (Movement type in moveType) {
                                if (type.square.HasValue) {
                                    var square = BoardEngine.CalcSquarePath(
                                        figPos,
                                        type.square.Value.side
                                    );

                                    if (fig.type == FigureType.Knight) {
                                        square = BoardEngine.ChangeSquarePath(square, 1);
                                    }

                                    possibleMoves.AddRange(ChessEngine.GetPossibleMoves(
                                        figPos,
                                        square,
                                        boardMap
                                    ));
                                    possibleMoves.AddRange(CalcCastlingRes().castlingMoves);
                                } else {
                                    var linear = type.linear.Value;
                                    var length = BoardEngine.CalcLinearLength(figPos,
                                        linear.dir,
                                        boardMap
                                    );

                                    possibleMoves.AddRange(ChessEngine.CalcPossibleLinearMoves(
                                        figPos,
                                        linear,
                                        length,
                                        boardMap
                                    ));
                                }

                            }

                            if (fig.type == FigureType.Pawn) {
                                possibleMoves = Master.ChangePawnMoves(
                                    figPos,
                                    possibleMoves,
                                    boardMap);
                            }

                            possibleMoveList = CreatingPossibleMoves(possibleMoves);

                        } else {
                            Move move = new Move {
                                from = figPos,
                                to = new Vector2Int(x, y)
                            };

                            if (Master.IsCastlingMove(move, CalcCastlingRes().castlingMoves)) {
                                Castling(CalcCastlingRes(), move);

                            } else {

                                foreach (Move possMove in possibleMoves) {
                                    if (Equals(move, possMove)) {
                                        Relocation(move, boardMap);
                                    }
                                }
                            }

                            possibleMoves.Clear();
                        }
                    }
                }
            }
        }

        private void Relocation (Move move, Option<Fig>[,] board) {
            var fig = board[move.from.x, move.from.y].Peel();

            if (fig.type == FigureType.King) {
                if (fig.white) {
                    isWLongCastling = false;
                    isWShortCastling = false;
                } else {
                    isBLongCastling = false;
                    isBShortCastling = false;
                }
            }

            if (fig.type == FigureType.Rook) {
                if (fig.white) {
                    if (move.from.y == 7) {

                        isWShortCastling = false;
                    } else {
                        isWLongCastling = false;
                    }
                } else {
                    if (move.from.y == 7) {
                        isBShortCastling = false;
                    } else {
                        isBLongCastling = false;
                    }
                }
            }

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
            var kingPos = Master.FindKingPos(!whiteMove, boardMap);
            Master.CheckKing(moveTypes, allMovement, kingPos, boardMap);
            whiteMove = !whiteMove;
        }

        private CastlingRes CalcCastlingRes() {
            var castlingRes = new CastlingRes();
            var castlingMoves = new List<Move>();
            var kingPos = Master.FindKingPos(whiteMove, boardMap);
            var king = boardMap[kingPos.x, kingPos.y].Peel();

            var right1 = boardMap[kingPos.x, 5].IsNone();
            var right2 = boardMap[kingPos.x, 6].IsNone();
            var left1 = boardMap[kingPos.x, 3].IsNone();
            var left2 = boardMap[kingPos.x, 2].IsNone();
            var left3 = boardMap[kingPos.x, 1].IsNone();
            var move = new Move();
            var rookPos = new Vector2Int();
            move.from = kingPos;

            if (king.white) {
                if (right1 && right2 && isWShortCastling) {
                    move.to = new Vector2Int(kingPos.x, 6);
                    rookPos = new Vector2Int(kingPos.x, 7);
                    castlingMoves.Add(move);
                }

                if (left1 && left2 && left3 && isWLongCastling) {
                    move.to = new Vector2Int(kingPos.x, 2);
                    rookPos = new Vector2Int(kingPos.x, 0);
                    castlingMoves.Add(move);
                }

            } else {

                if (right1 && right2 && isBShortCastling) {
                    move.to = new Vector2Int(kingPos.x, 6);
                    rookPos = new Vector2Int(kingPos.x, 7);
                    castlingMoves.Add(move);
                }

                if (left1 && left2 && left3 && isBLongCastling) {
                    move.to = new Vector2Int(kingPos.x, 2);
                    rookPos = new Vector2Int(kingPos.x, 0);
                    castlingMoves.Add(move);
                }
            }
            castlingRes.rookPos = rookPos;
            castlingRes.castlingMoves = castlingMoves;

            return castlingRes;
        }

        private void Castling(CastlingRes castlingRes, Move kingMove) {
            var kingMoveRes = Master.MoveFigure(kingMove, boardMap);
            var kingPos = Master.FindKingPos(whiteMove, boardMap);
            var rookMove = new Move();

            if (castlingRes.rookPos.y == 0) {
                rookMove.from = new Vector2Int(kingPos.x, 0);
                rookMove.to = new Vector2Int(kingPos.x, kingPos.y + 1);
            }

            if (castlingRes.rookPos.y == 7) {
                rookMove.from = new Vector2Int(kingPos.x, 7);
                rookMove.to = new Vector2Int(kingPos.x, kingPos.y - 1); 
            }
            var rookMoveRes = Master.MoveFigure(rookMove, boardMap);

            Relocation(kingMove, boardMap);
            Relocation(rookMove, boardMap);
            whiteMove = !whiteMove;
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