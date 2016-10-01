using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


    public class BoardMonoBehaviour: MonoBehaviour
    {
    /// <summary>
    /// Shortcut to get the BoardManager that contains references to a buncha typed UI objects.</summary>
    public BoardManager BM
        {get { 
     return GameObject.Find("Board").GetComponent<BoardManager>();
        }
    }
    }
