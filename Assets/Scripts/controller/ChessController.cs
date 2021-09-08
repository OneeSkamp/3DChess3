using System.Collections.Generic;
using UnityEngine;
using inspector;
using chess;
using board;
using option;
using move;
using movements;

namespace controller {
    public enum State {
        None,
        FigureSelected
    }

    public struct Map {
        public Option<Fig>[,] board;
        public GameObject[,] figures;
    }

    public struct KingPos {
        public Vector2Int white;
        public Vector2Int black;
    }

    public struct Cell {
        public int count;
        public float size;
        public float offset;
    }

    public class ChessController : MonoBehaviour {
        public Transform empty1;
        public Transform empty2;

        public Cell cell;

        public Map map;
        public bool whiteMove = true;

        public GameObject highlight;

        public GameObject changePawnUi;

        public FigureSpawner figureSpawner;
        public FigureResourses figContent;

        public KingPos kingPos;
 
        private List<DoubleMove> possibleMoves = new List<DoubleMove>();

        private Vector2Int selectFigurePos;
        private Vector2Int promotionPawnPos;

        private State state;

        private void Awake() {
            var emp1PosX = empty1.localPosition.x;
            var emp2PosX = empty2.localPosition.x;

            cell.count = 8;
            cell.size = (Mathf.Abs(emp1PosX) + Mathf.Abs(emp2PosX)) / cell.count;
            cell.offset = empty1.position.x - cell.size / 2;

            kingPos = new KingPos {
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

            var localHit = figureSpawner.boardTransform.InverseTransformPoint(hit.point);

            var pos = new Vector2Int (
                Mathf.Abs((int)((localHit.x - empty1.position.x) / cell.size)),
                Mathf.Abs((int)((localHit.z - empty1.position.x) / cell.size))
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
                state = State.None;
            }

            var fig = figOpt.Peel();

            switch (state) {
                case State.None:
                    if (figOpt.Peel().white != whiteMove) {
                        break;
                    }
                    var movement = movements[figOpt.Peel().type];
                    selectFigurePos = pos;

                    possibleMoves.Clear();

                    var kingPos = this.kingPos.black;
                    if (whiteMove) {
                        kingPos = this.kingPos.white;
                    }

                    possibleMoves = ChessInspector.GetPossibleMoves(
                        selectFigurePos,
                        kingPos,
                        map.board
                    );

                    CreatingHighlight();
                    state = State.FigureSelected;
                    break;
                case State.FigureSelected:
                    var move = new Move {
                        from = selectFigurePos,
                        to = pos
                    };

                    foreach (DoubleMove possMove in possibleMoves) {
                        if (!possMove.first.HasValue) {
                            continue;
                        }

                        var firstMove = possMove.first.Value;
                        if (move.to == firstMove.to && move.from == firstMove.from) {
                            if (IsPromotionMove(move)) {
                                changePawnUi.SetActive(!changePawnUi.activeSelf);
                                this.enabled = !this.enabled;
                            }
                            Relocate(possMove.first.Value, map.board);
                            whiteMove = !whiteMove;

                            if (possMove.second.HasValue) {
                                Relocate(possMove.second.Value, map.board);
                            }
                            break;
                        }
                    }

                    kingPos = this.kingPos.black;
                    if (whiteMove) {
                        kingPos = this.kingPos.white;
                    }

                    possibleMoves.Clear();
                    break;
            }
        }

        private void CreatingHighlight() {
            foreach (DoubleMove move in possibleMoves) {
                var posX = move.first.Value.to.x;
                var posY = move.first.Value.to.y;

                var newX = cell.offset - posX * cell.size;
                var newY = cell.offset - posY * cell.size;
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

        private void Relocate(Move move, Option<Fig>[,] board) {
            var fig = board[move.from.x, move.from.y].Peel();
            var moveRes = MoveEngine.MoveFigure(move, board);
            var posFrom = move.from;
            var posTo = move.to;

            if (fig.type == FigureType.King) {

                kingPos.black = posTo;
                if (fig.white) {
                    kingPos.white = posTo;
                }
            }

            if (move.destroyPos != null) {
                var posX = move.destroyPos.Value.x;
                var posY = move.destroyPos.Value.y;
                Destroy(map.figures[posX, posY]);
            }

            map.figures[posTo.x, posTo.y] = map.figures[posFrom.x, posFrom.y];
            map.figures[posFrom.x, posFrom.y] = null;

            var newX = cell.offset - posTo.x * cell.size;
            var newY = cell.offset - posTo.y * cell.size;
            var newPos = new Vector3(newX, 0.0f, newY);

            map.figures[posTo.x, posTo.y].transform.localPosition = newPos;
        }

        private bool IsPromotionMove(Move move) {
            var figOpt = map.board[move.from.x, move.from.y];
            if (figOpt.IsSome()) {
                var fig = map.board[move.from.x, move.from.y].Peel();

                if (fig.type == FigureType.Pawn && move.to.x == 0) {
                    promotionPawnPos = new Vector2Int(move.to.x, move.to.y);
                    return true;
                }

                if (fig.type == FigureType.Pawn && move.to.x == 7) {
                    promotionPawnPos = new Vector2Int(move.to.x, move.to.y);
                    return true;
                }
            }

            return false;
        }
        public void PromotionPawn(GameObject wFig, GameObject bFig, FigureType type) {
            var posX = promotionPawnPos.x;
            var posY = promotionPawnPos.y;

            var newX = cell.offset - posX * cell.size;
            var newY = cell.offset - posY * cell.size;
            var newPos = new Vector3(newX, 0.0f, newY);

            if (posX == 0) {
                var fig = Fig.CreateFig(true, type);
                map.board[posX, posY] = Option<Fig>.Some(fig);
                Destroy(map.figures[posX, posY]);

                map.figures[posX, posY] = Instantiate(
                    wFig,
                    newPos,
                    Quaternion.Euler(0, 90, 0),
                    figureSpawner.boardTransform
                );
            }

            if (posX == 7) {
                var fig = Fig.CreateFig(false, type);
                map.board[posX, posY] = Option<Fig>.Some(fig);
                Destroy(map.figures[posX, posY]);

                map.figures[posX, posY] = Instantiate(
                    bFig,
                    newPos,
                    Quaternion.Euler(0, 90, 0),
                    figureSpawner.boardTransform
                );
            }

            map.figures[posX, posY].transform.localPosition = newPos;
            changePawnUi.SetActive(!changePawnUi.activeSelf);
            this.enabled = !this.enabled;
        }

    }
}