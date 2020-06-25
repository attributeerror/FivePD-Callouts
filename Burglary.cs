using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using FivePD.API;

namespace Burglary
{
    [CalloutProperties("Burglary", "AttributeError", "1.1")]
    public class Burglary : FivePD.API.Callout
    {
        private Vector3[] SpawnPoints = new Vector3[]
        {
            new Vector3(-35.69f, 6641.91f, 30.32f), new Vector3(-20.19f, 6669.83f, 31.11f), new Vector3(26.25f, 6660.03f, 31.72f), new Vector3(-71.07f, 6633.54f, 31.73f),
            new Vector3(-12.03f, 6559.62f, 31.97f), new Vector3(-269.75f, 6397.1f, 30.95f), new Vector3(1666.79f, 4767.6f, 41.95f), new Vector3(1671.66f, 4747.83f, 41.9f),
            new Vector3(1734.9f, 4667.37f, 43.59f), new Vector3(1742.15f, 4649.18f, 43.31f), new Vector3(1980.89f, 4641.79f, 41.32f), new Vector3(1852.08f, 3781.02f, 33.06f),
            new Vector3(1872.84f, 3811.71f, 32.78f), new Vector3(1839.25f, 3901.55f, 33.26f), new Vector3(1818.55f, 3911.89f, 33.51f), new Vector3(1778.15f, 3920.56f, 34.57f),
            new Vector3(1732.57f, 3904.36f, 34.9f), new Vector3(1664f, 3825.71f, 34.89f), new Vector3(1652.3f, 3790.25f, 34.89f)
        };
        private Dictionary<string, int>[] MSuspectComps = new Dictionary<string, int>[]
        {
            new Dictionary<string, int>() //upper body
            {
                { "componentId", 3 },
                { "drawableId", 15 },
                { "textureId", 0 },
                { "paletteId", 0 }
            },
            new Dictionary<string, int>() //lower body
            {
                { "componentId", 4 },
                { "drawableId", 3 },
                { "textureId", 3 },
                { "paletteId", 0 }
            },
            new Dictionary<string, int>() //shoes
            {
                { "componentId", 6 },
                { "drawableId", 9 },
                { "textureId", 14 },
                { "paletteId", 0 }
            },
            new Dictionary<string, int>() //shirt
            {
                { "componentId", 8 },
                { "drawableId", 15 },
                { "textureId", 0 },
                { "paletteId", 0 }
            },
            new Dictionary<string, int>() //shirt overlay/jackets
            {
                { "componentId", 11 },
                { "drawableId", 50 },
                { "textureId", 0 },
                { "paletteId", 0 }
            }
        };
        private PedHash[] dogHashes = new PedHash[] { PedHash.Chop, PedHash.Poodle, PedHash.Pug, PedHash.Retriever, PedHash.Rottweiler, PedHash.Shepherd, PedHash.Westy };

        private Random rnd = new Random();
        private Vector3 SpawnPoint;
        private Ped suspect;
        private Blip suspectBlip;
        private Ped dog;
        private bool burglary = false;
        private string hammerOrCutters = "hammer";

        private string playerCallsign = "You";

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

        private void ShowSubtitle(string text)
        {
            BeginTextCommandPrint("STRING");
            AddTextComponentSubstringPlayerName(text);
            EndTextCommandPrint(5000, true);
        }

        public Burglary()
        {
            try
            {
                this.SpawnPoint = new Vector3(50000, 50000, 50000);
                foreach (Vector3 loc in this.SpawnPoints)
                {
                    if (Vector3.Distance(Game.PlayerPed.Position, loc) < Vector3.Distance(Game.PlayerPed.Position, this.SpawnPoint))
                    {
                        this.SpawnPoint = loc;
                    }
                }
                this.burglary = rnd.Next(0, 10) % 3 == 0;

                this.InitInfo(this.SpawnPoint);

                this.ShortName = "Burglary";
                this.XP = 100;
                this.CalloutDescription = "A caller has reported an attempted burglary - get to the scene and arrest the suspect!";
                this.ResponseCode = 3;
                this.FixedLocation = true;

                this.StartDistance = 100f;
            }
            catch (Exception)
            { }
        }

