

/// <summary>
/// Enums
/// </summary>
namespace ErgoShop.Utils
{
    /// <summary>
    ///     used to create wall or opening
    /// </summary>
    public enum WallType
    {
        Wall,
        Door,
        Window,
        Room,
        SlideDoor,
        None
    }

    /// <summary>
    ///     Enum containing 5 types of characters, with or without wheelchair and differents positions
    /// </summary>
    public enum CharacterType
    {
        StandUp,
        Sitting,
        LyingDown,
        OnWheelChair,
        WheelChairEmpty
    }

    /// <summary>
    ///     Enum containing ids of unity layers to show only 2d objects in 2d view, and 3d objects in 3d view
    /// </summary>
    public enum ErgoLayers
    {
        Top = 9,
        ThreeD = 10
    }
}