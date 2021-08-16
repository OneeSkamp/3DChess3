using System.Collections.Generic;
using chess;
using board;

public class MoveTypes {
    public static readonly Dictionary<FigureType, MoveType> moveTypes = 
        new Dictionary<FigureType, MoveType> {

            {FigureType.Bishop, new MoveType(false, false, true)},
            {FigureType.Rook, new MoveType(true, false, false)},
            {FigureType.Knight, new MoveType(false, true, false)},
            {FigureType.Queen, new MoveType(true, false, true)},
            {FigureType.Pawn, new MoveType(true, false, true)},
            {FigureType.King, new MoveType(true, false, true)}
    };

}
