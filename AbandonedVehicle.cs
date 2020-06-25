using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using FivePD.API;

namespace AbandonedVehicle
{
    [CalloutProperties("Abandoned Vehicle", "AttributeError", "1.1")]
    public class AbandonedVehicle : FivePD.API.Callout
    {
        private Random rnd = new Random();
        private Vehicle veh;
        private Vector3 SpawnPoint;
        private Blip SearchArea;
        private bool SearchAreaRoute = false;
        private Blip VehicleBlip;
        private VehicleHash[] VehiclesToChooseFrom = new VehicleHash[] {
            VehicleHash.Blista, VehicleHash.Brioso, VehicleHash.Dilettante, VehicleHash.Dilettante2, VehicleHash.Issi2, VehicleHash.Panto, VehicleHash.Prairie, VehicleHash.Rhapsody,
            VehicleHash.Asea, VehicleHash.Asea2, VehicleHash.Asterope, VehicleHash.Cog55, VehicleHash.Cognoscenti, VehicleHash.Emperor, VehicleHash.Emperor2, VehicleHash.Emperor3, VehicleHash.Fugitive, VehicleHash.Glendale, VehicleHash.Ingot, VehicleHash.Intruder, VehicleHash.Premier, VehicleHash.Primo, VehicleHash.Regina, VehicleHash.Schafter2, VehicleHash.Stanier, VehicleHash.Stratum, VehicleHash.Superd, VehicleHash.Tailgater, VehicleHash.Warrener, VehicleHash.Washington,
            VehicleHash.CogCabrio, VehicleHash.Exemplar, VehicleHash.F620, VehicleHash.Felon, VehicleHash.Felon2, VehicleHash.Jackal, VehicleHash.Oracle, VehicleHash.Oracle2, VehicleHash.Sentinel, VehicleHash.Sentinel2,
            VehicleHash.Akuma, VehicleHash.Avarus, VehicleHash.Bagger, VehicleHash.Bati, VehicleHash.Bati2, VehicleHash.BF400, VehicleHash.CarbonRS, VehicleHash.Chimera, VehicleHash.Cliffhanger, VehicleHash.Daemon, VehicleHash.Daemon2, VehicleHash.Defiler, VehicleHash.Double, VehicleHash.Enduro, VehicleHash.Esskey, VehicleHash.Faggio, VehicleHash.Faggio2, VehicleHash.Faggio3, VehicleHash.FCR, VehicleHash.Gargoyle, VehicleHash.Hakuchou, VehicleHash.Hexer, VehicleHash.Innovation, VehicleHash.Lectro, VehicleHash.Manchez, VehicleHash.Nemesis, VehicleHash.Nightblade, VehicleHash.PCJ, VehicleHash.RatBike, VehicleHash.Ruffian, VehicleHash.Sanchez2, VehicleHash.Sanctus, VehicleHash.Shotaro, VehicleHash.Sovereign, VehicleHash.Thrust, VehicleHash.Vader, VehicleHash.Vindicator, VehicleHash.Vortex
        };
        private VehicleHash chosenVehicle;
        private Dictionary<string, string> colorNames = new Dictionary<string, string>
        {
            { "0", "Black" }, { "1", "Graphite Black" }, { "2", "Black Steal" }, { "3", "Dark Silver" }, { "4", "Silver" }, { "5", "Blue Silver" }, { "6", "Steel Gray" },
            { "7", "Shadow Silver" }, { "8", "Stone Silver" }, { "9", "Midnight Silver" }, { "10", "Gun Metal" }, { "11", "Anthracite Grey" }, { "12", "Black" },
            { "13", "Gray" }, { "14", "Light Grey" }, { "15", "Black" }, { "16", "Black Poly" }, { "17", "Dark silver" }, { "18", "Silver" }, { "19", "Gun Metal" },
            { "20", "Shadow Silver" }, { "21", "Black" }, { "22", "Graphite" }, { "23", "Silver Grey" }, { "24", "Silver" }, { "25", "Blue Silver" }, { "26", "Shadow Silver" },
            { "27", "Red" }, { "28", "Torino Red" }, { "29", "Formula Red" }, { "30", "Blaze Red" }, { "31", "Graceful Red" }, { "32", "Garnet Red" }, { "33", "Desert Red" },
            { "34", "Cabernet Red" }, { "35", "Candy Red" }, { "36", "Sunrise Orange" }, { "37", "Classic Gold" }, { "38", "Orange" }, { "39", "Red" }, { "40", "Dark Red" },
            { "41", "Orange" }, { "42", "Yellow" }, { "43", "Red" }, { "44", "Bright Red" }, { "45", "Garnet Red" }, { "46", "Red" }, { "47", "Golden Red" }, { "48", "Dark Red" }, 
            { "49", "Dark Green"}, { "50", "Racing Green" }, { "51", "Sea Green" }, { "52", "Olive Green" }, { "53", "Green" }, { "54", "Gasoline Blue Green" }, { "55", "Lime Green" },
            { "56", "Dark Green" }, { "57", "Green" }, { "58", "Dark Green" }, { "59", "Green" }, { "60", "Sea Wash" }, { "61", "Midnight Blue" }, { "62", "Dark Blue" }, { "63", "Saxony Blue" },
            { "64", "Blue" }, { "65", "Mariner Blue" }, { "66", "Harbor Blue" }, { "67", "Diamond Blue" }, { "68", "Surf Blue" }, { "69", "Nautical Blue" }, { "70", "Bright Blue" }, { "71", "Purple Blue" },
            { "72", "Spinnaker Blue" }, { "73", "Ultra Blue" }, { "74", "Bright Blue" }, { "75", "Dark Blue" }, { "76", "Midnight Blue" }, { "77", "Blue" }, { "78", "Sea Foam Blue" },
            { "79", "Uil Lightning Blue" }, { "80", "Maui Blue Poly" }, { "81", "Bright Blue" }, { "82", "Dark Blue" }, { "83", "Blue" }, { "84", "Midnight Blue" }, { "85", "Dark Blue" },
            { "86", "Blue" }, { "87", "Light Blue" }, { "88", "Taxi Yellow" }, { "89", "Race Yellow" }, { "90", "Bronze" }, { "91", "Yellow Bird" }, { "92", "Lime" }, { "93", "Champagne" }, { "94", "Pueblo Beige" },
            { "95", "Dark Ivory" }, { "96", "Choco Brown" }, { "97", "Golden Brown" }, { "98", "Light Brown" }, { "99", "Straw Beige" }, { "100", "Moss Brown" }, { "101", "Biston Brown" }, { "102", "Beechwood" },
            { "103", "Dark Beechwood" }, { "104", "Choco Orange" }, { "105", "Beach Sand" }, { "106", "Sun Bleeched Sand" }, { "107", "Cream" }, { "108", "Brown" }, { "109", "Medium Brown" },
            { "110", "Light Brown" }, { "111", "White" }, { "112", "Frost White" }, { "113", "Honey Beige" }, { "114", "Brown" }, { "115", "Dark Brown" }, { "116", "Straw Beige" },
            { "117", "Brushed Steel" }, { "118", "Brushed Black Steel" }, { "119", "Brushed Aluminium" }, { "120", "Chrome" }, { "121", "Off White" }, { "122", "Off White" },
            { "123", "Orange" }, { "124", "Light Orange" }, { "125", "Securicor Green" }, { "126", "Yellow" }, { "127", "Blue" }, { "128", "Green" }, { "129", "Brown" },
            { "130", "Orange" }, { "131", "White" }, { "132", "White" }, { "133", "Olive Green" },  { "134", "Pure White" }, { "135", "Hot Pink" }, { "136", "Salmon Pink" }, 
            { "137", "Vermillion Pink" }, { "138", "Orange" }, { "139", "Green" }, { "140", "Blue" }, { "141", "Black Blue" }, { "142", "Black Purple" }, { "143", "Black Red" },
            { "144", "Hunter Green" }, { "145", "Purple" }, { "146", "Dark Blue" }, { "147", "Black" }, { "148", "Purple" }, { "149", "Dark Purple" }, { "150", "Lava Red" }, { "151", "Forest Green" },
            { "152", "Olive Drab" }, { "153", "Desert Brown" }, { "154", "Desert Tan" }, { "155", "Foilage Green" }, { "156", "DEFAULT ALLOY COLOR" }, { "157", "Epsilon Blue" }
        };
        private int scenarioProbability = -1;
        private string scenarioMessage = "";

