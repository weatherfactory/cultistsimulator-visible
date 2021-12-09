using System;
using SecretHistories.Fucine;

namespace SecretHistories.Spheres
{
    public enum BlockReason
    {
        GreedyAngel,
        InboundTravellingStack,
        Inactive
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