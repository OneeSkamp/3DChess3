using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using visual;

namespace ui {
    public class UiController : MonoBehaviour {
        public GameObject CheckMateUi;
        public GameObject ChangePawnUi;
        public GameObject MenuUi;

        public Text endGameText;
        public string checkmate;
        public string stalemate;

        public Button menuBut;
        public Button newGameBut;
        public Button newGameMenuBut;
        public Button saveGameBut;
        public Button loadGameBut;
        public Button queenBut;
        public Button bishopBut;
        public Button rookBut;
        public Button knightBut;

        private ChessController chessController;
        private FigureResurses figCont;

        private Action openMenu;
        private Action newGame;
        private Action saveGame;
        private Action loadGame;
        private Action changeOnQueen;
        private Action changeOnBishop;
        private Action changeOnRook;
        private Action changeOnKnight;

        private void Awake() {

            chessController = gameObject.GetComponent<ChessController>();
            figCont = gameObject.GetComponent<FigureResurses>();

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

        public void OpenMenu() {
            MenuUi.SetActive(!MenuUi.activeSelf);
            chessController.enabled = !chessController.enabled;
        }
    }
}

