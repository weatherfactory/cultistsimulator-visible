using System.Collections;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Services;
using SecretHistories.Spheres;

namespace SecretHistories.Constants
{
    public interface IGameDataImporter
    {
        void ImportCharacter(SourceForGameState source, Character character);
        void ImportTableState(SourceForGameState source,Sphere tabletop);
        bool IsSavedGameActive(SourceForGameState source, bool temp);
    }
}