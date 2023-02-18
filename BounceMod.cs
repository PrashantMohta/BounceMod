using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using HKMirror;

namespace BounceMod
{
    public static class OriginalValues
    {
        public static float RECOIL_VELOCITY, RECOIL_HOR_VELOCITY_LONG, RECOIL_HOR_VELOCITY, RECOIL_DOWN_VELOCITY;
        public static float WALK_SPEED,RUN_SPEED, RUN_SPEED_CH, RUN_SPEED_CH_COMBO;
    }
    public class BounceMod : Mod, IGlobalSettings<GlobalSettings>
    {
        internal static BounceMod Instance;

        static int damageCount = 1;

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            Log("Initializing");
            Instance = this;
            On.HeroController.Start += HeroControllerStart;
            On.HeroController.TakeDamage += HeroController_TakeDamage;
            On.HeroController.AddHealth += HeroController_AddHealth;
            On.HeroController.MaxHealth += HeroController_MaxHealth;
            On.HeroController.Die += HeroController_Die;
        }

        private void HeroController_MaxHealth(On.HeroController.orig_MaxHealth orig, HeroController self)
        {
            resetSpeed();
            orig(self);
        }
        private void resetSpeed()
        {
            Log("Resetting speed");
            HeroController.instance.WALK_SPEED = OriginalValues.WALK_SPEED * globalSettings.fastBoiFactor;
            HeroController.instance.RUN_SPEED = OriginalValues.RUN_SPEED * globalSettings.fastBoiFactor;
            HeroController.instance.RUN_SPEED_CH = OriginalValues.RUN_SPEED_CH * globalSettings.fastBoiFactor;
            HeroController.instance.RUN_SPEED_CH_COMBO = OriginalValues.RUN_SPEED_CH_COMBO * globalSettings.fastBoiFactor;
        }
        private IEnumerator HeroController_Die(On.HeroController.orig_Die orig, HeroController self)
        {
            reset();
            //reset after death 
            resetSpeed();
            return orig(self);
        }

        private void HeroController_AddHealth(On.HeroController.orig_AddHealth orig, HeroController self, int amount)
        {
            damageCount = 1;
            reset();
            orig(self, amount);
            //reset after heal 
            resetSpeed();

        }

        private void HeroController_TakeDamage(On.HeroController.orig_TakeDamage orig, HeroController self, GameObject go, GlobalEnums.CollisionSide damageSide, int damageAmount, int hazardType)
        {
            if (HKMirror.Reflection.SingletonClasses.HeroControllerR.CanTakeDamage()) { 
                OnDamageTaken();
            }
            orig(self,go, damageSide,damageAmount, hazardType);
            reset();
        }

        private void OnDamageTaken()
        {
            if(globalSettings.maxDamageCount >= damageCount)
            {
                damageCount++;
            }
            HeroController.instance.RECOIL_VELOCITY = OriginalValues.RECOIL_VELOCITY * damageCount * globalSettings.recoilFactor;
            HeroController.instance.RECOIL_HOR_VELOCITY_LONG = OriginalValues.RECOIL_HOR_VELOCITY_LONG * damageCount * globalSettings.recoilFactor;
            HeroController.instance.RECOIL_HOR_VELOCITY = OriginalValues.RECOIL_HOR_VELOCITY * damageCount * globalSettings.recoilFactor;
            HeroController.instance.RECOIL_DOWN_VELOCITY = OriginalValues.RECOIL_DOWN_VELOCITY * damageCount * globalSettings.recoilFactor;
            //slow down after damage taken
            HeroController.instance.WALK_SPEED = OriginalValues.WALK_SPEED * globalSettings.slowBoiFactor;
            HeroController.instance.RUN_SPEED = OriginalValues.RUN_SPEED * globalSettings.slowBoiFactor;
            HeroController.instance.RUN_SPEED_CH = OriginalValues.RUN_SPEED_CH * globalSettings.slowBoiFactor;
            HeroController.instance.RUN_SPEED_CH_COMBO = OriginalValues.RUN_SPEED_CH_COMBO * globalSettings.slowBoiFactor;
            Log("current recoil factor: " + damageCount * globalSettings.recoilFactor);
            Log("current speed factor: " + globalSettings.slowBoiFactor);

        }

        private void reset()
        {
            Log("Resetting recoil");
            HeroController.instance.RECOIL_VELOCITY = OriginalValues.RECOIL_VELOCITY ;
            HeroController.instance.RECOIL_HOR_VELOCITY_LONG = OriginalValues.RECOIL_HOR_VELOCITY_LONG;
            HeroController.instance.RECOIL_HOR_VELOCITY = OriginalValues.RECOIL_HOR_VELOCITY ;
            HeroController.instance.RECOIL_DOWN_VELOCITY = OriginalValues.RECOIL_DOWN_VELOCITY ;

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
            // fast boi mode
            resetSpeed();
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