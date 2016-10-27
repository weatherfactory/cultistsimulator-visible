using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


    public class BoardMonoBehaviour: MonoBehaviour
    {
        private static BoardManager board;
    /// <summary>
    /// Shortcut to get the BoardManager that contains references to a buncha typed UI objects.</summary>
    public BoardManager BM
        {get
        {
            if (BoardMonoBehaviour.board != null)
                return BoardMonoBehaviour.board;
          
                BoardMonoBehaviour.board = GameObject.Find("Board").GetComponent<BoardManager>();
            return BoardMonoBehaviour.board;
        }
    }
    }
