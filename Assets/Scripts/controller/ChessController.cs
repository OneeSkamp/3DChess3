using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;
using move;
using pawn;
using king;
using castling;

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

        public Dictionary<CastlingType, bool> castlings;

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

            castlings = new Dictionary<CastlingType, bool>() {
                {CastlingType.BLongCastling, true},
                {CastlingType.BShortCastling, true},
                {CastlingType.WLongCastling, true},
                {CastlingType.WShortCastling, true}
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
                                    var squarePath = BoardEngine.GetSquarePath(
                                        figPos,
                                        type.square.Value.side
                                    );
                                    var square = new List<Vector2Int>();
                                    if (fig.type == FigureType.Knight) {
                                        square = BoardEngine.ChangeSquarePath(squarePath, 0, 1);
                                    }

                                    if (fig.type == FigureType.King) {
                                        square = BoardEngine.ChangeSquarePath(squarePath, 0, 0);
                                    }

                                    possibleMoves.AddRange(MoveController.GetPossibleMoves(
                                        figPos,
                                        square,
                                        boardMap
                                    ));
                                    var castlingInfo = CastlingController.GetCastlingInfo(
                                        castlings,
                                        whiteMove,
                                        boardMap
                                    );
                                    possibleMoves.AddRange(castlingInfo.castlingMoves);
                                } else {
                                    var linear = type.linear.Value;
                                    var length = BoardEngine.GetLinearLength(figPos,
                                        linear.dir,
                                        boardMap
                                    );

                                    possibleMoves.AddRange(MoveController.GetPossibleLinearMoves(
                                        figPos,
                                        linear,
                                        length,
                                        boardMap
                                    ));
                                }
                            }

                            if (fig.type == FigureType.Pawn) {
                                possibleMoves = PawnController.ChangePawnMoves(
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

                            var castlingInfo = CastlingController.GetCastlingInfo(
                                castlings,
                                whiteMove,
                                boardMap
                            );

                            var castlingMoves = castlingInfo.castlingMoves;

                            if (MoveController.IsCastlingMove(move, castlingMoves)) {
                                Castling(castlingInfo, move);

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

            CastlingController.ChangeCastlingValues(castlings, move, boardMap);

            var moveRes = MoveController.MoveFigure(move, board);
            var posFrom = move.from;
            var posTo = move.to;

            if (moveRes.error == MoveError.MoveOnFigure) {
                Destroy(figuresMap[moveRes.pos.x, moveRes.pos.y]);
            }

            figuresMap[posTo.x, posTo.y] = figuresMap[posFrom.x, posFrom.y];
            figuresMap[posFrom.x, posFrom.y] = null;

            var newPos = new Vector3(CONST - posTo.x * 1.5f, 0.0f, CONST - posTo.y * 1.5f);
            figuresMap[posTo.x, posTo.y].transform.position = newPos;
            var kingPos = KingController.FindKingPos(!whiteMove, boardMap);
            KingController.CheckKing(moveTypes, allMovement, kingPos, boardMap);
            whiteMove = !whiteMove;
        }

        private void Castling(CastlingInfo castlingInfo, Move kingMove) {
            Relocation(kingMove, boardMap);
            var rookMove = new Move();
            var kingPos = KingController.FindKingPos(!whiteMove, boardMap);

            if (castlingInfo.rookPos.y == 0) {
                rookMove.from = new Vector2Int(kingPos.x, 0);
                rookMove.to = new Vector2Int(kingPos.x, kingPos.y + 1);
            }

            if (castlingInfo.rookPos.y == 7) {
                rookMove.from = new Vector2Int(kingPos.x, 7);
                rookMove.to = new Vector2Int(kingPos.x, kingPos.y - 1); 
            }
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