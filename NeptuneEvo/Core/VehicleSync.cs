using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Redage.SDK;

//Disapproved by god himself

//Just use the API functions, you have nothing else to worry about

//Things to note
//More things like vehicle mods will be added in the next version

/* API FUNCTIONS:
public static void SetVehicleWindowState(Vehicle veh, WindowID window, WindowState state)
public static WindowState GetVehicleWindowState(Vehicle veh, WindowID window)
public static void SetVehicleWheelState(Vehicle veh, WheelID wheel, WheelState state)
public static WheelState GetVehicleWheelState(Vehicle veh, WheelID wheel)
public static void SetVehicleDirt(Vehicle veh, float dirt)
public static float GetVehicleDirt(Vehicle veh)
public static void SetDoorState(Vehicle veh, DoorID door, DoorState state)
public static DoorState GetDoorState(Vehicle veh, DoorID door)
public static void SetEngineState(Vehicle veh, bool status)
public static bool GetEngineState(Vehicle veh)
public static void SetLockStatus(Vehicle veh, bool status)
public static bool GetLockState(Vehicle veh)
*/

namespace NeptuneEvo.Core
{
    //Enums for ease of use
    public enum WindowID
    {
        WindowFrontRight,
        WindowFrontLeft,
        WindowRearRight,
        WindowRearLeft
    }

    public enum WindowState
    {
        WindowFixed,
        WindowDown,
        WindowBroken
    }

    public enum DoorID
    {
        DoorFrontLeft,
        DoorFrontRight,
        DoorRearLeft,
        DoorRearRight,
        DoorHood,
        DoorTrunk
    }

    public enum DoorState
    {
        DoorClosed,
        DoorOpen,
        DoorBroken,
    }

    public enum WheelID
    {
        Wheel0,
        Wheel1,
        Wheel2,
        Wheel3,
        Wheel4,
        Wheel5,
        Wheel6,
        Wheel7,
        Wheel8,
        Wheel9
    }

    public enum WheelState
    {
        WheelFixed,
        WheelBurst,
        WheelOnRim,
    }

    public class VehicleStreaming : Script
    {
        private static nLog Log = new nLog("VehicleStreaming");
        //This is the data object which will be synced to vehicles
        public class VehicleSyncData
        {
            public bool Locked { get; set; } = false;
            public bool Engine { get; set; } = false;
            public bool LeftIL { get; set; } = false;
            public bool RightIL { get; set; } = false;
            public float Dirt { get; set; } = 0.0f;

            //Doors 0-7 (0 = closed, 1 = open, 2 = broken) (This uses enums so don't worry about it)
            public List<int> Door { get; set; } = new List<int>(8) { 0, 0, 0, 0, 0, 0, 0, 0 };
        }
        private static Dictionary<NetHandle, VehicleSyncData> VehiclesSyncDatas = new Dictionary<NetHandle, VehicleSyncData>();

        [ServerEvent(Event.EntityDeleted)]
        public void Event_EntityDeleted(NetHandle entity)
        {
            try
            {
                if (NAPI.Entity.GetEntityType(entity) == EntityType.Vehicle && VehiclesSyncDatas.ContainsKey(entity)) VehiclesSyncDatas.Remove(entity);
            }
            catch (Exception e) { Log.Write("Event_EntityDeleted: " + e.Message); }
        }

        [ServerEvent(Event.PlayerExitVehicleAttempt)]
        public void VehStreamExitAttempt(Client player, Vehicle veh) {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();
            UpdateVehicleSyncData(veh, data);
            Trigger.ClientEvent(player, "VehStream_PlayerExitVehicleAttempt", veh, data.Engine);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void VehStreamEnter(Client player, Vehicle veh, sbyte seat) {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();
            UpdateVehicleSyncData(veh, data);
            Trigger.ClientEvent(player, "VehStream_PlayerEnterVehicle", veh, seat, data.Engine);
        }

        public static void SetDoorState(Vehicle veh, DoorID door, DoorState state)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Door[(int)door] = (int)state;
            UpdateVehicleSyncData(veh, data);
            Trigger.ClientEventInRange(veh.Position, 250, "VehStream_SetVehicleDoorStatus_Single", veh, (int)door, (int)state);
        }

        public static DoorState GetDoorState(Vehicle veh, DoorID door)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return (DoorState)data.Door[(int)door];
        }

        public static void SetEngineState(Vehicle veh, bool status)
        {
            NAPI.Vehicle.SetVehicleEngineStatus(veh, status);
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            data.Engine = status;
            data.RightIL = false;
            data.LeftIL = false;
            veh.SetSharedData("rightlight", false);
            veh.SetSharedData("leftlight", false);
            UpdateVehicleSyncData(veh, data);
            NAPI.Vehicle.SetVehicleEngineStatus(veh, status);
            Trigger.ClientEventInRange(veh.Position, 250, "VehStream_SetEngineStatus", veh, status, true, data.LeftIL, data.RightIL);
        }

        public static bool GetEngineState(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Engine;
        }

        public static void SetVehicleIndicatorLights(Vehicle veh, int light, bool state) {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            if(data.Engine) {
                if (light == 0) {
                    veh.SetSharedData("rightlight", state);
                    data.RightIL = state;
                } else {
                    veh.SetSharedData("leftlight", state);
                    data.LeftIL = state;
                }
                UpdateVehicleSyncData(veh, data);
                NAPI.ClientEvent.TriggerClientEventInDimension(veh.Dimension, "VehStream_SetVehicleIndicatorLights_Single", veh.Handle, light, state);
            }
        }

        public static bool GetVehicleIndicatorLights(Vehicle veh, bool light) {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            if (!light) return data.RightIL;
            else return data.LeftIL;
        }

