using ExtensionMethods;
using System.Reflection;


namespace TacViewDataLogger
{
    public class ACMI
    {
        public string acmi21Header()
        {
            return $@"FileType=text/acmi/tacview
FileVersion=2.1
0,DataRecorder={Globals.projectName} {Globals.projectVersion} by {Globals.projectAuthor}
0,DataSource={TacViewDataLogger.dataSource}";
        }

        public string recordingInfo(string author = null, string title = null, string category = null, string briefing = null, string debriefing = null, string comments = null)
        {

            string output = "";
            if (author != null)
            {
                output += $"0,Author={author}";
            }
            if (title != null)
            {
                output += $"0,Title={title}";
            }
            if (category != null)
            {
                output += $"0,Category={category}";
            }
            if (briefing != null)
            {
                output += $"0,Briefing={briefing}";
            }
            if (debriefing != null)
            {
                output += $"0,Debriefing={debriefing}";
            }
            if (comments != null)
            {
                output += $"0,Comments={comments}";
            }

            return output;

        }

        public string ACMIEvent(string eventType, string message = null, string gameObject = null)
        {
            if (gameObject != null)
            {
                if (message != null)
                {
                    return $"0,Event={eventType}|{gameObject}|{message}";
                }
                else
                {
                    return $"0,Event={eventType}|{gameObject}";
                }

            }
            else
            {
                if (message != null)
                {
                    return $"0,Event={eventType}|{message}";
                }
                else
                {
                    return $"0,Event={eventType}";
                }
            }

        }
    }

    public class ACMIDataEntry
    {
        public string objectId { get; set; }

        private string _longitude;
        public string longitude
        {
            get { return "Longitude=" + _longitude; }
            set { _longitude = value; }
        }

        private string _latitude;
        public string latitude
        {
            get { return "Latitude=" + _latitude; }
            set { _latitude = value; }
        }

        private string _altitude;
        public string altitude
        {
            get { return "Altitude=" + _altitude; }
            set { _altitude = value; }
        }

        private string _locData;
        public string locData
        {
            get { return "T=" + _locData; }
            set { _locData = value; }
        }


        private string _name;
        public string name
        {
            get { return "Name=" + _name; }
            set { _name = value; }
        }

        private string _parent;
        public string parent
        {
            get { return "Parent=" + _parent; }
            set { _parent = value; }
        }

        private string _longName;
        public string longName
        {
            get { return "LongName=" + _longName; }
            set { _longName = value; }
        }

        private string _fullName;
        public string fullName
        {
            get { return "FullName=" + _fullName; }
            set { _fullName = value; }
        }

        private string _callSign;
        public string callSign
        {
            get { return "CallSign=" + _callSign; }
            set { _callSign = value; }
        }

        private string _registration;
        public string registration
        {
            get { return "Registration=" + _registration; }
            set { _registration = value; }
        }

        private string _squawk;
        public string squawk
        {
            get { return "Squawk=" + _squawk; }
            set { _squawk = value; }
        }

        private string _pilot;
        public string pilot
        {
            get { return "Pilot=" + _pilot; }
            set { _pilot = value; }
        }

        private string _group;
        public string group
        {
            get { return "Group=" + _group; }
            set { _group = value; }
        }

        private string _country;
        public string country
        {
            get { return "Country=" + _country; }
            set { _country = value; }
        }

        private string _coalition;
        public string coalition
        {
            get { return "Coalition=" + _coalition; }
            set { _coalition = value; }
        }

        private string _color;
        public string color
        {
            get { return "Color=" + _color; }
            set { _color = value; }
        }

        private string _shape;
        public string shape
        {
            get { return "Shape=" + _shape; }
            set { _shape = value; }
        }

        private string _debug;
        public string debug
        {
            get { return "Debug=" + _debug; }
            set { _debug = value; }
        }

        private string _label;
        public string label
        {
            get { return "Label=" + _label; }
            set { _label = value; }
        }

        private string _focusedTarget;
        public string focusedTarget
        {
            get { return "FocusedTarget=" + _focusedTarget; }
            set { _focusedTarget = value; }
        }

        private string _lockedTarget;
        public string lockedTarget
        {
            get { return "LockedTarget=" + _lockedTarget; }
            set { _lockedTarget = value; }
        }

        private string _importance;
        public string importance
        {
            get { return "Importance=" + _importance; }
            set { _importance = value; }
        }

