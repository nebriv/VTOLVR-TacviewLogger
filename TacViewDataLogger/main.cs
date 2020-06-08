using Harmony;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using System.Threading.Tasks;

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

        private GameObject currentVehicle;
        private string TacViewFolder;

        private string path;
        public int iterator;
        public int secondCount;
        private float elapsedSeconds;

        public ACMI acmi;

        public Queue<string> dataLog = new Queue<string>();

        public Dictionary<String, ACMIDataEntry> knownActors = new Dictionary<String, ACMIDataEntry>();

        
        public string acmiString;
        List<String> actorIDList;
        List<Actor> actors;
        List<String> removedActors;

        public ACMIDataEntry newEntry;
        public ACMIDataEntry oldEntry;
        public ACMIDataEntry entry;

        public List<CMFlare> flares;
        public List<Bullet> bullets;


        public double saveTime;
        public float writeWaitTime;
        public float minWriteWateTime = 30f;


        public bool customScene = false;
        public float customSceneOffset = 0f;


        private float nextActionTime = 0.0f;
        public float period = 0.5f;


        private void Start()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("tacview.logger");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            api = VTOLAPI.instance;


            System.IO.Directory.CreateDirectory("TacViewDataLogs");

            support.WriteLog("TacView Data Logger Loaded. Waiting for Scene Start!");

            SceneManager.sceneLoaded += SceneLoaded;

        }


        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.buildIndex == 7 || arg0.buildIndex == 11)
                if (arg0.buildIndex == 11)
                {
                    customScene = true;
                }
                else
                {
                    customScene = false;
                }

            StartCoroutine(WaitForScenario());

            if (runlogger)
            {
                if (arg0.buildIndex != 7 && arg0.buildIndex != 11)
                {
                    ResetLogger();
                }
            }
        }

        void Update()
        {
            if (Time.time > nextActionTime)
            {
                nextActionTime += period;
                if (runlogger)
                {
                    elapsedSeconds += period;
                    dataLog.Enqueue($"#{elapsedSeconds}");
                    try
                    {

                        GCLatencyMode oldMode = GCSettings.LatencyMode;
                        RuntimeHelpers.PrepareConstrainedRegions();

                        try
                        {
                            GCSettings.LatencyMode = GCLatencyMode.LowLatency;

                            TacViewDataLogACMI();
                        }
                        finally
                        {
                            GCSettings.LatencyMode = oldMode;
                        }

                    }
                    catch (Exception ex)
                    {
                        support.WriteErrorLog("Error getting data." + ex.ToString());
                    }
                }
            }
        }


        private IEnumerator WaitForScenario()
        {
            while (VTMapManager.fetch == null || !VTMapManager.fetch.scenarioReady)
            {
                yield return null;
            }

            support.WriteLog("Scenario Ready!");

            support.WriteLog("Getting Players Vehicle");
            currentVehicle = VTOLAPI.instance.GetPlayersVehicleGameObject();


            support.WriteLog("Creating TacView Directory");
            System.IO.Directory.CreateDirectory("TacViewDataLogs\\" + DateTime.UtcNow.ToString("yyyy-MM-dd HHmm"));

            TacViewFolder = "TacViewDataLogs\\" + DateTime.UtcNow.ToString("yyyy-MM-dd HHmm") + "\\";

            path = @TacViewFolder + "datalog.acmi";

            acmi = new ACMI();

            support.WriteLog("Creating TacView File");
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine(acmi.acmi21Header());
            }


            support.WriteLog("Writing Reference Time");
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("0,ReferenceTime=" + timestamp);
            }
            support.WriteLog("Running Logger");
            runlogger = true;

            StartCoroutine(writeString());
        }


        public List<CMFlare> getFlares()
        {
            //flares = new List<CMFlare>();

            //if (elapsedSeconds % 2 == 0)
            //{
            flares = new List<CMFlare>(FindObjectsOfType<CMFlare>());
            //}

            return flares;
        }


        public List<Bullet> getBullets()
        {

            //bullets = new List<Bullet>();

            //if (elapsedSeconds % 2 == 0)
            //{
            bullets = new List<Bullet>(FindObjectsOfType<Bullet>());
            //}

            return bullets;
        }

        public IEnumerator writeString()
        {
            while (runlogger)
            {

                yield return new WaitForSeconds(15);

                if (dataLog.Count > 500)
                {
                    //File.AppendAllLines(path, dataLog);
                    //dataLog.Clear();
                    Task t1 = new Task(writeStringTask);
                    t1.Start();
                }
            }

        }

        public void writeStringTask()
        {
            File.AppendAllLines(path, dataLog);
            dataLog.Clear();
        }


        public void ResetLogger()
        {
            runlogger = false;

            UnityEngine.Debug.Log("Scene end detected. Stopping TacView Recorder");

            Start();
        }

        public void TacViewDataLogACMI()
        {
            actors = TargetManager.instance.allActors;

            actorIDList = new List<String>();
            acmiString = "";

            // Processing game actors

            for (int i = 0; i < actors.Count; i++)
            {
                    
                acmiString = "";
                newEntry = buildDataEntry(actors[i]);

                actorIDList.Add(support.getActorID(actors[i]));

                // If this is already a tracked actor
                if (knownActors.ContainsKey(support.getActorID(actors[i])))
                {
                    oldEntry = knownActors[support.getActorID(actors[i])];

                    // Diff the old entry and the new entry. Update the old entry with the new entry.
                    //acmiString = newEntry.ACMIString();
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[support.getActorID(actors[i])] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(support.getActorID(actors[i]), newEntry);
                }
                if ((acmiString != "") && (acmiString.Contains(",")))
                {
                    dataLog.Enqueue(acmiString);
                }
            }

            // Getting flares and processing them
            flares = getFlares();
            acmiString = "";
            for (int i = 0; i < flares.Count; i++)
            {
                acmiString = "";
                actorIDList.Add(support.getFlareID(flares[i]));

                newEntry = buildFlareEntry(flares[i]);
                
                if (knownActors.ContainsKey(support.getFlareID(flares[i])))
                {
                    oldEntry = knownActors[support.getFlareID(flares[i])];
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[support.getFlareID(flares[i])] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(support.getFlareID(flares[i]), newEntry);
                }
                if (acmiString != "")
                {
                    dataLog.Enqueue(acmiString);
                }
            }

            // Getting bullets and processing them
            bullets = getBullets();
            for (int i = 0; i < bullets.Count; i++)
            {

                actorIDList.Add(support.getBulletID(bullets[i]));

                newEntry = buildBulletEntry(bullets[i]);
                acmiString = "";
                if (knownActors.ContainsKey(support.getBulletID(bullets[i])))
                {
                    oldEntry = knownActors[support.getBulletID(bullets[i])];
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[support.getBulletID(bullets[i])] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(support.getBulletID(bullets[i]), newEntry);
                }
                if (acmiString != "")
                {
                    dataLog.Enqueue(acmiString);
                }
            }


            removedActors = new List<String>();

            foreach (String actor in knownActors.Keys)
            {
                if (!actorIDList.Contains(actor))
                {
                    removedActors.Add(actor);

                    
                    // Need to handle checks for non vehicle actors
                    //dataLog.Enqueue(acmi.ACMIEvent("Destroyed", null, actor));
                    //
                    
                    dataLog.Enqueue($"-{actor}");

                }
            }


            for (int i = 0; i < removedActors.Count; i++)
            {
                knownActors.Remove(removedActors[i]);
            }

        }

        public ACMIDataEntry buildFlareEntry(CMFlare flare)
        {
            ACMIDataEntry entry = new ACMIDataEntry();

            entry.objectId = support.getFlareID(flare);

            Vector3D coords = support.convertPositionToLatLong_raw(flare.transform.position);

            entry.locData = $"{coords.y} | {coords.x} | {coords.z}";
            entry._specificTypes = "Flare";

            return entry;
        }



        public ACMIDataEntry buildBulletEntry(Bullet bullet)
        {
            entry = new ACMIDataEntry();

            entry.objectId = support.getBulletID(bullet);

            Vector3D coords = support.convertPositionToLatLong_raw(bullet.transform.position);

            entry.locData = $"{coords.y} | {coords.x} | {coords.z}";
            entry._specificTypes = "Bullet";

            return entry;
        }


        public ACMIDataEntry buildDataEntry(Actor actor)
        {
            entry = new ACMIDataEntry();
            entry.objectId = actor.gameObject.GetInstanceID().ToString("X").ToLower();

            if (actor.team.ToString() == "Allied")
            {
                entry.color = "Blue";
            }
            else
            {
                entry.color = "Red";
            }

            if (PilotSaveManager.current.pilotName == actor.actorName)
            {
                entry = actorProcessor.airVehicleDataEntry(actor, entry, customSceneOffset);
                entry = actorProcessor.playerVehicleDataEntry(actor, entry, customSceneOffset);

            }
            else if (actor.role == Actor.Roles.Air)
            {
                entry = actorProcessor.airVehicleDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.Ground)
            {
                entry = actorProcessor.groundVehicleDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.GroundArmor)
            {
                entry = actorProcessor.groundVehicleDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.Ship)
            {
                entry = actorProcessor.shipVehicleDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.Missile)
            {
                entry = actorProcessor.missileDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.None)
            {
                entry = actorProcessor.genericDataEntry(actor, entry, customSceneOffset);
            }

            return entry;
        }


    }
}
