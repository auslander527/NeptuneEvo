using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using GTANetworkAPI;
using NeptuneEvo.Core;
using Newtonsoft.Json;
using Redage.SDK;

namespace NeptuneEvo.Fractions
{
    class Configs : Script
    {
        private static nLog Log = new nLog("FractionConfigs");
        // fractionid - vehicle number - vehiclemodel, position, rotation, min rank, color1, color2
        public static Dictionary<int, Dictionary<string, Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>> FractionVehicles = new Dictionary<int, Dictionary<string, Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>>();
        public static Dictionary<int, string> FractionTypes = new Dictionary<int, string>()
        {
            { 1, "FAMILY" },
            { 2, "BALLAS" },
            { 3, "VAGOS" },
            { 4, "MARABUNTA" },
            { 5, "BLOOD" },
            { 6, "CITY" },
            { 7, "POLICE" },
            { 8, "EMS" },
            { 9, "FIB" },
            { 10, "LCN" },
            { 11, "RUSSIAN" },
            { 12, "YAKUZA" },
            { 13, "ARMENIAN" },
            { 14, "ARMY" },
            { 15, "LSNEWS" },
            { 16, "THELOST" },
            { 17, "MERRYWEATHER" },
        };
        // fractionid - ranknumber - rankname, rankclothes
        public static Dictionary<int, Dictionary<int, Tuple<string, string, string, int>>> FractionRanks = new Dictionary<int, Dictionary<int, Tuple<string, string, string, int>>>();
        // fractionid - commandname, minrank
        public static Dictionary<int, Dictionary<string, int>> FractionCommands = new Dictionary<int, Dictionary<string, int>>();
        // fractionid - commandname, minrank
        public static Dictionary<int, Dictionary<string, int>> FractionWeapons = new Dictionary<int, Dictionary<string, int>>();
        public static void LoadFractionConfigs()
        {
            #region loadconfigstodb
            /*for (int i = 0; i < Army.ArmyCarsCoords.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (14,'number_{i}','model','{JsonConvert.SerializeObject(Army.ArmyCarsCoords[i])}','{JsonConvert.SerializeObject(Army.ArmyCarsRot[i])}',0)");
            }
            for (int i = 0; i < Cityhall.CityCarsCoords.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (6,'number_{i}','model','{JsonConvert.SerializeObject(Cityhall.CityCarsCoords[i])}','{JsonConvert.SerializeObject(Cityhall.CityCarsRot[i])}',0)");
            }
            for (int i = 0; i < Ems.EmsCarsCoords.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (8,'number_{i}','model','{JsonConvert.SerializeObject(Ems.EmsCarsCoords[i])}','{JsonConvert.SerializeObject(Ems.EmsCarsRot[i])}',0)");
            }
            for (int i = 0; i < Fbi.FbiCarsCoords.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (9,'number_{i}','model','{JsonConvert.SerializeObject(Fbi.FbiCarsCoords[i])}','{JsonConvert.SerializeObject(Fbi.FbiCarsRot[i])}',0)");
            }
            for (int i = 0; i < Gangs.FamCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (1,'number_{i}','model','{JsonConvert.SerializeObject(Gangs.FamCarCoord[i])}','{JsonConvert.SerializeObject(Gangs.FamCarRot[i])}',0)");
            }
            for (int i = 0; i < Gangs.BallasCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (2,'number_{i}','model','{JsonConvert.SerializeObject(Gangs.BallasCarCoord[i])}','{JsonConvert.SerializeObject(Gangs.BallasCarRot[i])}',0)");
            }
            for (int i = 0; i < Gangs.VagosCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (3,'number_{i}','model','{JsonConvert.SerializeObject(Gangs.VagosCarCoord[i])}','{JsonConvert.SerializeObject(Gangs.VagosCarRot[i])}',0)");
            }
            for (int i = 0; i < Gangs.MarabuntaCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (4,'number_{i}','model','{JsonConvert.SerializeObject(Gangs.MarabuntaCarCoord[i])}','{JsonConvert.SerializeObject(Gangs.MarabuntaCarRot[i])}',0)");
            }
            for (int i = 0; i < Gangs.BloodCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (5,'number_{i}','model','{JsonConvert.SerializeObject(Gangs.BloodCarCoord[i])}','{JsonConvert.SerializeObject(Gangs.BloodCarRot[i])}',0)");
            }
            for (int i = 0; i < Mafia.LcnCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (10,'number_{i}','model','{JsonConvert.SerializeObject(Mafia.LcnCarCoord[i])}','{JsonConvert.SerializeObject(Mafia.LcnCarRot[i])}',0)");
            }
            for (int i = 0; i < Mafia.RusCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (11,'number_{i}','model','{JsonConvert.SerializeObject(Mafia.RusCarCoord[i])}','{JsonConvert.SerializeObject(Mafia.RusCarRot[i])}',0)");
            }
            for (int i = 0; i < Mafia.YakuzaCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (12,'number_{i}','model','{JsonConvert.SerializeObject(Mafia.YakuzaCarCoord[i])}','{JsonConvert.SerializeObject(Mafia.YakuzaCarRot[i])}',0)");
            }
            for (int i = 0; i < Mafia.ArmCarCoord.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (13,'number_{i}','model','{JsonConvert.SerializeObject(Mafia.ArmCarCoord[i])}','{JsonConvert.SerializeObject(Mafia.ArmCarRot[i])}',0)");
            }
            for (int i = 0; i < Police.PoliceCarsCoords.Count; i++)
            {
                MySQL.Query($"INSERT INTO `fractionvehicles`(`fraction`,`number`,`model`,`position`,`rotation`,`rank`) " +
                    $"VALUES (7,'number_{i}','model','{JsonConvert.SerializeObject(Police.PoliceCarsCoords[i])}','{JsonConvert.SerializeObject(Police.PoliceCarsRot[i])}',0)");
            }
            return;
            */
            #endregion
            for (int i = 1; i <= 17; i++)
                FractionVehicles.Add(i, new Dictionary<string, Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>>());
            for (int i = 1; i <= 17; i++)
                FractionRanks.Add(i, new Dictionary<int, Tuple<string, string, string, int>>());
            for (int i = 1; i <= 17; i++)
                FractionCommands.Add(i, new Dictionary<string, int>());
            for (int i = 1; i <= 17; i++)
                FractionWeapons.Add(i, new Dictionary<string, int>());

            // loading fraction vehicle configs and spawn
            DataTable result = MySQL.QueryRead("SELECT * FROM `fractionvehicles`");
            if (result == null || result.Rows.Count == 0) return;
            foreach (DataRow Row in result.Rows)
            {
                var fraction = Convert.ToInt32(Row["fraction"]);
                var number = Row["number"].ToString();
                var model = NAPI.Util.VehicleNameToModel(Row["model"].ToString());
                var position = JsonConvert.DeserializeObject<Vector3>(Row["position"].ToString());
                var rotation = JsonConvert.DeserializeObject<Vector3>(Row["rotation"].ToString());
                var minrank = Convert.ToInt32(Row["rank"]);
                var color1 = Convert.ToInt32(Row["colorprim"]);
                var color2 = Convert.ToInt32(Row["colorsec"]);
                VehicleManager.VehicleCustomization components = JsonConvert.DeserializeObject<VehicleManager.VehicleCustomization>(Row["components"].ToString());

                FractionVehicles[fraction].Add(number, new Tuple<VehicleHash, Vector3, Vector3, int, int, int, VehicleManager.VehicleCustomization>(model, position, rotation, minrank, color1, color2, components));
            }

            foreach (var fraction in FractionVehicles.Keys)
                SpawnFractionCars(fraction);

            // load fraction ranks configs
            result = MySQL.QueryRead("SELECT * FROM `fractionranks`");
            if (result == null || result.Rows.Count == 0) return;
            foreach (DataRow Row in result.Rows)
            {
                var fraction = Convert.ToInt32(Row["fraction"]);
                var rank = Convert.ToInt32(Row["rank"]);
                var payday = Convert.ToInt32(Row["payday"]);
                var name = Row["name"].ToString();
                var clothesm = Row["clothesm"].ToString();
                var clothesf = Row["clothesf"].ToString();

                FractionRanks[fraction].Add(rank, new Tuple<string, string, string, int>(name, clothesm, clothesf, payday));
            }

            result = MySQL.QueryRead("SELECT * FROM `fractionaccess`");
            if (result == null || result.Rows.Count == 0) return;
            foreach (DataRow Row in result.Rows)
            {
                var fraction = Convert.ToInt32(Row["fraction"]);
                var dictionaryCmd = JsonConvert.DeserializeObject<Dictionary<string, int>>(Row["commands"].ToString());
                var dictionaryWeap = JsonConvert.DeserializeObject<Dictionary<string, int>>(Row["weapons"].ToString());

                FractionCommands[fraction] = dictionaryCmd;
                FractionWeapons[fraction] = dictionaryWeap;
            }

            Manager.onResourceStart();
        }
        
