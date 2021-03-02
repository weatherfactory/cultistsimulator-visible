// based on LoadingScreenManager
// --------------------------------
// built by Martin Nerurkar (http://www.martin.nerurkar.de)
// for Nowhere Prophet (http://www.noprophet.com)
//
// Licensed under GNU General Public License v3.0
// http://www.gnu.org/licenses/gpl-3.0.txt

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Application.Entities.NullEntities;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.UI;
using SecretHistories.Constants;
using SecretHistories.Constants.Modding;
using SecretHistories.Services;
using SecretHistories.Enums.UI;
using SecretHistories.Infrastructure;
using SecretHistories.Infrastructure.Persistence;
using TMPro;

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
    [SerializeField] public Notifier notifier;

    [Header("DLC & Mods")]
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
                {Storefront.Steam, "https://store.steampowered.com/app/1259930/Cultist_Simulator_The_Exile"},
                {Storefront.Gog, "https://www.gog.com/game/cultist_simulator_the_exile"},
                {Storefront.Humble, "https://www.humblebundle.com/store/cultist-simulator-the-exile"},
                {Storefront.Unknown,"https://www.cultistsimulator.com" }
            },
            true,
            null
            )
    };

    
    void Start() {

        InitialiseServices();

        //// We delay the showing to get a proper fade in
        Invoke("UpdateAndShowMenu", 0.1f);
        
        var concursum = Watchman.Get<Concursum>();

        concursum.ContentUpdatedEvent.AddListener(OnContentUpdated);

    }

    private void OnContentUpdated(ContentUpdatedArgs args)
    {
        BuildLegacyStartsPanel(Watchman.Get<MetaInfo>().Storefront);
    }


    void InitialiseServices()
	{
        currentVersion = Watchman.Get<MetaInfo>().VersionNumber;

        var store = Watchman.Get<MetaInfo>().Storefront;

        BuildLegacyStartsPanel(store);
        BuildModsPanel(store);
        BuildLanguagesAvailablePanel();

        new Watchman().Register(notifier); //we register a different notifier in the later tabletop scene

    }




    void UpdateAndShowMenu()
    {
        
        var currentCharacter = Watchman.Get<Stable>().Protag();

        // Show the buttons as needed
        var savedGameExists = (currentCharacter.State != CharacterState.Unformed);

        newGameButton.gameObject.SetActive(!savedGameExists);
        continueGameButton.gameObject.SetActive(savedGameExists);
        purgeButton.gameObject.SetActive(savedGameExists);

      
        
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

        overlay.gameObject.SetActive(true);
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

        var defaultLegacy = Watchman.Get<Compendium>().GetEntitiesAsList<Legacy>().First();
        BeginNewSaveWithSpecifiedLegacy(defaultLegacy.Id);

    }

    public void ContinueGame()
	{
        if (!canTakeInput)
            return;
		
        if (Watchman.Get<Stable>().Protag().State==CharacterState.Viable) {
            //back into the game!
            Watchman.Get<StageHand>().LoadGameOnTabletop(new DefaultGamePersistenceProvider());
            return;
        }

        Watchman.Get<StageHand>().LegacyChoiceScreen();
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
        Watchman.Get<StageHand>().MenuScreen();
    }

    private void ResetToLegacy(Legacy activeLegacy)
    {
     //   Watchman.Get<Stable>().Protag().Reincarnate(activeLegacy,NullEnding.Create());


      throw new NotImplementedException("save here?");

    }

    public void BeginNewSaveWithSpecifiedLegacy(string legacyId)
    {
        var legacy= Watchman.Get<Compendium>().GetEntityById<Legacy>(legacyId);
        var freshGamePersistenceProvider=new FreshGameProvider(legacy);
       
        Watchman.Get<StageHand>().LoadGameOnTabletop(freshGamePersistenceProvider);
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

        var culture = Watchman.Get<Compendium>().GetEntityById<Culture>(lang_code);

        Watchman.Get<Concursum>().SetNewCulture(culture);

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

        var legacies = Watchman.Get<Compendium>().GetEntitiesAsList<Legacy>();

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
        
        foreach (var mod in Watchman.Get<ModManager>().GetCataloguedMods())
        {
            var modEntry = Instantiate(modEntryPrefab).GetComponent<ModEntry>();
            modEntry.transform.SetParent(modEntries, false);
            modEntry.Initialise(mod,store);
        }

        //display 'no mods' if no mods are available to enable
        modEmptyMessage.enabled = !Watchman.Get<ModManager>().GetCataloguedMods().Any();
    }

    private void BuildLanguagesAvailablePanel()
    {
        foreach(Transform languageAvailable in LanguagesAvailable)
            Destroy(languageAvailable.gameObject);



        foreach (var culture in Watchman.Get<Compendium>().GetEntitiesAsList<Culture>().Where(c=>c.Released))
        {
            var languageChoice =Instantiate(languageChoicePrefab).GetComponent<LanguageChoice>();
            languageChoice.transform.SetParent(LanguagesAvailable,false);
            languageChoice.Label.text = culture.Endonym;
            languageChoice.Label.font = Watchman.Get<ILocStringProvider>().GetFont(LanguageManager.eFontStyle.Button, culture.FontScript);
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
