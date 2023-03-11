using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using HKMirror;

namespace SmashKnight
{
    public static class OriginalValues
    {
        public static float RECOIL_VELOCITY, RECOIL_HOR_VELOCITY_LONG, RECOIL_HOR_VELOCITY, RECOIL_DOWN_VELOCITY;
        public static float WALK_SPEED,RUN_SPEED, RUN_SPEED_CH, RUN_SPEED_CH_COMBO;
    }
    public class SmashKnight : Mod, IGlobalSettings<GlobalSettings>
    {
        public override string GetVersion()
        {
            return "1.0.0";
        }

        internal static SmashKnight Instance;

        internal static float MaxHealth = 0f, HealthFactor = 0f;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;
            On.HeroController.Start += HeroControllerStart;
            On.HeroController.Update += HeroController_Update;
            ModHooks.CharmUpdateHook += ModHooks_CharmUpdateHook;
            ModHooks.BlueHealthHook += ModHooks_BlueHealthHook;
        }

        private int ModHooks_BlueHealthHook()
        {
            MaxHealth = 0f;
            return PlayerDataAccess.healthBlue;
        }

        private void ModHooks_CharmUpdateHook(PlayerData data, HeroController controller)
        {
            MaxHealth = 0f;
        }

        private void HeroController_Update(On.HeroController.orig_Update orig, HeroController self)
        {
            orig(self);
            MaxHealth = globalSettings.disableBlueMasks ? PlayerDataAccess.maxHealth  : Math.Max(MaxHealth, PlayerDataAccess.maxHealth + PlayerDataAccess.healthBlue);
            var currentHealth = globalSettings.disableBlueMasks ? PlayerDataAccess.health : (PlayerDataAccess.health + PlayerDataAccess.healthBlue);
            var _HealthFactor = currentHealth / MaxHealth;
            if(HealthFactor != _HealthFactor) {
                HealthFactor = _HealthFactor;
                Log($"percent:{HealthFactor}");
                setSpeedPercent(HealthFactor);
                setRecoilPercent(1 - HealthFactor);
            }
        }

        private void setSpeedPercent(float percent)
        {
            var SpeedFactor = globalSettings.MinSpeedFactor + (percent * globalSettings.MaxSpeedFactor);
            Log($"speed:{SpeedFactor}");
            HeroController.instance.WALK_SPEED = OriginalValues.WALK_SPEED * SpeedFactor;
            HeroController.instance.RUN_SPEED = OriginalValues.RUN_SPEED * SpeedFactor;
            HeroController.instance.RUN_SPEED_CH = OriginalValues.RUN_SPEED_CH * SpeedFactor;
            HeroController.instance.RUN_SPEED_CH_COMBO = OriginalValues.RUN_SPEED_CH_COMBO * SpeedFactor;
        }
        private void setRecoilPercent(float percent)
        {
            var recoilFactor = globalSettings.MinRecoilFactor + (percent * globalSettings.MaxRecoilFactor);
            Log($"recoil:{recoilFactor}");
            HeroController.instance.RECOIL_VELOCITY = OriginalValues.RECOIL_VELOCITY * recoilFactor;
            HeroController.instance.RECOIL_HOR_VELOCITY_LONG = OriginalValues.RECOIL_HOR_VELOCITY_LONG * recoilFactor;
            HeroController.instance.RECOIL_HOR_VELOCITY = OriginalValues.RECOIL_HOR_VELOCITY * recoilFactor;
            HeroController.instance.RECOIL_DOWN_VELOCITY = OriginalValues.RECOIL_DOWN_VELOCITY * recoilFactor;

        }


        private void saveOriginal()
        {
            OriginalValues.RECOIL_VELOCITY = HeroController.instance.RECOIL_VELOCITY;
            OriginalValues.RECOIL_HOR_VELOCITY_LONG = HeroController.instance.RECOIL_HOR_VELOCITY_LONG;
            OriginalValues.RECOIL_HOR_VELOCITY = HeroController.instance.RECOIL_HOR_VELOCITY;
            OriginalValues.RECOIL_DOWN_VELOCITY = HeroController.instance.RECOIL_DOWN_VELOCITY;


            OriginalValues.WALK_SPEED = HeroController.instance.WALK_SPEED;
            OriginalValues.RUN_SPEED = HeroController.instance.RUN_SPEED;
            OriginalValues.RUN_SPEED_CH = HeroController.instance.RUN_SPEED_CH;
            OriginalValues.RUN_SPEED_CH_COMBO = HeroController.instance.RUN_SPEED_CH_COMBO;
        }
        private void HeroControllerStart(On.HeroController.orig_Start orig, HeroController self)
        {
            saveOriginal();
            orig(self);
        }

        private static GlobalSettings globalSettings = new GlobalSettings();
        public void OnLoadGlobal(GlobalSettings s)
        {
            globalSettings = s;
        }

        public GlobalSettings OnSaveGlobal()
        {
            return globalSettings;
        }
    }
}