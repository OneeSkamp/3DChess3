using System.Collections.Generic;
using UnityEngine;
using chess;
using board;
using option;
using move;

namespace controller {
    public enum PlayerAction {
        None,
        Select,
        Move
    }

    public struct Map {
        public Option<Fig>[,] startBoard;
        public Option<Fig>[,] board;
        public GameObject[,] figures;
    }

    [System.Serializable]
    public struct BoardInfo {
        public Transform leftTop;
        public Transform rightBottom;
    }

    public struct CellInfo {
        public int count;
        public float size;
        public float offset;
    }

    [System.Serializable]
    public struct Highlights {
        public GameObject possible;
        public GameObject check;
    }

    public class ChessController : MonoBehaviour {
        public Transform boardTransform;

        public Map map;
        public BoardInfo boardInfo;

        public FigColor moveColor;
        public CellInfo cellInfo;
        public Highlights highlights;
        public FigureResourses figContent;
        public FigShadow? lastShadow;

        private List<MoveInfo> possibleMoves = new List<MoveInfo>();
        private Vector2Int selectFigurePos;
        private PlayerAction playerAction;

        private void Awake() {
            var leftTopX = boardInfo.leftTop.localPosition.x;
            var rightBottomX = boardInfo.rightBottom.localPosition.x;

            cellInfo.count = 8;
            cellInfo.size = (Mathf.Abs(leftTopX) + Mathf.Abs(rightBottomX)) / cellInfo.count;
            cellInfo.offset = boardInfo.leftTop.position.x - cellInfo.size / 2;

            map = new Map {
                startBoard = new Option<Fig>[8, 8],
                board = new Option<Fig>[8, 8],
                figures = new GameObject[8, 8]
            };

            map.board[0, 0] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.Rook));
            map.board[0, 7] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.Rook));

            map.board[0, 1] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.Knight));
            map.board[0, 6] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.Knight));

            map.board[0, 2] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.Bishop));
            map.board[0, 5] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.Bishop));

            map.board[0, 3] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.Queen));
            map.board[0, 4] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.King));

            for (int x = 0; x <= 7; x++) {
               map.board[1, x] = Option<Fig>.Some(Fig.CreateFig(FigColor.Black, FigureType.Pawn));
            }

            map.board[7, 0] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.Rook));
            map.board[7, 7] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.Rook));

            map.board[7, 1] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.Knight));
            map.board[7, 6] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.Knight));

            map.board[7, 2] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.Bishop));
            map.board[7, 5] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.Bishop));

            map.board[7, 3] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.Queen));
            map.board[7, 4] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.King));

            for (int x = 0; x <= 7; x++) {
               map.board[6, x] = Option<Fig>.Some(Fig.CreateFig(FigColor.White, FigureType.Pawn));
            }

            map.startBoard = BoardEngine.CopyBoard(map.board);
        }

        private void Update() {
            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit)) {
                return;
            }

            var localHit = boardTransform.InverseTransformPoint(hit.point);

            var hitOffcet = (localHit - boardInfo.leftTop.position) / cellInfo.size;
            var pos = new Vector2Int(Mathf.Abs((int)hitOffcet.x), Mathf.Abs((int)hitOffcet.z));

            if (!BoardEngine.IsOnBoard(pos, map.board)) {
                return;
            }

            foreach (Transform cell in highlights.possible.transform) {
                Destroy(cell.gameObject);
            }

            var figOpt = map.board[pos.x, pos.y];
            if (figOpt.IsSome() && figOpt.Peel().color == moveColor) {
                playerAction = PlayerAction.Select;
            }

            switch (playerAction) {
                case PlayerAction.Select:
                    if (figOpt.IsNone()) {
                        Debug.LogError("No figure clicked, but we are trying to select");
                        return;
                    }

                    var fig = figOpt.Peel();
                    if (fig.color != moveColor) {
                        Debug.LogError("Wrong figure color, not your turn!");
                        return;
                    }

                    selectFigurePos = pos;

                    possibleMoves = MoveEngine.GetFigMoves(
                        new FigLoc { pos = pos, board = map.board },
                        lastShadow
                    ).Item1;

                    CreatePossibleHighlights(possibleMoves);
                    playerAction = PlayerAction.Move;
                    break;
                case PlayerAction.Move:
                    var move = Move.Mk(selectFigurePos, pos);
                    foreach (MoveInfo possMove in possibleMoves) {
                        if (!possMove.move.first.HasValue) {
                            continue;
                        }
                        var firstMove = possMove.move.first.Value;
                        if (move.to == firstMove.to && move.from == firstMove.from) {
                            HandleMove(possMove, map.board);
                            if (IsCheckMate(moveColor, map.board)) {
                                Debug.Log("CheckMate");
                            }
                            break;
                        }
                    }

                    playerAction = PlayerAction.None;
                    break;
            }
        }

        private void CreatePossibleHighlights(List<MoveInfo> possibleMoves) {
            if (possibleMoves == null) {
                return;
            }

            foreach (var move in possibleMoves) {
                if (!move.move.first.HasValue) {
                    continue;
                }

                var highlight = figContent.blueBacklight;
                var pos = (Vector2)move.move.first.Value.to;
                var cellOff = new Vector2(cellInfo.offset, cellInfo.offset);

                var newPos = cellOff - pos * cellInfo.size;
                var objPos = new Vector3(newPos.x, highlight.transform.position.y, newPos.y);

                var obj = Instantiate(highlight);
                obj.transform.parent = highlights.possible.transform;
                obj.transform.localPosition = objPos;
            }
        }

        private void Relocate(Move move, Option<Fig>[,] board) {
            var fig = board[move.from.x, move.from.y].Peel();
            var posFrom = move.from;
            var posTo = move.to;

            map.figures[posTo.x, posTo.y] = map.figures[posFrom.x, posFrom.y];
            map.figures[posFrom.x, posFrom.y] = null;

            var newX = cellInfo.offset - posTo.x * cellInfo.size;
            var newY = cellInfo.offset - posTo.y * cellInfo.size;
            var newPos = new Vector3(newX, 0.0f, newY);

            map.figures[posTo.x, posTo.y].transform.localPosition = newPos;
        }

        public bool IsCheckMate(FigColor color, Option<Fig>[,] board) {
            var allMoves = new List<MoveInfo>();
            for (int i = 0; i < board.GetLength(0); i++) {
                for (int j = 0; j < board.GetLength(1); j++) {
                    var figOpt = board[i, j];
                    if (figOpt.IsNone()) {
                        continue;
                    }

                    var fig = figOpt.Peel();
                    if (fig.color == color) {
                        var figLoc = new FigLoc {
                            pos = new Vector2Int(i, j),
                            board = board
                        };
                        var (possMoves, err) = MoveEngine.GetFigMoves(figLoc, lastShadow);

                        allMoves.AddRange(possMoves);
                    }
                }
            }

            if (allMoves.Count == 0) {
                return true;
            }

            return false;
        }

        private void HandleMove(MoveInfo moveInfo, Option<Fig>[,] board) {
            if (moveInfo.shadow.HasValue) {
                lastShadow = moveInfo.shadow.Value;
            }

            if (moveInfo.sentenced.HasValue) {
                var sentenced = moveInfo.sentenced.Value;
                board[sentenced.x, sentenced.y] = Option<Fig>.None();
                Destroy(map.figures[sentenced.x, sentenced.y]);
            }


            if (moveInfo.move.first.HasValue) {
                MoveEngine.MoveFigure(moveInfo.move.first.Value, board);
                Relocate(moveInfo.move.first.Value, board);
            }

            if (moveInfo.move.second.HasValue) {
                MoveEngine.MoveFigure(moveInfo.move.second.Value, board);
                Relocate(moveInfo.move.second.Value, board);
            }

            if (moveColor == FigColor.White) {
                moveColor = FigColor.Black;
            } else {
                moveColor = FigColor.White;
            }
        }
    }
}