using System.Collections.Generic;
using UnityEngine;
using inspector;
using chess;
using board;
using option;
using move;
using movements;

namespace controller {
    public enum PlayerAction {
        Select,
        Move
    }

    public struct Map {
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

    public class ChessController : MonoBehaviour {

        public Transform boardTransform;

        public CellInfo cellInfo;

        public Map map;
        public BoardInfo boardInfo;
        public bool whiteMove = true;

        public GameObject highlight;
        public GameObject checkHighlight;

        public GameObject changePawnUi;
        public GameObject checkMateUi;

        public FigureResourses figContent;

        public MoveInfo lastMove;
 
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
                board = new Option<Fig>[8, 8],
                figures = new GameObject[8, 8]
            };

            map.board[0, 0] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Rook));
            map.board[0, 7] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Rook));

            map.board[0, 1] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Knight));
            map.board[0, 6] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Knight));

            map.board[0, 2] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Bishop));
            map.board[0, 5] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Bishop));

            map.board[0, 3] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Queen));
            map.board[0, 4] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.King));

            for (int x = 0; x <= 7; x++) {
               map.board[1, x] = Option<Fig>.Some(Fig.CreateFig(false, FigureType.Pawn));
            }

            map.board[7, 0] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Rook));
            map.board[7, 7] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Rook));

            map.board[7, 1] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Knight));
            map.board[7, 6] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Knight));

            map.board[7, 2] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Bishop));
            map.board[7, 5] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Bishop));

            map.board[7, 3] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Queen));
            map.board[7, 4] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.King));

            for (int x = 0; x <= 7; x++) {
               map.board[6, x] = Option<Fig>.Some(Fig.CreateFig(true, FigureType.Pawn));
            }
        }

        private void Update() {
            if (!Input.GetMouseButtonDown(0)) {
                return;
            }

            var movements = Movements.movements;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit)) {
                return;
            }

            var localHit = boardTransform.InverseTransformPoint(hit.point);

            var pos = new Vector2Int (
                Mathf.Abs((int)((localHit.x - boardInfo.leftTop.position.x) / cellInfo.size)),
                Mathf.Abs((int)((localHit.z - boardInfo.leftTop.position.x) / cellInfo.size))
            );

            var size = new Vector2Int(map.board.GetLength(0), map.board.GetLength(1));
            if (!BoardEngine.IsOnBoard(pos, size)) {
                return;
            }

            foreach (Transform cell in highlight.transform) {
                Destroy(cell.gameObject);
            }

            var figOpt = map.board[pos.x, pos.y];
            if (figOpt.IsSome() && figOpt.Peel().white == whiteMove) {
                playerAction = PlayerAction.Select;
            }


            switch (playerAction) {
                case PlayerAction.Select:
                    if (figOpt.IsNone()) {
                        Debug.LogError("No figure clicked, but we are trying to select");
                        return;
                    }

                    var fig = figOpt.Peel();

                    if (fig.white != whiteMove) {
                        Debug.LogError("Wrong figure color, not your turn!");
                        return;
                    }

                    var movement = movements[fig.type];
                    selectFigurePos = pos;

                    var kingsPos = ChessInspector.GetKingsPos(map.board);
                    var kingPos = kingsPos.black;
                    if (whiteMove) {
                        kingPos = kingsPos.white;
                    }

                    possibleMoves = ChessInspector.GetPossibleMoves(
                        selectFigurePos,
                        kingPos,
                        lastMove,
                        map.board
                    );

                    CreatePossibleHighlight();
                    playerAction = PlayerAction.Move;
                    break;
                case PlayerAction.Move:
                    var move = Move.Mk(selectFigurePos, pos);
                    foreach (MoveInfo possMove in possibleMoves) {
                        var firstMove = possMove.move.first.Value;
                        if (move.to == firstMove.to && move.from == firstMove.from) {
                            MoveEngine.GetMoveInfo(possMove, map.board);
                            HandleMoveInfo(possMove, map.board);
                            break;
                        }
                    }

                    kingsPos = ChessInspector.GetKingsPos(map.board);
                    kingPos = kingsPos.black;
                    if (whiteMove) {
                        kingPos = kingsPos.white;
                    }

                    var allMoves = GetAllPossibleMoves(kingPos);
                    if (allMoves.Count == 0) {
                        checkMateUi.SetActive(!changePawnUi.activeSelf);
                    }

                    Destroy(checkHighlight);
                    if (ChessInspector.IsUnderAttackPos(kingPos, whiteMove, lastMove, map.board)) {
                        CreateCheckHighlight(kingPos);
                    }
                    playerAction = PlayerAction.Select;
                    break;
            }
        }

        private void CreatePossibleHighlight() {
            foreach (var move in possibleMoves) {
                var pos = (Vector2)move.move.first.Value.to;
                var cellOff = new Vector2(cellInfo.offset, cellInfo.offset);

                var newPos = cellOff - pos * cellInfo.size;
                var objPos = new Vector3(newPos.x, 0.01f, newPos.y);

                var obj = Instantiate(figContent.blueBacklight);
                obj.transform.parent = highlight.transform;
                obj.transform.localPosition = objPos;
            }
        }

        private void CreateCheckHighlight(Vector2Int kingPos) {
            var cellOff = new Vector2(cellInfo.offset, cellInfo.offset);
            var newPos = cellOff - (Vector2)kingPos * cellInfo.size;
            var objPos = new Vector3(newPos.x, 0.01f, newPos.y);

            checkHighlight = Instantiate(figContent.redBacklight);
            checkHighlight.transform.parent = boardTransform;
            checkHighlight.transform.localPosition = objPos;
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

        private void HandleMoveInfo(MoveInfo moveInfo, Option<Fig>[,] board) {
            if (moveInfo.sentenced.HasValue) {
                var sentenced = moveInfo.sentenced.Value;
                Destroy(map.figures[sentenced.x, sentenced.y]);
            }

            if (moveInfo.move.first.HasValue) {
                Relocate(moveInfo.move.first.Value, board);
            }

            if (moveInfo.move.second.HasValue) {
                Relocate(moveInfo.move.second.Value, board);
            }

            if (moveInfo.promote.HasValue) {
                changePawnUi.SetActive(!changePawnUi.activeSelf);
                this.enabled = !this.enabled;
            }

            lastMove = moveInfo;

            whiteMove = !whiteMove;
        }

        public void PromotionPawn(GameObject wFig, GameObject bFig, FigureType type) {
            var figObj = bFig;
            var fig = Fig.CreateFig(false, type);
            var cellOff = new Vector2(cellInfo.offset, cellInfo.offset);
            var prom = (Vector2)lastMove.promote.Value;

            var pos = cellOff - prom * cellInfo.size;
            var newPos = new Vector3(prom.x, 0.0f, prom.y);

            if (prom.x == 0) {
                fig = Fig.CreateFig(true, type);
                figObj = wFig;
            }

            map.board[(int)prom.x, (int)prom.y] = Option<Fig>.Some(fig);
            Destroy(map.figures[(int)prom.x, (int)prom.y]);
            map.figures[(int)prom.x, (int)prom.y] = Instantiate(figObj);
            map.figures[(int)prom.x, (int)prom.y].transform.parent = boardTransform;

            map.figures[(int)prom.x, (int)prom.x].transform.localPosition = newPos;
            changePawnUi.SetActive(!changePawnUi.activeSelf);
            this.enabled = !this.enabled;
        }

        public List<MoveInfo> GetAllPossibleMoves(Vector2Int kingPos) {
            var allMoves = new List<MoveInfo>();
            for (int i = 0; i < map.board.GetLength(0); i++) {
                for (int j = 0; j < map.board.GetLength(1); j++) {
                    if (map.board[i, j].IsSome() && map.board[i, j].Peel().white == map.board[kingPos.x, kingPos.y].Peel().white) {
                        var moves = ChessInspector.GetPossibleMoves(
                            new Vector2Int(i, j),
                            kingPos,
                            lastMove,
                            map.board
                        );
                        allMoves.AddRange(moves);
                    }
                }
            }
            return allMoves;
        }
    }
}