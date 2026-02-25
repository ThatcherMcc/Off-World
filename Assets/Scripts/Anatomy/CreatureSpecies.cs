namespace OffWorld.Anatomy
{
    /// <summary>All creature species in the game. Add new entries as creatures are created.</summary>
    public enum CreatureSpecies
    {
        Wolf,
        Frog,
        Bug,
        JumpingAlien,
        RockBoss,
        // Future creatures
        Thornback,
        Gloomray,
        Burrower,
        Sporemother,
        MimicStalker,
        Shellcrab,
        Voltwasp,
        Coralgor
    }

    /// <summary>Body slot for physical grafts.</summary>
    public enum BodySlot
    {
        Head,
        Body,
        LeftArm,
        RightArm,
        Legs,
        Back
    }

    /// <summary>Graft rarity tier. Apex parts (from bosses) bypass incompatibility penalties.</summary>
    public enum PartTier
    {
        Standard,
        Apex
    }

    /// <summary>DNA extraction quality. Prime = precise incapacitation, Degraded = sloppy.</summary>
    public enum DNATier
    {
        Prime,
        Degraded
    }
}