        private string _slot;
        public string slot
        {
            get { return "Slot=" + _slot; }
            set { _slot = value; }
        }

        private string _disabled;
        public string disabled
        {
            get { return "Disabled=" + _disabled; }
            set { _disabled = value; }
        }

        private string _length;
        public string length
        {
            get { return "Length=" + _length; }
            set { _length = value; }
        }

        private string _width;
        public string width
        {
            get { return "Width=" + _width; }
            set { _width = value; }
        }

        private string _height;
        public string height
        {
            get { return "Height=" + _height; }
            set { _height = value; }
        }

        private string _radius;
        public string radius
        {
            get { return "Radius=" + _radius; }
            set { _radius = value; }
        }

        private string _ias;
        public string ias
        {
            get { return "IAS=" + _ias; }
            set { _ias = value; }
        }

        private string _cas;
        public string cas
        {
            get { return "CAS=" + _cas; }
            set { _cas = value; }
        }

        private string _tas;
        public string tas
        {
            get { return "TAS=" + _tas; }
            set { _tas = value; }
        }

        private string _mach;
        public string mach
        {
            get { return "Mach=" + _mach; }
            set { _mach = value; }
        }

        private string _aoa;
        public string aoa
        {
            get { return "AOA=" + _aoa; }
            set { _aoa = value; }
        }

        private string _agl;
        public string agl
        {
            get { return "AGL=" + _agl; }
            set { _agl = value; }
        }

        private string _hdg;
        public string hdg
        {
            get { return "HDG=" + _hdg; }
            set { _hdg = value; }
        }

        private string _hdm;
        public string hdm
        {
            get { return "HDM=" + _hdm; }
            set { _hdm = value; }
        }

        private string _throttle;
        public string throttle
        {
            get { return "Throttle=" + _throttle; }
            set { _throttle = value; }
        }

        private string _afterburner;
        public string afterburner
        {
            get { return "Afterburner=" + _afterburner; }
            set { _afterburner = value; }
        }

        private string _airBrakes;
        public string airBrakes
        {
            get { return "AirBrakes=" + _airBrakes; }
            set { _airBrakes = value; }
        }

        private string _flaps;
        public string flaps
        {
            get { return "Flaps=" + _flaps; }
            set { _flaps = value; }
        }

        private string _landingGear;
        public string landingGear
        {
            get { return "LandingGear=" + _landingGear; }
            set { _landingGear = value; }
        }

        private string _landingGearHandle;
        public string landingGearHandle
        {
            get { return "LandingGearHandle=" + _landingGearHandle; }
            set { _landingGearHandle = value; }
        }

        private string _tailhook;
        public string tailhook
        {
            get { return "Tailhook=" + _tailhook; }
            set { _tailhook = value; }
        }

        private string _parachute;
        public string parachute
        {
            get { return "Parachute=" + _parachute; }
            set { _parachute = value; }
        }

        private string _dragChute;
        public string dragChute
        {
            get { return "DragChute=" + _dragChute; }
            set { _dragChute = value; }
        }

        private string _fuelWeight;
        public string fuelWeight
        {
            get { return "FuelWeight=" + _fuelWeight; }
            set { _fuelWeight = value; }
        }

        private string _fuelVolume;
        public string fuelVolume
        {
            get { return "FuelVolume=" + _fuelVolume; }
            set { _fuelVolume = value; }
        }

        private string _fuelFlowWeight;
        public string fuelFlowWeight
        {
            get { return "FuelFlowWeight=" + _fuelFlowWeight; }
            set { _fuelFlowWeight = value; }
        }

        private string _fuelFlowVolume;
        public string fuelFlowVolume
        {
            get { return "FuelFlowVolume=" + _fuelFlowVolume; }
            set { _fuelFlowVolume = value; }
        }

        private string _radarMode;
        public string radarMode
        {
            get { return "RadarMode=" + _radarMode; }
            set { _radarMode = value; }
        }

        private string _radarAzimuth;
        public string radarAzimuth
        {
            get { return "RadarAzimuth=" + _radarAzimuth; }
            set { _radarAzimuth = value; }
        }

        private string _radarElevation;
        public string radarElevation
        {
            get { return "RadarElevation=" + _radarElevation; }
            set { _radarElevation = value; }
        }

        private string _radarRange;
        public string radarRange
        {
            get { return "RadarRange=" + _radarRange; }
            set { _radarRange = value; }
        }

