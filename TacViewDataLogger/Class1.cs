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

public struct TacViewData
{
    public string Longitude;
    public string Latitude;
    public string Altitude;
    public string Roll;
    public string Pitch;
    public float Yaw_f;
    public string Yaw;

    public TacViewData(string Longitude, string Latitude, string Altitude, string Roll, string Pitch, string Yaw)
    {

        this.Longitude = Longitude;
        this.Latitude = Latitude;
        this.Altitude = Altitude;
        this.Roll = Roll;
        this.Pitch = Pitch;
        this.Yaw_f = float.Parse(Yaw) - float.Parse("90.0");
        this.Yaw = this.Yaw_f.ToString();
    }
    public override string ToString()
    {
        return this.Longitude + "," + this.Latitude + "," + this.Altitude + "," + this.Roll + "," + this.Pitch + "," + this.Yaw;
    }
}

public struct FlightInfoLogger
{

    public string StallDetector;
    public string MissileDetected;
    public string TailHook;
    public string Health;
    public string Flaps;
    public string Brakes;
    public string GearState;
    public string EjectionState;
    public Dictionary<string,string> FlightInfo;
    public Dictionary<string, string> Lights;
    public string Location;
    public string RadarState;
    public string RadarCrossSection;
    public string BatteryLevel;
    public List<Dictionary<string, string>> Engines;
    public string FuelLevel;
    public string FuelBurnRate;
    public string FuelDensity;



    public FlightInfoLogger(string StallDetector, string MissileDetected, string RadarCrossSection, string TailHook, string Health, string Flaps, string Brakes, string GearState, string EjectionState, Dictionary<string, string> FlightInfo, string Location, string RadarState, string BatteryLevel, List<Dictionary<string, string>> Engines, string FuelLevel, string FuelBurnRate, string FuelDensity, Dictionary<string, string> Lights)
    {
        this.StallDetector = StallDetector;
        this.MissileDetected = MissileDetected;
        this.RadarCrossSection = RadarCrossSection;
        this.TailHook = TailHook;
        this.Health = Health;
        this.Flaps = Flaps;
        this.Brakes = Brakes;
        this.GearState = GearState;
        this.EjectionState = EjectionState;
        this.FlightInfo = FlightInfo;
        this.Location = Location;
        this.RadarState = RadarState;
        this.BatteryLevel = BatteryLevel;
        this.Engines = Engines;
        this.FuelLevel = FuelLevel;
        this.FuelBurnRate = FuelBurnRate;
        this.FuelDensity = FuelDensity;
        this.Lights = Lights;

    }
} 

public class TacViewDataLogger : VTOLMOD
{

    private VTOLAPI api;
    public Vector3 spawnPos, spawnRot;
    TimeSpan interval = new TimeSpan(0, 0, 2);
    private ModuleEngine[] engines;

    private WheelsController wheelsController;
    private AeroController aeroController;
    private VRThrottle vRThrottle;
    private Battery battery;
    private Radar player_radar;

    private bool runlogger;
    private int iterator;
    private GameObject currentVehicle;
    private string TacViewFolder;

    private string path;

    private int secondsElapsed;

    private void Awake()
    {
        wheelsController = GetComponent<WheelsController>();
        aeroController = GetComponent<AeroController>();
        //tiltController = GetComponent<TiltController>();
        engines = GetComponentsInChildren<ModuleEngine>();
        player_radar = GetComponent<Radar>();
        battery = GetComponent<Battery>();

    }