        private string vehicleColor;
        private string vehicleModel;
        private string vehiclePlate;

        private void ShowNotification(string text)
        {
            SetNotificationTextEntry("STRING");
            AddTextComponentString(text);
            DrawNotification(false, true);
        }
        private void ShowHelp(string text)
        {
            SetTextComponentFormat("STRING");
            AddTextComponentString(text);
            DisplayHelpTextFromStringLabel(0, false, false, -1);
        }

        public AbandonedVehicle()
        {
            this.SpawnPoint = World.GetNextPositionOnStreet(Game.PlayerPed.GetPositionOffset(new Vector3((float)rnd.Next(100, 300), (float)rnd.Next(100, 300), 0.0f)));
            this.chosenVehicle = this.VehiclesToChooseFrom[rnd.Next(this.VehiclesToChooseFrom.Length)];

            this.scenarioProbability = rnd.Next(5);
            if (this.scenarioProbability == 0) this.scenarioMessage = "The vehicle has been abandoned for a few days and no-one has seen the owner.";
            else if (this.scenarioProbability == 1) this.scenarioMessage = "The vehicle looks to have been in a front-end collision and has been abandoned.";
            else if (this.scenarioProbability == 2) this.scenarioMessage = "The vehicle looks to have been in a rear-end collision and has been abandoned.";
            else if(this.scenarioProbability == 3) this.scenarioMessage = "The vehicle has broken down and there's smoke coming from the engine.";
            else if(this.scenarioProbability == 4) this.scenarioMessage = "The vehicle has been abandoned and some of the tires have been slashed.";

            this.InitInfo(this.SpawnPoint);

            this.ShortName = "Abandoned Vehicle";
            this.XP = 100;
            this.CalloutDescription = "A caller has reported a suspicious abandoned vehicle, respond to investigate. " + this.scenarioMessage;
            this.ResponseCode = 2;

            this.StartDistance = 75f;
        }
        public async override Task OnAccept()
        {
            this.InitBlip();
            this.Marker?.Delete();

            Vehicle vehicle = await SpawnVehicle(this.chosenVehicle, this.SpawnPoint, 0f);
            vehicle.IsPersistent = true;
            this.veh = vehicle;

            int primaryColor = 0;
            int secondaryColor = 0;
            GetVehicleColours(this.veh.Handle, ref primaryColor, ref secondaryColor);
            vehicleColor = colorNames[primaryColor.ToString()];
            vehicleModel = this.veh.DisplayName;
            vehiclePlate = GetVehicleNumberPlateText(this.veh.Handle);

            this.CalloutDescription += "You are looking for a: " + vehicleColor + " " + vehicleModel + " with the plate: " + vehiclePlate;
            ShowNotification("~b~Control~s~: You are looking for a: ~o~" + vehicleColor + " ~y~" + vehicleModel + "~s~. The plate is: ~r~" + vehiclePlate + "~s~.");
            ShowNotification("~b~Control~s~: " + this.scenarioMessage);

            if (this.scenarioProbability != 0 && this.scenarioProbability != 4)
            {
                SetVehicleEngineHealth(this.veh.Handle, 3f); //make vehicle smoke 
                SetVehicleDoorOpen(this.veh.Handle, 1, false, false);

                int doorProbability = rnd.Next(0, 10);
                if (doorProbability % 2 == 0)
                {
                    int[] vehicleDoors = new int[] { 0, 2, 3, 5, 6, 7 };
                    int vehicleDoor = -1;
                    while (!DoesVehicleHaveDoor(this.veh.Handle, vehicleDoor))
                    {
                        await BaseScript.Delay(0);
                        vehicleDoor = vehicleDoors[rnd.Next(vehicleDoors.Length)];
                    }
                    SetVehicleDoorOpen(this.veh.Handle, vehicleDoor, false, false);
                }
            }

            if (scenarioProbability == 1) //front-end 
            {
                SmashVehicleWindow(this.veh.Handle, 6); //smash windscreen 
                SetVehicleDoorOpen(this.veh.Handle, 4, false, true); //pop hood to simulate front-end collision
            } else if(scenarioProbability == 2) //rear-end 
            {
                SmashVehicleWindow(this.veh.Handle, 8); //smash read windscreen
                SetVehicleDoorOpen(this.veh.Handle, 5, false, true); //pop trunk to simulate rear-end collision
            } else if(scenarioProbability == 3) //broken down 
            {
                SetVehicleUndriveable(this.veh.Handle, true); //player cannot move vehicle 
                SetVehicleEngineOn(this.veh.Handle, false, true, true); //engine is off
            } else if(scenarioProbability == 4) //slashed tires
            {
                int tiresToSlash = rnd.Next(8);
                for(int i = 0; i < tiresToSlash; i++)
                {
                    SetVehicleTyreBurst(this.veh.Handle, i, rnd.Next(100) % 3 == 0, rnd.Next(500, 1000));
                }
            }

            float correctionX = 20;
            Vector3 coords = GetOffsetFromEntityInWorldCoords(this.veh.Handle, correctionX, 0, 0);
            while (!IsPointOnRoad(coords.X, coords.Y, coords.Z, this.veh.Handle))
            {
                correctionX -= 0.5f;
                coords = GetOffsetFromEntityInWorldCoords(this.veh.Handle, correctionX, 0, 0);
            }
            this.veh.Position = coords;
            SetEntityCoords(this.veh.Handle, coords.X, coords.Y, coords.Z, false, false, false, true);
            this.SpawnPoint = this.veh.Position;

            this.SearchArea = new Blip(AddBlipForRadius(this.SpawnPoint.X, this.SpawnPoint.Y, this.SpawnPoint.Z, 145f));
            this.SearchArea.Name = "Search Area";
            this.SearchArea.Color = BlipColor.Yellow;
            this.SearchArea.Alpha = 45;
            if (Vector3.Distance(Game.PlayerPed.Position, this.veh.Position) > 145f) this.SearchAreaRoute = true;
            this.SearchArea.ShowRoute = this.SearchAreaRoute;

            Tick += UpdateSearchArea;
            Tick += FoundVehicleCheck;
            Tick += VehicleDestroyedCheck;

            Tick += DrawVehicleInfo;

            await Task.FromResult(0);
        }

