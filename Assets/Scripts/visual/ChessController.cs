using System.Collections.Generic;
using UnityEngine;
using chess;

namespace visual {
    public class ChessController : MonoBehaviour {
        public Board board = new Board(true);

        private int x;
        private int y;

        private Ray ray;
        private RaycastHit hit;

        public GameObject[,] figuresMap = new GameObject[8,8];

        private void Update() {

            if (Input.GetMouseButtonDown(0)) {

                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit)) {

                    // float a = hit.point.x;
                    // float b = hit.point.z;

                    x = Mathf.Abs((int)(hit.point.x - 4f));
                    y = Mathf.Abs((int)(hit.point.z - 4f));
                    Debug.Log(x + "//" + y);
                    //Debug.Log(a + "//" + b);

                }
            }
        }

        private Position SelectFigure(int x, int y) {
            var figPos = new Position();
            figPos.x = x;
            figPos.y = y;
            return figPos;
        }
    }
}