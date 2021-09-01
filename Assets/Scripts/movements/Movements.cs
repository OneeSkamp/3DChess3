using System.Collections.Generic;
using UnityEngine;
using board;
using chess;

namespace movements {
    public static class Movements {
        public static List<Movement> GetBishopMovement() {
            var bishopMovement = new List<Movement> {
                new Movement {
                    linear = new LinearMovement {
                        dir = new Vector2Int(1, 1)
                    }
                },
                new Movement {
                    linear = new LinearMovement {
                        dir = new Vector2Int(-1, 1)
                    }
                },
                new Movement {
                    linear = new LinearMovement {
                        dir = new Vector2Int(1, -1)
                    }
                },
                new Movement {
                    linear = new LinearMovement {
                        dir = new Vector2Int(-1, -1)
                    }
                }
            };
            return bishopMovement;
        }

        public static List<Movement> GetRookMovement() {
            var rookMovement = new List<Movement> {
                new Movement {
                    linear = new LinearMovement {
                        dir = new Vector2Int(1, 0)
                    }
                },
                new Movement {
                    linear = new LinearMovement {
                        dir = new Vector2Int(0, 1)
                    }
                },
                new Movement {
                    linear = new LinearMovement {
                        dir = new Vector2Int(0, -1)
                    }
                },
                new Movement {
                    linear = new LinearMovement {
                        dir = new Vector2Int(-1, 0)
                    }
                }
            };

            return rookMovement;
        }

        public static List<Movement> GetKnightMovement() {
            var knightMovemetn = new List<Movement> {
                new Movement {
                    square = new SquareMovement {
                        side = 5
                    }
                }
            };

            return knightMovemetn;
        }

        public static List<Movement> GetKingMovement() {
            var kingMovement = new List<Movement> {
                new Movement {
                    square = new SquareMovement {
                        side = 3
                    }
                }
            };

            return kingMovement;
        }

        public static List<Movement> GetQueenMovement() {
            var queenMovement = new List<Movement>();
            queenMovement.AddRange(GetBishopMovement());
            queenMovement.AddRange(GetRookMovement());

            return queenMovement;
        }

        public static Dictionary<FigureType, List<Movement>> GetMovements() {
            var movements =  new Dictionary<FigureType, List<Movement>> {
                {FigureType.Bishop, GetBishopMovement()},
                {FigureType.Rook, GetRookMovement()},
                {FigureType.Queen, GetQueenMovement()},
                {FigureType.Knight, GetKnightMovement()},
                {FigureType.Pawn, GetQueenMovement()},
                {FigureType.King, GetKingMovement()}
            };

            return movements;
        }
    }
}

