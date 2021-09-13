using System;
using UnityEngine.UI;
using UnityEngine;
using controller;
using parse;
using chess;

namespace ui {
    public class UiController : MonoBehaviour {
        public GameObject MenuUi;

        public FigureResourses figCont;

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
        public ParseJson parseJson;

        private Action openMenu;
        private Action newGame;
        private Action saveGame;
        private Action loadGame;

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

        private void Awake() {
            openMenu += OpenMenu;
            menuBut.onClick.AddListener(() => openMenu());
            saveGameBut.onClick.AddListener(() => saveGame());
            newGameBut.onClick.AddListener(() => newGame());
            newGameMenuBut.onClick.AddListener(() => newGame());
            loadGameBut.onClick.AddListener(() => loadGame());
        }

        private void OpenMenu() {
            MenuUi.SetActive(!MenuUi.activeSelf);
            chessController.enabled = !chessController.enabled;
        }

        public void SaveGame() {
            OpenMenu();
        }
    }
}

