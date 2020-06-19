using System;
using UnityEngine;

namespace TacViewDataLogger
{
    public class actorProcessor
    {

        public support support = new support();

        public ACMIDataEntry airVehicleDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
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

        public ACMIDataEntry playerVehicleDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            Vector3D coords = support.convertPositionToLatLong_raw(actor.transform.position);
            entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)} | {Math.Round(actor.flightInfo.roll, 2)} | {Math.Round(actor.flightInfo.pitch, 2)} | {Math.Round(actor.flightInfo.heading, 2) - customOffset}";

            

            Actor targettedActor = DataGetters.getRadarLockTarget(actor.gameObject, actor);

            if (targettedActor != null)
            {
                entry.lockedTarget = support.getActorID(targettedActor);
            }

            entry.pilot = actor.actorName;
            entry.callSign = actor.actorName;
            entry.name = actor.name.Replace("(Clone)", "");

            return entry;
        }


        public ACMIDataEntry groundVehicleDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            Vector3D coords = support.convertPositionToLatLong_raw(actor.transform.position);
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

            entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)} | {Math.Round(roll, 2)} | {Math.Round(pitch, 2)} | {Math.Round(headingNum, 2) - customOffset}";

            entry.name = actor.actorName;

            if (actor.currentlyTargetingActor != null)
            {
                entry.lockedTarget = support.getActorID(actor.currentlyTargetingActor);
            }


            // This is all done in the VTOL XML file now! Easier to update.
            //if (actor.name.Contains("Infantry ")){
            //    entry._specificTypes = "Infantry";
            //    entry._objectClass = "Ground";

            //} else if (actor.name.Contains("Tank"))
            //{
            //    entry._specificTypes = "Tank";
            //    entry._basicTypes = "Vehicle";
            //    entry._objectClass = "Ground";
            //}
            //else if (actor.name.Contains("APC"))
            //{
            //    entry._basicTypes = "Armor";
            //    entry._objectClass = "Ground";
            //}
            ////else if (actor.name.Contains("MPA-155"))
            ////{
            ////    entry._basicTypes = "Armor";
            ////    entry._objectClass = "Ground";
            ////}
            //else if (actor.name.Contains("SAM FireCtrl Radar"))
            //{
            //    entry._basicTypes = "AntiAircraft";
            //}
            //else if (actor.name.Contains("SAM Launcher"))
            //{
            //    entry._basicTypes = "AntiAircraft";
            //}
            //else if (actor.name.Contains("SMC Radar"))
            //{
            //    entry._basicTypes = "AntiAircraft";
            //}
            //else if (actor.name.Contains("Radar"))
            //{
            //    entry._basicTypes = "AntiAircraft";
            //}
            //else if (actor.name.Contains("SAM Battery"))
            //{
            //    entry._basicTypes = "AntiAircraft";
            //}
            //else
            //{
            //    entry._basicTypes = "Vehicle";
            //    entry._objectClass = "Ground";
            //}

            

            return entry;
        }

        public ACMIDataEntry shipVehicleDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            Vector3D coords = support.convertPositionToLatLong_raw(actor.transform.position);
            entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)}";
            entry.name = actor.actorName;

            return entry;
        }

        public ACMIDataEntry genericDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
        {
            //Vector3D coords = support.convertPositionToLatLong_raw(actor.transform.position);
            //entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)} | {Math.Round(actor.flightInfo.roll, 2)} | {Math.Round(actor.flightInfo.pitch, 2)} | {Math.Round(actor.flightInfo.heading, 2) - customOffset}";
            Vector3D coords = support.convertPositionToLatLong_raw(actor.transform.position);
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

            entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)} | {Math.Round(roll, 2)} | {Math.Round(pitch, 2)} | {Math.Round(headingNum, 2) - customOffset}";

            entry._basicTypes = "Vehicle";
            entry.name = actor.actorName;
            return entry;
        }

        public ACMIDataEntry missileDataEntry(Actor actor, ACMIDataEntry entry, float customOffset = 0f)
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


        public ACMIDataEntry airportEntry(AirportManager airport)
        {
            ACMIDataEntry entry = new ACMIDataEntry();
            support.WriteLog($"Processing Airport {airport.airportName}");
            entry.objectId = support.getAirportID(airport);

            if (airport.team == Teams.Allied)
            {
                entry.color = "Blue";
            }
            else
            {
                entry.color = "Red";
            }

            Vector3D coords = support.convertPositionToLatLong_raw(airport.transform.position);

            double headingNum = Math.Atan2(airport.transform.forward.x, airport.transform.forward.z) * Mathf.Rad2Deg;

            if (headingNum < 0)
            {
                headingNum += 360;
            }


            Vector3 forward = airport.transform.forward;
            forward.y = 0f;

            float pitch = VectorUtils.SignedAngle(forward, airport.transform.forward, Vector3.up);

            Vector3 toDirection = Vector3.ProjectOnPlane(airport.transform.up, forward);
            float roll = VectorUtils.SignedAngle(Vector3.up, toDirection, Vector3.Cross(Vector3.up, forward));

            entry.locData = $"{Math.Round(coords.y, 7)} | {Math.Round(coords.x, 7)} | {Math.Round(coords.z, 7)} | {Math.Round(roll, 2)} | {Math.Round(pitch, 2)} | {Math.Round(headingNum, 2)}";

            entry.fullName = airport.airportName;

            entry.width = "200";
            entry.length = "80";
            entry.height = "2";

            entry._specificTypes = "Airport";
            entry.shape = "airbase1.obj";

            return entry;
        }

    }
}
