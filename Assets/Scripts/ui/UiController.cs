using UnityEngine.UI;
using UnityEngine;
using controller;
using chess;
using board;

namespace ui {
    public class UiController : MonoBehaviour {
        public GameObject MenuUi;
        public GameObject ChackMateUi;
        public Button menuBut;
        public Button newGameBut;
        public Button newGameMenuBut;
        public Button saveGameBut;
        public Button loadGameBut;
        public Button queenBut;
        public Button bishopBut;
        public Button rookBut;
        public Button knightBut;

        public ChessController chessController;
        public FigureSpawner figureSpawner;

        public void PromoteOnQueen() {
            chessController.PromotionPawn(FigureType.Queen);
        }

        public void PromoteOnBishop() {
            chessController.PromotionPawn(FigureType.Bishop);
        }

        public void PromoteOnRook() {
            chessController.PromotionPawn(FigureType.Rook);
        }

        public void PromoteOnKnight() {
            chessController.PromotionPawn(FigureType.Knight);
        }

        private void OpenMenu() {
            MenuUi.SetActive(!MenuUi.activeSelf);
            chessController.enabled = !chessController.enabled;
        }

        public void NewGame() {
            foreach (var fig in chessController.map.figures) {
                Destroy(fig);
            }

            chessController.whiteMove = true;
            chessController.map.board = BoardEngine.CopyBoard(chessController.map.startBoard);
            figureSpawner.CreateFiguresOnBoard(chessController.map.startBoard);
            ChackMateUi.SetActive(!ChackMateUi.activeSelf);
        }

        public void SaveGame() {
            OpenMenu();
        }
    }
}

