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
        void ImportCharacter(GamePersistenceProvider source, Character character);
        void ImportTableState(GamePersistenceProvider source,Sphere tabletop);
        bool IsSavedGameActive(GamePersistenceProvider source, bool temp);
    }
}