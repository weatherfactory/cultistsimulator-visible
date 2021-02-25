using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Application.Abstract;
using SecretHistories.Assets.Scripts.Application.Abstract;
using SecretHistories.Core;
using SecretHistories.Fucine;

namespace SecretHistories.Assets.Scripts.Application.Entities.NullEntities
{
    public sealed class FucineRoot: IHasFucinePath
    {
        static readonly FucineRoot instance=new FucineRoot();

        static FucineRoot()
        {
            //Jon Skeet says do this because
            // "Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit"
        }

        public string Id => FucinePath.ROOT.ToString();
        public AspectsDictionary GetAspects(bool includeSelf)
        {
            return new AspectsDictionary();
        }

        public Dictionary<string, int> Mutations { get; }=new AspectsDictionary();
        public void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive)
        {
           //
        }

        public static FucineRoot Get()
        {
            return instance;
        }

        public FucinePath GetPath()
        {
            throw new NotImplementedException();
        }

        public FucinePath GetAbsolutePath()
        {
            return FucinePath.Root();
        }

    }
}
