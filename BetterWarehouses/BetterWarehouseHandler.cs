using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CMS.UI;
using CMS.UI.Windows;
using HarmonyLib;
// using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using MelonLoader;
using OrbCreationExtensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BetterWarehouses
{
    public class BetterWarehouseHandler : MonoBehaviour
    {
        private static bool warehouseInfoCreated = false;
        private static GameObject warehouseInfo = null;
        private static Text warehouseTextComponent = null;
        private static Warehouse warehouse = null;
        
        public BetterWarehouseHandler(IntPtr ptr) : base(ptr)
        {
            MelonLogger.Msg("[BW] Entered Constructor");
        }

        [HarmonyPostfix]
        public static void Awake()
        {
            MelonLogger.Msg("[BW] Awake!");
        }

        [HarmonyPostfix]
        public static void Start()
        {
            MelonLogger.Msg("[BW] Starting Up...");
        }

        [HarmonyPostfix]
        public static void Update()
        {
            if (warehouseInfoCreated != true)
            {
                // var conditionGameObject = UIManager.Get().PartInspector.condition.gameObject;
                // var inspectorTransformParent = UIManager.Get().PartInspector.gameObject.transform.Find("Inspector");
                // warehouseInfo = Instantiate(conditionGameObject, inspectorTransformParent);
                
                MelonLogger.Msg("Setting up warehouse");
                // warehouseInfo.name = "warehouseInfo";
                // MelonLogger.Msg("finished setting name");
                // warehouseInfo.transform.position += Vector3.up * 80.0f;
                // warehouseInfo.GetComponent<Image>().color = new Color(0.6604f, 1, 0.9812f, 1);
                
                // warehouseTextComponent = warehouseInfo.transform.Find("ConditionPercentage").GetComponent<Text>();
                     
                warehouse = GameScript.Get().gameObject.GetComponent<Warehouse>();
                warehouseInfoCreated = true;
            }

            if (!Input.GetKeyDown(KeyCode.G)) return;
            
            var internalPart = GameScript.Get().GetPartMouseOver();
            var bodyPart = GameScript.Get().GetIOMouseOverCarLoader();

            var warehouseFinds = new List<BaseItem>();
            if (internalPart != null)
            {
                var id = internalPart.GetIDWithTuned();
                var allWarehouseItems = warehouse.GetAllItemsAndGroups(); 
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
}