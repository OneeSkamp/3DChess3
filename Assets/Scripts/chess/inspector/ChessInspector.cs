using UnityEngine;
using System.Collections.Generic;
using option;
using chess;
using move;
using board;
using movements;

namespace inspector {
    public class ChessInspector : MonoBehaviour {
        public static bool IsUnderAttackPos(Vector2Int pos, bool white, Option<Fig>[,] board) {
            var figMoves = new List<DoubleMove>();
            var hasFig = true;
            var movements = Movements.movements;

            if (board[pos.x, pos.y].IsNone()) {
                var fig = Fig.CreateFig(white, FigureType.Knight);
                hasFig = false;
                board[pos.x, pos.y] = Option<Fig>.Some(fig);
            }

            var moves = MoveEngine.GetFigureMoves(pos, movements[FigureType.Queen], board);
            
            moves.AddRange(MoveEngine.GetFigureMoves(pos, movements[FigureType.Knight], board));


            foreach (var move in moves) {
                var size = new Vector2Int(board.GetLength(0), board.GetLength(1));
                var toX = move.first.Value.to.x;
                var toY = move.first.Value.to.y;
                if (BoardEngine.IsOnBoard(new Vector2Int(toX, toY), size)) {
                    var figOpt = board[toX, toY];

                    if (figOpt.IsSome()) {
                        var fig = figOpt.Peel();
                        var dmoves = MoveEngine.GetFigureMoves(
                            move.first.Value.to,
                            movements[board[toX, toY].Peel().type],
                            board);
                        figMoves.AddRange(dmoves);
                    }
                }
            }

            foreach (var move in figMoves) {
                if (move.first.Value.to == pos) {
                    return true;
                }
            }

            if (!hasFig) {
                board[pos.x, pos.y] = Option<Fig>.None();
            }

            return false;
        }

        public static List<DoubleMove> GetPossibleMoves(
            Vector2Int pos,
            Vector2Int kingPos,
            Option<Fig>[,] board
        ) {
            var figPossMoves = new List<DoubleMove>();
            var savePos = new DoubleMove();
            var movements= Movements.movements[board[pos.x, pos.y].Peel().type];
            var figMoves = MoveEngine.GetFigureMoves(pos, movements, board);
            
            var boardClone = BoardEngine.CopyBoard(board);

            foreach (var figMove in figMoves) {
                savePos = figMove;
                var to = figMove.first.Value.to;
                var from = figMove.first.Value.from;
                var kPos = kingPos;
                if (board[from.x, from.y].Peel().type == FigureType.King) {
                    kPos = to;
                }

                if (boardClone[to.x, to.y].IsNone()) {
                    boardClone[to.x, to.y] = boardClone[from.x, from.y];
                    boardClone[from.x, from.y] = Option<Fig>.None();
                    if (!IsUnderAttackPos(kPos, true, boardClone)) {
                        figPossMoves.Add(figMove);
                    }

                    boardClone[from.x, from.y] = boardClone[to.x, to.y];
                    boardClone[to.x, to.y] = Option<Fig>.None();
                }

                if (boardClone[to.x, to.y].IsSome()) {
                    var fig = boardClone[to.x, to.y];
                    boardClone[to.x, to.y] = boardClone[from.x, from.y];
                    boardClone[from.x, from.y] = Option<Fig>.None();
                    if (!IsUnderAttackPos(kPos, true, boardClone)) {
                        figPossMoves.Add(figMove);
                    }

                    boardClone[from.x, from.y] = boardClone[to.x, to.y];
                    boardClone[figMove.first.Value.to.x, figMove.first.Value.to.y] = fig;
                }
                kPos = kingPos;
            }
            // foreach(var a in figPossMoves) {
            //             if(a.first.Value.destroyPos != null) {
            //                 Debug.Log("+");
            //             }
            //         }
            return figPossMoves;
        }
    }
}