using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core;
using Assets.CS.TabletopUI;
#if MODS
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
#endif
using Noon;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class ContentValidator
    {
        [MenuItem("Tools/Validate Content Import %#i")]
        private static void ValidateContentImport()
        {
            // Clear the console of previous messages to reduce confusion
            EditorUtils.ClearConsole();

            NoonUtility.Log("-- Checking content files for errors --");
#if MODS
            new Registry().Register(new ModManager(false));
#endif
            var contentImporter = new ContentImporter();
            var messages = contentImporter.PopulateCompendium(new Compendium());

            foreach (var p in messages.GetMessages().Where(m=>m.MessageLevel>0))
                NoonUtility.Log(p.Description, messageLevel: p.MessageLevel);

            NoonUtility.Log("-- Verification complete --");

        }


    }
    }

