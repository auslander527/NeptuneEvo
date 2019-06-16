using GTANetworkAPI;
using System;
using System.Collections.Generic;
using NeptuneEvo.GUI;
using NeptuneEvo.Core;
using Redage.SDK;

namespace NeptuneEvo.Voice
{
    public class Voice : Script
    {
        private static nLog Log = new nLog("Voice");
        public Voice()
        {
            RoomController.getInstance().CreateRoom("VoiceRoom");
        }

        public Client GetPlayerById(int id)
        {
            Client target = null;
            foreach (Client player in NAPI.Pools.GetAllPlayers())
            {
                if (player.Value == id)
                {
                    target = player;
                    break;
                }
            }
            return target;
        }
        
        public static void PlayerJoin(Client player)
        {
            try
            {
                VoiceMetaData DefaultVoiceMeta = new VoiceMetaData
                {
                    IsEnabledMicrophone = false,
                    RadioRoom = "",
                    StateConnection = "closed",
                    MicrophoneKey = 78 // N
                };

                VoicePhoneMetaData DefaultVoicePhoneMeta = new VoicePhoneMetaData
                {
                    CallingState = "nothing",
                    Target = null
                };

                player.SetData("Voip", DefaultVoiceMeta);
                player.SetData("PhoneVoip", DefaultVoicePhoneMeta);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        
        public static void PlayerQuit(Client player, string reson)
        {
            try
            {
                RoomController controller = RoomController.getInstance();
                VoiceMetaData voiceMeta = player.GetData("Voip");

                if (controller.HasRoom(voiceMeta.RadioRoom))
                {
                    controller.OnQuit(voiceMeta.RadioRoom, player);
                }

                VoicePhoneMetaData playerPhoneMeta = player.GetData("PhoneVoip");

                if (playerPhoneMeta.Target != null)
                {
                    Client target = playerPhoneMeta.Target;
                    VoicePhoneMetaData targetPhoneMeta = target.GetData("PhoneVoip");

                    var pSim = Main.Players[player].Sim;
                    var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();

                    Notify.Send(target, NotifyType.Alert, NotifyPosition.BottomCenter, $"{playerName} завершил вызов", 3000);
                    targetPhoneMeta.Target = null;
                    targetPhoneMeta.CallingState = "nothing";

                    target.ResetData("AntiAnimDown");
                    if (!target.IsInVehicle) target.StopAnimation();
                    else target.SetData("ToResetAnimPhone", true);

                    Core.BasicSync.DetachObject(target);

                    Trigger.ClientEvent(target, "voice.phoneStop");

                    target.SetData("PhoneVoip", targetPhoneMeta);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        [Command("v_reload")]
        public void voiceDebugReload(Client player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v1).");
            Trigger.ClientEvent(player, "v_reload");
        }

        [Command("v_reload2")]
        public void voiceDebug2Reload(Client player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v2).");
            Trigger.ClientEvent(player, "v_reload2");
        }

        [Command("v_reload3")]
        public void voiceDebug3Reload(Client player)
        {
            player.SendChatMessage("Вы успешно перезагрузили голосовой чат для себя (v3).");
            Trigger.ClientEvent(player, "v_reload3");
        }

        [RemoteEvent("add_voice_listener")]
        public void add_voice_listener(Client player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                Client target = (Client)arguments[0];
                if (!Main.Players.ContainsKey(target)) return;
                player.EnableVoiceTo(target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [RemoteEvent("remove_voice_listener")]
        public void remove_voice_listener(Client player, params object[] arguments)
        {
            try
            {
                if (!Main.Players.ContainsKey(player)) return;
                Client target = (Client)arguments[0];
                if (!Main.Players.ContainsKey(target)) return;
                player.DisableVoiceTo(target);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // METHODS //
        
        public static void PhoneCallCommand(Client player, Client target)
        {
            try
            {
                if (player.HasData("AntiAnimDown"))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Невозможно достать мобильный телефон", 3000);
                    return;
                }
                if (target != null && Main.Players.ContainsKey(target))
                {
                    VoicePhoneMetaData targetPhoneMeta = target.GetData("PhoneVoip");
                    VoicePhoneMetaData playerPhoneMeta = player.GetData("PhoneVoip");

                    if (playerPhoneMeta.Target != null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "В данный момент Вы уже разговариваете", 3000);
                        return;
                    }

                    var tSim = Main.Players[target].Sim;
                    var pSim = Main.Players[player].Sim;

                    var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                    var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                    if (targetPhoneMeta.Target != null)
                    {
                        Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент {targetName} занят", 3000);
                        Notify.Send(target, NotifyType.Alert, NotifyPosition.BottomCenter, $"{playerName} пытался Вам дозвониться", 3000);
                        return;
                    }

                    targetPhoneMeta.Target = player;
                    targetPhoneMeta.CallingState = "callMe";

                    playerPhoneMeta.Target = target;
                    playerPhoneMeta.CallingState = "callTo";

                    Main.OnAntiAnim(player);
                    player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                    Core.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));

                    player.SetData("PhoneVoip", playerPhoneMeta);
                    target.SetData("PhoneVoip", targetPhoneMeta);

                    NAPI.Task.Run(() => {
                        try
                        {
                            if (!Main.Players.ContainsKey(player) || !Main.Players.ContainsKey(target)) return;

                            VoicePhoneMetaData tPhoneMeta = target.GetData("PhoneVoip");
                            VoicePhoneMetaData pPhoneMeta = player.GetData("PhoneVoip");

                            if (pPhoneMeta.Target == null || pPhoneMeta.Target != target || pPhoneMeta.CallingState == "talk") return;

                            pPhoneMeta.Target = null;
                            tPhoneMeta.Target = null;

                            pPhoneMeta.CallingState = "nothing";
                            tPhoneMeta.CallingState = "nothing";

                            if (!player.IsInVehicle)
                                player.StopAnimation();
                            else
                                player.SetData("ToResetAnimPhone", true);
                            Core.BasicSync.DetachObject(player);

                            player.SetData("PhoneVoip", pPhoneMeta);
                            target.SetData("PhoneVoip", tPhoneMeta);

                            player.ResetData("AntiAnimDown");

                            Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, $"{targetName} не отвечает", 3000);
                            Notify.Send(target, NotifyType.Alert, NotifyPosition.BottomCenter, $"{playerName} завершил вызов", 3000);
                        }
                        catch { }
                        
                    }, 20000);

                    Notify.Send(target, NotifyType.Alert, NotifyPosition.BottomCenter, $"{playerName} звонит Вам. Откройте телефон, чтобы принять/отклонить вызов", 3000);
                    Notify.Send(player, NotifyType.Alert, NotifyPosition.BottomCenter, $"Вы звоните {targetName}", 3000);
                }
                else
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, "Абонент вне зоны действия сети", 3000);
                }

            }
            catch (Exception e)
            {
                Log.Write($"PhoneCall: {e.Message}", nLog.Type.Error);
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void Event_PlayerExitVehicle(Client player, Vehicle veh)
        {
            try
            {
                if (player.HasData("ToResetAnimPhone"))
                {
                    player.StopAnimation();
                    player.ResetData("ToResetAnimPhone");
                }
            }
            catch { }
        }

        //[Command("ca")]
        public static void PhoneCallAcceptCommand(Client player)
        {
            try
            {
                VoicePhoneMetaData playerPhoneMeta = player.GetData("PhoneVoip");

                if (playerPhoneMeta.Target == null || playerPhoneMeta.CallingState == "callTo" || !Main.Players.ContainsKey(playerPhoneMeta.Target))
                {
                    Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент Вам никто не звонит", 3000);
                    return;
                }

                Client target = playerPhoneMeta.Target;

                VoicePhoneMetaData targetPhoneMeta = target.GetData("PhoneVoip");

                playerPhoneMeta.CallingState = "talk";
                targetPhoneMeta.CallingState = "talk";

                var tSim = Main.Players[target].Sim;
                var pSim = Main.Players[player].Sim;

                var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"{playerName} принял Ваш вызов", 3000);
                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Вы приняли вызов от {targetName}", 3000);

                Main.OnAntiAnim(player);
                player.PlayAnimation("anim@cellphone@in_car@ds", "cellphone_call_listen_base", 49);
                Core.BasicSync.AttachObjectToPlayer(player, NAPI.Util.GetHashKey("prop_amb_phone"), 6286, new Vector3(0.06, 0.01, -0.02), new Vector3(80, -10, 110));

                Trigger.ClientEvent(player, "voice.phoneCall", target, 1);
                Trigger.ClientEvent(target, "voice.phoneCall", player, 1);

                player.ResetData("ToResetAnimPhone");
                target.ResetData("ToResetAnimPhone");

                player.SetData("PhoneVoip", playerPhoneMeta);
                target.SetData("PhoneVoip", targetPhoneMeta);
            }
            catch (Exception e)
            {
                Log.Write($"PhoneCallAccept: {e.Message}", nLog.Type.Error);
            }
        }

        //[Command("h")]
        public static void PhoneHCommand(Client player)
        {
            try
            {
                VoicePhoneMetaData playerPhoneMeta = player.GetData("PhoneVoip");

                if (playerPhoneMeta.Target == null || !Main.Players.ContainsKey(playerPhoneMeta.Target))
                {
                    if (!player.HasData("IS_DYING") && !player.GetData("CUFFED")) Notify.Send(player, NotifyType.Error, NotifyPosition.BottomCenter, $"В данный момент Вы не говорите по телефону", 3000);
                    return;
                }

                Client target = playerPhoneMeta.Target;
                VoicePhoneMetaData targetPhoneMeta = target.GetData("PhoneVoip");

                var tSim = Main.Players[target].Sim;
                var pSim = Main.Players[player].Sim;

                var playerName = (Main.Players[target].Contacts.ContainsKey(pSim)) ? Main.Players[target].Contacts[pSim] : pSim.ToString();
                var targetName = (Main.Players[player].Contacts.ContainsKey(tSim)) ? Main.Players[player].Contacts[tSim] : tSim.ToString();

                Notify.Send(player, NotifyType.Success, NotifyPosition.BottomCenter, $"Звонок завершен", 3000);
                Notify.Send(target, NotifyType.Success, NotifyPosition.BottomCenter, $"{playerName} завершил звонок", 3000);

                playerPhoneMeta.Target = null;
                targetPhoneMeta.Target = null;

                playerPhoneMeta.CallingState = "nothing";
                targetPhoneMeta.CallingState = "nothing";

                if (!player.IsInVehicle) player.StopAnimation();
                if (!target.IsInVehicle) target.StopAnimation();

                player.ResetData("AntiAnimDown");
                target.ResetData("AntiAnimDown");
                if (player.IsInVehicle) player.SetData("ToResetAnimPhone", true);
                if (player.IsInVehicle) target.SetData("ToResetAnimPhone", true);

                Core.BasicSync.DetachObject(player);
                Core.BasicSync.DetachObject(target);

                Trigger.ClientEvent(player, "voice.phoneStop");
                Trigger.ClientEvent(target, "voice.phoneStop");

                player.SetData("PhoneVoip", playerPhoneMeta);
                target.SetData("PhoneVoip", targetPhoneMeta);
            }
            catch (Exception e)
            {
                Log.Write($"PhoneCallCancel: {e.Message}", nLog.Type.Error);
            }
        }

        //[Command("changeroom")]
        public void ChangeRoomCommand(Client player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();
                    VoiceMetaData voiceMeta = player.GetData("Voip");

                    if (controller.HasRoom(name))
                    {
                        if (name.Equals(voiceMeta.RadioRoom))
                        {
                            player.SendChatMessage("You are already on this room");
                            return;
                        }

                        controller.OnQuit(name, player);
                        controller.OnJoin(name, player);
                    }
                    else
                    {
                        player.SendChatMessage("This room doesn't exist");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("createroom")]
        public void CreateRoomCommand(Client player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (!controller.HasRoom(name))
                    {
                        controller.CreateRoom(name);

                        player.SendChatMessage("You create room - " + name);
                    }
                    else
                    {
                        player.SendChatMessage("Room already created");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("removeroom")]
        public void RemoveRoomCommand(Client player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (controller.HasRoom(name))
                    {
                        controller.RemoveRoom(name);

                        player.SendChatMessage("You has removed room - " + name);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //[Command("leaveroom")]
        public void LeaveRoomCommand(Client player, string name)
        {
            try
            {
                name = name.ToUpper();

                if (name.Length != 0)
                {
                    RoomController controller = RoomController.getInstance();

                    if (controller.HasRoom(name))
                    {
                        VoiceMetaData voiceMeta = player.GetData("Voip");

                        if (name.Equals(voiceMeta.RadioRoom))
                        {
                            controller.OnQuit(name, player);
                        }

                        player.SendChatMessage("You leave from room - " + name);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void SetVoiceDistance(Client player, float distance)
        {
            player.SetSharedData("voice.distance", distance);
        }

        public float GetVoiceDistance(Client player)
        {
            return player.GetSharedData("voice.distance");
        }

        public bool IsMicrophoneEnabled(Client player)
        {
            VoiceMetaData voiceMeta = player.GetData("Voip");

            return voiceMeta.IsEnabledMicrophone;
        }

        public void SetVoiceMuted(Client player, bool isMuted)
        {
            player.SetSharedData("voice.muted", isMuted);
        }

        public bool GetVoiceMuted(Client player)
        {
            return player.GetSharedData("voice.muted");
        }

        public void SetMicrophoneKey(Client player, int microphoneKey)
        {
            try
            {
                VoiceMetaData voiceMeta = player.GetData("Voip");
                voiceMeta.MicrophoneKey = microphoneKey;

                Trigger.ClientEvent(player, "voice.changeMicrophoneActivationKey", microphoneKey);
                player.SetData("Voip", voiceMeta);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public int GetMicrophoneKey(Client player)
        {
            VoiceMetaData voiceMeta = player.GetData("Voip");
            return voiceMeta.MicrophoneKey;
        }
    }
}