        public async override Task OnAccept()
        {
            try
            {
                base.InitBlip();

                if (this.burglary)
                {
                    Ped suspPed = await SpawnPed(PedHash.FreemodeMale01, this.SpawnPoint);
                    foreach (Dictionary<string, int> pedComponent in MSuspectComps)
                    {
                        int componentId;
                        int drawableId;
                        int textureId;
                        int paletteId;
                        pedComponent.TryGetValue("componentId", out componentId);
                        pedComponent.TryGetValue("drawableId", out drawableId);
                        pedComponent.TryGetValue("textureId", out textureId);
                        pedComponent.TryGetValue("paletteId", out paletteId);

                        SetPedComponentVariation(suspPed.Handle, componentId, drawableId, textureId, paletteId);
                    }
                    this.suspect = suspPed;
                }
                else
                {
                    this.suspect = await SpawnPed(GetRandomPed(), this.SpawnPoint);
                    Vector3 dogSpawn = new Vector3(this.SpawnPoint.X, this.SpawnPoint.Y + 2f, this.SpawnPoint.Z + 2f);
                    this.dog = await SpawnPed(dogHashes[rnd.Next(dogHashes.Length)], dogSpawn);
                }

                this.suspect.IsPersistent = true;
                this.suspect.BlockPermanentEvents = true;
                this.dog.IsPersistent = true;
                this.dog.BlockPermanentEvents = true;


                ShowNotification("~b~Control~s~: Someone has reported that they witnessed an attempted burglary, respond to the scene to investigate.");
            }
            catch (Exception)
            { }
        }

        public async override void OnStart(Ped closest)
        {
            base.OnStart(closest);

            dynamic NewPedData = new ExpandoObject();
            List<object> items = new List<object>();
            dynamic CurrentPedData = await Utilities.GetPedData(this.suspect.NetworkId);
            object id = new
            {
                Name = "Valid ID for " + CurrentPedData.Firstname + " " + CurrentPedData.Lastname,
                IsIllegal = false
            };
            items.Add(id);
            object phone = new
            {
                Name = "mobile phone",
                IsIllegal = false
            };
            items.Add(phone);

            if(this.burglary)
            {
                if (rnd.Next(0, 10) == 9)
                {
                    object wireCutters = new
                    {
                        Name = "wire cutters",
                        IsIllegal = true
                    };
                    items.Add(wireCutters);
                    hammerOrCutters = "wire cutters";
                }
                else
                {
                    object hammer = new
                    {
                        Name = "hammer",
                        IsIllegal = true
                    };
                    items.Add(hammer);
                    hammerOrCutters = "hammer";
                }
            }

            NewPedData.DrugsUsed = new bool[]
            {
                rnd.Next(0, 10) % 5 == 0, //meth
                rnd.Next(0, 10) % 3 == 0, //cocaine 
                rnd.Next(0, 10) % 2 == 0 //marijuana
            };
            NewPedData.alcoholLevel = rnd.Next(0, 100) % 4 == 0 ? rnd.NextDouble() : 0.0;
            NewPedData.items = items;
            Utilities.SetPedData(this.suspect.NetworkId, NewPedData);

            dynamic PlayerData = Utilities.GetPlayerData();
            this.playerCallsign = PlayerData.Callsign;
            if (this.playerCallsign.Length < 1) this.playerCallsign = "You";

            Tick += Code4Check;

            try
            {
                if (Game.PlayerPed.IsInRangeOf(this.suspect.Position, 100f))
                {
                    Tick += FoundSuspectCheck;
                    ShowNotification("~b~" + this.playerCallsign + "~s~: Control, I've arrived on scene, attempting to locate the suspect.");
                }
            }
            catch (Exception)
            { }
        }

