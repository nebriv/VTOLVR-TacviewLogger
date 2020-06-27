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
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine.Events;

namespace TacViewDataLogger
{
    static class Globals
    {

        public static string projectName = "VTOL VR Tacview Data Logger";
        public static string projectAuthor = "Nebriv";
        public static string projectVersion = "v2.1";

    }

    public class TacViewDataLogger : VTOLMOD
    {

        public static string dataSource = $"VTOL VR v{GameStartup.version.ToString()}";


        private VTOLAPI api;

        TimeSpan interval = new TimeSpan(0, 0, 2);

        private bool runlogger;

        private GameObject currentVehicle;
        public string TacViewFolder;

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

        public int frameRateLogSize = 90;


        public FixedSizedQueue<float> frameRateLog;
        public float minFrameTime = .0085f;
        public float maxFrameTime = .0125f;

        public float minSampleTime = .025f;
        public float maxSampleTime = 1.26f;

        public float missionElapsedTime;

        public heightmapGeneration heightmapGeneration = new heightmapGeneration();

        public static UnityEngine.Events.UnityAction SceneReloaded;


        public VTMapManager mapManager;


        public actorProcessor actorProcessor = new actorProcessor();
        public support support = new support();

        public Texture2D gameHeightmap;


        private void Start()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("tacview.harmony");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            api = VTOLAPI.instance;


            System.IO.Directory.CreateDirectory("TacViewDataLogs");

            support.WriteLog($"TacView Data Logger {Globals.projectVersion} Loaded. Waiting for Scene Start!");

            SceneManager.sceneLoaded += SceneLoaded;
            SceneReloaded += RestartScenarioDetected;

            support.WriteLog($"VR Device Refresh Rate: {UnityEngine.XR.XRDevice.refreshRate.ToString()}");
            support.WriteLog($"Target frame time: {(1 / UnityEngine.XR.XRDevice.refreshRate).ToString()}");

            minFrameTime = (1 / UnityEngine.XR.XRDevice.refreshRate) + .0005f;
            maxFrameTime = (1 / UnityEngine.XR.XRDevice.refreshRate) + .001f;
            support.WriteLog($"Min: { minFrameTime} Max: {maxFrameTime}");

