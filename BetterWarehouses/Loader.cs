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
    // [HarmonyPatch(typeof(ExecuteEvents), "Execute", new Type[] { typeof(IPointerEnterHandler), typeof(BaseEventData) })]
    public class Patch
    {
        private static bool warehouseInfoCreated = false;
        private static GameObject warehouseInfo = null;
        private static Text warehouseTextComponent = null;
        private static Warehouse warehouse = null;

        // [HarmonyPostfix]
        public static void Postfix_EventSystems_ExecuteEvents(MethodBase __originalMethod, object handler, BaseEventData eventData)
        {
            var uiManager = UIManager.Get();
            if (warehouseInfoCreated != true)
            {
                if (uiManager.PartInspector == null) return;
                    
                MelonLogger.Msg("1");
                var partsInspector = uiManager.PartInspector ? uiManager.PartInspector.gameObject : null;
                
                MelonLogger.Msg("2");
                var conditionGameObject = uiManager.PartInspector.condition.gameObject;
                MelonLogger.Msg("3");
                var inspectorTransformParent = uiManager.PartInspector.gameObject.transform.Find("Inspector");
                MelonLogger.Msg("4");

                if (conditionGameObject != null && inspectorTransformParent != null)
                {
                    MelonLogger.Msg("5");
                    warehouseInfo = GameObject.Instantiate(conditionGameObject, inspectorTransformParent);

                    MelonLogger.Msg("setting name");
                    warehouseInfo.name = "warehouseInfo";
                    MelonLogger.Msg("finished setting name");
                    warehouseInfo.transform.position += Vector3.up * 80.0f;
                    // warehouseInfo.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
                    // warehouseInfo.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0.25f);
                    warehouseInfo.GetComponent<Image>().color = new Color(0.6604f, 1, 0.9812f, 1);

                    warehouseTextComponent = warehouseInfo.transform.Find("ConditionPercentage").GetComponent<Text>();

                    warehouse = GameScript.Get().gameObject.GetComponent<Warehouse>();
                    MelonLogger.Msg("6");
                    warehouseInfoCreated = true;
                }
            }
            
            // if (Input.GetKeyDown(KeyCode.G))
            // {
            MelonLogger.Msg("7");
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

                foreach (var grouping in warehouseFinds.GroupBy(i => i.ID))
                {
                    if (grouping.Any())
                    {
                        warehouseTextComponent.text =
                            $"{grouping.Count()} {grouping.First().GetLocalizedName()} in Warehouse";
                    }
                }
            // }
        }
    }

    public class Loader : MelonMod
    {
        
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Registering");
            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<BetterWarehouseHandler>();
                var go = new GameObject("BetterWarehouses");
                go.AddComponent<BetterWarehouseHandler>();
                Object.DontDestroyOnLoad(go);
            }
            catch (Exception ex)
            {
                MelonLogger.Msg($"Failed to register handler {ex}");
            }
            
            
            
            try
            {
                // Our Primary Unity Event Hooks 
                HarmonyLib.Harmony harmony = this.HarmonyInstance;
            
                // Update
                var originalUpdate = HarmonyLib.AccessTools.Method(typeof(GameScript), "Update"); // Change Type!
                MelonLogger.Msg("[BW] Harmony - Original Method: " + originalUpdate.DeclaringType.Name + "." + originalUpdate.Name);
                var postUpdate = HarmonyLib.AccessTools.Method(typeof(BetterWarehouseHandler), "Update");
                MelonLogger.Msg("[BW] Harmony - Postfix Method: " + postUpdate.DeclaringType.Name + "." + postUpdate.Name);
                harmony.Patch(originalUpdate, postfix: new HarmonyLib.HarmonyMethod(postUpdate));
                MelonLogger.Msg("[BW] Harmony - Runtime Patches Applied");
            }
            catch
            {
                MelonLogger.Msg("[BW] Harmony - FAILED to Apply Patches!");
            }

            // try
            // {
            //     HarmonyInstance harmony = this.Harmony;
            //     var originalUpdate = HarmonyLib.AccessTools.Method(typeof(BaseItem), "GetCondition");
            //     MelonLogger.Msg("[BW] Harmony - Original Method: " + originalUpdate.DeclaringType.Name + "." + originalUpdate.Name);
            //     var postUpdate = HarmonyLib.AccessTools.Method(typeof(BetterWarehouseHandler), "Update");
            //     MelonLogger.Msg("[BW] Harmony - Postfix Method: " + postUpdate.DeclaringType.Name + "." + postUpdate.Name);
            //     harmony.Patch(originalUpdate, postfix: new HarmonyLib.HarmonyMethod(postUpdate));
            //     MelonLogger.Msg("[BW] Harmony - Runtime Patches Applied");
            //
            // }
            // catch
            // {
            //     MelonLogger.Msg("[BW] Harmony - FAILED to Apply Patches!");
            // }
            base.OnApplicationStart();
        }
    }
}
