using MelonLoader;
using UnityEngine;

namespace RemoteControlLifts
{
    public class Loader : MelonMod
    {
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Loading preferences");
            var melonPreferencesCategory = MelonPreferences.CreateCategory("BetterWarehouse");
            if (!melonPreferencesCategory.HasEntry("checkInWarehouse"))
            {
                MelonLogger.Msg("Creating checkInWarehouse shortcut");
                melonPreferencesCategory.CreateEntry("checkInWarehouse", "G");
                melonPreferencesCategory.CreateEntry("raiseLowerLift1", "B");
            }

            base.OnApplicationStart();
        }
    }
}