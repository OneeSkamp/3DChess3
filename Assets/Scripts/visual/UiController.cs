using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using chess;

namespace visual {
    public class UiController : MonoBehaviour {
        public GameObject CheckMateUi;
        public GameObject ChangePawnUi;
        public GameObject MenuUi;
        private Action openMenu;
        private Action newGame;
        private Action saveGame;
        private Action loadGame;
        private Action changeOnQueen;
        private Action changeOnBishop;
        private Action changeOnRook;
        private Action changeOnKnight;
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
        private FigureContainer figCont;

        private void Start() {
            chessController = gameObject.GetComponent<ChessController>();
            figCont = gameObject.GetComponent<FigureContainer>();

            var queen = FigureType.Queen;
            var bishop = FigureType.Bishop;
            var rook = FigureType.Rook;
            var knight = FigureType.Knight;

            openMenu += chessController.OpenMenu;
            newGame += chessController.NewGame;
            saveGame += chessController.SaveGame;
            loadGame += chessController.LoadGame;

            changeOnQueen += () => chessController.ChangePawn(
                    figCont.wQueen, 
                    figCont.bQueen, 
                    queen
                );
            changeOnBishop += () => chessController.ChangePawn(
                    figCont.wBishop, 
                    figCont.bBishop, 
                    bishop
                );
            changeOnRook += () => chessController.ChangePawn(
                    figCont.wRook, 
                    figCont.bRook, 
                    rook
                );
            changeOnKnight += () => chessController.ChangePawn(
                    figCont.wKnight, 
                    figCont.bKnight, 
                    knight
                );

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
    }
}

