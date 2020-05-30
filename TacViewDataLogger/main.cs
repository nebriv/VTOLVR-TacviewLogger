using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Harmony;
using System.Reflection;
using System.Collections;
using Valve.Newtonsoft;
using System.Linq;
using ExtensionMethods;

namespace TacViewDataLogger
{
    static class Globals
    {

        public static string projectName = "VTOLVR TacView Data Logger";
        public static string projectAuthor = "Nebriv";
        public static string projectVersion = "v2.0";

    }

    public class TacViewDataLogger2 : VTOLMOD
    {

        public static string dataSource = $"VTOL VR v{GameStartup.version.ToString()}";


        private VTOLAPI api;

        TimeSpan interval = new TimeSpan(0, 0, 2);


        private bool runlogger;
        private int iterator;
        private GameObject currentVehicle;
        private string TacViewFolder;

        private string path;

        private int secondsElapsed;
        public ACMI acmi;

        public Dictionary<String, ACMIDataEntry> knownActors = new Dictionary<String, ACMIDataEntry>();

        private void Start()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("neb.logger.logger");
            harmony.PatchAll(Assembly.GetExecutingAssembly());            
            api = VTOLAPI.instance;


            System.IO.Directory.CreateDirectory("TacViewDataLogs");

            Support.WriteLog("TacView Data Logger Loaded. Waiting for Scene Start!");

