using System;
using SecretHistories.Fucine;

namespace SecretHistories.Spheres
{
    public enum BlockReason
    {
        GreedyAngel,
        Inactive,
        None //used to pass to 'except for reason' when there's no exception
    }

    public enum BlockDirection
    {
        None,
        Inward,
        Outward,
        All
    }

    [Serializable]
    public class SphereBlock
    {
        public BlockDirection BlockDirection { get; }
        public BlockReason BlockReason { get; }

        public FucinePath AtSpherePath { get; }

        public SphereBlock(FucinePath atSpherePath,BlockDirection direction, BlockReason reason)
        {
            AtSpherePath=atSpherePath;
            BlockDirection = direction; 
            BlockReason = reason;
        }
    }
}