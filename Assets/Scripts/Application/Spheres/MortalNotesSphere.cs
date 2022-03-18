using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Entities;
using SecretHistories.Commands;
using SecretHistories.Constants;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Manifestations;
using SecretHistories.Spheres;
using SecretHistories.UI;
using UnityEngine;

namespace Assets.Scripts.Application.Spheres //should be SecretHistories.Sphere. But that'll break save/load until I make save/load less fussy.
{


    [IsEmulousEncaustable(typeof(Sphere))]
    public class MortalNotesSphere : NotesSphere
    {

        public override Type GetDefaultManifestationType()
        {
            return typeof(MortalTextManifestation);
        }


    }

}