        private string _radarHorizontalBeamwidth;
        public string radarHorizontalBeamwidth
        {
            get { return "RadarHorizontalBeamwidth=" + _radarHorizontalBeamwidth; }
            set { _radarHorizontalBeamwidth = value; }
        }

        private string _radarVerticalBeamwidth;
        public string radarVerticalBeamwidth
        {
            get { return "RadarVerticalBeamwidth=" + _radarVerticalBeamwidth; }
            set { _radarVerticalBeamwidth = value; }
        }

        private string _lockedTargetMode;
        public string lockedTargetMode
        {
            get { return "LockedTargetMode=" + _lockedTargetMode; }
            set { _lockedTargetMode = value; }
        }

        private string _lockedTargetAzimuth;
        public string lockedTargetAzimuth
        {
            get { return "LockedTargetAzimuth=" + _lockedTargetAzimuth; }
            set { _lockedTargetAzimuth = value; }
        }

        private string _lockedTargetElevation;
        public string lockedTargetElevation
        {
            get { return "LockedTargetElevation=" + _lockedTargetElevation; }
            set { _lockedTargetElevation = value; }
        }

        private string _lockedTargetRange;
        public string lockedTargetRange
        {
            get { return "LockedTargetRange=" + _lockedTargetRange; }
            set { _lockedTargetRange = value; }
        }

        private string _engagementMode;
        public string engagementMode
        {
            get { return "EngagementMode=" + _engagementMode; }
            set { _engagementMode = value; }
        }

        private string _engagementMode2;
        public string engagementMode2
        {
            get { return "EngagementMode2=" + _engagementMode2; }
            set { _engagementMode2 = value; }
        }

        private string _engagementRange;
        public string engagementRange
        {
            get { return "EngagementRange=" + _engagementRange; }
            set { _engagementRange = value; }
        }

        private string _engagementRange2;
        public string engagementRange2
        {
            get { return "EngagementRange2=" + _engagementRange2; }
            set { _engagementRange2 = value; }
        }

        private string _verticalEngagementRange;
        public string verticalEngagementRange
        {
            get { return "VerticalEngagementRange=" + _verticalEngagementRange; }
            set { _verticalEngagementRange = value; }
        }

        private string _verticalEngagementRange2;
        public string verticalEngagementRange2
        {
            get { return "VerticalEngagementRange2=" + _verticalEngagementRange2; }
            set { _verticalEngagementRange2 = value; }
        }

        private string _rollControlInput;
        public string rollControlInput
        {
            get { return "RollControlInput=" + _rollControlInput; }
            set { _rollControlInput = value; }
        }

        private string _pitchControlInput;
        public string pitchControlInput
        {
            get { return "PitchControlInput=" + _pitchControlInput; }
            set { _pitchControlInput = value; }
        }

        private string _yawControlInput;
        public string yawControlInput
        {
            get { return "YawControlInput=" + _yawControlInput; }
            set { _yawControlInput = value; }
        }

        private string _rollControlPosition;
        public string rollControlPosition
        {
            get { return "RollControlPosition=" + _rollControlPosition; }
            set { _rollControlPosition = value; }
        }

        private string _pitchControlPosition;
        public string pitchControlPosition
        {
            get { return "PitchControlPosition=" + _pitchControlPosition; }
            set { _pitchControlPosition = value; }
        }

        private string _yawControlPosition;
        public string yawControlPosition
        {
            get { return "YawControlPosition=" + _yawControlPosition; }
            set { _yawControlPosition = value; }
        }

        private string _rollTrimTab;
        public string rollTrimTab
        {
            get { return "RollTrimTab=" + _rollTrimTab; }
            set { _rollTrimTab = value; }
        }

        private string _pitchTrimTab;
        public string pitchTrimTab
        {
            get { return "PitchTrimTab=" + _pitchTrimTab; }
            set { _pitchTrimTab = value; }
        }

        private string _yawTrimTab;
        public string yawTrimTab
        {
            get { return "YawTrimTab=" + _yawTrimTab; }
            set { _yawTrimTab = value; }
        }

        private string _aileronLeft;
        public string aileronLeft
        {
            get { return "AileronLeft=" + _aileronLeft; }
            set { _aileronLeft = value; }
        }