        private void DrawTextOnScreen(float x, float y, string text)
        {
            SetTextFont(4);
            SetTextProportional(true);
            SetTextScale(0.0f, 0.45f);
            SetTextColour(255, 255, 255, 255);
            SetTextDropshadow(0, 0, 0, 0, 255);
            SetTextEdge(1, 0, 0, 0, 150);
            SetTextOutline();
            SetTextEntry("STRING");
            AddTextComponentString(text);
            DrawText(x, y);
        }
        public async Task DrawVehicleInfo()
        {
            DrawRect(0.0775f, 0.23f, 0.13f, 0.11f, 0, 0, 0, 150); //main box 
            DrawRect(0.0775f, 0.19f, 0.13f, 0.03f, 0, 0, 0, 175); //title box 

            DrawTextOnScreen(0.043f, 0.172f, "Vehicle Information");
            DrawTextOnScreen(0.044f, 0.21f, vehicleColor + " " + vehicleModel);
            DrawTextOnScreen(0.06f, 0.24f, vehiclePlate);

            await Task.FromResult(0);
        }

        public async Task VehicleDestroyedCheck()
        {
            if (!this.veh.Exists() || this.veh.IsDead)
            {
                if (this.veh.Exists()) this.veh.Delete();
                ShowNotification("~r~[CALLOUT ENDED]~s~ The vehicle no longer exists, or has been destroyed.");
                Tick -= UpdateSearchArea;
                Tick -= FoundVehicleCheck;
                Tick -= VehicleDestroyedCheck;
                Tick -= VehicleAttachedCheck;

                EndCallout();
            }

            await Task.FromResult(0);
        }

