using System.Collections.Generic;
using UnityEngine;
using chess;
using option;
using king;

namespace castling {
    public enum CastlingType {
        WShortCastling,
        WLongCastling,
        BShortCastling,
        BLongCastling
    }

    public struct CastlingInfo {
        public Vector2Int rookPos;
        public List<Move> castlingMoves;
    }
    public static class CastlingController {
        public static CastlingInfo GetCastlingInfo(
            Dictionary<CastlingType, bool> castlings,
            bool whiteMove,
            Option<Fig>[,] board
        ) {
            var castlingRes = new CastlingInfo();
            var castlingMoves = new List<Move>();
            var kingPos = KingController.FindKingPos(whiteMove, board);
            var king = board[kingPos.x, kingPos.y].Peel();

            var right1 = board[kingPos.x, 5].IsNone();
            var right2 = board[kingPos.x, 6].IsNone();
            var left1 = board[kingPos.x, 3].IsNone();
            var left2 = board[kingPos.x, 2].IsNone();
            var left3 = board[kingPos.x, 1].IsNone();
            var move = new Move();
            var rookPos = new Vector2Int();
            move.from = kingPos;

            if (king.white) {
                if (right1 && right2 && castlings[CastlingType.WShortCastling]) {
                    move.to = new Vector2Int(kingPos.x, 6);
                    rookPos = new Vector2Int(kingPos.x, 7);
                    castlingMoves.Add(move);
                }

                if (left1 && left2 && left3 && castlings[CastlingType.WLongCastling]) {
                    move.to = new Vector2Int(kingPos.x, 2);
                    rookPos = new Vector2Int(kingPos.x, 0);
                    castlingMoves.Add(move);
                }

            } else {
                if (right1 && right2 && castlings[CastlingType.BShortCastling]) {
                    move.to = new Vector2Int(kingPos.x, 6);
                    rookPos = new Vector2Int(kingPos.x, 7);
                    castlingMoves.Add(move);
                }

                if (left1 && left2 && left3 && castlings[CastlingType.BLongCastling]) {
                    move.to = new Vector2Int(kingPos.x, 2);
                    rookPos = new Vector2Int(kingPos.x, 0);
                    castlingMoves.Add(move);
                }
            }
            castlingRes.rookPos = rookPos;
            castlingRes.castlingMoves = castlingMoves;

            return castlingRes;
        }

        public static void ChangeCastlingValues(
            Dictionary<CastlingType, bool> castlings,
            Move move,
            Option<Fig>[,] board
        ) {
            var fig = board[move.from.x, move.from.y].Peel();

            if (fig.type == FigureType.King) {
                if (fig.white) {
                    castlings[CastlingType.WLongCastling] = false;
                    castlings[CastlingType.WShortCastling] = false;
                } else {
                    castlings[CastlingType.BLongCastling] = false;
                    castlings[CastlingType.BShortCastling] = false;
                }
            }

            if (fig.type == FigureType.Rook) {
                if (fig.white) {
                    if (move.from.y == 7) {
                        castlings[CastlingType.WShortCastling] = false;
                    } else {
                        castlings[CastlingType.WLongCastling] = false;
                    }
                } else {
                    if (move.from.y == 7) {
                        castlings[CastlingType.BShortCastling] = false;
                    } else {
                        castlings[CastlingType.BLongCastling] = false;
                    }
                }
            }
        }
    }
}
