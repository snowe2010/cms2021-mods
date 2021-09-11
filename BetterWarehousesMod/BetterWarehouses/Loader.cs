using System;
using CMS.UI;
using CMS.UI.Windows;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // using Harmony;

namespace BetterWarehouses
{
    public class BaseItemWithData
    {
        public int Count = 0;
        public BaseItem Item;
        public int NumFullQuality = 0;
        public int NumGreenQuality = 0;
        public int NumYellowQuality = 0;
        public int NumOrangeQuality = 0;
        public int NumRedQuality = 0;

        public BaseItemWithData(BaseItem baseItem)
        {
            Item = baseItem;
        }
    }

    [HarmonyPatch]
    public static class Patch
    {
        private static bool _warehouseInfoCreated = false;
        private static GameObject _warehouseInfo = null;
        private static GameObject _warehouse100PercentItems = null;
        private static GameObject _warehouseGreenItems = null;
        private static GameObject _warehouseYellowItems = null;
        private static GameObject _warehouseOrangeItems = null;
        private static GameObject _warehouseRedItems = null;
        private static Text _warehouseTextComponent = null;
        private static Text _warehouse100PercentTextComponent = null;
        private static Text _warehouseGreenTextComponent = null;
        private static Text _warehouseYellowTextComponent = null;
        private static Text _warehouseOrangeTextComponent = null;
        private static Text _warehouseRedTextComponent = null;
        private static Warehouse _warehouse = null;
        private static List<BaseItem> _warehouseItems;
        private static System.Collections.Generic.Dictionary<string, BaseItemWithData> _warehouseIdMap;
        private static bool isWindowVisible = false;
        
        private static void CreateWarehouseStockUI()
        {
            var conditionGameObject = UIManager.Get().PartInspector.condition.gameObject;
            var inspectorTransformParent = UIManager.Get().PartInspector.gameObject.transform.Find("Inspector");
            _warehouseInfo = GameObject.Instantiate(conditionGameObject, inspectorTransformParent);
            _warehouse100PercentItems = GameObject.Instantiate(conditionGameObject, inspectorTransformParent);
            _warehouseGreenItems = GameObject.Instantiate(conditionGameObject, inspectorTransformParent);
            _warehouseYellowItems = GameObject.Instantiate(conditionGameObject, inspectorTransformParent);
            _warehouseOrangeItems = GameObject.Instantiate(conditionGameObject, inspectorTransformParent);
            _warehouseRedItems = GameObject.Instantiate(conditionGameObject, inspectorTransformParent);

            _warehouseInfo.name = "warehouseInfo";
            _warehouseInfo.transform.position += Vector3.up * 240.0f;
            _warehouseInfo.GetComponent<RectTransform>().sizeDelta += 3 * Vector2.up;
            _warehouseInfo.GetComponent<Image>().color = new Color(0.6604f, 1, 0.9812f, 1);

            _warehouse100PercentItems.name = "warehouse100PercentInfo";
            _warehouse100PercentItems.transform.position += Vector3.up * 200.0f;
            _warehouse100PercentItems.GetComponent<Image>().color = new Color(0.04f, 1f, 0.09f);

            _warehouseGreenItems.name = "warehouseGreenItems";
            _warehouseGreenItems.transform.position += Vector3.up * 169.0f;
            _warehouseGreenItems.GetComponent<Image>().color = new Color(0f, 1f, 0.51f);
            // _warehouseGreenItems.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1.0f);
            // _warehouseGreenItems.GetComponent<RectTransform>().ForceUpdateRectTransforms();


            _warehouseYellowItems.name = "warehouseYellowItems";
            _warehouseYellowItems.transform.position += Vector3.up * 138.0f;
            _warehouseYellowItems.GetComponent<Image>().color = new Color(1f, 0.94f, 0f);

            _warehouseOrangeItems.name = "warehouseOrangeItems";
            _warehouseOrangeItems.transform.position += Vector3.up * 107.0f;
            _warehouseOrangeItems.GetComponent<Image>().color = new Color(1f, 0.61f, 0f);

            _warehouseRedItems.name = "warehouseRedItems";
            _warehouseRedItems.transform.position += Vector3.up * 76.0f;
            _warehouseRedItems.GetComponent<Image>().color = new Color(1f, 0.03f, 0f);

            _warehouseTextComponent = _warehouseInfo.transform.Find("ConditionPercentage").GetComponent<Text>();
            _warehouse100PercentTextComponent =
                _warehouse100PercentItems.transform.Find("ConditionPercentage").GetComponent<Text>();
            _warehouseGreenTextComponent =
                _warehouseGreenItems.transform.Find("ConditionPercentage").GetComponent<Text>();
            _warehouseYellowTextComponent =
                _warehouseYellowItems.transform.Find("ConditionPercentage").GetComponent<Text>();
            _warehouseOrangeTextComponent =
                _warehouseOrangeItems.transform.Find("ConditionPercentage").GetComponent<Text>();
            _warehouseRedTextComponent = _warehouseRedItems.transform.Find("ConditionPercentage").GetComponent<Text>();

            _warehouseInfoCreated = true;
        }

