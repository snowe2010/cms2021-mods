using System;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace RemoteControlLifts
{
    public class Loader : MelonMod
    {
        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Loading preferences");
            var melonPreferencesCategory = MelonPreferences.CreateCategory("RemoteControlLifts");
            if (!melonPreferencesCategory.HasEntry("raiseLowerLift1"))
            {
                MelonLogger.Msg("Creating raiseLowerLift1 shortcut");
                melonPreferencesCategory.CreateEntry("raiseLowerLift1", "R");
            }

            if (!melonPreferencesCategory.HasEntry("raiseLowerLift2"))
            {
                MelonLogger.Msg("Creating raiseLowerLift2 shortcut");
                melonPreferencesCategory.CreateEntry("raiseLowerLift2", "P");
            }

            base.OnApplicationStart();
        }
    }

    [HarmonyPatch(typeof(GameScript), "Update", new Type[] { })]
    public class Patch
    {
        private static bool keycodesSet = false;
        private static KeyCode _changeLift1;
        private static KeyCode _changeLift2;

        private static void SetShortcut()
        {
            var prefs = MelonPreferences.GetCategory("RemoteControlLifts");
            var changeLift1 = prefs.GetEntry<string>("raiseLowerLift1");
            var changeLift2 = prefs.GetEntry<string>("raiseLowerLift2");
            if (Enum.TryParse(changeLift1.Value, true, out KeyCode key))
            {
                _changeLift1 = key;
            }
            else
            {
                MelonLogger.Msg($"Unable to parse raiseLowerLift1 shortcut as a key. {changeLift1.Value}");
                MelonLogger.Msg("Setting raiseLowerLift1 shortcut to default R");
                _changeLift1 = KeyCode.R;
            }

            if (Enum.TryParse(changeLift2.Value, true, out KeyCode key2))
            {
                _changeLift2 = key2;
            }
            else
            {
                MelonLogger.Msg($"Unable to parse raiseLowerLift2 shortcut as a key. {changeLift2.Value}");
                MelonLogger.Msg("Setting raiseLowerLift2 shortcut to default P");
                _changeLift2 = KeyCode.P;
            }

            keycodesSet = true;
        }


        [HarmonyPostfix]
        public static void Update()
        {
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            if (currentScene.name != "garage") return; // skip if we're not in the garage

            if (!keycodesSet)
            {
                SetShortcut();
            }

            if (Input.GetKeyDown(_changeLift1))
            {
                MelonLogger.Msg("trying to move lift 1");
                var carLifter = GameObject.Find("!Garage Dynamic/Car_lift_1").GetComponent<CarLifter>();
                MoveLifter(carLifter);
            }
            if (Input.GetKeyDown(_changeLift2))
            {
                MelonLogger.Msg("trying to move lift 2");
                var carLifter = GameObject.Find("!Garage/Garage/#LifterOn/Car_lift_2").GetComponent<CarLifter>();
                MoveLifter(carLifter);
            }
        }

        private static void MoveLifter(CarLifter carLifter)
        {
            switch (carLifter.currentState)
            {
                case CarLifterState.Middle:
                    carLifter.MoveUp();
                    carLifter.Action(0);
                    break;
                case CarLifterState.Up:
                    carLifter.MoveDown();
                    carLifter.OpenArmsAnimated(45, false);
                    carLifter.Action(1);
                    break;
                case CarLifterState.OnFloor:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


    [HarmonyPatch(typeof(CarLifter))]
    public class LogCarLifter
    {
        [HarmonyPostfix]
        [HarmonyPatch("Action")]
        public static void Action(int actionType)
        {
            MelonLogger.Msg($"Action {actionType}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        public static void Awake()
        {
            MelonLogger.Msg($"Awake");
        }

        [HarmonyPostfix]
        [HarmonyPatch("ConnectCar")]
        public static void ConnectCar(CarLoader car)
        {
            MelonLogger.Msg($"CarLoader {car.name}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("DisconnectCar")]
        public static void DisconnectCar()
        {
            MelonLogger.Msg($"DisconnectCar");
        }

        [HarmonyPostfix]
        [HarmonyPatch("EnableButton")]
        public static void EnableButton(LifterButtonTypes lifterButton, bool enable)
        {
            MelonLogger.Msg($"EnableButton {lifterButton}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("MoveDown")]
        public static void MoveDown(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"MoveDown {instant} {switchIO}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("MoveMedFromFloor")]
        public static void MoveMedFromFloor(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"MoveMedFromFloor {instant} {switchIO}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("MoveMiddleToFloor")]
        public static void MoveMiddleToFloor(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"MoveMiddleToFloor {instant} {switchIO}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("MoveUp")]
        public static void MoveUp(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"MoveUp {instant} {switchIO}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnCompleteMoveDown")]
        public static void OnCompleteMoveDown(bool instant, bool switchIO)
        {
            MelonLogger.Msg($"OnCompleteMoveDown {instant} {switchIO}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        public static void OnDestroy()
        {
            MelonLogger.Msg($"OnDestroy");
        }

        [HarmonyPostfix]
        [HarmonyPatch("OpenArmsAnimated")]
        public static void OpenArmsAnimated(float angle, bool instant)
        {
            MelonLogger.Msg($"OpenArmsAnimated {angle} {instant}");
        }

        [HarmonyPostfix]
        [HarmonyPatch("ResetActions")]
        public static void ResetActions()
        {
            MelonLogger.Msg($"ResetActions");
        }

        [HarmonyPostfix]
        [HarmonyPatch("ResetForTutorial")]
        public static void ResetForTutorial()
        {
            MelonLogger.Msg($"ResetForTutorial");
        }
    }
}