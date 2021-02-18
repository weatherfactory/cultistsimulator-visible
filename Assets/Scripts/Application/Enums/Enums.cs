using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Enums
{
    public enum NavigationAnimationDirection { MoveRight, MoveLeft, None }


    public enum CharacterState
    {
        Viable, Extinct, Unformed
    }

    public enum FucineValidity { Valid, ParsingError, TokenInRoot, Uninitialised,Empty }

    public enum GameSpeed
    {
        DeferToNextLowestCommand = -1,
        Paused = 0,
        Normal = 1,
        Fast = 2
    }

    public enum Interaction { OnClicked, OnReceivedADrop, OnPointerEntered, OnPointerExited, OnDoubleClicked, OnDragBegin, OnDrag, OnDragEnd }

    public enum MorphEffectType
    {
        Transform = 1,
        Spawn = 2,
        Mutate = 3
    }

    public enum PayloadChangeType
    {
        Fundamental,
        Update,
        Retirement
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
        Notes,
        SituationStorage,
        Output,
        World,
        Meta,
        Dormant,
        Null
    }


    public enum StateEnum {Unstarted=1,Halting=2,Ongoing=3, RequiringExecution=4, Complete=5, Unknown=0}

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
