using System.Collections;
using SecretHistories.Entities;
using SecretHistories.Services;

namespace SecretHistories.Infrastructure
{
    public interface IGameDataImporter
    {
        void ImportCharacter(SourceForGameState source, Character character);
        void ImportTableState(SourceForGameState source,Sphere tabletop);
        bool IsSavedGameActive(SourceForGameState source, bool temp);
    }
}