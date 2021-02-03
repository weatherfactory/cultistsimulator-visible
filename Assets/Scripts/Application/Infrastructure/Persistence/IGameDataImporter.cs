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
        void ImportCharacter(GamePersistence source, Character character);
        void ImportTableState(GamePersistence source,Sphere tabletop);
        bool IsSavedGameActive(GamePersistence source, bool temp);
    }
}