        public static void SetLockStatus(Vehicle veh, bool status)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
                data = new VehicleSyncData();

            veh.SetSharedData("LOCKED", status);
            data.Locked = status;
            UpdateVehicleSyncData(veh, data);
            Trigger.ClientEventInRange(veh.Position, 250, "VehStream_SetLockStatus", veh, status);
        }

        public static bool GetLockState(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Locked;
        }

        [RemoteEvent("VehStream_RadioChange")]
        public void VehStreamRadioChange(Client client, Vehicle vehicle, short index)
        {
            try
            {
                if(client.Vehicle != vehicle) return;
                NAPI.Data.SetEntitySharedData(vehicle, "vehradio", index);
            }
            catch (Exception e) { Log.Write("VehStream_RadioChange: " + e.Message); }
        }

        [RemoteEvent("VehStream_RequestFixStreamIn")]
        public void VehicleFixStreamIn(Client player, Vehicle veh)
        {
            try
            {
                if (veh != null && NAPI.Entity.DoesEntityExist(veh))
                {
                    VehicleSyncData data = GetVehicleSyncData(veh);
                    if (data == default(VehicleSyncData)) data = new VehicleSyncData();
                    UpdateVehicleSyncData(veh, data);

                    List<object> vData = new List<object>()
                    {
                        veh.NumberPlate,
                        veh.PrimaryColor,
                        veh.SecondaryColor,
                    };
                    if (veh.HasData("ACCESS") && veh.GetData("ACCESS") == "PERSONAL")
                    {
                        vData.Add(VehicleManager.Vehicles[veh.NumberPlate].Components);
                        vData.Add(VehicleManager.Vehicles[veh.NumberPlate].Dirt);
                    }
                    Trigger.ClientEvent(player, "VehStream_FixStreamIn", veh.Handle, JsonConvert.SerializeObject(vData));
                }
                return;
            }
            catch (Exception e) { Log.Write("VehStream_RequestFixStreamIn: " + e.Message, nLog.Type.Error); return; }
        }

        [RemoteEvent("VehStream_SetVehicleDirt")]
        public void SetVehicleDirtLevel(Client player, Vehicle vehicle, float dirt)
        {
            try
            {
                if (vehicle != null) SetVehicleDirt(vehicle, dirt);
            }
            catch (Exception e) { Log.Write("VehStream_SetVehicleDirt: " + e.Message, nLog.Type.Error); }
        }

        [RemoteEvent("VehStream_SetIndicatorLightsData")]
        public void VehStreamSetIndicatorLightsData(Client player, Vehicle veh, bool leftstate, bool rightstate) {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            if(data.Engine) {
                data.RightIL = rightstate;
                data.LeftIL = leftstate;
                veh.SetSharedData("rightlight", rightstate);
                veh.SetSharedData("leftlight", leftstate);
                UpdateVehicleSyncData(veh, data);
                Trigger.ClientEventInRange(veh.Position, 250, "VehStream_SetVehicleIndicatorLights", veh.Handle, leftstate, rightstate);
            }
        }

        [Command("setvehdirt")]
        public static void setvehdirt(Client player, float dirt)
        {
            if (!Group.CanUseCmd(player, "setvehdirt")) return;
            if (player.Vehicle != null)
            {
                SetVehicleDirt(player.Vehicle, dirt);
            }
        }

        public static void SetVehicleDirt(Vehicle veh, float dirt)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData)) data = new VehicleSyncData();
            data.Dirt = dirt;
            UpdateVehicleSyncData(veh, data);

            if (veh.HasData("ACCESS") && veh.GetData("ACCESS") == "PERSONAL")
            {
                if (!VehicleManager.Vehicles.ContainsKey(veh.NumberPlate)) return;

                VehicleManager.Vehicles[veh.NumberPlate].Dirt = dirt;
            }

            Trigger.ClientEventInDimension(veh.Dimension, "VehStream_SetVehicleDirtLevel", veh.Handle, dirt);
        }

        public static float GetVehicleDirt(Vehicle veh)
        {
            VehicleSyncData data = GetVehicleSyncData(veh);
            if (data == default(VehicleSyncData))
            {
                data = new VehicleSyncData();
                UpdateVehicleSyncData(veh, data);
            }
            return data.Dirt;
        }

        //Used internally only but publicly available in case any of you need it
        private static VehicleSyncData GetVehicleSyncData(Vehicle veh)
        {
            try
            {
                if (veh != null)
                {
                    if (NAPI.Entity.DoesEntityExist(veh))
                    {
                        if (VehiclesSyncDatas.ContainsKey(veh))
                            return VehiclesSyncDatas[veh];
                        else
                        {
                            VehiclesSyncDatas.Add(veh, new VehicleSyncData());
                            return VehiclesSyncDatas[veh];
                        }
                    }
                }
            }
            catch { };

            return default(VehicleSyncData); //null
        }

        //Used internally only but publicly available in case any of you need it
        public static bool UpdateVehicleSyncData(Vehicle veh, VehicleSyncData data)
        {
            try
            {
                if (veh != null)
                {
                    if (NAPI.Entity.DoesEntityExist(veh))
                    {
                        if (data != null)
                        {
                            if (VehiclesSyncDatas.ContainsKey(veh))
                                VehiclesSyncDatas[veh] = data;
                            else
                                VehiclesSyncDatas.Add(veh, data);
                            NAPI.Data.SetEntitySharedData(veh, "VehicleSyncData", JsonConvert.SerializeObject(data));
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception e) { Log.Write("UpdateVehicleSyncData: " + e.Message, nLog.Type.Error); return false; }
        }
    }
}