        private async Task FoundSuspectCheck()
        {
            try
            {
                if (Game.PlayerPed.IsInRangeOf(this.suspect.Position, 5f) && !Game.PlayerPed.IsInVehicle())
                {
                    Tick -= FoundSuspectCheck;
                    ShowNotification("~b~" + this.playerCallsign + "~s~: Control, I've located the suspect.");
                    ShowHelp("Press ~INPUT_DETONATE~ to talk to the subject.");

                    this.suspectBlip = this.suspect.AttachBlip();
                    this.suspectBlip.Color = BlipColor.Red;

                    int roll = rnd.Next(3);
                    string[] lines = new string[] { };
                    int currentLineIndex = 0;

                    if (!this.burglary)
                    {
                        if (roll == 0)
                        {
                            lines = new string[]
                            {
                            "~b~You~s~: Hello there! Stay where you are for me, I've just got some questions.",
                            "~o~Subject~s~: Good day, officer! How can I help you?",
                            "~b~You~s~: We've had a call that someone is attempting to break into this property.",
                            "~b~You~s~: Do you know anything about that?",
                            "~o~Subject~s~: Well... I have just been screamed at by the owners of this house.",
                            "~o~Subject~s~: Claiming that I'm trespassing!",
                            "~o~Subject~s~: I'm just trying to walk my dog...",
                            "~b~You~s~: I can see that... you haven't seen anyone else around here, have you?",
                            "~o~Subject~s~: No, not until these people came out of their house to shout at me.",
                            "~b~You~s~: Okay, I'm just going to search you and make sure you don't have anything on you, okay?",
                            "~o~Subject~s~: Sure thing! I've got my phone and ID on me, if that helps..."
                            };
                        }
                        else if (roll == 1)
                        {
                            lines = new string[]
                            {
                            "~b~You~s~: Hi, stay where you are for me. I've just got some questions for you.",
                            "~o~Subject~s~: Ah, officer! You scared me! How can I help?",
                            "~b~You~s~: Have you seen anyone attempt to break into a house around here in the past 5 minutes?",
                            "~o~Subject~s~: No, officer. I heard some people shouting from their window but other than that, no.",
                            "~b~You~s~: Ok. You match the description of a call we got so I've just got to do some checks, okay?",
                            "~o~Subject~s~: Alright... I don't have anything on me, I'm just trying to walk my dog...",
                            "~b~You~s~: Brilliant, I'm just going to search you to confirm that.",
                            "~o~Subject~s~: I haven't heard anything around here, I haven't even seen anyone else out walking.",
                            "~b~You~s~: Okay, that's fine. Thank you for your cooperation!"
                            };
                        }
                        else if (roll == 2)
                        {
                            lines = new string[]
                            {
                            "~b~You~s~: 'Ello! I've got some questions for you, just stay there, okay?",
                            "~o~Subject~s~: Sure? Is there a problem, officer?",
                            "~b~You~s~: We've had a call about an attempted break in around here. Have you seen anything?",
                            "~o~Subject~s~: No, officer, not at all. I haven't even seen anyone else out walking today.",
                            "~b~You~s~: Okay, I just need to search you to make sure you don't have anything on you, okay?",
                            "~o~Subject~s~: Sure thing, I have my phone and ID on me if that helps.",
                            "~b~You~s~: Alright. I have no reason to believe that you're breaking into anyone's house.",
                            "~b~You~s~: Thank you for your cooperation!"
                            };
                        }
                    }
                    else
                    {
                        if (roll == 0)
                        {
                            lines = new string[]
                            {
                            "~b~You~s~: Excuse me, stay where you are for me!",
                            "~o~Subject~s~: G'day, officer. Is there a problem?",
                            "~b~You~s~: There might be. We've had a call about someone attempting to break into a house.",
                            "~b~You~s~: and you match the description perfectly.",
                            "~o~Subject~s~: Uhhh, I don't know anything about that, officer...",
                            "~b~You~s~: Okay, I'm just going to search you for the safety of both of us.",
                            "~o~Subject~s~: Uhhh, sure...",
                            "~b~You~s~: What did you intend on using " + (hammerOrCutters == "hammer" ? "this hammer" : "these wire cutters") + " for?",
                            "~r~Suspect~s~: Sh*t, alright. You got me. I was going to break into this here house.",
                            "~b~You~s~: Brilliant. Put your hands behind your back for me."
                            };
                        }
                        else if (roll == 1)
                        {
                            lines = new string[]
                            {
                            "~o~Subject~s~: Aw f*ck...",
                            "~b~You~s~: Hello there. I've got some questions for you, if you don't mind?",
                            "~o~Subject~s~: OH! Hi, officer. How can I help?",
                            "~b~You~s~: We've had reports of someone trying to break into a house around here...",
                            "~b~You~s~: Do you happen to know anything about that?",
                            "~o~Subject~s~: Uhhhh... nooo, no idea what you're talking about officer.",
                            "~b~You~s~: Well, you match the description we were given perfectly.",
                            "~b~You~s~: I'm going to go ahead and search you for the safety of both of us.",
                            "~o~Subject~s~: Uuuuhhhh... ok....",
                            "~b~You~s~: What did you intend on using " + (hammerOrCutters == "hammer" ? "this hammer" : "these wire cutters") + " for then?",
                            "~r~Suspect~s~: Nothing.. I'm just walking back from work.",
                            "~b~You~s~: Is that so? Where do you work?",
                            "~r~Suspect~s~: Uhhhh... J-just up the road...",
                            "~b~You~s~: Okay, I'm not buying it. Spin around and put your hands behind your back for me."
                            };
                        }
                        else if (roll == 2)
                        {
                            lines = new string[]
                            {
                            "~o~Subject~s~: Hello, officer. Can I help you?",
                            "~b~You~s~: Yes, you can actually. You match the description of someone we're looking for.",
                            "~o~Subject~s~: Right.. why are you looking for them, may I ask?",
                            "~b~You~s~: We've had a call about an attempted break in at one of these houses.",
                            "~o~Subject~s~: Hmm.. I don't know anything about that, officer.",
                            "~b~You~s~: Okay, I'm going to go ahead and search you for the safety of both of us.",
                            "~o~Subject~s~: Sure thing...",
                            "~b~You~s~: Fancy explaining what " + (hammerOrCutters == "hammer" ? "this hammer is" : "these wire cutters are") + " for?",
                            "~r~Suspect~s~: F*ck, alright. I was going to break into this house here. Times are tough, alright?!",
                            "~b~You~s~: Understandable. Either way, burglary is a crime and you're under arrest.",
                            "~b~You~s~: Put your hands behind your back for me."
                            };
                        }
                    }

                    while (currentLineIndex < lines.Length)
                    {
                        await BaseScript.Delay(0);
                        if (IsControlJustPressed(0, 46) && Game.PlayerPed.IsInRangeOf(this.suspect.Position, 5f))
                        {
                            ShowSubtitle(lines[currentLineIndex]);
                            currentLineIndex += 1;
                        }
                    }

                    ShowHelp("Deal with the situation as you see fit, then press ~INPUT_DUCK~ + ~INPUT_TALK~ to end the callout.");
                }
                await Task.FromResult(0);
            }
            catch (Exception)
            { }
        }

        private async Task Code4Check()
        {
            try
            {
                while (IsControlPressed(0, 36))
                {
                    await BaseScript.Delay(0);
                    if (IsControlJustReleased(0, 46))
                    {
                        EndCallout();
                    }
                }
                await Task.FromResult(0);
            }
            catch (Exception)
            { }
        }

        public override void OnCancelBefore()
        {
            try
            {
                base.OnCancelBefore();

                if (this.suspect.AttachedBlip != null && this.suspect.AttachedBlip.Exists()) this.suspect.AttachedBlip.Delete();
                if (this.suspectBlip != null && this.suspectBlip.Exists()) this.suspectBlip.Delete();
                if (this.suspect != null && this.suspect.Exists()) this.suspect.IsPersistent = false;
                if (this.dog != null && this.dog.Exists())
                {
                    this.dog.Task.FollowToOffsetFromEntity(this.suspect, new Vector3(0f, 0f, 2f), -1, 2f);
                    this.dog.IsPersistent = false;
                }

                Tick -= Code4Check;
            }
            catch (Exception)
            { }
        }
    }
}
