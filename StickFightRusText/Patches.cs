using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace StickFightRusText
{
    [HarmonyPatch]
    public class Patches
    {
        /*[HarmonyPatch(typeof(Controller),"Start")]
        [HarmonyPostfix]
        static void ControllerStartPatch(Controller __instance)
        {
            if(!MatchmakingHandler.Instance.IsInsideLobby){return;}
            Plugin.InjectHandler(__instance.GetComponentInChildren<ChatManager>());
        }*/
        [HarmonyPatch(typeof(ChatManager),"Start")]
        [HarmonyPostfix]
        static void ChatAwakePatch(ChatManager __instance)
        {
            if(__instance.enabled&&__instance.GetComponentInParent<global::NetworkPlayer>().HasLocalControl)
            {
                Plugin.GlobalInjectHandlerQ(); //Plugin.InjectHandler(__instance);
            }
        }
        /*[HarmonyPatch(typeof(TMP_FontAsset),"HasCharacter",new Type[]{typeof(char),typeof(bool)})]
        [HarmonyPostfix]
        static void HasCharDebugPatch(TMP_FontAsset __instance,char character,bool searchFallbacks)
        {
            if((int)character>0)
            {
                Plugin.Log(0,(int)character);
            }
        }*/
        [HarmonyPatch(typeof(MatchmakingHandler),"Disconnect")]
        [HarmonyPostfix]
        static void ResetTags(MatchmakingHandler __instance,bool showScreen=true)
        {
            Plugin.taggedRussian=0;
            Plugin.haveTheMod=0;
        }
        [HarmonyPatch(typeof(ChatManager),"SendChatMessage")]
        [HarmonyPrefix]
        static bool Commands(ChatManager __instance,string message)
        {
            if(__instance.GetComponentInParent<global::NetworkPlayer>().HasLocalControl)
            {
                if(message=="#rutext_toggletrans")
                {
                    Plugin.translateBrokenRussian=!Plugin.translateBrokenRussian;
                    //__instance.Talk($"Toggled Translit Translation. (Now {Plugin.translateBrokenRussian})");
                    __instance.Talk($"Переключен перевод Translit. (Теперь {Plugin.translateBrokenRussian})");
                    return false;
                }
                else if(message=="#rutext_toggleprefix")
                {
                    if(Plugin.prefix.Length<5)
                    {
                        Plugin.prefix="[RuText Mod] ";
                    }
                    else
                    {
                        Plugin.prefix="[R: ";
                    }
                    __instance.Talk($"Переключен полный префикс. (Теперь {Plugin.prefix.Length>5})");
                    return false;
                }
                else if(message.StartsWith("#")&&message.EndsWith("rus"))
                {
                    char ch=message[1];
                    byte n=Plugin.colorPowerOfTwo[ch.ToString()];
                    Plugin.taggedRussian|=n;
                    //__instance.Talk($"{(ch=='y' ? "Yellow":(ch=='b' ? "Blue":(ch=='r' ? "Red":"Green")))} is now set to {((Plugin.taggedRussian&n)==n ? "":"not ")}be russian!");
                    __instance.Talk($"{(ch=='y' ? "Жёлтый":(ch=='b' ? "Синий":(ch=='r' ? "Красный":"Зелёный")))} теперь помечен как {((Plugin.taggedRussian&n)==n ? "":"не ")}русский!");
                    return false;
                }
                else if(message.StartsWith("#")&&message.EndsWith("rutxt"))
                {
                    char ch=message[1];
                    byte n=Plugin.colorPowerOfTwo[ch.ToString()];
                    Plugin.haveTheMod|=n;
                    //__instance.Talk($"{(ch=='y' ? "Yellow":(ch=='b' ? "Blue":(ch=='r' ? "Red":"Green")))} is now set to {((Plugin.taggedRussian&n)==n ? "":"not ")}be russian!");
                    __instance.Talk($"{(ch=='y' ? "Жёлтый":(ch=='b' ? "Синий":(ch=='r' ? "Красный":"Зелёный")))} теперь считается {((Plugin.haveTheMod&n)==n ? "с":"без")} RuText Mod!");
                    return false;
                }
                else if(message=="#rutext_resettags")
                {
                    Plugin.taggedRussian=0;
                    Plugin.haveTheMod=0;
                    //__instance.Talk("Reset Russian Tags.");
                    __instance.Talk("Сброшены пометки русских.");
                    return false;
                }
            }
            return true;
        }
        [HarmonyPatch(typeof(ChatManager),"Talk")]
        [HarmonyPrefix]
        static bool TranslateTranslit(ChatManager __instance,string t)
        {
            if(Plugin.dontProcessNextTalk){Plugin.dontProcessNextTalk=false;return true;}
            var netp=__instance.GetComponentInParent<global::NetworkPlayer>();
            byte n=Plugin.colorPowerOfTwo.Values.ToArray()[(int)netp.NetworkSpawnID];
            Plugin.InjectHandler(__instance);
            if(!netp.HasLocalControl&&Plugin.translateBrokenRussian&&(Plugin.taggedRussian&n)==n)
            {
                bool isRus=false;
                foreach(char ch in t.ToCharArray())
                {
                    int cha=(int)ch;
                    if(cha==168||cha==184||cha>191){isRus=true;break;}
                }
                if(!isRus)
                {
                    Plugin.dontProcessNextTalk=true;
                    __instance.Talk(Plugin.TranslateBrokenRussianSentence(t));
                    return false;
                }
            }
            return true;
        }
        [HarmonyPatch(typeof(P2PPackageHandler),"SendP2PPacketToUser",new Type[] {typeof(CSteamID),typeof(byte[]),typeof(P2PPackageHandler.MsgType),typeof(EP2PSend),typeof(int)})]
        [HarmonyPrefix]
        static bool NotifyRuTextMod(P2PPackageHandler __instance,CSteamID clientID,byte[] data,P2PPackageHandler.MsgType messageType,EP2PSend sendMethod=EP2PSend.k_EP2PSendReliable,int channel=0)
        {
            if(messageType==P2PPackageHandler.MsgType.PlayerTalked&&data[0]!=(byte)'['&&data[1]!=(byte)'R')
            {
                int id=-1;
                for(int i=0;i<GameManager.Instance.mMultiplayerManager.ConnectedClients.Length;i++)
                {
                    var cdata=GameManager.Instance.mMultiplayerManager.ConnectedClients[i];
                    if(cdata!=null&&cdata.ClientID==clientID){id=i;break;}
                }
                if(id==-1||id==GameManager.Instance.mMultiplayerManager.LocalPlayerIndex){return true;}
                byte n=Plugin.colorPowerOfTwo.Values.ToArray()[(int)id];
                if((Plugin.haveTheMod&n)!=n)
                {
                    var t=Encoding.UTF8.GetString(data);
                    bool isRus=false;
                    foreach(char ch in t.ToCharArray())
                    {
                        int cha=(int)ch;
                        if(cha==168||cha==184||cha>191){isRus=true;break;}
                    }
                    if(isRus)
                    {
                        byte[] newdata=Encoding.UTF8.GetBytes("[RuText Mod] "+Plugin.BreakRussianText(t));
                        __instance.SendP2PPacketToUser(clientID,newdata,messageType,sendMethod,channel);
                        return false;
                    }
                }
            }
            return true;
        }
        [HarmonyPatch(typeof(OnlinePlayerUI),"Awake")]
        [HarmonyPostfix]
        static void RussifyPlayerNames(OnlinePlayerUI __instance)
        {
            if(Plugin.rusHandleFont==null){Plugin.onlinePlayerUIPendingChange=true;return;}
            TextMeshProUGUI[] mPlayerTexts=((TextMeshProUGUI[])Plugin.playerTextsField.GetValue(__instance));
            for (int i=0;i<mPlayerTexts.Length;i++)
		    {
			    if(mPlayerTexts[i].linkedTextComponent!=null){mPlayerTexts[i].linkedTextComponent.font=Plugin.rusHandleFont;}
                mPlayerTexts[i].font=Plugin.rusHandleFont;
		    }
        }
    }
}
