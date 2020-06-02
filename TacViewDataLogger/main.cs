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
using System.Diagnostics;

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

        private float elapsedSeconds;

        public ACMI acmi;

        public Queue<string> dataLog = new Queue<string>();

        public Dictionary<String, ACMIDataEntry> knownActors = new Dictionary<String, ACMIDataEntry>();

        public double saveTime;
        public float writeWaitTime;
        public float minWriteWateTime = 30f;


        public bool customScene = false;
        public float customSceneOffset = 0f;

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

            iterator = 0;

            support.WriteLog("Writing Reference Time");
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("0,ReferenceTime=" + timestamp);
            }
            support.WriteLog("Running Logger");
            runlogger = true;

            StartCoroutine(mainLoop());
            StartCoroutine(writeString());
        }


        //private void FixedUpdate()
        //{
        //    if (iterator < 46)
        //    {
        //        iterator++;
        //    }
        //    else
        //    {
        //        iterator = 0;
        //        secondsElapsed++;

        //        if (runlogger)
        //        {
        //            if (SceneManager.GetActiveScene().buildIndex != 7 && SceneManager.GetActiveScene().buildIndex != 12)
        //            {
        //                ResetLogger();
        //            }
        //            Support.WriteLog(secondsElapsed.ToString());
        //            using (StreamWriter sw = File.AppendText(path))
        //            {
        //                sw.WriteLine("#" + secondsElapsed);
        //            }
        //            TacViewDataLogACMI();
                    
        //        }
        //    }
        //}

        public IEnumerator mainLoop()
        {
            while (runlogger)
            {
                yield return new WaitForSeconds(.25f);
                elapsedSeconds += .25f;

                if (SceneManager.GetActiveScene().buildIndex != 7 && SceneManager.GetActiveScene().buildIndex != 11)
                {
                    ResetLogger();
                }

                dataLog.Enqueue($"#{Math.Round(elapsedSeconds, 2)}");


                // Testing this for performance testing...
                StartCoroutine(TacViewDataLogACMI());

            }
        }
        
        public CMFlare[] getFlares()
        {

            var flares = FindObjectsOfType<CMFlare>();
            return flares;
        }


        public Bullet[] getBullets()
        {

            var bullets = FindObjectsOfType<Bullet>();
            return bullets;
        }

        public IEnumerator writeString()
        {
            while (runlogger)
            {

                yield return new WaitForSeconds(15);

                if (dataLog.Count > 100)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    support.WriteLog("Saving Data Log");
                    File.AppendAllLines(path, dataLog);
                    dataLog.Clear();
                    stopwatch.Stop();
                    
                    saveTime = stopwatch.Elapsed.TotalMilliseconds;


                    support.WriteLog($"Time to save: {stopwatch.Elapsed.ToString()}");
                }
            }

        }

        public void ResetLogger()
        {
            runlogger = false;

            UnityEngine.Debug.Log("Scene end detected. Stopping TacView Recorder");

            Start();
        }

        public IEnumerator TacViewDataLogACMI()
        {
            List < Actor > actors = TargetManager.instance.allActors;
            List<String> logStrings = new List<String>();
            List<String> actorIDList = new List<String>();

            // Processing game actors
            for (int i = 0; i < actors.Count; i++)
            {
                yield return new WaitForEndOfFrame();
                ACMIDataEntry newEntry = buildDataEntry(actors[i]);

                actorIDList.Add(support.getActorID(actors[i]));

                string acmiString = "";
                // If this is already a tracked actor
                if (knownActors.ContainsKey(support.getActorID(actors[i])))
                {
                    ACMIDataEntry oldEntry = knownActors[support.getActorID(actors[i])];

                    // Diff the old entry and the new entry. Update the old entry with the new entry.
                    acmiString = newEntry.ACMIString(oldEntry);
                    knownActors[support.getActorID(actors[i])] = newEntry;
                }
                else
                {
                    acmiString = newEntry.ACMIString();
                    knownActors.Add(support.getActorID(actors[i]), newEntry);
                }
                if (acmiString != "")
                {
                    dataLog.Enqueue(acmiString);
                }    
            }


            // Getting flares and processing them
            //var flares = getFlares();
            //foreach (CMFlare flare in flares)
            //{

            //    actorIDList.Add(support.getFlareID(flare));

            //    ACMIDataEntry newEntry = buildFlareEntry(flare);
            //    string acmiString = "";
            //    if (knownActors.ContainsKey(support.getFlareID(flare)))
            //    {
            //        ACMIDataEntry oldEntry = knownActors[support.getFlareID(flare)];
            //        acmiString = newEntry.ACMIString(oldEntry);
            //        knownActors[support.getFlareID(flare)] = newEntry;
            //    }
            //    else
            //    {
            //        acmiString = newEntry.ACMIString();
            //        knownActors.Add(support.getFlareID(flare), newEntry);
            //    }
            //    if (acmiString != "")
            //    {
            //        dataLog.Enqueue(acmiString);
            //    }
            //}

            // Getting bullets and processing them
            //var bullets = getBullets();
            //foreach (Bullet bullet in bullets)
            //{

            //    actorIDList.Add(support.getBulletID(bullet));

            //    ACMIDataEntry newEntry = buildBulletEntry(bullet);
            //    string acmiString = "";
            //    if (knownActors.ContainsKey(support.getBulletID(bullet)))
            //    {
            //        ACMIDataEntry oldEntry = knownActors[support.getBulletID(bullet)];
            //        acmiString = newEntry.ACMIString(oldEntry);
            //        knownActors[support.getBulletID(bullet)] = newEntry;
            //    }
            //    else
            //    {
            //        acmiString = newEntry.ACMIString();
            //        knownActors.Add(support.getBulletID(bullet), newEntry);
            //    }
            //    if (acmiString != "")
            //    {
            //        dataLog.Enqueue(acmiString);
            //    }
            //}

            StartCoroutine(cleanUpActors(actorIDList));

        }


        public IEnumerator cleanUpActors(List<String> actorIDList)
        {
            
            List<String> removedActors = new List<String>();

            foreach (String actor in knownActors.Keys)
            {
                if (!actorIDList.Contains(actor))
                {
                    yield return new WaitForEndOfFrame();
                    removedActors.Add(actor);
                    support.WriteLog($"Actor {actor} no longer exists");
                    dataLog.Enqueue(acmi.ACMIEvent("Destroyed", null, actor));
                    dataLog.Enqueue($"-{actor}");

                }
            }

            foreach (String actor in removedActors)
            {
                knownActors.Remove(actor);
            }
        }

        public ACMIDataEntry buildFlareEntry(CMFlare flare)
        {
            ACMIDataEntry entry = new ACMIDataEntry();

            entry.objectId = support.getFlareID(flare);

            Vector3D coords = support.convertPositionToLatLong_raw(flare.transform.position);

            entry.locData = $"{coords.x} | {coords.y} | {coords.z}";
            entry._specificTypes = "Flare";

            return entry;
        }



        public ACMIDataEntry buildBulletEntry(Bullet bullet)
        {
            ACMIDataEntry entry = new ACMIDataEntry();

            entry.objectId = support.getBulletID(bullet);

            Vector3D coords = support.convertPositionToLatLong_raw(bullet.transform.position);

            entry.locData = $"{coords.x} | {coords.y} | {coords.z}";
            entry._specificTypes = "Bullet";

            return entry;
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
                entry = actorProcessor.airVehicleDataEntry(actor, entry, customSceneOffset);
                entry = actorProcessor.playerVehicleDataEntry(actor, entry, customSceneOffset);
            } else if (actor.role == Actor.Roles.Air)
            {
                entry = actorProcessor.airVehicleDataEntry(actor, entry, customSceneOffset);
            } else if (actor.role == Actor.Roles.Ground)
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