            SceneManager.sceneLoaded += SceneLoaded;


        }


        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.buildIndex == 7 || arg0.buildIndex == 12)
                StartCoroutine(WaitForScenario());
        }

        private IEnumerator WaitForScenario()
        {
            while (VTMapManager.fetch == null || !VTMapManager.fetch.scenarioReady)
            {
                yield return null;
            }

            Support.WriteLog("Scenario Ready!");

            Support.WriteLog("Getting Players Vehicle");
            currentVehicle = VTOLAPI.instance.GetPlayersVehicleGameObject();


            Support.WriteLog("Creating TacView Directory");
            System.IO.Directory.CreateDirectory("TacViewDataLogs\\" + DateTime.UtcNow.ToString("yyyy-MM-dd HHmm"));

            TacViewFolder = "TacViewDataLogs\\" + DateTime.UtcNow.ToString("yyyy-MM-dd HHmm") + "\\";

            path = @TacViewFolder + "datalog.acmi";

            acmi = new ACMI();

            Support.WriteLog("Creating TacView File");
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(acmi.acmi21Header());
            }

            iterator = 0;

            Support.WriteLog("Writing Reference Time");
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("0,ReferenceTime=" + timestamp);
            }
            Support.WriteLog("Running Logger");
            runlogger = true;
        }


        private void FixedUpdate()
        {
            if (iterator < 46)
            {
                iterator++;
            }
            else
            {
                iterator = 0;
                secondsElapsed++;

                if (runlogger)
                {
                    if (SceneManager.GetActiveScene().buildIndex != 7 && SceneManager.GetActiveScene().buildIndex != 12)
                    {
                        ResetLogger();
                    }
                    Support.WriteLog(secondsElapsed.ToString());
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine("#" + secondsElapsed);
                    }
                    TacViewDataLogACMI();
                }
            }
        }

        public void ResetLogger()
        {
            runlogger = false;

            Debug.Log("Scene end detected. Stopping TacView Recorder");

            Start();
        }

        public string cleanString(string input)
        {
            string clean = input.Replace("\\", "").Replace("/", "").Replace("<", "").Replace(">", "").Replace("*", "").Replace("\"", "").Replace("?", "").Replace(":", "").Replace("|", "");
            return clean;
        }

        public string getActorID(Actor actor)
        {
            return actor.gameObject.GetInstanceID().ToString("X").ToLower();
        }

        public void TacViewDataLogACMI()
        {
            List < Actor > actors = TargetManager.instance.allActors;
            List<String> logStrings = new List<String>();
            List<String> actorIDList = new List<String>();

            foreach (Actor actor in actors)
            {

                ACMIDataEntry newEntry = buildDataEntry(actor);

                actorIDList.Add(getActorID(actor));

                string acmiString = "";
                if (knownActors.ContainsKey(getActorID(actor)))
                {
                    ACMIDataEntry oldEntry = knownActors[getActorID(actor)];
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[getActorID(actor)] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(getActorID(actor), newEntry);
                }
                if (acmiString != "")
                {
                    logStrings.Add(acmiString);
                }
                
            }

            List<String> removedActors = new List<String>();

            foreach (String actor in knownActors.Keys)
            {
                if (!actorIDList.Contains(actor))
                {
                    removedActors.Add(actor);
                    Support.WriteLog($"Actor {actor} no longer exists");
                    acmi.ACMIEvent("Destroyed", null, actor);
                    logStrings.Add($"-{actor}");

                }
            }

            foreach (String actor in removedActors)
            {
                knownActors.Remove(actor);
            }


            //Writing it all out once per second is way faster than writing it out per unit. Doh!
            File.AppendAllLines(path, logStrings);
        }
        public ACMIDataEntry buildDataEntry(Actor actor)
        {
            ACMIDataEntry entry = new ACMIDataEntry();
            entry.objectId = actor.gameObject.GetInstanceID().ToString("X").ToLower();

            if (actor.team.ToString() == "Allied")
            {
                entry.color = "Blue";
            }
            else
            {
                entry.color = "Red";
            }

            GameObject currentVehicle = VTOLAPI.instance.GetPlayersVehicleGameObject();
            
            if (PilotSaveManager.current.pilotName == actor.actorName)
            {
                entry = airVehicleDataEntry(actor, entry);
                entry = playerVehicleDataEntry(actor, entry);
            } else if (actor.role == Actor.Roles.Air)
            {
                entry = airVehicleDataEntry(actor, entry);
            } else if (actor.role == Actor.Roles.Ground)
            {
                entry = groundVehicleDataEntry(actor, entry);
            }
            else if (actor.role == Actor.Roles.GroundArmor)
            {
                entry = groundVehicleDataEntry(actor, entry);
            }
            else if (actor.role == Actor.Roles.Ship)
            {
                entry = shipVehicleDataEntry(actor, entry);
            }
            else if (actor.role == Actor.Roles.Missile)
            {
                entry = missileDataEntry(actor, entry);
            }
            else if (actor.role == Actor.Roles.None)
            {
                entry = genericDataEntry(actor, entry);
            }



            return entry;
        }

        public ACMIDataEntry airVehicleDataEntry(Actor actor, ACMIDataEntry entry)
        {
            Vector3D coords = convertPositionToLatLong_raw(actor.transform.position);
            entry.locData = $"{coords.x} | {coords.y} | {coords.z} | {actor.flightInfo.roll} | {actor.flightInfo.pitch} | {actor.flightInfo.heading - 90}";
            entry._basicTypes = "FixedWing";
            entry.callSign = actor.designation.ToString();
            entry.name = actor.actorName;
            if (actor.currentlyTargetingActor != null)
            {
                entry.lockedTarget = getActorID(actor.currentlyTargetingActor);
            }
            
            entry.aoa = actor.flightInfo.aoa.ToString();
            entry.ias = actor.flightInfo.airspeed.ToString();
            entry.altitude = actor.flightInfo.altitudeASL.ToString();

            return entry;
        }

        public ACMIDataEntry playerVehicleDataEntry(Actor actor, ACMIDataEntry entry)
        {
            Vector3D coords = convertPositionToLatLong_raw(actor.transform.position);
            entry.locData = $"{coords.x} | {coords.y} | {coords.z} | {actor.flightInfo.roll} | {actor.flightInfo.pitch} | {actor.flightInfo.heading - 90}";
            
            entry.pilot = actor.actorName;
            entry.callSign = actor.actorName;
            entry.name = actor.name.Replace("(Clone)", "");

            return entry;
        }

        public ACMIDataEntry missileDataEntry(Actor actor, ACMIDataEntry entry)
        {
            Vector3D coords = convertPositionToLatLong_raw(actor.transform.position);
            //entry.parent = actor.parentActor.gameObject.GetInstanceID().ToString("X").ToLower();
            entry.locData = $"{coords.x} | {coords.y} | {coords.z}";
            entry._basicTypes = "Missile";
            entry.name = actor.actorName;
            return entry;
        }

        public ACMIDataEntry groundVehicleDataEntry(Actor actor, ACMIDataEntry entry)
        {
            entry._basicTypes = "Vehicle";
            return entry;
        }

        public ACMIDataEntry shipVehicleDataEntry(Actor actor, ACMIDataEntry entry)
        {
            entry._basicTypes = "Watercraft";
            return entry;
        }

        public ACMIDataEntry genericDataEntry(Actor actor, ACMIDataEntry entry)
        {
            entry._basicTypes = "Vehicle";

            return entry;
        }

        private Vector3D convertPositionToLatLong_raw(Vector3 position)
        {
            Vector3D real_loc;
            Vector3D locationtext;

            real_loc = WorldPositionToGPSCoords(VTResources.GetMap(VTScenario.current.mapID), position);
            return real_loc;
        }

        private string convertPositionToLatLong(Vector3 position)
        {
            Vector3D real_loc;
            string locationtext;

            real_loc = WorldPositionToGPSCoords(VTResources.GetMap(VTScenario.current.mapID), position);
            locationtext = real_loc.ToString();
            return locationtext;
        }


        public Vector3D WorldPositionToGPSCoords(VTMap map, Vector3 worldPoint)
        {
            Vector3D vector3D = VTMapManager.WorldToGlobalPoint(worldPoint);
            double z = (double)(worldPoint.y - WaterPhysics.instance.height);
            double num = vector3D.z / 111319.9;
            double num2 = Math.Abs(Math.Cos(num * 0.01745329238474369) * 111319.9);
            double num3 = 0.0;
            if (num2 > 0.0)
            {
                num3 = vector3D.x / num2;
            }
            double num4 = num3;
            if (map)
            {
                num += (double)map.mapLatitude;
                num4 += (double)map.mapLongitude;
            }
            return new Vector3D(num, num4, z);
        }

    }
}