        public static void SpawnFractionCars(int fraction)
        {
            foreach (var vehicle in FractionVehicles[fraction])
            {
                var model = vehicle.Value.Item1;
                var canmats = (model == VehicleHash.Barracks || model == VehicleHash.Youga || model == VehicleHash.Burrito3); // "CANMATS"
                var candrugs = (model == VehicleHash.Youga || model == VehicleHash.Burrito3); // "CANDRUGS"
                var canmeds = (model == VehicleHash.Ambulance); // "CANMEDKITS"
                var veh = NAPI.Vehicle.CreateVehicle(model, vehicle.Value.Item2, vehicle.Value.Item3, vehicle.Value.Item5, vehicle.Value.Item6);

                NAPI.Data.SetEntityData(veh, "ACCESS", "FRACTION");
                NAPI.Data.SetEntityData(veh, "FRACTION", fraction);
                NAPI.Data.SetEntityData(veh, "MINRANK", vehicle.Value.Item4);
                NAPI.Data.SetEntityData(veh, "TYPE", FractionTypes[fraction]);
                if (canmats)
                    NAPI.Data.SetEntityData(veh, "CANMATS", true);
                if (candrugs)
                    NAPI.Data.SetEntityData(veh, "CANDRUGS", true);
                if (canmeds)
                    NAPI.Data.SetEntityData(veh, "CANMEDKITS", true);
                NAPI.Vehicle.SetVehicleNumberPlate(veh, vehicle.Key);
                Core.VehicleStreaming.SetEngineState(veh, false);
                VehicleManager.FracApplyCustomization(veh, fraction);
                if(model == VehicleHash.Submersible || model == VehicleHash.THRUSTER) veh.SetSharedData("PETROL", 0);
            }
        }
        public static void RespawnFractionCar(Vehicle vehicle)
        {
            try
            {
                var canmats = vehicle.HasData("CANMATS");
                var candrugs = vehicle.HasData("CANDRUGS");
                var canmeds = vehicle.HasData("CANMEDKITS");
                string number = vehicle.NumberPlate;
                int fraction = vehicle.GetData("FRACTION");

                NAPI.Entity.SetEntityPosition(vehicle, FractionVehicles[fraction][number].Item2);
                NAPI.Entity.SetEntityRotation(vehicle, FractionVehicles[fraction][number].Item3);
                VehicleManager.RepairCar(vehicle);
                NAPI.Data.SetEntityData(vehicle, "ACCESS", "FRACTION");
                NAPI.Data.SetEntityData(vehicle, "FRACTION", fraction);
                NAPI.Data.SetEntityData(vehicle, "MINRANK", FractionVehicles[fraction][number].Item4);
                if (canmats)
                    NAPI.Data.SetEntityData(vehicle, "CANMATS", true);
                if (candrugs)
                    NAPI.Data.SetEntityData(vehicle, "CANDRUGS", true);
                if (canmeds)
                    NAPI.Data.SetEntityData(vehicle, "CANMEDKITS", true);
                NAPI.Vehicle.SetVehicleNumberPlate(vehicle, number);
                Core.VehicleStreaming.SetEngineState(vehicle, false);
                VehicleManager.FracApplyCustomization(vehicle, fraction);
            }
            catch (Exception e) { Log.Write("RespawnFractionCar: " + e.Message, nLog.Type.Error); }
        }
    }
}
