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
        private Action changeOnQueen;
        private Action changeOnBishop;
        private Action changeOnRook;
        private Action changeOnKnight;

        private void Awake() {
            var queen = FigureType.Queen;
            var bishop = FigureType.Bishop;
            var rook = FigureType.Rook;
            var knight = FigureType.Knight;

            changeOnQueen += () => chessController.PromotionPawn(
                figCont.wQueen, 
                figCont.bQueen, 
                queen
            );
            changeOnBishop += () => chessController.PromotionPawn(
                figCont.wBishop, 
                figCont.bBishop, 
                bishop
            );
            changeOnRook += () => chessController.PromotionPawn(
                figCont.wRook, 
                figCont.bRook, 
                rook
            );
            changeOnKnight += () => chessController.PromotionPawn(
                figCont.wKnight, 
                figCont.bKnight, 
                knight
            );

            openMenu += OpenMenu;

            menuBut.onClick.AddListener(() => openMenu());
            saveGameBut.onClick.AddListener(() => saveGame());
            newGameBut.onClick.AddListener(() => newGame());
            newGameMenuBut.onClick.AddListener(() => newGame());
            loadGameBut.onClick.AddListener(() => loadGame());

            queenBut.onClick.AddListener(() => changeOnQueen());
            bishopBut.onClick.AddListener(() => changeOnBishop());
            rookBut.onClick.AddListener(() => changeOnRook());
            knightBut.onClick.AddListener(() => changeOnKnight());
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

