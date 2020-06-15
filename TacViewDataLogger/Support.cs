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

        public static string getActorID(Actor actor)
        {
            return actor.gameObject.GetInstanceID().ToString("X").ToLower();
        }

        public static string getFlareID(CMFlare flare)
        {
            return flare.gameObject.GetInstanceID().ToString("X").ToLower();
        }

        public static string getBulletID(Bullet bullet)
        {
            return bullet.gameObject.GetInstanceID().ToString("X").ToLower();
        }

        public static string getAirportID(AirportManager airport)
        {
            return airport.GetInstanceID().ToString("X").ToLower();
        }



        public static Vector3D convertPositionToLatLong_raw(Vector3 position)
        {
            Vector3D real_loc;

            real_loc = WorldPositionToGPSCoords(VTResources.GetMap(VTScenario.current.mapID), position);
            return real_loc;
        }

        public static string convertPositionToLatLong(Vector3 position)
        {
            Vector3D real_loc;
            string locationtext;

            real_loc = WorldPositionToGPSCoords(VTResources.GetMap(VTScenario.current.mapID), position);
            locationtext = real_loc.ToString();
            return locationtext;
        }


        public static Vector3D WorldPositionToGPSCoords(VTMap map, Vector3 worldPoint)
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
