using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assets.Core;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Assets.TabletopUi.Scripts.Services;
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

            new Registry().Register(new ModManager());

            var contentImporter = new CompendiumLoader();
            var messages = contentImporter.PopulateCompendium(new Compendium(), Registry.Get<Concursum>().GetCurrentCultureId());

            foreach (var p in messages.GetMessages().Where(m=>m.MessageLevel>0))
                NoonUtility.Log(p.Description, messageLevel: p.MessageLevel);

            NoonUtility.Log("-- Verification complete --");

        }


    }
    }

