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
        private FigureResurses figCont;

        private int x;
        private int y;

        private float changedX;
        private float changedY;
        private float changedZ;

        private const float CONST = 5.3f;

        private Ray ray;
        private RaycastHit hit;

        private List<GameObject> possibleMoveList;

        private void Awake() {
            figCont = GetComponent<FigureResurses>();
            figureSpawner = GetComponent<FigureSpawner>();

            changedX = figureSpawner.boardTransform.position.x;
            changedY = figureSpawner.boardTransform.position.y;
            changedZ = figureSpawner.boardTransform.position.z;
        }

        private void Update() {

            if (Input.GetMouseButtonDown(0)) {

                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit)) {

                    x = Mathf.Abs((int)((hit.point.x - changedX - 6f) / 1.5f));
                    y = Mathf.Abs((int)((hit.point.z - changedZ - 6f) / 1.5f));

                    Debug.Log(x + ",,," + y);

                    var fig = board.boardMap[x, y].Peel();

                    Debug.Log(fig.type);

                    if (fig.type != FigureType.None) {
                        var moveType = ChessEngine.GetMoveType(fig);
                        var movePaths = ChessEngine.CalcMovePaths(
                            new Position(x, y),
                            moveType,
                            board.boardMap
                        );

                        if (possibleMoveList != null) {
                            foreach (GameObject cell in possibleMoveList) {
                                Destroy(cell);
                            }
                        }

                        var possibleMoves = ChessEngine.CalcPossibleMoves(movePaths);

                        possibleMoveList = CreatingPossibleMoves(possibleMoves);

                    }
                }
            }
        }

        private List<GameObject> CreatingPossibleMoves(List<Position> possibleMoves) {
            var possibleMovesObj = new List<GameObject>();

            foreach (Position pos in possibleMoves) {
                var objPos = new Vector3(
                    (CONST - pos.x + changedX) - 1.5f, 
                    0.01f, 
                    (CONST - pos.y + changedZ) - 1.5f
                );
                var obj = Instantiate(figCont.blueBacklight, objPos, Quaternion.Euler(90, 0, 0));
                possibleMovesObj.Add(obj);
            }

            return possibleMovesObj;
        }
    }
}