        private string _aileronRight;
        public string aileronRight
        {
            get { return "AileronRight=" + _aileronRight; }
            set { _aileronRight = value; }
        }

        private string _elevator;
        public string elevator
        {
            get { return "Elevator=" + _elevator; }
            set { _elevator = value; }
        }

        private string _rudder;
        public string rudder
        {
            get { return "Rudder=" + _rudder; }
            set { _rudder = value; }
        }

        private string _visible;
        public string visible
        {
            get { return "Visible=" + _visible; }
            set { _visible = value; }
        }

        private string _pilotHeadRoll;
        public string pilotHeadRoll
        {
            get { return "PilotHeadRoll=" + _pilotHeadRoll; }
            set { _pilotHeadRoll = value; }
        }

        private string _pilotHeadPitch;
        public string pilotHeadPitch
        {
            get { return "PilotHeadPitch=" + _pilotHeadPitch; }
            set { _pilotHeadPitch = value; }
        }

        private string _pilotHeadYaw;
        public string pilotHeadYaw
        {
            get { return "PilotHeadYaw=" + _pilotHeadYaw; }
            set { _pilotHeadYaw = value; }
        }


        public string _objectClass { get; set; }
        public string _objectAttributes { get; set; }
        public string _basicTypes { get; set; }
        public string _specificTypes { get; set; }

        public string objectType()
        {
            string type = "";

            if (_objectAttributes != null)
            {
                if (type != "")
                {
                    type = $"{type}+{_objectAttributes}";
                }
                else
                {
                    type = $"{_objectAttributes}";
                }
            }
            if (_objectClass != null)
            {
                if (type != "")
                {
                    type = $"{type}+{_objectClass}";
                }
                else
                {
                    type = $"{_objectClass}";
                }
            }
            if (_basicTypes != null)
            {
                if (type != "")
                {
                    type = $"{type}+{_basicTypes}";
                }
                else
                {
                    type = $"{_basicTypes}";
                }
            }
            else if (_specificTypes != null)
            {
                if (type != "")
                {
                    type = $"{type}+{_specificTypes}";
                }
                else
                {
                    type = $"{_specificTypes}";
                }
            }
            return type;
        }

