// based on LoadingScreenManager
// --------------------------------
// built by Martin Nerurkar (http://www.martin.nerurkar.de)
// for Nowhere Prophet (http://www.noprophet.com)
//
// Licensed under GNU General Public License v3.0
// http://www.gnu.org/licenses/gpl-3.0.txt

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using Assets.TabletopUi;
using Assets.TabletopUi.Scripts.Infrastructure;
using Assets.TabletopUi.Scripts.Infrastructure.Modding;
using Assets.TabletopUi.Scripts.Interfaces;
using Assets.TabletopUi.Scripts.Services;
using Noon;
using TabletopUi.Scripts.Services;
using TabletopUi.Scripts.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuScreenController : LocalNexus {

    // can be used to disable interaction when we start loading into a scene
    public EventSystem eventSystem;

    [Header("Buttons")]
    public Button newGameButton;
    public Button continueGameButton;
    public Button purgeButton;
    public Button modsButton;
    public Button languageButton;

    [Header("Overlays")]
    public CanvasGroupFader modal;
    public CanvasGroupFader purgeConfirm;
	public CanvasGroupFader credits;
	public CanvasGroupFader settings;
	public CanvasGroupFader languageMenu;
    public CanvasGroupFader versionHints;
    public CanvasGroupFader modsPanel;
    public CanvasGroupFader startDLCLegacyConfirmPanel;


    [Header("Hints")]
    public GameObject brokenSaveMessage;
    public TextMeshProUGUI VersionNumber;
    public Animation versionAnim;
    public MenuSubtitle Subtitle;

    [Header("DLC & Mods)")]
    public Transform legacyStartEntries;
    public MenuLegacyStartEntry LegacyStartEntryPrefab;
    public GameObject modEntryPrefab;
    public TextMeshProUGUI steamWorkshopDownloadLink;
    public TextMeshProUGUI modEmptyMessage;
    public Transform modEntries;

    [Header("Localisation")]
    //  public Button russianLanguageButton;
    public Transform LanguagesAvailable;
    public GameObject languageChoicePrefab;


    bool canTakeInput;
    int sceneToLoad;
    VersionNumber currentVersion;
    CanvasGroupFader currentOverlay;

    


    private static readonly StartableLegacySpec[] DlcEntrySpecs =
    {
        new StartableLegacySpec(
            "dancer",
            "UI_DLC_TITLE_DANCER",
            new Dictionary<Storefront, string>
            {
                {Storefront.Steam, "https://store.steampowered.com/app/871650/Cultist_Simulator_The_Dancer/"},
                {Storefront.Gog, "https://www.gog.com/game/cultist_simulator_the_dancer"},
                {Storefront.Humble, "https://www.humblebundle.com/store/cultist-simulator-the-dancer"},
                {Storefront.Unknown,"https://www.cultistsimulator.com" }
            },
            true,
            null),
        new StartableLegacySpec(
            "priest",
            "UI_DLC_TITLE_PRIEST",
            new Dictionary<Storefront, string>
            {
                {Storefront.Steam, "https://store.steampowered.com/app/871651/Cultist_Simulator_The_Priest/"},
                {Storefront.Gog, "https://www.gog.com/game/cultist_simulator_the_priest"},
                {Storefront.Humble, "https://www.humblebundle.com/store/cultist-simulator-the-priest"},
                {Storefront.Unknown,"https://www.cultistsimulator.com" }
            },
            true,
                null),
        new StartableLegacySpec(
            "ghoul",
            "UI_DLC_TITLE_GHOUL",
            new Dictionary<Storefront, string>
            {
                {Storefront.Steam, "https://store.steampowered.com/app/871900/Cultist_Simulator_The_Ghoul/"},
                {Storefront.Gog, "https://www.gog.com/game/cultist_simulator_the_ghoul"},
                {Storefront.Humble, "https://www.humblebundle.com/store/cultist-simulator-the-ghoul"},
                {Storefront.Unknown,"https://www.cultistsimulator.com" }
            },
            true,
                null)
        ,
        new StartableLegacySpec(
            "exile",
            "UI_DLC_TITLE_EXILE",
            new Dictionary<Storefront, string>
            {
                {Storefront.Steam, "https://weatherfactory.biz/mar-1-edmund/"},
                {Storefront.Gog, "https://weatherfactory.biz/mar-1-edmund/"},
                {Storefront.Humble, "https://weatherfactory.biz/mar-1-edmund/"},
                {Storefront.Unknown, "https://weatherfactory.biz/mar-1-edmund/" }
            },
            true,
            null
            )
    };

    
    void Start() {

        InitialiseServices();

        //// We delay the showing to get a proper fade in
        Invoke("UpdateAndShowMenu", 0.1f);
        
        var concursum = Registry.Get<Concursum>();

        concursum.ContentUpdatedEvent.AddListener(OnContentUpdated);

    }

    private void OnContentUpdated(ContentUpdatedArgs args)
    {
        BuildLegacyStartsPanel(Registry.Get<MetaInfo>().Storefront);
    }

    private static void SetEditionStatus()
    {
        string perpetualEditionDumbfileLocation = Application.streamingAssetsPath + "/edition/semper.txt";
        if (File.Exists(perpetualEditionDumbfileLocation))
            NoonUtility.PerpetualEdition = true;
    }



    void InitialiseServices()
	{
        currentVersion = Registry.Get<MetaInfo>().VersionNumber;

        var store = Registry.Get<MetaInfo>().Storefront;

        BuildLegacyStartsPanel(store);
        BuildModsPanel(store);
        BuildLanguagesAvailablePanel();

    }




    void UpdateAndShowMenu()
    {
        
        var currentCharacter = Registry.Get<Character>();

        // Show the buttons as needed
        var savedGameExists = (currentCharacter.State != CharacterState.Unformed);

        newGameButton.gameObject.SetActive(!savedGameExists);
        continueGameButton.gameObject.SetActive(savedGameExists);
        purgeButton.gameObject.SetActive(savedGameExists);

        //update subtitle text
        SetEditionStatus();

        
        if (currentCharacter.ActiveLegacy != null)
            //we need to go the long wway round because the label on the legacy entity in the character won't have changed if the compendium has just been repopulated with a different culture
            Subtitle.SetText(Registry.Get<ICompendium>().GetEntityById<Legacy>(currentCharacter.ActiveLegacy.Id).Label);
        else
        {
            if (NoonUtility.PerpetualEdition)
            {
                Subtitle.UpdateWithLocValue("UI_PERPETUAL_EDITION");
            }
            else
		    {
                Subtitle.UpdateWithLocValue("UI_BRING_THE_DAWN");
               
            }
        }
        
        //brokenSaveMessage.gameObject.SetActive(savedGameExists);
        UpdateVersionNumber();
        HideAllOverlays();

        // now we can take input
		canTakeInput = true;
    }

    void UpdateVersionNumber() {
        // Show the current version number
        VersionNumber.text = currentVersion.Version;

       // if (hasNews)
     //       versionAnim.Play();
      //  else
     //       versionAnim.Stop();
    }

    void HideAllOverlays() {
        modal.gameObject.SetActive(false);
        purgeConfirm.gameObject.SetActive(false);
        credits.gameObject.SetActive(false);
        versionHints.gameObject.SetActive(false);
        modsPanel.gameObject.SetActive(false);
        languageMenu.gameObject.SetActive(false);
    startDLCLegacyConfirmPanel.gameObject.SetActive(false);


}

#region -- View Changes ------------------------



    void ShowOverlay(CanvasGroupFader overlay) {
		if (currentOverlay != null)
            return;

        currentOverlay = overlay;

        overlay.Show();
        modal.Show();
    }

    void HideCurrentOverlay() {
        if (currentOverlay == null)
            return;

        currentOverlay.Hide();
        modal.Hide();
        currentOverlay = null;
    }

#endregion

#region -- User Actions via Scene Buttons ------------------------

    public void BeginGameWithDefaultLegacy()
	{
        if (!canTakeInput)
            return;

        var defaultLegacy = Registry.Get<ICompendium>().GetEntitiesAsList<Legacy>().First();
        BeginNewSaveWithSpecifiedLegacy(defaultLegacy.Id);

    }

    public void ContinueGame()
	{
        if (!canTakeInput)
            return;
		
        if (Registry.Get<Character>().State==CharacterState.Viable) {
            //back into the game!
            Registry.Get<StageHand>().LoadGameOnTabletop(SourceForGameState.DefaultSave);
            return;
        }

        Registry.Get<StageHand>().LegacyChoiceScreen();
    }


    public void TryPurgeSave() {
        if (!canTakeInput)
            return;

        ShowOverlay(purgeConfirm);
    }

    public void PurgeSave() {
        if (!canTakeInput)
            return;

        ResetToLegacy(null);


        currentOverlay = null; // to ensure we can re-open another overlay afterwards
        Registry.Get<StageHand>().MenuScreen();
    }

    private async void ResetToLegacy(Legacy activeLegacy)
    {
        Registry.Get<Character>().Reset(activeLegacy,null);
        var saveGameManager =
            new GameSaveManager(new GameDataImporter(Registry.Get<ICompendium>()), new GameDataExporter());

        var saveTask = saveGameManager.SaveActiveGameAsync(new InactiveTableSaveState(), Registry.Get<Character>(), SourceForGameState.DefaultSave);
        await saveTask;

    }

    public void BeginNewSaveWithSpecifiedLegacy(string legacyId)
    {
        var legacy= Registry.Get<ICompendium>().GetEntityById<Legacy>(legacyId);
        ResetToLegacy(legacy);
        
        Registry.Get<StageHand>().NewGameOnTabletop();
    }

    public void ShowCredits() {
        if (!canTakeInput)
            return;

        ShowOverlay(credits);
	}

	public void ShowSettings() {
		if (!canTakeInput)
			return;

		ShowOverlay(settings);
	}

	public void ShowLanguage()
	{
		if (!canTakeInput)
			return;
		
		ShowOverlay(languageMenu);
	}

	public void SetLanguage( string lang_code )
	{
		if (!canTakeInput)
			return;

        var culture = Registry.Get<ICompendium>().GetEntityById<Culture>(lang_code);

        Registry.Get<Concursum>().SetNewCulture(culture);

        HideCurrentOverlay();

        UpdateAndShowMenu();
	}

    public void ShowVersionHints() {
        if (!canTakeInput)
            return;

        ShowOverlay(versionHints);
        versionAnim.Stop(); // Ensure that the anim is no longer playing
    }


    public void ShowModsPanel()
    {
        if (!canTakeInput)
            return;
        
        ShowOverlay(modsPanel);
    }

    public void ShowStartLegacyConfirmPanel(Legacy legacy)
    {
        HideCurrentOverlay();
        StartDLCLegacyConfirm confirmPanelComponent=startDLCLegacyConfirmPanel.GetComponent<StartDLCLegacyConfirm>();
        confirmPanelComponent.LegacyId = legacy.Id;
        confirmPanelComponent.TitleText.text = legacy.Label;
        confirmPanelComponent.DescriptionText.text =$"\"{legacy.Description}\"";
        confirmPanelComponent.msc = this;

        ShowOverlay(startDLCLegacyConfirmPanel);
        
    }

    private void BuildLegacyStartsPanel(Storefront store)
    {
        foreach (Transform legacyStartEntry in legacyStartEntries)
            Destroy(legacyStartEntry.gameObject);

        var legacies = Registry.Get<ICompendium>().GetEntitiesAsList<Legacy>();

        var allNewStartLegacies = legacies.Where(l => l.NewStart).ToList();


        steamWorkshopDownloadLink.enabled = store == Storefront.Steam;

        var startableLegacySpecs=new List<StartableLegacySpec>();


        //Look for legacies which match a DLC spec
        foreach (var spec in DlcEntrySpecs)
        {
            var matchingLegacy = allNewStartLegacies.SingleOrDefault(l => l.Id == spec.Id);


            //if a legacy is present, that DLC is installed. Add the legacy to the spec for the startable, and remove it from the list of legacies to match.
            if (matchingLegacy != null)
            {
                spec.Legacy = matchingLegacy;
                allNewStartLegacies.Remove(matchingLegacy);
            }
            
            startableLegacySpecs.Add(spec);
        }

        //These are legacies which don't match DLCEntries: so they're mods.
        foreach (var remainingLegacy in allNewStartLegacies)
        {
            
            var specForLegacy=new StartableLegacySpec(remainingLegacy.Id,remainingLegacy.Label, new Dictionary<Storefront, string>(),false,remainingLegacy );
            startableLegacySpecs.Add(specForLegacy);
        }

   
        startableLegacySpecs=startableLegacySpecs.OrderByDescending(ls => ls.ReleasedByWf).ToList();


        foreach (var legacySpec in startableLegacySpecs)
        {
            var legacyStartEntry = Instantiate(LegacyStartEntryPrefab, legacyStartEntries);
            legacyStartEntry.Initialize(legacySpec, store, this);
        }


    }
    

    private void BuildModsPanel(Storefront store)
    {
        // Clear the list and repopulate with one entry per loaded mod
        foreach (Transform modEntry in modEntries)
            Destroy(modEntry.gameObject);
        
        foreach (var mod in Registry.Get<ModManager>().GetCataloguedMods())
        {
            var modEntry = Instantiate(modEntryPrefab).GetComponent<ModEntry>();
            modEntry.transform.SetParent(modEntries, false);
            modEntry.Initialise(mod,store);
        }

        //display 'no mods' if no mods are available to enable
        modEmptyMessage.enabled = !Registry.Get<ModManager>().GetCataloguedMods().Any();
    }

    private void BuildLanguagesAvailablePanel()
    {
        foreach(Transform languageAvailable in LanguagesAvailable)
            Destroy(languageAvailable.gameObject);



        foreach (var culture in Registry.Get<ICompendium>().GetEntitiesAsList<Culture>())
        {
            var languageChoice =Instantiate(languageChoicePrefab).GetComponent<LanguageChoice>();
            languageChoice.transform.SetParent(LanguagesAvailable,false);
            languageChoice.Label.text = culture.Endonym;
            languageChoice.Label.font = Registry.Get<ILocStringProvider>().GetFont(LanguageManager.eFontStyle.Button, culture.FontScript);
            languageChoice.gameObject.GetComponent<Button>().onClick.AddListener(()=>SetLanguage(culture.Id));
        }

    
    }
    
    public void CloseCurrentOverlay() {
        if (!canTakeInput)
            return;

        HideCurrentOverlay();
    }

    public void Exit() {
        if (!canTakeInput)
            return;

        Application.Quit();
    }



    public void ShowPromo()
    {
        SoundManager.PlaySfx("UIButtonClick");
        Application.OpenURL(
            "https://weatherfactory.us13.list-manage.com/subscribe?u=97d06a3faac1573fa4330bb7d&id=c3f9b32720");
    }

#endregion
}
