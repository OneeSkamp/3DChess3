using System.Collections.Generic;
using UnityEngine;
using chess;

namespace visual {
    public class ChessController : MonoBehaviour {
        public Board board = new Board(true);

        public FigureSpawner figureSpawner;
        private FigureResurses figCont;

        private int x;
        private int y;

        private float changedX;
        private float changedY;
        private float changedZ;

        private const float CONST = 3.5f;

        private Ray ray;
        private RaycastHit hit;

        public GameObject[,] figuresMap = new GameObject[8,8];

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

                    x = Mathf.Abs((int)(hit.point.x - changedX - 4f));
                    y = Mathf.Abs((int)(hit.point.z - changedZ - 4f));

                    Debug.Log(x + ",,," + y);

                    var fig = board.boardMap[x, y];
                    if (fig.type != FigureType.None) {
                        var moveType = ChessEngine.CalcMoveType(fig);
                        var movePaths = ChessEngine.CalcMovePaths(
                            new Position(x, y), 
                            moveType, 
                            board.boardMap, 
                            board.whiteMove
                        );

                        var possibleMoves = ChessEngine.CalcPossibleMoves(movePaths);

                        CreatingPossibleMoves(possibleMoves);

                    }
                }
            }
        }

        private List<GameObject> CreatingPossibleMoves(List<Position> possibleMoves) {
            var possibleMoveList = new List<GameObject>();

            foreach (Position pos in possibleMoves) {
                var objPos = new Vector3(CONST - pos.x + changedX, 0.01f, CONST - pos.y + changedZ);
                var obj = Instantiate(figCont.blueBacklight, objPos, Quaternion.Euler(90, 0, 0));
                possibleMoveList.Add(obj);
            }

            return possibleMoveList;
        }
    }
}