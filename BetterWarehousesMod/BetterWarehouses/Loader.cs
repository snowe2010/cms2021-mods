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
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            if (currentScene.name != "garage") return; // skip if we're not in the garage
            
            if (_warehouseInfoCreated != true)
            {
                SetShortcut();
            }

            if (Input.GetKeyDown(_checkInWarehouseShortcut))
            {
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

            if (Input.GetKeyDown(KeyCode.N))
            {
                MelonLogger.Msg("trying to move lift 1 down");
                var carLifter = GameObject.Find("!Garage Dynamic/Car_lift_1").GetComponent<CarLifter>();
                carLifter.MoveDown();
                carLifter.OpenArmsAnimated(45, false);
                carLifter.Action(1);
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                MelonLogger.Msg("trying to move lift 1 up");
                var carLifter = GameObject.Find("!Garage Dynamic/Car_lift_1").GetComponent<CarLifter>();
                carLifter.MoveUp();
                carLifter.Action(0);
            }
        }
    }

    [HarmonyPatch(typeof(CarLifter))]
    public class LogStuff
    {
        [HarmonyPostfix]
        [HarmonyPatch( "Action")]
        public static void Action(int actionType)
        {
            MelonLogger.Msg($"Action {actionType}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "Awake")]
        public static void Awake()
        {
            MelonLogger.Msg($"Awake");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "ConnectCar")]
        public static void ConnectCar(CarLoader car)
        {
            MelonLogger.Msg($"CarLoader {car.name}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "DisconnectCar")]
        public static void DisconnectCar()
        {
            MelonLogger.Msg($"DisconnectCar");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "EnableButton")]
        public static void EnableButton(LifterButtonTypes lifterButton, bool enable)
        {
            MelonLogger.Msg($"EnableButton {lifterButton}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "MoveDown")]
        public static void MoveDown(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"MoveDown {instant} {switchIO}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "MoveMedFromFloor")]
        public static void MoveMedFromFloor(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"MoveMedFromFloor {instant} {switchIO}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "MoveMiddleToFloor")]
        public static void MoveMiddleToFloor(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"MoveMiddleToFloor {instant} {switchIO}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "MoveUp")]
        public static void MoveUp(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"MoveUp {instant} {switchIO}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "OnCompleteMoveDown")]
        public static void OnCompleteMoveDown(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"OnCompleteMoveDown {instant} {switchIO}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "OnDestroy")]
        public static void OnDestroy()
        {
            MelonLogger.Msg($"OnDestroy");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "OpenArmsAnimated")]
        public static void OpenArmsAnimated(float angle, bool instant)
        {
            MelonLogger.Msg($"OpenArmsAnimated {angle} {instant}");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "ResetActions")]
        public static void ResetActions()
        {
            MelonLogger.Msg($"ResetActions");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch( "ResetForTutorial")]
        public static void ResetForTutorial()
        {
            MelonLogger.Msg($"ResetForTutorial");
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
                melonPreferencesCategory.CreateEntry("raiseLowerLift1", "B");
            }

            base.OnApplicationStart();
        }
    }
}