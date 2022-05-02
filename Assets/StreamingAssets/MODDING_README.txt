CULTIST SIMULATOR: MODDING

A Cultist Simulator mod is a folder, with contents detailed below. You can install a mod

from the Steam Workshop: by hitting 'Subscribe' on the mod and restarting the game.

locally: by copying that mod to the /mods folder. (Location varies by OS: start your game, open the Options menu, click on "BROWSE FILES", and you'll see it.) Installing mods locally is useful if you're developing your own, or if you don't have access to Steam for whatever reason.

(You can have the same mod installed from a Steam subscription and locally; but you can't activate both versions at once)

CREATING A MOD
--------------

A mod consists of:

synopsis.json: a JSON file providing some meta information about your mod, such as its title and description.

content/ - a folder that can contain content files (decks, elements, endings, legacies, recipes and verbs), with any number of subdirectories.

loc/ - a folder that can contain loc files - see the loc_[culture] folders in the original game install for examples. See also CULTURES, below.

images/ - a directory that can contain game images. These are supported:

images/aspects
images/burns
images/cardbacks
images/elements
images/elements/anim
images/endings
images/ui/loc_[cultureid]
images/legacies
images/statusbaricons
images/verbs
images/verbs/anim
images/ui
images/ui/loc_[cultureid]

You can add the following image files to a loc_[culture] folder to display alternative localised versions of these images:
logo.png (main menu logo)
mailing_list_button.png (main menu mailing list)
map.png (Mansus map)
textbinary.png (autosave toast)

(I'll fold this into something more sensible in culture entities later)


If you encounter problems while working on your mod, check your game's player.log for useful messages. You can find it next to your save file.

synopsis.json

{
    "name": "Mod Name",
    "author": "Mod Author",
    "version": "1.0.0",

    "description": "Mod Description, shown in-game.",
    "description_long": "Long Mod Description."
}

The version number should follow semantic versioning[semver.org] rules, for consistency with other mods.


LOADING ORDER
-------------

The game will process this content in this order:
- all the original game core content files
- all the content files for any enabled mods
- all the original game loc content files
- all the loc files for any enabled mods.

Loading order of mods can be specified by the player. Modders can override loading order of individual entities with the $priority


CONTENT FOR MODS
----------------

A mod's content files use the same format as the core game's content files, except that mod files are always loaded after the base game's "core/" folders (see above). Of course, you may want to change core content, so there's some additional support for that.


Overwriting an entity: Any entity that uses the same ID as a base game entity will overwrite all specified properties for that base entity. Any properties you haven't specified will remain unchanged. For example


{
	"id": "erudition",
	"label": "Enlightenment"
}

will overwrite the label for Erudition but leave everything else unchanged.

The entity can be overwritten more than once. There's no reason to do this in a single mod, but it may be useful if you want more than one mod to work together. e.g.:

{
	"id": "erudition",
	"label": "Enlightenment"
}

{
	"id": "despair",
	"description": "An inheritance of stones"
}




Extending an entity: By adding the property "extends", you can tell Cultist Simulator that this entity should inherit from a list of base game entities, e.g.:

{
    "id": "my_new_entity",
    "extends": ["my_old_entity_a", "my_old_entity_b", "my_old_entity_c"]
}


Per-property extending: Sometimes when extending you just need to make a small change, like adding an entry to a deck spec or incrementing a number.
This is where property operations come into play. When using "extend", you can specify some properties with the format "property_name$operation"
to apply "$operation" to "property_name", where "$operation" is one of the following:

$append:
appends a list of items to the original list property.

$prepend:
prepends a list of items to the original list property.

$add:
adds the specified properties to a dictionary.

$remove:
removes each element in the list from the original property, which can either be a list or a dictionary.

$plus/minus:
modify numeric properties.

$prefix/$postfix/$replace/$replacelast:
modify string properties.

$listedit/$dictedit:
apply $ operations to nested collections.

There are also a handful of element-level modding operations:
$priority - define loading order priority for each entity;
$derives - like 'extends', but child doesn't override parents' collections;
$contentgroups - allows to tag content for whatever purpose;
$mute - boolean, don't display log messages when exporting this entity;


CULTURES
--------

The game currently ships with English, Russian, German, Japanese and Chinese (Simplified). If you'd like to add localisations for another language, add a json file with a culture entity to the /content/cultures folder in your mod. You can copy the format from the existing cultures in /core/cultures.

A culture entity allows you to specify:
- the name of the language
- values for the UI labels in the game
- which base font script a mod will use. 'Font script' here means a selection of font assets that support a particular writing system.

fontscript='latin' uses Philosopher, Titania and Belgrad and should be okay for EFIGS-neighbourhood languages. Font support for Central and Eastern European languages
depends on whether those fonts support Extended Latin, Latin-A and Latin-B. You might be lucky.
fontscript='cyrillic' uses Philosopher, Titania (for numbers) and NotoSans, with the basic Roman and Cyrillic characters.
fontscript='cjk' uses NotoSansCJK, a Google font which provides common characters used in Chinese, Japanese and Korean.
fontscript='jp' uses NotoSansJP: our Japanese-speaking players pointed out cases where this is better for Japanese than NotoSansCJK.
fontscript='latinplus' uses Titania for numbers and NotoSans for everything else. It supports Latin Extended, Latin A and Latin B. If you get white squares when you're trying to localise to
a Central or Eastern European language, try this. (If fontscript isn't specified at all, the game will default to this.)

 This will cover a lot of languages that use or can use a Latin-based script / Romanization, but crikey I'm only starting to understand how complicated all this is. If you're trying to translate something that needs to use other characters, contact us and we'll see what we can do.

CUSTOM DLL LOADING (CURRENTLY EXPERIMENTAL, UNSTABLE, WINDOWS ONLY)
-------------------------------------------------------------------

You can compile a .dll and upload it as part of a mod. Anything up to .NET 4.x should be compatible.

A dll for a mod...

- must go in /[modfolder]/dll
- must have a dll with same name as the mod (absent spaces and special characters) - or must be called main.dll (but this is deprecated and will eventually be removed)
- must contain a class with the same name as the mod (absent spaces and special characters)
- that class must have a public static method with one of these two signatures: Initialise(ISecretHistoriesMod mod) or Initialise()

That's your entry point. From then on you can do anything you like, including loading other code.

DLLs are loaded *when the main app initialises* - before JSON content is loaded. If you disable or enable one, you'll need to restart the CS app to see the effect.

ACKNOWLEDGEMENTS
----------------
The modding framework is based on Lyrositor's original fan-contributed modding framework.
Chelnoque suggested the approach and sample code for DLL loading, and made substantial contributions to mod stability and $operations.


- AK 29/04/2022