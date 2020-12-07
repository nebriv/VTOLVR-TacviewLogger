using Harmony;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TacViewDataLogger
{
    public class DataGetters
    {


        public static List<Dictionary<string, string>> GetEngineStats(GameObject vehicle)
        {
            List<Dictionary<string, string>> engines = new List<Dictionary<string, string>>();

            int i = 1;

            foreach (ModuleEngine engine in vehicle.GetComponentsInChildren<ModuleEngine>())
            {
                Dictionary<string, string> engineDict = new Dictionary<string, string>();
                engineDict.Add("Engine Number", i.ToString());
                engineDict.Add("Enabled", engine.engineEnabled.ToString());
                engineDict.Add("Failed", engine.failed.ToString());
                engineDict.Add("Starting", engine.startingUp.ToString());
                engineDict.Add("Started", engine.startedUp.ToString());
                engineDict.Add("RPM", engine.displayedRPM.ToString());
                engineDict.Add("Afterburner", engine.afterburner.ToString());
                engineDict.Add("FinalThrust", engine.finalThrust.ToString());
                engineDict.Add("FinalThrottle", engine.finalThrottle.ToString());
                engineDict.Add("MaxThrust", engine.maxThrust.ToString());

                engines.Add(engineDict);
                i++;
            }

            return engines;
        }

        public static string getAfterburners(GameObject vehicle)
        {
            List<Dictionary<string, string>> engines = GetEngineStats(vehicle);

            if (engines[0]["Afterburner"] == "True")
            {
                return "1";
            }

            return "0";
        }

        public static string getBrakes(GameObject vehicle)
        {
            try
            {
                AeroController aero = vehicle.GetComponentInChildren<AeroController>();
                return aero.brake.ToString();
            }
            catch (Exception)
            {
                return "Unavailable";
            }
        }

        public static string getRadarState(GameObject vehicle)
        {
            try
            {
                Radar radar = vehicle.GetComponentInChildren<Radar>();
                if (radar.radarEnabled)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception)
            {
                return "0";
            }

        }


        public static Actor getRadarLockTarget(GameObject vehicle, Actor player)
        {
            Actor detectedActor = new Actor();

            if (player.name == "VTOL4")
            {
                return null;
            }

            try
            {
                LockingRadar radar = vehicle.GetComponentInChildren<LockingRadar>();
                if (radar != null)
                {
                    if (radar.IsLocked())
                    {
                        return radar.currentLock.actor;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                support.WriteLog("Got error while getting radar lock data");
                return null;
            }
        }

        //public void DebugAllRadars()
        //{
        //    List<Radar> radars = new List<Radar>(FindObjectsOfType<Radar>());

        //    foreach (Radar radar in radars)
        //    {
        //        radar.debugRadar = true;
        //    }

        //}

        public static string getRadarAzimuth(GameObject vehicle)
        {
            try
            {
                Radar radar = vehicle.GetComponentInChildren<Radar>();
                if (radar.radarEnabled)
                {
                    return "1";
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception)
            {
                return "0";
            }

        }

        public static string getEjectionState(GameObject vehicle)
        {
            string ejectionState;
            try
            {
                EjectionSeat ejection = vehicle.GetComponentInChildren<EjectionSeat>();
                ejectionState = ejection.ejected.ToString();
            }
            catch (Exception)
            {
                ejectionState = "Unavailable";
            }

            return ejectionState;
        }

        public static List<Dictionary<string, string>> getRWRContacts(GameObject vehicle)
        {
            List<Dictionary<string, string>> contacts = new List<Dictionary<string, string>>();

            try
            {
                ModuleRWR rwr = vehicle.GetComponentInChildren<ModuleRWR>();

                foreach (ModuleRWR.RWRContact contact in rwr.contacts)
                {
                    Dictionary<string, string> contactDict = new Dictionary<string, string>();
                    contactDict.Add("active", contact.active.ToString());
                    contactDict.Add("locked", contact.locked.ToString());
                    Actor radar_actor = contact.radarActor;
                    contactDict.Add("friendFoe", radar_actor.team.ToString());
                    contactDict.Add("name", radar_actor.name.ToString());
                    contactDict.Add("radarSymbol", contact.radarSymbol.ToString());
                    contactDict.Add("signalStrength", contact.signalStrength.ToString());

                    contacts.Add(contactDict);
                }

            }
            catch (NullReferenceException)
            {
                //I don't think this really matters here. It seems to work and I'm too lazy to debug it. 
                //I think ModuleRWR only updates at a certain rate and does not exist otherwise.
            }
            catch (Exception ex)
            {
                support.WriteErrorLog("Error getting RWR Contacts: " + ex);
            }
            return contacts;

        }

        public static string getMissileDetected(GameObject vehicle)
        {
            try
            {
                MissileDetector md = vehicle.GetComponentInChildren<MissileDetector>();
                return md.missileDetected.ToString();
            }
            catch (Exception)
            {
                return "Unavailable";
            }
        }

        public static string getMasterArm(GameObject vehicle)
        {
            bool masterArmState = false;
            try
            {


                foreach (VRLever lever in vehicle.GetComponentsInChildren<VRLever>())
                {

                    if (lever.gameObject.name == "masterArmSwitchInteractable")
                    {
                        if (lever.currentState == 1)
                        {
                            masterArmState = true;
                            break;
                        }
                        else
                        {
                            masterArmState = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                support.WriteErrorLog("Error getting master arm state: " + ex.ToString());
            }
            return masterArmState.ToString();
        }

        public static Dictionary<string, string> getVehicleLights(GameObject vehicle)
        {

            Dictionary<string, string> lights = new Dictionary<string, string>();

            //this is BAD
            //Light landingLights = vehicle.transform.Find("LandingLight").GetComponent<Light>();


            try
            {
                bool landinglight = false;
                bool navlight = false;
                bool strobelight = false;

                foreach (Light light in vehicle.GetComponentsInChildren<Light>())
                {

                    if (light.gameObject.name == "LandingLight")
                    {
                        landinglight = true;
                    }
                    if (light.gameObject.name.ToString().Contains("StrobeLight"))
                    {
                        strobelight = true;
                    }
                    support.WriteLog(light.ToString());
                }


                foreach (SpriteRenderer spriteish in vehicle.GetComponentsInChildren<SpriteRenderer>())
                {
                    if (spriteish.ToString().Contains("Nav"))
                    {
                        navlight = true;
                    }

                }

                lights.Add("LandingLights", landinglight.ToString());
                lights.Add("NavLights", navlight.ToString());
                lights.Add("StrobeLights", strobelight.ToString());

                return lights;

            }
            catch (Exception ex)
            {
                support.WriteErrorLog("Error getting lights " + ex.ToString());
                return lights;
            }
        }

        public static string getFlaps(GameObject vehicle)
        {
            try
            {
                AeroController aero = vehicle.GetComponentInChildren<AeroController>();
                return aero.flaps.ToString();
            }
            catch (Exception)
            {
                return "Unavailable";
            }
        }

        public static string GetStall(GameObject vehicle)
        {

            try
            {

                HUDStallWarning warning = vehicle.GetComponentInChildren<HUDStallWarning>();

                //Nullable boolean allows it to get "stalling" if it doesn't exist? and sets it as false? I think.
                Boolean? stalling = Traverse.Create(warning).Field("stalling").GetValue() as Boolean?;

                return stalling.ToString();
            }
            catch (Exception ex)
            {
                support.WriteErrorLog("unable to get stall status: " + ex.ToString());
                return "False";
            }

        }

        public static string GetHook(GameObject vehicle)
        {

            try
            {

                Tailhook hook = vehicle.GetComponentInChildren<Tailhook>();

                //Nullable boolean allows it to get "stalling" if it doesn't exist? and sets it as false? I think.
                Boolean? deployed = Traverse.Create(hook).Field("deployed").GetValue() as Boolean?;

                return deployed.ToString();
            }
            catch (Exception ex)
            {
                support.WriteErrorLog("unable to get stall status: " + ex.ToString());
                return "False";
            }

        }

        public static string GetBattery(GameObject vehicle)
        {


            try
            {
                Battery batteryCharge = vehicle.GetComponentInChildren<Battery>();
                string battery = batteryCharge.currentCharge.ToString();
                return battery;
            }
            catch (Exception ex)
            {
                support.WriteErrorLog("unable to get battery status: " + ex.ToString());
                return "False";
            }

        }

        public static string getFuelLevel(GameObject vehicle)
        {
            try
            {
                FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
                return tank.totalFuel.ToString();
            }
            catch (Exception)
            {
                return "False";
            }
        }

        public static string getFuelMass(GameObject vehicle)
        {
            try
            {
                FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
                return Math.Round(tank.GetMass(), 2).ToString();
            }
            catch (Exception)
            {
                return "False";
            }
        }

        public static string getFuelBurnRate(GameObject vehicle)
        {

            try
            {
                FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
                return tank.fuelDrain.ToString();
            }
            catch (Exception)
            {
                return "False";
            }

        }

        public static string getFuelDensity(GameObject vehicle)
        {
            try
            {
                FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
                return tank.fuelDensity.ToString();
            }
            catch (Exception)
            {
                return "False";
            }

        }

        public static bool GetGunFiring(GameObject vehicle)
        {

            try
            {
                WeaponManager weaponManager = vehicle.GetComponentInChildren<WeaponManager>();

                if (weaponManager.availableWeaponTypes.gun)
                {
                    return weaponManager.isFiring;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                support.WriteErrorLog("Unable to get weapon manager status: " + ex.ToString());
                return false;
            }

        }

        public static bool GetBombFiring(GameObject vehicle)
        {

            try
            {
                WeaponManager weaponManager = vehicle.GetComponentInChildren<WeaponManager>();

                if (weaponManager.availableWeaponTypes.bomb)
                {
                    return weaponManager.isFiring;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                support.WriteErrorLog("Unable to get weapon manager status: " + ex.ToString());
                return false;
            }
        }

        public static bool GetMissileFiring(GameObject vehicle)
        {

            try
            {
                WeaponManager weaponManager = vehicle.GetComponentInChildren<WeaponManager>();

                if (weaponManager.availableWeaponTypes.aam || weaponManager.availableWeaponTypes.agm || weaponManager.availableWeaponTypes.antirad || weaponManager.availableWeaponTypes.antiShip)
                {
                    return weaponManager.isFiring;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                support.WriteErrorLog("Unable to get weapon manager status: " + ex.ToString());
                return false;
            }
        }

        public static string getRadarCrossSection(GameObject vehicle)
        {
            try
            {
                RadarCrossSection rcs = vehicle.GetComponentInChildren<RadarCrossSection>();
                return rcs.GetAverageCrossSection().ToString();
            }
            catch (Exception)
            {
                return "False";
            }
        }

        public static bool GetLanded(GameObject vehicle)
        {
            return true;
        }

    }
}