        private static BaseItemWithData SearchWarehouse()
        {
            var internalPart = GameScript.Get().GetPartMouseOver();
            var bodyPart = GameScript.Get().GetIOMouseOverCarLoader();

            if (internalPart != null)
            {
                var id = internalPart.GetIDWithTuned();
                return _warehouseIdMap.ContainsKey(id) ? _warehouseIdMap[id] : new BaseItemWithData(new BaseItem());
            }
            else if (bodyPart != null)
            {
                // can't do anything with bodyParts right now
            }

            return new BaseItemWithData(new BaseItem());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameScript), "Update", new Type[] { })]
        private static void Update()
        {
            var currentScene = SceneManager.GetActiveScene();

            if (currentScene.name != "garage") return; // skip if we're not in the garage
            if (isWindowVisible) return; // skip if inventory or warehouses etc are open

            if (_warehouseInfoCreated != true) CreateWarehouseStockUI();

            var item = SearchWarehouse();
            _warehouseTextComponent.text = $"Warehouse Total: {item.Count}";
            _warehouse100PercentTextComponent.text = $"100%: {item.NumFullQuality}";
            _warehouseGreenTextComponent.text = $"80%+: {item.NumGreenQuality}";
            _warehouseYellowTextComponent.text = $"50%+: {item.NumYellowQuality}";
            _warehouseOrangeTextComponent.text = $"15%+: {item.NumOrangeQuality}";
            _warehouseRedTextComponent.text = $"0%+: {item.NumRedQuality}";
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(WarehouseWindow), "OnWindowActiveStateChange")]
        private static void OnWindowActiveStateChange(WindowID windowID, bool stateChange)
        {
            MelonLogger.Msg("Hiding the warehouse window and updating warehouse contents");
            UpdateMap();
        }

        private static void UpdateMap()
        {
            _warehouseItems = _warehouse.GetAllItemsAndGroups();
            _warehouseIdMap = new System.Collections.Generic.Dictionary<string, BaseItemWithData>();
            foreach (var item in _warehouseItems)
            {
                var baseItemWithData = _warehouseIdMap.ContainsKey(item.ID)
                    ? _warehouseIdMap[item.ID]
                    : new BaseItemWithData(item);
                var condition = item.GetCondition();
                switch (condition)
                {
                    case var _ when condition >= .99:
                        baseItemWithData.NumFullQuality++;
                        break;
                    case var _ when condition >= .80:
                        baseItemWithData.NumGreenQuality++;
                        break;
                    case var _ when condition >= .50:
                        baseItemWithData.NumYellowQuality++;
                        break;
                    case var _ when condition > 0:
                        baseItemWithData.NumOrangeQuality++;
                        break;
                    default:
                        baseItemWithData.NumRedQuality++;
                        break;
                }

                baseItemWithData.Count++;
                _warehouseIdMap[item.ID] = baseItemWithData;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Warehouse), "Load")]
        private static void WarehouseLoad()
        {
            _warehouse = GameScript.Get().gameObject.GetComponent<Warehouse>();
            MelonLogger.Msg("Warehouse loading");
            UpdateMap();
        }
    }

    public class Loader : MelonMod
    {
        public override void OnApplicationStart()
        {
            // MelonLogger.Msg("Loading preferences");
            // var melonPreferencesCategory = MelonPreferences.CreateCategory("BetterWarehouse");

            base.OnApplicationStart();
        }
    }
}