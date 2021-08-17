using System.Collections.Generic;
using board;

namespace chess {
    public class MoveTypes {
        public static readonly Dictionary<FigureType, MoveType> moveTypes = 
            new Dictionary<FigureType, MoveType> {
                {FigureType.Bishop, new MoveType(false, false, true, 8)},
                {FigureType.Rook, new MoveType(true, false, false, 8)},
                {FigureType.Knight, new MoveType(false, true, false, 1)},
                {FigureType.Queen, new MoveType(true, false, true, 8)},
                {FigureType.Pawn, new MoveType(true, false, true, 1)},
                {FigureType.King, new MoveType(true, false, true, 1)}
            };
    }
}

