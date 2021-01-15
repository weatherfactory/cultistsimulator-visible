using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Enums
{
    public enum AnchorDurability
    {
        Enduring,
        Transient
    }

    public enum CharacterState
    {
        Viable, Extinct, Unformed
    }
    public enum GameSpeed
    {
        DeferToNextLowestCommand = -1,
        Paused = 0,
        Normal = 1,
        Fast = 2
    }
    public enum MorphEffectType
    {
        Transform = 1,
        Spawn = 2,
        Mutate = 3
    }

    public enum PortalEffect
    {
        None = 0,
        Wood = 10,
        WhiteDoor = 20,
        StagDoor = 30,
        SpiderDoor = 40,
        PeacockDoor = 50,
        TricuspidGate = 60
    }
    public enum RetirementVFX
    {
        CardBurn,
        CardBlood,
        CardBloodSplatter,
        CardDrown,
        CardLight,
        CardLightDramatic,
        CardSpend,
        CardTaken,
        CardTakenShadow,
        CardTakenShadowSlow,
        CardTransformWhite,
        CardHide,
        VerbAnchorVanish,
        None
    }

    public enum SourceType
    {
        Existing,
        Fresh,
        Transformed
    }

    public enum SphereCategory
    {
        Threshold,
        SituationStorage,
        Output,
        World,
        Meta,
        Dormant,
        Null
    }

    public enum SourceForGameState
    {
        NewGame = -1,
        DefaultSave = 0,
        DevSlot1 = 1,
        DevSlot2 = 2,
        DevSlot3 = 3,
        DevSlot4 = 5,
        DevSlot5 = 5,
        DevSlot6 = 6,
        DevSlot7 = 7,
        DevSlot8 = 8,
        DevSlot9 = 9,
        DevSlot10 = 10
    }

    public enum StateEnum {Unstarted,Halting,Ongoing, RequiringExecution, Complete }

    public enum Storefront
    {
        Unknown,
        Steam,
        Gog,
        Itch,
        Humble
    }

    public enum Style
    {
        Assertive,
        Subtle
    }

    public enum SphereRetirementType
    {
        Destructive=1,
        Graceful=2
    }

}
