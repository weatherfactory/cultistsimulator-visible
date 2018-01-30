#pragma warning disable 0649
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;

public class ConfirmationPanel : MonoBehaviour
{

    [SerializeField]
    CanvasGroupFader canvasGroupFader;

    [SerializeField] MenuScreenController menuScreenController;
    public void Show()
    {
        if (canvasGroupFader.IsFading())
            return;

        canvasGroupFader.Show();
    }

    
    public void Hide()
    {
        if (canvasGroupFader.IsFading())
            return;

        canvasGroupFader.Hide();
    }

    public void DeleteSave()
    {
        var saveGameManager = new GameSaveManager(new GameDataImporter(Registry.Retrieve<ICompendium>()), new GameDataExporter());
        saveGameManager.DeleteCurrentSave();
        Hide();
        menuScreenController.SetBeginContinueCondition();
    }

}
