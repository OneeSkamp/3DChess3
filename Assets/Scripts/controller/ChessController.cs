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

    public struct KingsPos {
        public Vector2Int white;
        public Vector2Int black;
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

        public FigureResourses figContent;

        public KingsPos kingsPos;
 
        private List<DoubleMove> possibleMoves = new List<DoubleMove>();

        private Vector2Int selectFigurePos;

        private PlayerAction playerAction;

        private void Awake() {
            var emp1PosX = boardInfo.leftTop.localPosition.x;
            var emp2PosX = boardInfo.rightBottom.localPosition.x;

            cellInfo.count = 8;
            cellInfo.size = (Mathf.Abs(emp1PosX) + Mathf.Abs(emp2PosX)) / cellInfo.count;
            cellInfo.offset = boardInfo.leftTop.position.x - cellInfo.size / 2;

            kingsPos = new KingsPos {
                white = new Vector2Int(7, 4),
                black = new Vector2Int(0, 4)
            };

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

            var fig = figOpt.Peel();

            switch (playerAction) {
                case PlayerAction.Select:
                    if (figOpt.Peel().white != whiteMove) {
                        break;
                    }
                    var movement = movements[figOpt.Peel().type];
                    selectFigurePos = pos;

                    var kingPos = kingsPos.black;
                    if (whiteMove) {
                        kingPos = kingsPos.white;
                    }

                    possibleMoves = ChessInspector.GetPossibleMoves(
                        selectFigurePos,
                        kingPos,
                        map.board
                    );

                    CreateHighlight();
                    playerAction = PlayerAction.Move;
                    break;
                case PlayerAction.Move:
                    var move = Move.Mk(selectFigurePos, pos);

                    foreach (DoubleMove possMove in possibleMoves) {
                        if (!possMove.first.HasValue) {
                            continue;
                        }

                        var firstMove = possMove.first.Value;
                        if (move.to == firstMove.to && move.from == firstMove.from) {
                            Relocate(possMove.first.Value, map.board);
                            selectFigurePos = move.to;
                            whiteMove = !whiteMove;

                            if (possMove.first.Value.promotionPos.HasValue) {
                                changePawnUi.SetActive(!changePawnUi.activeSelf);
                                this.enabled = !this.enabled;
                            }

                            if (possMove.second.HasValue) {
                                Relocate(possMove.second.Value, map.board);
                            }
                            break;
                        }
                    }

                    kingPos = this.kingsPos.black;
                    if (whiteMove) {
                        kingPos = this.kingsPos.white;
                    }

                    Destroy(checkHighlight);
                    if (ChessInspector.IsUnderAttackPos(kingPos, whiteMove, map.board)) {
                        CreateCheckHighlight(kingPos);
                    }

                    possibleMoves.Clear();
                    break;
            }
        }

        private void CreateHighlight() {
            foreach (var move in possibleMoves) {
                var posX = move.first.Value.to.x;
                var posY = move.first.Value.to.y;

                var newX = cellInfo.offset - posX * cellInfo.size;
                var newY = cellInfo.offset - posY * cellInfo.size;
                var objPos = new Vector3(newX, 0.01f, newY);

                var obj = Instantiate(
                    figContent.blueBacklight,
                    objPos,
                    Quaternion.Euler(90, 0, 0),
                    highlight.transform
                );

                obj.transform.localPosition = objPos;
            }
        }

        private void CreateCheckHighlight(Vector2Int kingPos) {
            var newX = cellInfo.offset - kingPos.x * cellInfo.size;
            var newY = cellInfo.offset - kingPos.y * cellInfo.size;
            var objPos = new Vector3(newX, 0.01f, newY);

            checkHighlight = Instantiate(
                figContent.redBacklight,
                objPos,
                Quaternion.Euler(90, 0, 0),
                boardTransform
            );

            checkHighlight.transform.localPosition = objPos;
        }

        private void Relocate(Move move, Option<Fig>[,] board) {
            var fig = board[move.from.x, move.from.y].Peel();
            var moveRes = MoveEngine.MoveFigure(move, board);
            var posFrom = move.from;
            var posTo = move.to;

            if (fig.type == FigureType.King) {

                kingsPos.black = posTo;
                if (fig.white) {
                    kingsPos.white = posTo;
                }
            }

            if (move.destroyPos != null) {
                var posX = move.destroyPos.Value.x;
                var posY = move.destroyPos.Value.y;
                Destroy(map.figures[posX, posY]);
            }

            map.figures[posTo.x, posTo.y] = map.figures[posFrom.x, posFrom.y];
            map.figures[posFrom.x, posFrom.y] = null;

            var newX = cellInfo.offset - posTo.x * cellInfo.size;
            var newY = cellInfo.offset - posTo.y * cellInfo.size;
            var newPos = new Vector3(newX, 0.0f, newY);

            map.figures[posTo.x, posTo.y].transform.localPosition = newPos;
        }

        public void PromotionPawn(GameObject wFig, GameObject bFig, FigureType type) {
            var figObj = bFig;
            var fig = Fig.CreateFig(false, type);
            var posX = selectFigurePos.x;
            var posY = selectFigurePos.y;

            var newX = cellInfo.offset - posX * cellInfo.size;
            var newY = cellInfo.offset - posY * cellInfo.size;
            var newPos = new Vector3(newX, 0.0f, newY);

            if (posX == 0) {
                fig = Fig.CreateFig(true, type);
                figObj = wFig;
            }

            map.board[posX, posY] = Option<Fig>.Some(fig);
            Destroy(map.figures[posX, posY]);
            map.figures[posX, posY] = Instantiate(
                figObj,
                newPos,
                Quaternion.Euler(0, 90, 0),
                boardTransform
            );

            map.figures[posX, posY].transform.localPosition = newPos;
            changePawnUi.SetActive(!changePawnUi.activeSelf);
            this.enabled = !this.enabled;
        }
    }
}