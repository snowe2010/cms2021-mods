using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
// using Harmony;
using HarmonyLib;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BetterWarehouses
{
    [HarmonyPatch(typeof(GameScript), "Update", new Type[] { })]
    public class Patch
    {
        private static bool _warehouseInfoCreated = false;
        private static GameObject _warehouseInfo = null;
        private static Text _warehouseTextComponent = null;
        private static Warehouse _warehouse = null;
        private static KeyCode _checkInWarehouseShortcut;

        private static void SetShortcut()
        {
            var betterWarehousePrefs = MelonPreferences.GetCategory("BetterWarehouse");
            var checkInWarehouseEntry = betterWarehousePrefs.GetEntry<string>("checkInWarehouse");
            if (Enum.TryParse(checkInWarehouseEntry.Value, true, out KeyCode key))
            {
                _checkInWarehouseShortcut = key;
            }
            else
            {
                MelonLogger.Msg($"Unable to parse checkInWarehouse shortcut as a key. {checkInWarehouseEntry.Value}");
                MelonLogger.Msg("Setting checkInWarehouse shortcut to default G");
                _checkInWarehouseShortcut = KeyCode.G;
            }
                
            // var conditionGameObject = UIManager.Get().PartInspector.condition.gameObject;
            // var inspectorTransformParent = UIManager.Get().PartInspector.gameObject.transform.Find("Inspector");
            // warehouseInfo = Instantiate(conditionGameObject, inspectorTransformParent);
            // warehouseInfo.name = "warehouseInfo";
            // MelonLogger.Msg("finished setting name");
            // warehouseInfo.transform.position += Vector3.up * 80.0f;
            // warehouseInfo.GetComponent<Image>().color = new Color(0.6604f, 1, 0.9812f, 1);

            // warehouseTextComponent = warehouseInfo.transform.Find("ConditionPercentage").GetComponent<Text>();

            _warehouse = GameScript.Get().gameObject.GetComponent<Warehouse>();
            _warehouseInfoCreated = true;
        }

        private static List<IGrouping<string, BaseItem>> searchWarehouse()
        {
            
            var internalPart = GameScript.Get().GetPartMouseOver();
            var bodyPart = GameScript.Get().GetIOMouseOverCarLoader();
            
            var warehouseFinds = new List<BaseItem>();
            if (internalPart != null)
            {
                var id = internalPart.GetIDWithTuned();
                var allWarehouseItems = _warehouse.GetAllItemsAndGroups();
                foreach (var warehouseItem in allWarehouseItems)
                {
                    // MelonLogger.Msg($"warehouse item {warehouseItem.GetLocalizedName()}");
                    if (warehouseItem.ID == id)
                    {
                        warehouseFinds.Add(warehouseItem);
                    }
                }
            }
            else if (bodyPart != null)
            {
                // var id = bodyPart.GetIDWithTuned();
                // var allWarehouseItems = warehouse.GetAllItemsAndGroups();
                // foreach (var warehouseItem in allWarehouseItems)
                // {
                //     if (warehouseItem.ID == id)
                //     {       
                //         warehouseFinds.Add(warehouseItem);
                //     }
                // }
            }
            
            var warehouseFindsByCount = warehouseFinds.GroupBy(i => i.ID).ToList();
            return warehouseFindsByCount;
        }
        
        [HarmonyPostfix]
        public static void Update()
        {
            if (_warehouseInfoCreated != true)
            {
                SetShortcut();
            }

            if (!Input.GetKeyDown(_checkInWarehouseShortcut)) return;
            
            var warehouseFindsByCount = searchWarehouse();
            foreach (var grouping in warehouseFindsByCount.Where(grouping => grouping.Any()))
            {
                UIManager.Get().ShowPopup("Warehouse", $"{grouping.Count()} in warehouse", PopupType.Buy);
                // warehouseTextComponent.text =
                // $"{grouping.Count()} {grouping.First().GetLocalizedName()} in Warehouse";
            }

            if (!warehouseFindsByCount.Any())
            {
                UIManager.Get().ShowPopup("Warehouse", $"0 in warehouse", PopupType.Buy);
            }
        }
    }

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
            }

            base.OnApplicationStart();
        }
    }
}