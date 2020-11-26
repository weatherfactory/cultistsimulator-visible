using Assets.Core.Entities;

namespace Assets.TabletopUi.Scripts.UI
{
    public abstract class SettingControlStrategy
    {
        protected Setting boundSetting;

        public void Initialise(Setting settingToBind)
        {
            boundSetting = settingToBind;
        }

        public string SettingId
        {
            get { return boundSetting.Id; }
        }
        public string SettingHint
        {
            get { return boundSetting.Hint; }
        }

        public string SettingTabId
        {
            get { return boundSetting.TabId; }
        }
    }
}