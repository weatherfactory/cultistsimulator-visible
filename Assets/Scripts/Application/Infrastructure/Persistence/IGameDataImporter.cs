using System.Collections;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Infrastructure.Persistence;
using SecretHistories.Services;
using SecretHistories.Spheres;

namespace SecretHistories.Constants
{
    public interface IGameDataImporter
    {
        void ImportCharacter(PersistableGameState source, Character character);
        void ImportTableState(PersistableGameState source,Sphere tabletop);
        bool IsSavedGameActive(PersistableGameState source, bool temp);
    }
}