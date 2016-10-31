using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


    public class BoardMonoBehaviour: MonoBehaviour
    {
        private static BoardManager board;
        private static Heart heart;

    public BoardManager BM
        {get
        {
            if (BoardMonoBehaviour.board != null)
                return BoardMonoBehaviour.board;
          
                BoardMonoBehaviour.board = GameObject.Find("Board").GetComponent<BoardManager>();
            return BoardMonoBehaviour.board;
        }
    }
    public Heart Heart
    {
        get
        {
            if (BoardMonoBehaviour.heart != null)
                return BoardMonoBehaviour.heart;

            BoardMonoBehaviour.heart = GameObject.Find("Heart").GetComponent<Heart>();
            return BoardMonoBehaviour.heart;
        }
    }
}
