namespace SecretHistories.Enums
{
    public enum NavigationAnimationDirection { MoveRight, MoveLeft, None }


    public enum CharacterState
    {
        Viable, Extinct, Unformed
    }

    public enum VisibleCharacteristicId
    {Greedy,Consuming}

    public enum SituationDominionEnum { VerbThresholds = 1, RecipeThresholds = 2, Notes = 3, Storage = 4, Output = 5,  Unknown = 0 }

    //exotic effects on tokens, which might include 'set fire to' or 'choose form appropriate for context'. These should be simple! Anything more complex is recipe 
    //territory
    public enum ExoticEffect
    {
        Purge=1, //'Not allowed here, be gone with no particular extra logic or special effects'. This might mean retire, decay to default next stage, or be banished
        BurnPurge=2,//Purge but with a hotter SFX. If no specific Burn behaviour, default to purge.
        DrownPurge=3,//Immersion or dissolution: leaks, flooded rooms. If no specific Drown behaviour, default to purge.
        Halt = 10 //'Stop immediately but non-destructively'
    } 

    public enum FucineValidity { Valid, TokenInRoot, Uninitialised,Empty }

    public enum GameId
    {
        XX=0,
        CS=1,
        BH=2,
        LG=3
    }
    public enum GameSpeed
    {
        DeferToNextLowestCommand = -1,
        Paused = 0,
        Normal = 1,
        Fast = 2
    }

    public enum Interaction { OnClicked, OnReceivedADrop, OnPointerEntered, OnPointerExited, OnDoubleClicked, OnRightClicked,OnDragBegin, OnDrag, OnDragEnd }

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
        Retirement,
        Opening,
        Closing
    }

    public enum PortalEffect
    {
        None = 0,
        Wood = 10,
        WhiteDoor = 20,
        StagDoor = 30,
        SpiderDoor = 40,
        PeacockDoor = 50,
        TricuspidGate = 60,
        ILIKECAKE=70
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
        Default,
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


    public enum StateEnum {Unstarted=1,Halting=2,Ongoing=3, RequiringExecution=4, Complete=5, Inchoate=0}

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

    public enum VerbCategory
    {
        Shabda=1,
        Someone=2,
        Workstation=3
    }

}
