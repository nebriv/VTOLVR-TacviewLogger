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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TacViewDataLogger
{
    static class Globals
    {

        public static string projectName = "VTOL VR Tacview Data Logger";
        public static string projectAuthor = "Nebriv,TytanRock,mattidg";
        public static string projectVersion = "v2.7";

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

        public StringBuilder dataLog = new StringBuilder();

        public Dictionary<String, ACMIDataEntry> knownActors = new Dictionary<String, ACMIDataEntry>();


        public string acmiString;
        List<Actor> actors;

        public ACMIDataEntry newEntry;
        public ACMIDataEntry oldEntry;
        public ACMIDataEntry entry;


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
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

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

        void MissionReloaded()
        {
            if (TacViewDataLogger.SceneReloaded != null)
                TacViewDataLogger.SceneReloaded.Invoke();
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
                    Stopwatch timer = new Stopwatch();
                    timer.Start();

                    if (frameRateLog.Count == frameRateLogSize)
                    {
                        //support.WriteLog($"Current Sampling Rate: {period} - Current Frame Average: {frameRateLog.Average()}, min: {minFrameTime}, max: {maxFrameTime}");
                    }

                    nextActionTime += period;

                    elapsedSeconds += period;
                    dataLog.Append($"\n#{elapsedSeconds}");
                    GCLatencyMode oldMode = GCSettings.LatencyMode;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    GCSettings.LatencyMode = GCLatencyMode.LowLatency;
                    TacViewDataLogACMI();
                    GCSettings.LatencyMode = oldMode;
                    timer.Stop();
                    //Log("Time taken to get ACMI data: " + timer.ElapsedMilliseconds + "ms");

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

                // Delaying start of capture process for two seconds to let the game catch up.
                yield return new WaitForSeconds(2);
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
                }
                else if (VTScenario.current.envName == "morning")
                {
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd");
                        string minsec = DateTime.UtcNow.ToString("mm:ss");
                        timestamp += $"T06:{minsec}Z";
                        sw.WriteLine("0,ReferenceTime=" + timestamp);
                    }

                }
                else if (VTScenario.current.envName == "night")
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
            dataLog.Append("\n" + $"0,Event=Bookmark|Objective '{obj.objectiveName}' Started");
        }

        public void objectiveComplete(MissionObjective obj)
        {
            dataLog.Append("\n" + $"0,Event=Bookmark|Objective '{obj.objectiveName}' Completed");
        }
        public void objectiveFail(MissionObjective obj)
        {
            dataLog.Append("\n" + $"0,Event=Bookmark|Objective '{obj.objectiveName}' Failed");
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
                support.WriteLog("Done getting map");
                string title = "";

                if (map == null)
                {
                    support.WriteErrorLog("Unable to get custom map!");
                    title = $"0,Title={VTScenario.current.scenarioName.Replace(",", "\\,")} on Unknown Map";
                }
                else
                {
                    support.WriteLog("Map is not null!");
                    if (map.mapName != null)
                    {
                        title = $"0,Title={VTScenario.current.scenarioName.Replace(",", "\\,")} on {map.mapName.Replace(",", "\\,")}";
                    }
                    else
                    {
                        support.WriteErrorLog("Map does not have a name.");
                        title = $"0,Title={VTScenario.current.scenarioName.Replace(",", "\\,")} on Unknown Map Name";
                    }

                }
                support.WriteLog("Done writing title");
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
                support.WriteLog("Getting not custom map");
                VTMap map = support.getMap();
                string title = "";

                if (map == null)
                {
                    support.WriteErrorLog("Unable to get custom map!");
                    title = $"0,Title={VTScenario.current.scenarioName.Replace(",", "\\,")} on Unknown Map";
                }
                else
                {
                    support.WriteErrorLog("Map is null!2");
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



        }

        public void getAirports()
        {
            VTMapManager[] mm = FindObjectsOfType<VTMapManager>();
            foreach (AirportManager manager in mm[0].airports)
            {
                newEntry = actorProcessor.airportEntry(manager);

                dataLog.Append("\n" + newEntry.ACMIString());

            }
        }

        public IEnumerable<CMFlare> getFlares()
        {
            return FindObjectsOfType<CMFlare>();
        }
        public IEnumerable<ChaffCountermeasure.Chaff> getChaff()
        {
            var allChaff = new List<ChaffCountermeasure.Chaff>();
            foreach (var chaffCM in FindObjectsOfType<ChaffCountermeasure>())
            {
                foreach (ChaffCountermeasure.Chaff ch in Traverse.Create(chaffCM).Field("chaffs").GetValue() as ChaffCountermeasure.Chaff[])
                {
                    if (ch.decayed)
                    {
                        /* Do nothing */
                    }
                    else
                    {
                        allChaff.Add(ch);
                    }
                }
            }
            return allChaff;
        }
        public IEnumerable<Bullet> getBullets()
        {
            return FindObjectsOfType<Bullet>();
        }
        public IEnumerable<Rocket> getRockets()
        {
            return Rocket.allFiredRockets;
        }

        public IEnumerator writeString()
        {
            while (runlogger)
            {

                if (dataLog.Length > 0)
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
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            using (var writer = new StreamWriter(path, append: true))
            {
                /* Maybe this will be faster than using a queue? */
                writer.Write(dataLog.ToString());
            }
            dataLog.Clear();
        }


        public void TacViewDataLogACMI()
        {
            actors = TargetManager.instance.allActors;

            acmiString = "";

            // Processing game actors

            foreach (var actor in actors)
            {
                if (actor != null) {
                    acmiString = "";
                    support.UpdateID(actor);

                    newEntry = buildDataEntry(actor);

                    // If this is already a tracked actor
                    if (knownActors.ContainsKey(support.GetObjectID(actor)))
                    {
                        oldEntry = knownActors[support.GetObjectID(actor)];

                        // Diff the old entry and the new entry. Update the old entry with the new entry.
                        //acmiString = newEntry.ACMIString();
                        acmiString = newEntry.ACMIString(oldEntry);
                        knownActors[support.GetObjectID(actor)] = newEntry;
                    }
                    else
                    {
                        acmiString = newEntry.ACMIString();
                        knownActors.Add(support.GetObjectID(actor), newEntry);
                    }
                    if ((acmiString != "") && (acmiString.Contains(",")))
                    {
                        dataLog.Append("\n" + acmiString);
                    }
                } else
                {
                    //support.WriteLog("Error - Got a null actor!");
                }
            }

            // Getting flares and processing them
            acmiString = "";
            foreach (var flare in getFlares())
            {
                acmiString = "";
                support.UpdateID(flare);

                newEntry = buildFlareEntry(flare);

                if (knownActors.ContainsKey(support.GetObjectID(flare)))
                {
                    oldEntry = knownActors[support.GetObjectID(flare)];
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[support.GetObjectID(flare)] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(support.GetObjectID(flare), newEntry);
                }
                if (acmiString != "")
                {
                    dataLog.Append("\n" + acmiString);
                }
            }
            // Getting Chaff and processing them
            acmiString = "";
            foreach (var chaff in getChaff())
            {
                acmiString = "";
                support.UpdateID(chaff);

                newEntry = buildChaffEntry(chaff);

                if (knownActors.ContainsKey(support.GetObjectID(chaff)))
                {
                    oldEntry = knownActors[support.GetObjectID(chaff)];
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[support.GetObjectID(chaff)] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(support.GetObjectID(chaff), newEntry);
                }
                if (acmiString != "")
                {
                    dataLog.Append("\n" + acmiString);
                }
            }

            // Getting bullets and processing them
            foreach (var bullet in getBullets())
            {
                /* If this isn't active, don't update it or use it */
                if (!bullet.isActiveAndEnabled) continue;

                support.UpdateID(bullet);

                newEntry = buildBulletEntry(bullet);
                acmiString = "";
                if (knownActors.ContainsKey(support.GetObjectID(bullet)))
                {
                    oldEntry = knownActors[support.GetObjectID(bullet)];
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[support.GetObjectID(bullet)] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(support.GetObjectID(bullet), newEntry);
                }
                if (acmiString != "")
                {
                    dataLog.Append("\n" + acmiString);
                }
            }

            foreach (var rocket in getRockets())
            {
                /* If this isn't active, don't update it or use it */
                if (!rocket.isActiveAndEnabled) continue;

                support.UpdateID(rocket);

                newEntry = buildRocketEntry(rocket);
                acmiString = "";
                if (knownActors.ContainsKey(support.GetObjectID(rocket)))
                {
                    oldEntry = knownActors[support.GetObjectID(rocket)];
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[support.GetObjectID(rocket)] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(support.GetObjectID(rocket), newEntry);
                }
                if (acmiString != "")
                {
                    dataLog.Append("\n" + acmiString);
                }
            }


            foreach (var actor in support.ClearAndGetOldObjectIds())
            {
                /* If we weren't updated, then we don't exist anymore */

                // Need to handle checks for non vehicle actors
                //if (knownActors[actor]._basicTypes.Contains("FixedWing") ||
                //    knownActors[actor]._basicTypes.Contains("Vehicle"))
                //{
                //    /* If this is a vehicle, we can send the destroyed ACMI event */
                //    dataLog.Append("\n" +acmi.ACMIEvent("Destroyed", null, actor));
                //}

                dataLog.Append("\n" + $"-{actor}");
                knownActors.Remove(actor);
            }
        }

        public ACMIDataEntry buildFlareEntry(CMFlare flare)
        {
            ACMIDataEntry entry = new ACMIDataEntry();

            entry.objectId = support.GetObjectID(flare);

            Vector3D coords = support.convertPositionToLatLong_raw(flare.transform.position);

            entry.locData = $"{coords.y} | {coords.x} | {coords.z}";
            entry._specificTypes = "Flare";

            return entry;
        }
        public ACMIDataEntry buildChaffEntry(ChaffCountermeasure.Chaff chaff)
        {
            ACMIDataEntry entry = new ACMIDataEntry();

            entry.objectId = support.GetObjectID(chaff);

            Vector3D coords = support.convertPositionToLatLong_raw(chaff.position);

            entry.locData = $"{coords.y} | {coords.x} | {coords.z}";
            entry._specificTypes = "Chaff";

            return entry;
        }

        public ACMIDataEntry buildBulletEntry(Bullet bullet)
        {
            entry = new ACMIDataEntry();

            entry.objectId = support.GetObjectID(bullet);

            Vector3D coords = support.convertPositionToLatLong_raw(bullet.transform.position);

            entry.locData = $"{coords.y} | {coords.x} | {coords.z}";
            entry._specificTypes = "Bullet";

            return entry;
        }

        public ACMIDataEntry buildRocketEntry(Rocket rocket, float customOffset = 0f)
        {
            entry = new ACMIDataEntry();

            entry.name = rocket.name.Replace("(Clone)", "");

            Vector3D coords = support.convertPositionToLatLong_raw(rocket.transform.position);

            double headingNum = Math.Atan2(rocket.transform.forward.x, rocket.transform.forward.z) * Mathf.Rad2Deg;

            if (headingNum < 0)
            {
                headingNum += 360;
            }

            Vector3 forward = rocket.transform.forward;
            forward.y = 0f;

            float pitch = VectorUtils.SignedAngle(forward, rocket.transform.forward, Vector3.up);

            Vector3 toDirection = Vector3.ProjectOnPlane(rocket.transform.up, forward);
            float roll = VectorUtils.SignedAngle(Vector3.up, toDirection, Vector3.Cross(Vector3.up, forward));

            entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)} | {Math.Round(roll, 2)} | {Math.Round(pitch, 2)} | {Math.Round(headingNum, 2) - customOffset}";

            entry.objectId = support.GetObjectID(rocket);

            entry._specificTypes = "Rocket";

            return entry;
        }

        public ACMIDataEntry buildDataEntry(Actor actor)
        {

            entry = new ACMIDataEntry();
            entry.objectId = support.GetObjectID(actor);

            //actorName = actor's name in the mission
            //name = actor's unit name

            bool isRed;
            if (actor.team.ToString() == "Allied")
            {
                entry.color = "Blue";
                isRed = false;
            }
            else
            {
                entry.color = "Red";
                isRed = true;
            }

            if (PilotSaveManager.current.pilotName == actor.actorName)
            {

                entry = actorProcessor.airVehicleDataEntry(actor, entry, isRed, customSceneOffset);
                entry = actorProcessor.playerVehicleDataEntry(actor, entry, isRed, customSceneOffset);

            }
            else if (actor.role == Actor.Roles.Air)
            {
                //support.WriteLog("Air");
                entry = actorProcessor.airVehicleDataEntry(actor, entry, isRed, customSceneOffset);
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
