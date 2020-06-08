using System;
using UnityEngine;

namespace TacViewDataLogger
{
    class actorProcessor
    {

        public static ACMIDataEntry airVehicleDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            Vector3D coords = support.convertPositionToLatLong_raw(actor.transform.position);
            entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)} | {Math.Round(actor.flightInfo.roll, 2)} | {Math.Round(actor.flightInfo.pitch, 2)} | {Math.Round(actor.flightInfo.heading, 2) - customOffset}";
            entry._basicTypes = "FixedWing";
            entry.callSign = actor.designation.ToString();
            entry.name = actor.actorName;
            if (actor.currentlyTargetingActor != null)
            {
                entry.lockedTarget = support.getActorID(actor.currentlyTargetingActor);
            }

            entry.aoa = Math.Round(actor.flightInfo.aoa, 2).ToString();
            entry.ias = Math.Round(actor.flightInfo.airspeed, 2).ToString();
            entry.altitude = Math.Round(actor.flightInfo.altitudeASL, 2).ToString();
            entry.agl = Math.Round(actor.flightInfo.radarAltitude).ToString();

            //entry.afterburner = DataGetters.getAfterburners(actor.gameObject);
            //entry.radarMode = DataGetters.getRadarState(actor.gameObject);
            //entry.fuelWeight = DataGetters.getFuelMass(actor.gameObject);

            return entry;
        }

        public static ACMIDataEntry playerVehicleDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            Vector3D coords = support.convertPositionToLatLong_raw(actor.transform.position);
            entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)} | {Math.Round(actor.flightInfo.roll, 2)} | {Math.Round(actor.flightInfo.pitch, 2)} | {Math.Round(actor.flightInfo.heading, 2) - customOffset}";

            entry.pilot = actor.actorName;
            entry.callSign = actor.actorName;
            entry.name = actor.name.Replace("(Clone)", "");

            return entry;
        }


        public static ACMIDataEntry groundVehicleDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            entry._basicTypes = "Vehicle";
            return entry;
        }

        public static ACMIDataEntry shipVehicleDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            entry._basicTypes = "Watercraft";
            return entry;
        }

        public static ACMIDataEntry genericDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            entry._basicTypes = "Vehicle";

            return entry;
        }

        public static ACMIDataEntry missileDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            Vector3D coords = support.convertPositionToLatLong_raw(actor.transform.position);
            //entry.parent = actor.parentActor.gameObject.GetInstanceID().ToString("X").ToLower();

            entry.name = actor.name;

            double headingNum = Math.Atan2(actor.transform.forward.x, actor.transform.forward.z) * Mathf.Rad2Deg;

            if (headingNum < 0)
            {
                headingNum += 360;
            }


            Vector3 forward = actor.transform.forward;
            forward.y = 0f;

            float pitch = VectorUtils.SignedAngle(forward, actor.transform.forward, Vector3.up);

            Vector3 toDirection = Vector3.ProjectOnPlane(actor.transform.up, forward);
            float roll = VectorUtils.SignedAngle(Vector3.up, toDirection, Vector3.Cross(Vector3.up, forward));

            entry.locData = $"{Math.Round(coords.y,7)} | {Math.Round(coords.x,7)} | {Math.Round(coords.z,7)} | {Math.Round(roll, 2)} | {Math.Round(pitch, 2)} | {Math.Round(headingNum, 2) - customOffset}";
            entry._basicTypes = "Missile";
            entry.name = actor.actorName;
            entry.callSign = actor.actorName;
            return entry;
        }


    }
}
