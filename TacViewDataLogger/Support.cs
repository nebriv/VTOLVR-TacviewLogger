using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace TacViewDataLogger
{
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        private readonly object syncObject = new object();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);
            lock (syncObject)
            {
                while (base.Count > Size)
                {
                    T outObj;
                    base.TryDequeue(out outObj);
                }
            }
        }
    }


    public class support
    {
        struct ActorEntryValue
        {
            private static int redCallsignIndex = 0;
            private static int blueCallsignIndex = 0;
            public ActorEntryValue(String uniqueID)
            {
                this.uniqueID = uniqueID;
                recentlyAdded = true;
                updated = true;
                needsUpdating = true;
                callsign = "";
            }
            public void Update()
            {
                updated = true;
            }
            public void Clear()
            {
                recentlyAdded = false;
                updated = false;
            }
            public void GenerateCallsign(bool isRed)
            {
                if (isRed)
                {
                    callsign = "Tango " + redCallsignIndex++;
                }
                else
                {
                    callsign = "Alpha " + blueCallsignIndex++;
                }
            }
            public String uniqueID;
            public bool recentlyAdded;
            public bool updated;
            public bool needsUpdating;
            public String callsign;
        }
        static Dictionary<object, ActorEntryValue> objectIDs = new Dictionary<object, ActorEntryValue>();
        static List<string> removedObjectIDs = new List<string>();

        private static long nextID = 0x2000;
        public static string GenerateUniqueID()
        {
            return nextID++.ToString("X").ToLower();
        }

        /* Since the bullets come from a pool, we have to access the bullet's KillBullet method
         * in order to keep track of which bullets are still being used
         * By keeping track of the removed bullets we can maintain a list of "killed" bullets
         * and add them to the removed list when it's called, and we can remove the bullet from the dictionary
         * so we don't keep track of it and a new entry is created when a new bullet is fired
         */
        [Harmony.HarmonyPatch(typeof(Bullet), "KillBullet")]
        class BulletKillPatch
        {
            static void Postfix(object __instance)
            {
                if (!objectIDs.ContainsKey(__instance)) return;
                removedObjectIDs.Add(GetObjectID(__instance));
                objectIDs.Remove(__instance);
            }
        }
        [Harmony.HarmonyPatch(typeof(FlareCountermeasure), "OnFlareDecayed")]
        class FlareDecayPatch
        {
            static void Postfix(CMFlare f)
            {
                if (!objectIDs.ContainsKey(f)) return;
                removedObjectIDs.Add(GetObjectID(f));
                objectIDs.Remove(f);
            }
        }


        public VTMapManager mm;
        public static void WriteLog(string line)
        {
            Debug.Log($"{Globals.projectName} - {line}");
        }

        public static void WriteErrorLog(string line)
        {
            //Debug.LogError($"{Globals.projectName} - {line}");
        }

        public static List<Actor> getActorsByRoll(Actor.Roles role)
        {

            List<Actor> actors = TargetManager.instance.allActors;
            List<Actor> filtered = new List<Actor>();

            for (int i = 0; i < actors.Count; i++)
            {
                if ((actors[i].role == role) && (PilotSaveManager.current.pilotName != actors[i].actorName))
                {
                    filtered.Add(actors[i]);
                }
            }

            return filtered;

        }

        public static string GetObjectID(object obj)
        {
            if (!objectIDs.ContainsKey(obj))
            {
                /* If we don't have this object, return "1" to ensure it doesn't conflict with a known ID */
                return "1";
            }
            return objectIDs[obj].uniqueID;
        }
        public static void UpdateID(object obj, bool needsUpdating = true)
        {
            if (!objectIDs.ContainsKey(obj))
            {
                objectIDs[obj] = new ActorEntryValue(GenerateUniqueID());
            }
            ActorEntryValue tmp;
            objectIDs.TryGetValue(obj, out tmp);
            tmp.Update();
            tmp.needsUpdating = needsUpdating;
            objectIDs[obj] = tmp;
        }
        public static string GetObjectCallsign(object obj, bool isRed)
        {
            if (objectIDs[obj].callsign == "")
            {
                ActorEntryValue tmp;
                objectIDs.TryGetValue(obj, out tmp);
                tmp.GenerateCallsign(isRed);
                objectIDs[obj] = tmp;
            }
            return objectIDs[obj].callsign;
        }
        public static IEnumerable<string> ClearAndGetOldObjectIds()
        {
            var notUpdated = new List<string>();

            var newDictionary = new Dictionary<object, ActorEntryValue>();

            foreach (var obj in objectIDs)
            {
                if (!obj.Value.updated && obj.Value.needsUpdating)
                {
                    notUpdated.Add(obj.Value.uniqueID);
                }
                else
                {
                    ActorEntryValue tmp = obj.Value;
                    tmp.Clear();
                    newDictionary[obj.Key] = tmp;
                }
            }
            objectIDs = newDictionary;

            /* Add the objects removed via patches */
            notUpdated.AddRange(removedObjectIDs);
            removedObjectIDs = new List<string>();

            return notUpdated;
        }

        public static string cleanString(string input)
        {
            string clean = input.Replace("\\", "").Replace("/", "").Replace("<", "").Replace(">", "").Replace("*", "").Replace("\"", "").Replace("?", "").Replace(":", "").Replace("|", "").Replace(" ", "");
            return clean;
        }
        public VTMap getMap()
        {
            VTMap map;

            support.WriteLog($"Looking for Map ID {VTScenario.current.mapID}");
            map = VTResources.GetMap(VTScenario.current.mapID);
            support.WriteLog("Done looking for map id");
            if (map == null)
            {
                support.WriteLog("Getting custom map 1");
                VTMapCustom custommap = VTResources.GetCustomMap(VTScenario.current.mapID);
                return custommap;
            }
            if (map == null)
            {
                support.WriteLog("Getting custom map 2");
                VTMapCustom custommap = VTResources.GetSteamWorkshopMap(VTScenario.current.mapID);
                return custommap;
            }

            support.WriteLog("MM check");
            if (mm != null)
            {
                support.WriteLog("MM exists, Map null check?");
                if (map == null)
                {
                    support.WriteLog("Map not null. Get map from map manager");
                    return VTMapManager.fetch.map;
                }
                else
                {
                    support.WriteLog("Map is null");
                }
            }
            else
            {
                support.WriteLog("Map Manager is null");
            }


            if (map != null)
            {
                support.WriteLog("Returning map.");
                return map;
            }
            else
            {
                support.WriteLog("Unable to find a valid VTMap!");
                return null;
            }

        }


        public Vector3D convertPositionToLatLong_raw(Vector3 position)
        {
            Vector3D real_loc;

            real_loc = mm.WorldPositionToGPSCoords(position);

            real_loc = WorldPositionToGPSCoords(position);
            return real_loc;
        }

        public static string convertPositionToLatLong(Vector3 position)
        {
            Vector3D real_loc;
            string locationtext;

            real_loc = WorldPositionToGPSCoords(position);
            locationtext = real_loc.ToString();
            return locationtext;
        }


        public static Vector3D WorldPositionToGPSCoords(Vector3 worldPoint)
        {
            VTMap map = VTMapManager.fetch.map;

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
