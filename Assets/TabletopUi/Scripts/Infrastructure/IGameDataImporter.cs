using System.Collections;
using Assets.Core.Entities;
using Assets.TabletopUi.Scripts.Services;

namespace Assets.TabletopUi.Scripts.Infrastructure
{
    public interface IGameDataImporter
    {
        void ImportCharacter(SourceForGameState source, Character character);
        void ImportTableState(SourceForGameState source,TabletopTokenContainer tabletop);
        bool IsSavedGameActive(SourceForGameState source, bool temp);
    }
}