            frameRateLogSize = (int)UnityEngine.XR.XRDevice.refreshRate * 2;
            frameRateLog = new FixedSizedQueue<float>(frameRateLogSize);
        }


        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.buildIndex == 7 || arg0.buildIndex == 11)
            {
                if (arg0.buildIndex == 11)
                {
                    customScene = true;
                }
                else
                {
                    customScene = false;
                }

                StartCoroutine(WaitForScenario());
            }


            if (runlogger)
            {
                if (arg0.buildIndex != 7 && arg0.buildIndex != 11)
                {
                    ResetLogger();
                }
            }
        }

        [HarmonyPatch(typeof(VTMapManager), "RestartCurrentScenario")]
        class Patch
        {
            static void Postfix(VTMapManager __instance)
            {
                if (TacViewDataLogger.SceneReloaded != null)
                    TacViewDataLogger.SceneReloaded.Invoke();
            }
        }

        void manageSamplingRate()
        {
            if (frameRateLog.Count == frameRateLogSize)
            {
                if (frameRateLog.Average() > maxFrameTime)
                {
                    if (period < maxSampleTime)
                    {
                        period += .01f;
                        //support.WriteLog($"Average Framerate is more than .001.... Decreasing sample rate to {period}");
                    }

                }
                else if ((frameRateLog.Average() < minFrameTime) && (frameRateLog.Average() > minFrameTime - 0.004f))
                {
                    if (period > minSampleTime)
                    {
                        period -= .01f;
                        //support.WriteLog($"Average Framerate is less than ..0085 and more than .004.... Increasing sample rate to {period}");
                    }
                }
            }
        }

        void Update()
        {

            if (runlogger)
            {

                if (elapsedSeconds > 5)
                {
                    // Disabling this for now as it seems to screw with the time scaling in TacView

                    //frameRateLog.Enqueue(Time.deltaTime);
                    //manageSamplingRate();
                }

                if ((Time.time > nextActionTime) || (nextActionTime == 0.0f))
                {

                    if (frameRateLog.Count == frameRateLogSize)
                    {
                        //support.WriteLog($"Current Sampling Rate: {period} - Current Frame Average: {frameRateLog.Average()}, min: {minFrameTime}, max: {maxFrameTime}");
                    }

                    nextActionTime += period;

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
            if (!runlogger)
            {
                while (VTMapManager.fetch == null || !VTMapManager.fetch.scenarioReady)
                {
                    yield return null;
                }

                mapManager = VTMapManager.fetch;
                actorProcessor.support = support;
                heightmapGeneration.support = support;
                support.mm = mapManager;


                support.WriteLog("Map ID:");
                support.WriteLog(mapManager.map.mapID);

                if (customScene)
                {
                    VTMapCustom[] customMaps = FindObjectsOfType<VTMapCustom>();

                    foreach (VTMapCustom map in customMaps)
                    {
                        if (mapManager.map.mapID == map.mapID)
                        {
                            support.WriteLog("I THINK I CAN I THINK I CAN I THINK I CAN!");
                            gameHeightmap = map.heightMap;
                        }
                    }
                }
                else
                {
                    gameHeightmap = mapManager.fallbackHeightmap;
                }

                heightmapGeneration.gameHeightmap = gameHeightmap;


                support.WriteLog("Scenario Ready!");

                support.WriteLog("Getting Players Vehicle");
                currentVehicle = VTOLAPI.GetPlayersVehicleGameObject();

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

                // Setting the current recording time
                support.WriteLog("Writing Reference Time");
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("0,RecordingTime=" + timestamp);
                }

                support.WriteLog($"Env Name: {VTScenario.current.envName}");


                //Setting a custom hour to simulate the time of day based on the current scenario
                if (VTScenario.current.envName == "day")
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
                        string minsec = DateTime.UtcNow.ToString("mm:ss");
                        timestamp += $"T13:{minsec}Z";
                        sw.WriteLine("0,ReferenceTime=" + timestamp);
                    }
                } else if (VTScenario.current.envName == "morning")
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
                        string minsec = DateTime.UtcNow.ToString("mm:ss");
                        timestamp += $"T06:{minsec}Z";
                        sw.WriteLine("0,ReferenceTime=" + timestamp);
                    }

                } else if (VTScenario.current.envName == "night")
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
                        string minsec = DateTime.UtcNow.ToString("mm:ss");
                        timestamp += $"T23:{minsec}Z";
                        sw.WriteLine("0,ReferenceTime=" + timestamp);
                    }
                }
                else
                {
                    support.WriteLog($"Unknown Scenario Env (day/morning/night) {VTScenario.current.envName}");
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine("0,ReferenceTime=" + timestamp);
                    }
                    
                }

                // Custom Scene is set for anything not on the Akutan map
                support.WriteLog($"Custom Scene: {customScene}");

                getGlobalMissionProperties();
                getObjectives();

                //// Elapsed time isn't used for anything anymore apparently... I'll keep it around for a reminder of better times.
                ////missionElapsedTime = FlightSceneManager.instance.missionElapsedTime;

                support.WriteLog("Running Logger");
                runlogger = true;

                // Start the function to save the mission data
                StartCoroutine(writeString());

                //// Get the heightmap info (Requires the map manager for the built in map)
                heightmapGeneration.getHeightMap(customScene, TacViewFolder, mapManager);


                // Get the airports
                getAirports();
            }

        }

        public void RestartScenarioDetected()
        {
            runlogger = false;
            writeStringTask();
            elapsedSeconds = 0f;
            nextActionTime = 0.0f;
            period = 0.5f;
            knownActors = new Dictionary<String, ACMIDataEntry>();

            support.WriteLog("Scene Restart detected. Restarting TacView Recorder");

            StartCoroutine(WaitForScenario());

        }

        public void ResetLogger()
        {
            runlogger = false;
            writeStringTask();
            elapsedSeconds = 0f;
            nextActionTime = 0.0f;
            period = 0.5f;
            knownActors = new Dictionary<String, ACMIDataEntry>();

            support.WriteLog("Scene end detected. Stopping TacView Recorder");

            //StartCoroutine(WaitForScenario());

        }

        public void objectiveBegin(MissionObjective obj)
        {
            dataLog.Enqueue($"0,Event=Bookmark|Objective '{obj.objectiveName}' Started");
        }

        public void objectiveComplete(MissionObjective obj)
        {
            dataLog.Enqueue($"0,Event=Bookmark|Objective '{obj.objectiveName}' Completed");
        }
        public void objectiveFail(MissionObjective obj)
        {
            dataLog.Enqueue($"0,Event=Bookmark|Objective '{obj.objectiveName}' Failed");
        }

        private void getObjectives()
        {
            MissionObjective[] objectives = FindObjectsOfType<MissionObjective>();

            foreach (MissionObjective objective in objectives)
            {
                objective.OnBegin.AddListener(() => objectiveBegin(objective));
                objective.OnComplete.AddListener(() => objectiveComplete(objective));
                objective.OnFail.AddListener(() => objectiveFail(objective));
            }

        }

        private void getGlobalMissionProperties()
        {
            if (customScene)
            {
                support.WriteLog("Getting custom map");
                VTMap map = support.getMap();
                string title = "";

                if (map == null)
                {
                    support.WriteErrorLog("Unable to get custom map!");
                    title = $"0,Title={VTScenario.current.scenarioName.Replace(",", "\\,")} on Unknown Map";
                }
                else
                {
                    title = $"0,Title={VTScenario.current.scenarioName.Replace(",", "\\,")} on {map.mapName.Replace(",", "\\,")}";
                    
                }

                string briefing = $"0,Briefing={VTScenario.current.scenarioDescription.Replace(",", "\\,")}";
                string author = $"0,Author={PilotSaveManager.current.pilotName.Replace(",", "\\,")}";
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(title);
                    sw.WriteLine(briefing);
                    sw.WriteLine(author);
                }
            }
            else
            {
                VTMap map = support.getMap();
                string title = $"0,Title={VTScenario.current.scenarioName.Replace(",", "\\,")} on {map.mapName.Replace(",", "\\,")}";
                string briefing = $"0,Briefing={VTScenario.current.scenarioDescription.Replace(",", "\\,")}";
                string author = $"0,Author={PilotSaveManager.current.pilotName.Replace(",", "\\,")}";
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(title);
                    sw.WriteLine(briefing);
                    sw.WriteLine(author);
                }
            }



        }

        public void getAirports()
        {
            VTMapManager[] mm = FindObjectsOfType<VTMapManager>();
            foreach (AirportManager manager in mm[0].airports)
            {
                newEntry = actorProcessor.airportEntry(manager);

                dataLog.Enqueue(newEntry.ACMIString());

            }
        }

        public List<CMFlare> getFlares()
        {
            flares = new List<CMFlare>(FindObjectsOfType<CMFlare>());

            return flares;
        }


        public List<Bullet> getBullets()
        {
            bullets = new List<Bullet>(FindObjectsOfType<Bullet>());

            return bullets;
        }

        public IEnumerator writeString()
        {
            while (runlogger)
            {

                if (dataLog.Count > 0)
                {
                    //File.AppendAllLines(path, dataLog);
                    //dataLog.Clear();
                    Task t1 = new Task(writeStringTask);
                    t1.Start();
                }
                yield return new WaitForSeconds(15);
            }

        }

        public void writeStringTask()
        {
            File.AppendAllLines(path, dataLog);
            dataLog.Clear();
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

            //actorName = actor's name in the mission
            //name = actor's unit name


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
                //support.WriteLog("Air");
                entry = actorProcessor.airVehicleDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.Ground)
            {
                //support.WriteLog("Ground");
                entry = actorProcessor.groundVehicleDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.GroundArmor)
            {
                //support.WriteLog("GroundArmor");
                entry = actorProcessor.groundVehicleDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.Ship)
            {
                //support.WriteLog("Ship");
                entry = actorProcessor.shipVehicleDataEntry(actor, entry, customSceneOffset);
            }
            else if (actor.role == Actor.Roles.Missile)
            {
                //support.WriteLog("Missile");
                entry = actorProcessor.missileDataEntry(actor, entry, customSceneOffset);
            }
            else
            {
                //support.WriteLog("Other");
                entry = actorProcessor.genericDataEntry(actor, entry, customSceneOffset);
            }

            return entry;
        }


    }
}
