using UnityEngine;
using System.Collections.Generic;
using chess;
using board;
using option;

namespace master {
    public static class Master {
        public static List<Move> ChangePawnMoves(
            Vector2Int pos,
            List<Move> moves,
            Option<Fig>[,] board
        ) {
            var newMoves = new List<Move>();
            var fig = board[pos.x, pos.y].Peel();
            var prop = 1;

            if (fig.white) {
                prop = -1;
            }

            var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
            var nextFig = board[pos.x + prop, pos.y];
            var leftPos = new Vector2Int(pos.x + prop, pos.y + prop);
            var rightPos = new Vector2Int(pos.x + prop, pos.y - prop);
            var leftOnBoard = BoardEngine.IsOnBoard(leftPos, size);
            var rightOnBoard = BoardEngine.IsOnBoard(rightPos, size);

            foreach (Move move in moves) {
                if (pos.x == 6 && prop == -1 || pos.x == 1 && prop == 1) {
                    var newPos = new Vector2Int(pos.x + prop * 2, pos.y);
                    if (Equals(newPos, move.to) && nextFig.IsNone()) {
                        newMoves.Add(move);
                    }
                }

                if (Equals(new Vector2Int(pos.x + prop, pos.y), move.to) && nextFig.IsNone()) {
                    newMoves.Add(move);
                }

                if (leftOnBoard && Equals(leftPos, move.to) 
                    && board[pos.x + prop, pos.y + prop].IsSome()) {

                    newMoves.Add(move);
                }

                if (rightOnBoard && Equals(rightPos, move.to) 
                    && board[pos.x + prop, pos.y - prop].IsSome()) {

                    newMoves.Add(move);
                }
            }
            return newMoves;
        }
    }
}