        public string BuildString()
        {
            string outputString = "";

            if (!objectId.EndsWith("="))
            {
                outputString += $",{objectId}";
            }
            if (!longitude.EndsWith("="))
            {
                outputString += $",{longitude}";
            }
            if (!latitude.EndsWith("="))
            {
                outputString += $",{latitude}";
            }
            if (!altitude.EndsWith("="))
            {
                outputString += $",{altitude}";
            }
            if (!locData.EndsWith("="))
            {
                outputString += $",{locData}";
            }
            if (!name.EndsWith("="))
            {
                outputString += $",{name}";
            }
            if (!parent.EndsWith("="))
            {
                outputString += $",{parent}";
            }
            if (!longName.EndsWith("="))
            {
                outputString += $",{longName}";
            }
            if (!fullName.EndsWith("="))
            {
                outputString += $",{fullName}";
            }
            if (!callSign.EndsWith("="))
            {
                outputString += $",{callSign}";
            }
            if (!registration.EndsWith("="))
            {
                outputString += $",{registration}";
            }
            if (!squawk.EndsWith("="))
            {
                outputString += $",{squawk}";
            }
            if (!pilot.EndsWith("="))
            {
                outputString += $",{pilot}";
            }
            if (!group.EndsWith("="))
            {
                outputString += $",{group}";
            }
            if (!country.EndsWith("="))
            {
                outputString += $",{country}";
            }
            if (!coalition.EndsWith("="))
            {
                outputString += $",{coalition}";
            }
            if (!color.EndsWith("="))
            {
                outputString += $",{color}";
            }
            if (!shape.EndsWith("="))
            {
                outputString += $",{shape}";
            }
            if (!debug.EndsWith("="))
            {
                outputString += $",{debug}";
            }
            if (!label.EndsWith("="))
            {
                outputString += $",{label}";
            }
            if (!focusedTarget.EndsWith("="))
            {
                outputString += $",{focusedTarget}";
            }
            if (!lockedTarget.EndsWith("="))
            {
                outputString += $",{lockedTarget}";
            }
            if (!importance.EndsWith("="))
            {
                outputString += $",{importance}";
            }
            if (!slot.EndsWith("="))
            {
                outputString += $",{slot}";
            }
            if (!disabled.EndsWith("="))
            {
                outputString += $",{disabled}";
            }
            if (!length.EndsWith("="))
            {
                outputString += $",{length}";
            }
            if (!width.EndsWith("="))
            {
                outputString += $",{width}";
            }
            if (!height.EndsWith("="))
            {
                outputString += $",{height}";
            }
            if (!radius.EndsWith("="))
            {
                outputString += $",{radius}";
            }
            if (!ias.EndsWith("="))
            {
                outputString += $",{ias}";
            }
            if (!cas.EndsWith("="))
            {
                outputString += $",{cas}";
            }
            if (!tas.EndsWith("="))
            {
                outputString += $",{tas}";
            }
            if (!mach.EndsWith("="))
            {
                outputString += $",{mach}";
            }
            if (!aoa.EndsWith("="))
            {
                outputString += $",{aoa}";
            }
            if (!agl.EndsWith("="))
            {
                outputString += $",{agl}";
            }
            if (!hdg.EndsWith("="))
            {
                outputString += $",{hdg}";
            }
            if (!hdm.EndsWith("="))
            {
                outputString += $",{hdm}";
            }
            if (!throttle.EndsWith("="))
            {
                outputString += $",{throttle}";
            }
            if (!afterburner.EndsWith("="))
            {
                outputString += $",{afterburner}";
            }
            if (!airBrakes.EndsWith("="))
            {
                outputString += $",{airBrakes}";
            }
            if (!flaps.EndsWith("="))
            {
                outputString += $",{flaps}";
            }
            if (!landingGear.EndsWith("="))
            {
                outputString += $",{landingGear}";
            }
            if (!landingGearHandle.EndsWith("="))
            {
                outputString += $",{landingGearHandle}";
            }
            if (!tailhook.EndsWith("="))
            {
                outputString += $",{tailhook}";
            }
            if (!parachute.EndsWith("="))
            {
                outputString += $",{parachute}";
            }
            if (!dragChute.EndsWith("="))
            {
                outputString += $",{dragChute}";
            }
            if (!fuelWeight.EndsWith("="))
            {
                outputString += $",{fuelWeight}";
            }
            if (!fuelVolume.EndsWith("="))
            {
                outputString += $",{fuelVolume}";
            }
            if (!fuelFlowWeight.EndsWith("="))
            {
                outputString += $",{fuelFlowWeight}";
            }
            if (!fuelFlowVolume.EndsWith("="))
            {
                outputString += $",{fuelFlowVolume}";
            }
            if (!radarMode.EndsWith("="))
            {
                outputString += $",{radarMode}";
            }
            if (!radarAzimuth.EndsWith("="))
            {
                outputString += $",{radarAzimuth}";
            }
            if (!radarElevation.EndsWith("="))
            {
                outputString += $",{radarElevation}";
            }
            if (!radarRange.EndsWith("="))
            {
                outputString += $",{radarRange}";
            }
            if (!radarHorizontalBeamwidth.EndsWith("="))
            {
                outputString += $",{radarHorizontalBeamwidth}";
            }
            if (!radarVerticalBeamwidth.EndsWith("="))
            {
                outputString += $",{radarVerticalBeamwidth}";
            }
            if (!lockedTargetMode.EndsWith("="))
            {
                outputString += $",{lockedTargetMode}";
            }
            if (!lockedTargetAzimuth.EndsWith("="))
            {
                outputString += $",{lockedTargetAzimuth}";
            }
            if (!lockedTargetElevation.EndsWith("="))
            {
                outputString += $",{lockedTargetElevation}";
            }
            if (!lockedTargetRange.EndsWith("="))
            {
                outputString += $",{lockedTargetRange}";
            }
            if (!engagementMode.EndsWith("="))
            {
                outputString += $",{engagementMode}";
            }
            if (!engagementMode2.EndsWith("="))
            {
                outputString += $",{engagementMode2}";
            }
            if (!engagementRange.EndsWith("="))
            {
                outputString += $",{engagementRange}";
            }
            if (!engagementRange2.EndsWith("="))
            {
                outputString += $",{engagementRange2}";
            }
            if (!verticalEngagementRange.EndsWith("="))
            {
                outputString += $",{verticalEngagementRange}";
            }
            if (!verticalEngagementRange2.EndsWith("="))
            {
                outputString += $",{verticalEngagementRange2}";
            }
            if (!rollControlInput.EndsWith("="))
            {
                outputString += $",{rollControlInput}";
            }
            if (!pitchControlInput.EndsWith("="))
            {
                outputString += $",{pitchControlInput}";
            }
            if (!yawControlInput.EndsWith("="))
            {
                outputString += $",{yawControlInput}";
            }
            if (!rollControlPosition.EndsWith("="))
            {
                outputString += $",{rollControlPosition}";
            }
            if (!pitchControlPosition.EndsWith("="))
            {
                outputString += $",{pitchControlPosition}";
            }
            if (!yawControlPosition.EndsWith("="))
            {
                outputString += $",{yawControlPosition}";
            }
            if (!rollTrimTab.EndsWith("="))
            {
                outputString += $",{rollTrimTab}";
            }
            if (!pitchTrimTab.EndsWith("="))
            {
                outputString += $",{pitchTrimTab}";
            }
            if (!yawTrimTab.EndsWith("="))
            {
                outputString += $",{yawTrimTab}";
            }
            if (!aileronLeft.EndsWith("="))
            {
                outputString += $",{aileronLeft}";
            }
            if (!aileronRight.EndsWith("="))
            {
                outputString += $",{aileronRight}";
            }
            if (!elevator.EndsWith("="))
            {
                outputString += $",{elevator}";
            }
            if (!rudder.EndsWith("="))
            {
                outputString += $",{rudder}";
            }
            if (!visible.EndsWith("="))
            {
                outputString += $",{visible}";
            }
            if (!pilotHeadRoll.EndsWith("="))
            {
                outputString += $",{pilotHeadRoll}";
            }
            if (!pilotHeadPitch.EndsWith("="))
            {
                outputString += $",{pilotHeadPitch}";
            }
            if (!pilotHeadYaw.EndsWith("="))
            {
                outputString += $",{pilotHeadYaw}";
            }
            if (this.objectType() != "")
            {
                outputString += $",Type={this.objectType()}";
            }

            return outputString;
        }

