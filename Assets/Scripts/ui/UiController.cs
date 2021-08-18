using System;
using UnityEngine.UI;
using UnityEngine;
using controller;
using parse;

namespace ui {
    public class UiController : MonoBehaviour {
        public GameObject MenuUi;

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

