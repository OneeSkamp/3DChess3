using System;
using System.Collections.Generic;
using option;

namespace board {
    [Serializable]
    public class Board<T> {
        public bool whiteMove;
        public int height = 8;
        public int width = 8;
        public Option<T>[,] boardMap;

        public readonly List<Dir> lineDir = new List<Dir>();
        public readonly List<Dir> diagonalDir = new List<Dir>();
        public readonly List<Dir> circularDir = new List<Dir>();

        public Board(bool whiteMove, Option<T>[,] figs) {
            this.whiteMove = whiteMove;
            boardMap = new Option<T>[height, width];
            this.boardMap = figs;

            lineDir.Add(new Dir(1, 0));
            lineDir.Add(new Dir(0, 1));
            lineDir.Add(new Dir(-1, 0));
            lineDir.Add(new Dir(0, -1));

            diagonalDir.Add(new Dir(1, 1));
            diagonalDir.Add(new Dir(1, -1));
            diagonalDir.Add(new Dir(-1, 1));
            diagonalDir.Add(new Dir(-1, -1));

            circularDir.Add(new Dir(2, 1));
            circularDir.Add(new Dir(2, -1));
            circularDir.Add(new Dir(-2, 1));
            circularDir.Add(new Dir(-2, -1));
            circularDir.Add(new Dir(1, 2));
            circularDir.Add(new Dir(-1, 2));
            circularDir.Add(new Dir(1, -2));
            circularDir.Add(new Dir(-1, -2));
        }
    }
}

