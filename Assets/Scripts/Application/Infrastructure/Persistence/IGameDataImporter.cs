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
        void ImportCharacter(PersistedGame source, Character character);
        void ImportTableState(PersistedGame source,Sphere tabletop);
        bool IsSavedGameActive(PersistedGame source, bool temp);
    }
}