        public string shortString()
        {
            string outputString = "";

            if (!objectId.EndsWith("="))
            {
                outputString += $",{objectId}";
            }

            if (!locData.EndsWith("="))
            {
                outputString += $",{locData}";
            }

            if (!ias.EndsWith("="))
            {
                outputString += $",{ias}";
            }
            if (!cas.EndsWith("="))
            {
                outputString += $",{cas}";
            }
            if (!tas.EndsWith("="))
            {
                outputString += $",{tas}";
            }
            if (!mach.EndsWith("="))
            {
                outputString += $",{mach}";
            }
            if (!aoa.EndsWith("="))
            {
                outputString += $",{aoa}";
            }
            if (!agl.EndsWith("="))
            {
                outputString += $",{agl}";
            }
            if (!hdg.EndsWith("="))
            {
                outputString += $",{hdg}";
            }
            //if (!hdm.EndsWith("="))
            //{
            //    outputString += $",{hdm}";
            //}
            //if (!throttle.EndsWith("="))
            //{
            //    outputString += $",{throttle}";
            //}
            if (!debug.EndsWith("="))
            {
                outputString += $",{debug}";
            }
            if (!label.EndsWith("="))
            {
                outputString += $",{label}";
            }
            if (!focusedTarget.EndsWith("="))
            {
                outputString += $",{focusedTarget}";
            }
            if (!lockedTarget.EndsWith("="))
            {
                outputString += $",{lockedTarget}";
            }
            if (!disabled.EndsWith("="))
            {
                outputString += $",{disabled}";
            }
            if (!radarMode.EndsWith("="))
            {
                outputString += $",{radarMode}";
            }
            if (!radarAzimuth.EndsWith("="))
            {
                outputString += $",{radarAzimuth}";
            }
            if (!radarElevation.EndsWith("="))
            {
                outputString += $",{radarElevation}";
            }
            if (!radarRange.EndsWith("="))
            {
                outputString += $",{radarRange}";
            }
            if (!radarHorizontalBeamwidth.EndsWith("="))
            {
                outputString += $",{radarHorizontalBeamwidth}";
            }
            if (!radarVerticalBeamwidth.EndsWith("="))
            {
                outputString += $",{radarVerticalBeamwidth}";
            }
            if (!lockedTargetMode.EndsWith("="))
            {
                outputString += $",{lockedTargetMode}";
            }
            if (!lockedTargetAzimuth.EndsWith("="))
            {
                outputString += $",{lockedTargetAzimuth}";
            }
            if (!lockedTargetElevation.EndsWith("="))
            {
                outputString += $",{lockedTargetElevation}";
            }
            if (!lockedTargetRange.EndsWith("="))
            {
                outputString += $",{lockedTargetRange}";
            }
            if (!engagementMode.EndsWith("="))
            {
                outputString += $",{engagementMode}";
            }
            if (!engagementMode2.EndsWith("="))
            {
                outputString += $",{engagementMode2}";
            }
            if (!visible.EndsWith("="))
            {
                outputString += $",{visible}";
            }
            //if (!longitude.EndsWith("="))
            //{
            //    outputString += $",{longitude}";
            //}
            //if (!latitude.EndsWith("="))
            //{
            //    outputString += $",{latitude}";
            //}
            //if (!altitude.EndsWith("="))
            //{
            //    outputString += $",{altitude}";
            //}
            //if (!name.EndsWith("="))
            //{
            //    outputString += $",{name}";
            //}
            //if (!parent.EndsWith("="))
            //{
            //    outputString += $",{parent}";
            //}
            //if (!longName.EndsWith("="))
            //{
            //    outputString += $",{longName}";
            //}
            //if (!fullName.EndsWith("="))
            //{
            //    outputString += $",{fullName}";
            //}
            //if (!callSign.EndsWith("="))
            //{
            //    outputString += $",{callSign}";
            //}
            //if (!registration.EndsWith("="))
            //{
            //    outputString += $",{registration}";
            //}
            //if (!squawk.EndsWith("="))
            //{
            //    outputString += $",{squawk}";
            //}
            //if (!pilot.EndsWith("="))
            //{
            //    outputString += $",{pilot}";
            //}
            //if (!group.EndsWith("="))
            //{
            //    outputString += $",{group}";
            //}
            //if (!country.EndsWith("="))
            //{
            //    outputString += $",{country}";
            //}
            //if (!coalition.EndsWith("="))
            //{
            //    outputString += $",{coalition}";
            //}
            //if (!color.EndsWith("="))
            //{
            //    outputString += $",{color}";
            //}
            //if (!shape.EndsWith("="))
            //{
            //    outputString += $",{shape}";
            //}

            //if (!importance.EndsWith("="))
            //{
            //    outputString += $",{importance}";
            //}
            //if (!slot.EndsWith("="))
            //{
            //    outputString += $",{slot}";
            //}

            //if (!length.EndsWith("="))
            //{
            //    outputString += $",{length}";
            //}
            //if (!width.EndsWith("="))
            //{
            //    outputString += $",{width}";
            //}
            //if (!height.EndsWith("="))
            //{
            //    outputString += $",{height}";
            //}
            //if (!radius.EndsWith("="))
            //{
            //    outputString += $",{radius}";
            //}





            //if (!afterburner.EndsWith("="))
            //{
            //    outputString += $",{afterburner}";
            //}
            //if (!airBrakes.EndsWith("="))
            //{
            //    outputString += $",{airBrakes}";
            //}
            //if (!flaps.EndsWith("="))
            //{
            //    outputString += $",{flaps}";
            //}
            //if (!landingGear.EndsWith("="))
            //{
            //    outputString += $",{landingGear}";
            //}
            //if (!landingGearHandle.EndsWith("="))
            //{
            //    outputString += $",{landingGearHandle}";
            //}
            //if (!tailhook.EndsWith("="))
            //{
            //    outputString += $",{tailhook}";
            //}
            //if (!parachute.EndsWith("="))
            //{
            //    outputString += $",{parachute}";
            //}
            //if (!dragChute.EndsWith("="))
            //{
            //    outputString += $",{dragChute}";
            //}
            //if (!fuelWeight.EndsWith("="))
            //{
            //    outputString += $",{fuelWeight}";
            //}
            //if (!fuelVolume.EndsWith("="))
            //{
            //    outputString += $",{fuelVolume}";
            //}
            //if (!fuelFlowWeight.EndsWith("="))
            //{
            //    outputString += $",{fuelFlowWeight}";
            //}
            //if (!fuelFlowVolume.EndsWith("="))
            //{
            //    outputString += $",{fuelFlowVolume}";
            //}

            //if (!engagementRange.EndsWith("="))
            //{
            //    outputString += $",{engagementRange}";
            //}
            //if (!engagementRange2.EndsWith("="))
            //{
            //    outputString += $",{engagementRange2}";
            //}
            //if (!verticalEngagementRange.EndsWith("="))
            //{
            //    outputString += $",{verticalEngagementRange}";
            //}
            //if (!verticalEngagementRange2.EndsWith("="))
            //{
            //    outputString += $",{verticalEngagementRange2}";
            //}
            //if (!rollControlInput.EndsWith("="))
            //{
            //    outputString += $",{rollControlInput}";
            //}
            //if (!pitchControlInput.EndsWith("="))
            //{
            //    outputString += $",{pitchControlInput}";
            //}
            //if (!yawControlInput.EndsWith("="))
            //{
            //    outputString += $",{yawControlInput}";
            //}
            //if (!rollControlPosition.EndsWith("="))
            //{
            //    outputString += $",{rollControlPosition}";
            //}
            //if (!pitchControlPosition.EndsWith("="))
            //{
            //    outputString += $",{pitchControlPosition}";
            //}
            //if (!yawControlPosition.EndsWith("="))
            //{
            //    outputString += $",{yawControlPosition}";
            //}
            //if (!rollTrimTab.EndsWith("="))
            //{
            //    outputString += $",{rollTrimTab}";
            //}
            //if (!pitchTrimTab.EndsWith("="))
            //{
            //    outputString += $",{pitchTrimTab}";
            //}
            //if (!yawTrimTab.EndsWith("="))
            //{
            //    outputString += $",{yawTrimTab}";
            //}
            //if (!aileronLeft.EndsWith("="))
            //{
            //    outputString += $",{aileronLeft}";
            //}
            //if (!aileronRight.EndsWith("="))
            //{
            //    outputString += $",{aileronRight}";
            //}
            //if (!elevator.EndsWith("="))
            //{
            //    outputString += $",{elevator}";
            //}
            //if (!rudder.EndsWith("="))
            //{
            //    outputString += $",{rudder}";
            //}

            //if (!pilotHeadRoll.EndsWith("="))
            //{
            //    outputString += $",{pilotHeadRoll}";
            //}
            //if (!pilotHeadPitch.EndsWith("="))
            //{
            //    outputString += $",{pilotHeadPitch}";
            //}
            //if (!pilotHeadYaw.EndsWith("="))
            //{
            //    outputString += $",{pilotHeadYaw}";
            //}

            return outputString;
        }

