using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// I used this so everything could have a reference back to BoardManager, when I was still putting everything together.
/// So the first-pass approach was: BMs know about everything, everything knows about BM, the logic is in BM. This is obviously NOT IDEAL
/// The model I'm moving towards is:
/// - State is kept in entity objects like Character, RecipeSituation and SituationElementContainer
/// - When entity objects are created, UI elements (and other entities that need to know about changes) are added as subscribers
/// - state changes are published and the UI updates
/// 
/// At the moment, the BoardManager tends to be a subscriber itself- I would rather make lower-level UI components subscribers
/// Ultimately, I would expect to make BoardManager an explicit reference in the Inspector in all lower-level components, and
/// retire this class completely.
/// </summary>
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