        public async Task UpdateSearchArea()
        {
            this.Marker?.Delete();

            if (!this.SearchArea.Exists())
            {
                this.SearchArea = new Blip(AddBlipForRadius(this.SpawnPoint.X, this.SpawnPoint.Y, this.SpawnPoint.Z, 145f));
                this.SearchArea.Name = "Search Area";
                this.SearchArea.Color = BlipColor.Yellow;
                this.SearchArea.Alpha = 45;
            }

            if(Vector3.Distance(Game.PlayerPed.Position, this.veh.Position) <= 145f && this.SearchAreaRoute)
            {
                this.SearchAreaRoute = false;
                this.SearchArea.ShowRoute = this.SearchAreaRoute;
            }

            await Task.FromResult(0);
        }

        public async Task FoundVehicleCheck()
        {
            if (Vector3.Distance(Game.PlayerPed.Position, this.veh.Position) < 20f)
            {
                if (!Game.PlayerPed.IsInVehicle() && Vector3.Distance(Game.PlayerPed.Position, this.veh.Position) < 5f)
                {
                    Tick -= FoundVehicleCheck;
                    Tick -= UpdateSearchArea;
                    Tick += VehicleAttachedCheck;

                    int blip = this.SearchArea.Handle;
                    RemoveBlip(ref blip);
                    if (this.SearchArea.Exists()) this.SearchArea.Delete();
                    this.SearchArea = null;

                    this.VehicleBlip = this.veh.AttachBlip();
                    this.VehicleBlip.Color = BlipColor.Red;
                    this.VehicleBlip.ShowRoute = true;

                    ShowNotification("~b~You~s~: Control, I've located the abandoned vehicle.");
                    ShowHelp("Write up your report and tow the vehicle.");
                }
            }
            await Task.FromResult(0);
        }

        public async Task VehicleAttachedCheck()
        {
            if(this.veh.IsAttached())
            {
                Tick -= VehicleAttachedCheck;
                ShowNotification("~b~You~s~: Control, tow has arrived and has towed the vehicle. I'll be back available for calls.");
                EndCallout();
            }

            await Task.FromResult(0);
        }

        public override void OnStart(Ped closest)
        {
            base.OnStart(closest);
        }

        public override async void OnCancelBefore()
        {
            Tick -= DrawVehicleInfo;

            if (this.SearchArea != null && this.SearchArea.Exists()) this.SearchArea.Delete();
            this.veh.IsPersistent = false;
            base.OnCancelBefore();

            await Task.FromResult(0);
        }

        public override async Task<bool> CheckRequirements()
        {
            await Task.FromResult(0);

            if (World.CurrentDayTime.Hours < 9 && World.CurrentDayTime.Hours > 20) return false;
            return true;
        }
    }
}