        public string ACMIString(ACMIDataEntry oldEntry = null)
        {

            string outputString = "";
            if (oldEntry != null)
            {
                if (oldEntry.BuildString() != this.BuildString())
                {
                    outputString = shortString();
                }

            }
            else
            {
                outputString = BuildString();
            }
            if (outputString.StartsWith(","))
            {
                outputString = outputString.Substring(1);
            }

            return outputString;

        }

        public string ACMIStringOld(ACMIDataEntry oldEntry = null)
        {
            // This is no longer used currently. The comparison function is too expensive due to the reflection/recursion to be called on every actor, on every scene.

            string outputString = "";
            if (oldEntry != null)
            {
                string data = "";
                var Diffs = this.DetailedCompare<ACMIDataEntry>(oldEntry);
                foreach (var entry in Diffs)
                {
                    data += "," + entry.valB;
                }
                if (data != "")
                {
                    outputString = $"{objectId}{data}";
                }

            }
            else
            {
                bool moreProps = false;
                PropertyInfo[] fi = this.GetType().GetProperties();
                foreach (PropertyInfo f in fi)
                {

                    if (f.GetValue(this) != null)
                    {
                        if (f.ToString() == "objectId")
                        {
                            moreProps = true;
                        }
                        if (!f.ToString().Contains(" _"))
                        {
                            if (!f.GetValue(this).ToString().EndsWith("="))
                            {
                                outputString += "," + f.GetValue(this);
                            }
                        }

                    }

                }
                if (this.objectType() != "")
                {
                    moreProps = true;
                    outputString += $",Type={this.objectType()}";
                }

                if (!moreProps)
                {
                    outputString = "";
                }

            }
            if (outputString != "")
            {
                if (outputString.StartsWith(","))
                {
                    return outputString.Substring(1);
                }
                else
                {
                    return outputString;
                }

            }
            else
            {
                return outputString;
            }


        }
    }


}
