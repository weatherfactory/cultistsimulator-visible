using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Core;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace Assets.Scripts.Application.Abstract
{
    public interface IHasAspects: IHasFucinePath
    {

        AspectsDictionary GetAspects(bool includeSelf);
        Dictionary<string, int> Mutations { get; }
        void SetMutation(string mutationEffectMutate, int mutationEffectLevel, bool mutationEffectAdditive);
        /// <summary>
        /// Unique value based on id, aspects, and mutations
        /// </summary>
        /// <returns></returns>
        string GetSignature();

        Token Token { get; }
    }
}