    private void Start()
    {
        HarmonyInstance harmony = HarmonyInstance.Create("neb.logger.logger");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        Debug.Log("Starting Data Logger");
        api = VTOLAPI.instance;
        currentVehicle = VTOLAPI.instance.GetPlayersVehicleGameObject();

        System.IO.Directory.CreateDirectory("TacViewDataLogs");
        System.IO.Directory.CreateDirectory("TacViewDataLogs\\" + DateTime.UtcNow.ToString("yyyy-MM-dd HHmm"));

        TacViewFolder = "TacViewDataLogs\\" + DateTime.UtcNow.ToString("yyyy-MM-dd HHmm") + "\\";

        path = @TacViewFolder + "datalog.acmi";

        if (!File.Exists(path))
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("FileType=text/acmi/tacview");
                sw.WriteLine("FileVersion=2.1");
            }
        }

        WaitForMap();
        runlogger = true;
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        using (StreamWriter sw = File.AppendText(path))
        {
            sw.WriteLine("0,ReferenceTime=" + timestamp);
        }

    }

    private IEnumerator WaitForMap()
    {
        Log("Started WaitForMap");
        while (VTMapManager.fetch == null || !VTMapManager.fetch.scenarioReady)
        {
            yield return null;
        }
        Debug.Log("Wait for map finished");
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
            //LogData();
            if (runlogger)
            {

                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine("#" + secondsElapsed);
                }
                //TacViewDebug();
                TacViewDataLogACMI();
            }
        }
    }


    public string cleanString(string input)
    {
        string clean = input.Replace("\\", "").Replace("/", "").Replace("<", "").Replace(">", "").Replace("*", "").Replace("\"", "").Replace("?", "").Replace(":", "").Replace("|", "");
        return clean;
    }


    public void TacViewDebug()
    {
        Actor[] actors = GameObject.FindObjectsOfType<Actor>();
        Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        foreach (Actor actor in actors)
        {
            string color;
            string unit_name;

            Debug.Log("Name " + actor.actorName + " Position " + actor.transform.position);

            string T;
            T = "T=" + convertPositionToLatLong_raw(actor.transform.position).x.ToString() + "|" + convertPositionToLatLong_raw(actor.transform.position).y.ToString() + "|" + convertPositionToLatLong_raw(actor.transform.position).z.ToString();
            Debug.Log(T);

            string X;
            X = "X=" + convertPositionToLatLong_raw(actor.position).x.ToString() + "|" + convertPositionToLatLong_raw(actor.position).y.ToString() + "|" + convertPositionToLatLong_raw(actor.position).z.ToString();
            Debug.Log(X);


            if (actor.GetMissile() != null)
            {
                Debug.Log("Missile Info " + actor.GetMissile().debugMissile.ToString());
            }
            Debug.Log("--------");
        }
    }

    public void TacViewDataLogACMI()
    {
        Actor[] actors = GameObject.FindObjectsOfType<Actor>();
        List<String> logStrings = new List<String>();

        foreach (Actor actor in actors)
        {
            string color;
            string unit_name;

            List<String> parameters = new List<String>();


            if (actor.team.ToString() == "Allied")
            {
                color = "Blue";
            }
            else
            {
                color = "Red";
            }

            try
            {
                    
                string actorID = (actor.actorID + 1).ToString();
                parameters.Add(actorID);

                if (actor.unitSpawn)
                {
                    try
                    {
                        string T;
                        if (actor.unitSpawn.actor.flightInfo != null)
                        {
                            T = "T=" + convertPositionToLatLong_raw(actor.unitSpawn.actor.position).x.ToString() + "|" + convertPositionToLatLong_raw(actor.unitSpawn.actor.position).y.ToString() + "|" + convertPositionToLatLong_raw(actor.unitSpawn.actor.position).z.ToString() + "|" + actor.unitSpawn.actor.flightInfo.roll.ToString() + "|" + actor.unitSpawn.actor.flightInfo.pitch.ToString() + "|" + (float.Parse(actor.unitSpawn.actor.flightInfo.heading.ToString()) - float.Parse("90")).ToString();
                        }
                        else
                        {
                            T = "T=" + convertPositionToLatLong_raw(actor.transform.position).x.ToString() + "|" + convertPositionToLatLong_raw(actor.transform.position).y.ToString() + "|" + convertPositionToLatLong_raw(actor.transform.position).z.ToString();
                        }
                        parameters.Add(T);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error getting T " + ex.ToString());

                    }
                }
                else
                {
                    try
                    {
                        string T;
                        T = "T=" + convertPositionToLatLong_raw(actor.transform.position).x.ToString() + "|" + convertPositionToLatLong_raw(actor.transform.position).y.ToString() + "|" + convertPositionToLatLong_raw(actor.transform.position).z.ToString();
                        parameters.Add(T);

                    }
                    catch (Exception ex2)
                    {
                        Debug.LogError("IT REALLY WONT GET IT" + ex2.ToString());
                    }
                }



                try
                {
                    string ias;
                    if (actor.unitSpawn.actor.flightInfo != null)
                    {
                        ias = "IAS=" + actor.unitSpawn.actor.flightInfo.airspeed.ToString();
                        parameters.Add(ias);
                    }
                    
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error getting IAS " + ex.ToString());
                }

                try
                {
                    string aoa;
                    if (actor.unitSpawn.actor.flightInfo != null)
                    {
                        aoa = "AOA=" + actor.unitSpawn.actor.flightInfo.aoa.ToString();
                        parameters.Add(aoa);
                    }
                    
                } catch (Exception ex)
                {
                    Debug.LogError("Error getting AOA" + ex.ToString());
                }

                try
                {
                    string actorName = "Name=" + actor.actorName;
                    parameters.Add(actorName);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error getting Name" + ex.ToString());
                }

                try
                {
                    string obj_type;
                    if (actor.role == Actor.Roles.Air)
                    {
                        obj_type = "Type=Air+FixedWing";
                        parameters.Add(obj_type);
                    }
                    else if (actor.role == Actor.Roles.Ground)
                    {
                        obj_type = "Type=Ground+Vehicle";
                        parameters.Add(obj_type);
                    }
                    else if (actor.role == Actor.Roles.GroundArmor)
                    {
                        obj_type = "Type=Ground+Tank";
                        parameters.Add(obj_type);
                    }
                    else if (actor.role == Actor.Roles.Ship)
                    {
                        obj_type = "Type=Sea+Watercraft";
                        parameters.Add(obj_type);
                    }
                    else if (actor.role == Actor.Roles.Missile)
                    {
                        obj_type = "Type=Air+Missile";
                        parameters.Add(obj_type);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error getting Type " + ex.ToString());
                }


                try
                {
                    parameters.Add("Coalition=" + actor.team.ToString());
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error getting Team " + ex.ToString());
                }

                try
                {
                    parameters.Add("Color=" + color);
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error getting Color " + ex.ToString());
                }

                try
                {
                    string actor_designator;
                    if (actor.unitSpawn.actor)
                    {
                        actor_designator = actor.unitSpawn.actor.designation.ToString();
                        parameters.Add(actor_designator);
                    }
                    
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error getting Designator " + ex.ToString());
                }

                try
                {
                    if (!actor.alive)
                    {
                        using (StreamWriter sw = File.AppendText(path))
                        {
                            sw.WriteLine("0,EventDestroyed|" + actorID + "|");
                        }
                        parameters.Add("Disabled=1");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("Error getting Alive " + ex.ToString());
                }


                String seperator = ",";
                string output_line = String.Join(seperator, parameters);

                logStrings.Add(output_line);

            } catch (Exception ex)
            {
                Debug.LogError("Exception caught for " + actor.actorName + ex.ToString());
            }

        }
        //Writing it all out once per second is way faster than writing it out per unit. Doh!
        File.AppendAllLines(path, logStrings);
    }

    public void TacViewDataLog()
    {
        Actor[] actors = GameObject.FindObjectsOfType<Actor>();
        Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        foreach (Actor actor in actors)
        {
            string color;
            string unit_name;

            if (actor.alive)
            {
                if (actor.team.ToString() == "Allied")
                {
                    color = "Blue";
                }
                else
                {
                    color = "Red";
                }


                if (actor.unitSpawn != null)
                {
                    try
                    {
                        unit_name = cleanString(actor.unitSpawn.unitName.ToString());
                    }
                    catch (Exception Ex)
                    {
                        Debug.LogError("Error getting actor (" + actor.name + ") unit name " + Ex.ToString());
                        unit_name = "Unknown";
                    }



                    string path = @TacViewFolder + unit_name + " (" + cleanString(actor.name.ToString().Replace("[", "").Replace("]", "")) + ") " + "[" + color + "].csv";

                    if (!File.Exists(path))
                    {
                        // Create a file to write to.
                        using (StreamWriter sw = File.CreateText(path))
                        {
                            sw.WriteLine("Time,Longitude,Latitude,Altitude,Roll (deg),Pitch (deg),Yaw (deg)");
                        }
                    }

                    try
                    {
                        TacViewData unitData = new TacViewData(convertPositionToLatLong_raw(actor.unitSpawn.actor.position).x.ToString(),
                        convertPositionToLatLong_raw(actor.unitSpawn.actor.position).y.ToString(),
                        actor.flightInfo.altitudeASL.ToString(),
                        actor.flightInfo.roll.ToString(),
                        actor.flightInfo.pitch.ToString(),
                        actor.flightInfo.heading.ToString());

                        using (StreamWriter sw = File.AppendText(path))
                        {
                            Debug.Log(actor.name + unitData.ToString());
                            sw.WriteLine(unixTimestamp + "," + unitData.ToString());
                        }
                    }
                    catch (Exception Ex)
                    {
                        Debug.Log("Error parsing data required for TacView " + Ex.ToString());
                        using (StreamWriter sw = File.AppendText(path))
                        {
                            sw.WriteLine(unixTimestamp + "," + "Invalid Data");
                        }
                    }
                } else
                {
                    Debug.Log("Actor " + actor.name + " Missing UnitSpawn?");
                    
                    if (actor.flightInfo != null)
                    {
                        Debug.Log("It has flight info... wtf");
                    }
                    if (actor.position != null)
                    {
                        Debug.Log("But does it have a position " + actor.position.ToString());
                    }

                }


            }
        }
    }

    public void LogData()
    {

        api = VTOLAPI.instance;
        GameObject currentVehicle = VTOLAPI.instance.GetPlayersVehicleGameObject();

        string path = @"FlightLog.txt";

        if (!File.Exists(path))
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("Flight Log Started");
            }
        }

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffZ");

        FlightInfoLogger datalogger = new FlightInfoLogger(getStallDetector(currentVehicle),
            getMissileDetected(currentVehicle),
            getRadarCrossSection(currentVehicle),
            getTailHook(currentVehicle),
            getHealth(currentVehicle),
            getFlaps(currentVehicle),
            getBrakes(currentVehicle),
            getGearState(currentVehicle),
            getEjectionState(currentVehicle),
            getFlightInfo(currentVehicle.GetComponent<FlightInfo>()),
            getVehicleLocation(currentVehicle),
            getRadarState(currentVehicle),
            getBatteryLevel(currentVehicle),
            GetEngineStats(currentVehicle),
            getFuelLevel(currentVehicle),
            getFuelBurnRate(currentVehicle),
            getFuelDensity(currentVehicle),
            getVehicleLights(currentVehicle));

        string json = Valve.Newtonsoft.Json.JsonConvert.SerializeObject(datalogger);
        //Debug.Log(json);

        string datalog = timestamp + "\t" + json;

        



        using (StreamWriter sw = File.AppendText(path))
        {
            Debug.Log(datalog);
            sw.WriteLine(datalog);
        }

    }

    private List<Actor> getUnits()
    {
        List<Actor> unitList = new List<Actor>();
        ScenarioUnits units = VTScenario.current.units;
        foreach (KeyValuePair<int, UnitSpawner> unit in units.units)
        {
            //[0, Player [0] (UnitSpawner)]
            //[1, Aircraft Carrier [1] (UnitSpawner)]
            //[2, KC-49 [2] (UnitSpawner)]

            unitList.Add(unit.Value.spawnedUnit.actor);

        }
        return unitList;
    }

    private string getStallDetector(GameObject vehicle)
    {

        try
        {
            HUDStallWarning hudstall = vehicle.GetComponentInChildren<HUDStallWarning>();
            //var stalling = Traverse.Create<bool>.Method("HUDStallWarning").GetValue<stalling>();
            //bool stall = stallwarning.GetComponentInChildren(stalling);
            Debug.Log("Stalling Value " + GetPropValue(hudstall, "stalling"));

            return "Not Implemented";
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception caught getting stall state " + ex.ToString());
            return "Unavailable";
        }
    }

    private string getMissileDetected(GameObject vehicle)
    {
        try
        {
            MissileDetector md = vehicle.GetComponentInChildren<MissileDetector>();
            return md.missileDetected.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }
    }

    private string getRadarCrossSection(GameObject vehicle)
    {
        try
        {
            RadarCrossSection rcs = vehicle.GetComponentInChildren<RadarCrossSection>();
            return rcs.GetAverageCrossSection().ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }
    }

    private string getTailHook(GameObject vehicle)
    {
        try
        {
            Tailhook hook = vehicle.GetComponentInChildren<Tailhook>();
            return hook.isDeployed.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }
    }
    private string getHealth(GameObject vehicle)
    {
        try
        {
            Health health = vehicle.GetComponentInChildren<Health>();
            return health.currentHealth.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }
    }

    private string getFlaps(GameObject vehicle)
    {
        try
        {
            AeroController aero = vehicle.GetComponentInChildren<AeroController>();
            return aero.flaps.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }
    }

    private string getBrakes(GameObject vehicle)
    {
        try
        {
            AeroController aero = vehicle.GetComponentInChildren<AeroController>();
            return aero.brake.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }
    }

    private string getGearState(GameObject vehicle)
    {
        try
        {
            GearAnimator gear = vehicle.GetComponentInChildren<GearAnimator>();
            return gear.state.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }
    }

    private string getEjectionState(GameObject vehicle)
    {
        string ejectionState;
        try
        {
            EjectionSeat ejection = vehicle.GetComponentInChildren<EjectionSeat>();
            ejectionState = ejection.ejected.ToString();
        }
        catch (Exception ex)
        {
            ejectionState = "Unavailable";
        }
        
        return ejectionState;
    }


    private Dictionary<string, string> getFlightInfo(FlightInfo info)
    {

        Dictionary<string, string> flightInfo = new Dictionary<string, string>();

        flightInfo.Add("AoA", info.aoa.ToString());
        flightInfo.Add("Airspeed", info.airspeed.ToString());
        flightInfo.Add("Heading", info.heading.ToString());
        flightInfo.Add("Vertical Speed", info.verticalSpeed.ToString());
        flightInfo.Add("Roll", info.roll.ToString());
        flightInfo.Add("Pitch", info.pitch.ToString());
        flightInfo.Add("G Force", info.playerGs.ToString());
        flightInfo.Add("Altitude (ASL)", info.altitudeASL.ToString());
        flightInfo.Add("Altitude (Radar)", info.radarAltitude.ToString());




        return flightInfo;
    }

    private Dictionary<string, string> getVehicleLights(GameObject vehicle)
    {
        try
        {
            ExteriorLightsController lightcontroller = vehicle.GetComponentInChildren<ExteriorLightsController>();
            Dictionary<string, string> lights = new Dictionary<string, string>();

            lights.Add("Landing Lights", lightcontroller.landingLights.ToString());
            lights.Add("Navigation Lights", lightcontroller.navLights.ToString());
            lights.Add("Strobe Lights", lightcontroller.strobeLights.ToString());
            return lights;
        } catch (Exception ex)
        {
            Dictionary<string, string> lights = new Dictionary<string, string>();
            Debug.LogError("Error getting lights " + ex.ToString());
            return lights;
        }
    }

    public static object GetPropValue(object src, string propName)
    {
        return src.GetType().GetProperty(propName).GetValue(src, null);
    }


    private string getMap()
    {
        return VTScenario.current.mapID;
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

    private string getVehicleLocation(GameObject vehicle)
    {

        Vector3 location_val;
        string locationtext;

        try
        {
            location_val = currentVehicle.transform.position;
            return convertPositionToLatLong(location_val);

        }
        catch (Exception ex)
        {
            Debug.LogError("Error getting vehicle location " + ex.ToString());
            locationtext = "None";
            return locationtext;
        }
        
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
    private string getRadarState(GameObject vehicle)
    {
        try
        {
            Radar radar = vehicle.GetComponentInChildren<Radar>();
            return radar.radarEnabled.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }

    }

    private string getBatteryLevel(GameObject vehicle)
    {

        try
        {
            Battery battery = vehicle.GetComponentInChildren<Battery>();
            return battery.currentCharge.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }


    }




    private List<Dictionary<string, string>> GetEngineStats(GameObject vehicle)
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

            engines.Add(engineDict);
            i++;
        }

        return engines;
    }


    private string getFuelLevel(GameObject vehicle)
    {
        try
        {
            FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
            return tank.totalFuel.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }
    }

    private string getFuelBurnRate(GameObject vehicle)
    {

        try
        {
            FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
            return tank.fuelDrain.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }

    }

    private string getFuelDensity(GameObject vehicle)
    {
        try
        {
            FuelTank tank = vehicle.GetComponentInChildren<FuelTank>();
            return tank.fuelDensity.ToString();
        }
        catch (Exception ex)
        {
            return "Unavailable";
        }

    }

    public enum Vehicle { FA26B, AV42C }
    public Vehicle vehicle = Vehicle.AV42C;
    public string pilotName = "neb";
    private void LoadLevel()
    {
        Debug.Log("Loading Scene");
        VTMapManager.nextLaunchMode = VTMapManager.MapLaunchModes.Scenario;
        LoadingSceneController.LoadScene(7);

        //yield return new WaitForSeconds(5);
        //After here we should be in the loader scene

        PilotSaveManager.current = PilotSaveManager.pilots[pilotName];
        Debug.Log("Going though All built in campaigns");
        if (VTResources.GetBuiltInCampaigns() != null)
        {
            foreach (VTCampaignInfo info in VTResources.GetBuiltInCampaigns())
            {

                if (vehicle == Vehicle.AV42C && info.campaignID == "av42cQuickFlight")
                {
                    Debug.Log("Setting Campaign");
                    PilotSaveManager.currentCampaign = info.ToIngameCampaign();
                    Debug.Log("Setting Vehicle");
                    PilotSaveManager.currentVehicle = VTResources.GetPlayerVehicle(info.vehicle);
                    break;
                }

                if (vehicle == Vehicle.FA26B && info.campaignID == "fa26bFreeFlight")
                {
                    Debug.Log("Setting Campaign");
                    PilotSaveManager.currentCampaign = info.ToIngameCampaign();
                    Debug.Log("Setting Vehicle");
                    PilotSaveManager.currentVehicle = VTResources.GetPlayerVehicle(info.vehicle);
                    break;
                }
            }
        }
        else
            Debug.Log("Campaigns are null");

        Debug.Log("Going though All missions in that campaign");
        foreach (CampaignScenario cs in PilotSaveManager.currentCampaign.missions)
        {
            Debug.Log("CampaignScenario == " + cs.scenarioID);
            if (cs.scenarioID == "freeFlight" || cs.scenarioID == "Free Flight")
            {
                Debug.Log("Setting Scenario");
                PilotSaveManager.currentScenario = cs;
                break;
            }
        }

        VTScenario.currentScenarioInfo = VTResources.GetScenario(PilotSaveManager.currentScenario.scenarioID, PilotSaveManager.currentCampaign);

        Debug.Log(string.Format("Loading into game, Pilot:{3}, Campaign:{0}, Scenario:{1}, Vehicle:{2}",
            PilotSaveManager.currentCampaign.campaignName, PilotSaveManager.currentScenario.scenarioName,
            PilotSaveManager.currentVehicle.vehicleName, pilotName));

        LoadingSceneController.instance.PlayerReady(); //<< Auto Ready

        while (SceneManager.GetActiveScene().buildIndex != 7)
        {
            //Pausing this method till the loader scene is unloaded
            //yield return null;
        }

    }

}