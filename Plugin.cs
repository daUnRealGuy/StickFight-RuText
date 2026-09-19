using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking.Types;
using UnityEngine.SceneManagement;

namespace StickFightRusText
{
    [BepInPlugin("exmagikguy.stickfightthegame.rutext","Rus Text","1.1.1")]
    [BepInProcess("StickFight.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin mevar;
        public static TMP_FontAsset rusHandleFont=null;
        public static FieldInfo localChatManagerField=typeof(NetworkPlayer).GetField("mLocalChatManager",BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Static);
        public static FieldInfo chatFieldField=typeof(ChatManager).GetField("chatField",BindingFlags.NonPublic|BindingFlags.Instance);
        public static FieldInfo textField=typeof(ChatManager).GetField("text",BindingFlags.NonPublic|BindingFlags.Instance);
        public static FieldInfo playerTextsField=typeof(OnlinePlayerUI).GetField("mPlayerTexts",BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.Instance);
        public const string Guid="exmagikguy.stickfightthegame.rutext";
        public const string VersionNumber="1.1.1";
        public static Texture2D rusFontAtlas=null;
        public static byte taggedRussian=byte.MinValue;
        public static byte haveTheMod=byte.MinValue;
        public static bool translateBrokenRussian=true;
        public static bool dontProcessNextTalk=false;
        public static bool onlinePlayerUIPendingChange=true;
        public static string prefix="[RuText Mod] ";
        public static Dictionary<string,byte> colorPowerOfTwo=new Dictionary<string,byte>()
        {
            {"y",1},
            {"b",2},
            {"r",4},
            {"g",8}
        };
        public static List<string> englishWords=new List<string>()
        {
            "cool",
            "hi",
            "mega",
            "awesome",
            "yellow",
            "blue",
            "green",
            "red",
            "weapon",
            "hp",
            "lobby",
            "spawn",
            "map",
			":d",
			":c",
			":o",
			"always",
            "eng",
            "english",
            "xd"
        };
        void Awake()
        {
            mevar=this;
            Log(0,"By downloading this mod you confirm that you are RUSSIAN!!");
            //byte[] data = File.ReadAllBytes(Path.Combine(Path.Combine(Assembly.GetExecutingAssembly().Location,".."),"RusFont.png"));
            Texture2D texture2D=new Texture2D(2,2,TextureFormat.RGBA32,false);
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("StickFightRusText.RusFont.png"))
            {
                if (stream==null)
                {
                    return;
                }
                byte[] data=new byte[stream.Length];
                stream.Read(data,0,data.Length);
                texture2D.LoadImage(data);
            }
            rusFontAtlas=texture2D;
            //SceneManager.activeSceneChanged+=OnSceneChanged;
            //Invoke("InjectHandler",0.2f);
            new Harmony("exmagikguy.stickfightthegame.rutext").PatchAll();
            Log(0,"By downloading this mod you confirm that you are RUSSIAN!!!");
        }
        public static void MakeRusFont(TMP_FontAsset original)
        {
            rusHandleFont=TMP_FontAsset.Instantiate<TMP_FontAsset>(original);
            /*
            /////////////////DEBUG////-
            var source=rusHandleFont.atlas;
            Debug.Log(
                $"Atlas: {source.width}x{source.height}, " +
                $"format={source.format}, " +
                $"mipmapCount={source.mipmapCount}, " +
                $"filter={source.filterMode}"
            );
            RenderTexture rt = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32
            );
            Graphics.Blit(source, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;
            Texture2D readable = new Texture2D(
                source.width,
                source.height,
                TextureFormat.RGBA32,
                false
            );
            readable.ReadPixels(
                new Rect(0, 0, source.width, source.height),
                0,
                0
            );
            readable.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            byte[] png = readable.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Path.Combine(Paths.PluginPath,"RuText"),"defaultAtlas.png"),png);
            /////////////////DEBUG////-
            */
            rusHandleFont.atlas=rusFontAtlas;
            rusHandleFont.material.mainTexture=rusFontAtlas;
            rusHandleFont.fontInfo.AtlasHeight=2324f;
            rusHandleFont.fontInfo.Name="StickFight RUS Text";
            List<TMP_Glyph> russianGlyphs=new List<TMP_Glyph>();
            russianGlyphs.AddRange(rusHandleFont.characterDictionary.Values.ToArray());
            //Заглавные
            russianGlyphs.Add(NewGlyph(1040,343,1238,113,128,5f,115f,1f,110f)); //А
            russianGlyphs.Add(NewGlyph(1041,15,1566,86,128,0f,115f,1f,85f)); //Б
            russianGlyphs.Add(NewGlyph(1042,254,1234,79,128,0f,115f,1f,85f)); //В
            russianGlyphs.Add(NewGlyph(1043,639,1049,79,128,0f,115f,1f,85f)); //Г
            russianGlyphs.Add(NewGlyph(1044,898,1226,112,163,0f,135f,1f,115f)); //Д
            russianGlyphs.Add(NewGlyph(1045,443,1053,70,119,0f,112f,1f,85f)); //Е
            russianGlyphs.Add(NewGlyph(1025,582,2157,70,143,0f,135f,1f,85f)); //Ё
            russianGlyphs.Add(NewGlyph(1046,13,1435,123,121,0f,115f,1f,120f)); //Ж
            russianGlyphs.Add(NewGlyph(1047,434,2172,79,129,0f,115f,1f,85f)); //З
            russianGlyphs.Add(NewGlyph(1048,698,1437,92,118,0f,115f,1f,110f)); //И
            russianGlyphs.Add(NewGlyph(1049,3,1022,90,149,0f,140f,1f,110f)); //Й
            russianGlyphs.Add(NewGlyph(1050,349,1053,76,120,0f,115f,1f,85f)); //К
            russianGlyphs.Add(NewGlyph(1051,783,1240,95,127,0f,115f,1f,105f)); //Л
            russianGlyphs.Add(NewGlyph(1052,559,1436,114,120,0f,115f,1f,120f)); //М
            russianGlyphs.Add(NewGlyph(1053,529,1052,91,120,0f,115f,1f,95f)); //Н
            russianGlyphs.Add(NewGlyph(1054,664,1236,109,127,0f,115f,1f,110f)); //О
            russianGlyphs.Add(NewGlyph(1055,463,1240,93,120,0f,115f,1f,95f)); //П
            russianGlyphs.Add(NewGlyph(1056,581,1240,75,120,0f,115f,1f,85f)); //Р
            russianGlyphs.Add(NewGlyph(1057,443,1429,95,128,0f,115f,1f,100f)); //С
            russianGlyphs.Add(NewGlyph(1058,805,1437,99,117,0f,120f,1f,110f)); //Т
            russianGlyphs.Add(NewGlyph(1059,236,1052,100,122,0f,115f,1f,105f)); //У
            russianGlyphs.Add(NewGlyph(1060,9,1240,108,121,0f,115f,1f,115f)); //Ф
            russianGlyphs.Add(NewGlyph(1061,686,2098,97,118,0f,115f,1f,105f)); //Х
            russianGlyphs.Add(NewGlyph(1062,122,1054,104,142,0f,130f,1f,115f)); //Ц
            russianGlyphs.Add(NewGlyph(1063,340,1438,81,116,0f,115f,1f,90f)); //Ч
            russianGlyphs.Add(NewGlyph(1064,730,1055,122,116,0f,115f,1f,120f)); //Ш
            russianGlyphs.Add(NewGlyph(1065,879,1055,137,141,0f,120f,1f,125f)); //Щ
            russianGlyphs.Add(NewGlyph(1066,807,2180,101,116,0f,115f,1f,105f)); //Ъ
            russianGlyphs.Add(NewGlyph(1067,130,1238,109,123,0f,115f,1f,110f)); //Ы
            russianGlyphs.Add(NewGlyph(1068,920,1438,70,116,0f,115f,1f,85f)); //Ь
            russianGlyphs.Add(NewGlyph(1069,147,1433,81,124,0f,115f,1f,90f)); //Э
            russianGlyphs.Add(NewGlyph(1070,113,1569,131,122,0f,115f,1f,140f)); //Ю
            russianGlyphs.Add(NewGlyph(1071,238,1438,82,116,0f,115f,1f,90f)); //Я
            //Строчные
            russianGlyphs.Add(NewGlyph(1072,748,1796,77,89,0f,85f,1f,80f)); //а
            russianGlyphs.Add(NewGlyph(1073,174,2146,79,129,0f,115f,1f,85f)); //б
            russianGlyphs.Add(NewGlyph(1074,672,1798,60,85,0f,85f,1f,70f)); //в
            russianGlyphs.Add(NewGlyph(1075,833,1603,62,85,0f,85f,1f,70f)); //г
            russianGlyphs.Add(NewGlyph(1076,203,1993,95,104,0f,100f,1f,100f)); //д
            russianGlyphs.Add(NewGlyph(1077,638,1601,69,89,0f,85f,1f,85f)); //е
            russianGlyphs.Add(NewGlyph(1105,921,2128,70,116,0f,115f,1f,85f)); //ё
            russianGlyphs.Add(NewGlyph(1078,309,1992,109,86,0f,85f,1f,115f)); //ж
            russianGlyphs.Add(NewGlyph(1079,154,1796,61,89,0f,85f,1f,85f)); //з
            russianGlyphs.Add(NewGlyph(1080,881,1993,69,85,0f,85f,1f,85f)); //и
            russianGlyphs.Add(NewGlyph(1081,267,1565,69,123,0f,115f,1f,85f)); //й
            russianGlyphs.Add(NewGlyph(1082,558,1603,62,85,0f,85f,1f,70f)); //к
            russianGlyphs.Add(NewGlyph(1083,106,1993,76,85,0f,85f,1f,85f)); //л
            russianGlyphs.Add(NewGlyph(1084,765,1993,85,85,0f,85f,1f,95f)); //м
            russianGlyphs.Add(NewGlyph(1085,733,1603,69,85,0f,85f,1f,85f)); //н
            russianGlyphs.Add(NewGlyph(1086,14,1991,81,89,0f,85f,1f,90f)); //о
            russianGlyphs.Add(NewGlyph(1087,844,1798,69,85,0f,85f,1f,80f)); //п
            russianGlyphs.Add(NewGlyph(1088,944,1796,77,118,0f,85f,1f,85f)); //р
            russianGlyphs.Add(NewGlyph(1089,677,1991,66,89,0f,85f,1f,85f)); //с
            russianGlyphs.Add(NewGlyph(1090,6,2188,77,85,0f,85f,1f,85f)); //т
            russianGlyphs.Add(NewGlyph(1091,460,1603,82,116,0f,85f,0.7f,85f)); //у
            russianGlyphs.Add(NewGlyph(1092,422,1767,109,147,0f,125f,1f,115f)); //ф
            russianGlyphs.Add(NewGlyph(1093,231,1798,84,85,0f,85f,1f,85f)); //х
            russianGlyphs.Add(NewGlyph(1094,364,1603,84,104,0f,100f,1f,85f)); //ц
            russianGlyphs.Add(NewGlyph(1095,590,1993,62,85,0f,85f,1f,85f)); //ч
            russianGlyphs.Add(NewGlyph(1096,912,1603,105,85,0f,85f,1f,115f)); //ш
            russianGlyphs.Add(NewGlyph(1097,20,1798,120,104,0f,100f,1f,125f)); //щ
            russianGlyphs.Add(NewGlyph(1098,324,1798,81,85,0f,85f,1f,85f)); //ъ
            russianGlyphs.Add(NewGlyph(1099,556,1798,84,85,0f,85f,1f,95f)); //ы
            russianGlyphs.Add(NewGlyph(1100,100,2188,58,85,0f,85f,1f,85f)); //ь
            russianGlyphs.Add(NewGlyph(1101,428,1991,66,89,0f,90f,1f,85f)); //э
            russianGlyphs.Add(NewGlyph(1102,275,2186,106,89,0f,85f,1f,120f)); //ю
            russianGlyphs.Add(NewGlyph(1103,506,1993,63,85,0f,85f,1f,85f)); //я
            rusHandleFont.AddGlyphInfo(russianGlyphs.ToArray());
            rusHandleFont.ReadFontDefinition();
        }
        /*public static void ChatManagerEnabled()
        {
            mevar.Invoke("InjectHandler",0.2f);
        }*/
        public static TMP_Glyph NewGlyph(int id,int x,int y,int w,int h,float xo=0f,float yo=0f,float sc=1f,float th=1f)
        {
            TMP_Glyph output=new TMP_Glyph();
            output.x=x;
            output.y=y;
            output.width=(float)w;
            output.height=(float)h;
            output.scale=sc;
            output.xOffset=xo;
            output.yOffset=yo;
            output.id=id;
            output.xAdvance=th;
            return output;
        }
        public static void InjectHandler(ChatManager chatManager)
        {
            //if(((ChatManager)localChatManagerField.GetValue(null))!=null)
            //{
            if(rusHandleFont==null){MakeRusFont(((TMP_InputField)chatFieldField.GetValue((ChatManager)localChatManagerField.GetValue(null))).textComponent.font);}
            if(onlinePlayerUIPendingChange)
            {
                foreach(OnlinePlayerUI opui in UnityEngine.Object.FindObjectsOfType<OnlinePlayerUI>())
                {
                    try
                    {
                        TextMeshProUGUI[] mPlayerTexts=((TextMeshProUGUI[])playerTextsField.GetValue(opui));
                        for (int i=0;i<mPlayerTexts.Length;i++)
		                {
			                if(mPlayerTexts[i].linkedTextComponent!=null){mPlayerTexts[i].linkedTextComponent.font=rusHandleFont;}
                            mPlayerTexts[i].font=rusHandleFont;
		                }
                    }catch{}
                }
                onlinePlayerUIPendingChange=false;
            }
            //((TMP_InputField)chatFieldField.GetValue((ChatManager)localChatManagerField.GetValue(null))).textComponent.font.fallbackFontAssets.Insert(0,rusHandleFont);
            ((TMP_InputField)chatFieldField.GetValue(chatManager)).textComponent.font=rusHandleFont;
            ((TextMeshPro)textField.GetValue(chatManager)).font=rusHandleFont;
            //}
            //Log(0,"Inject Handler call!");
        }
        public static void GlobalInjectHandlerQ()
        {
            mevar.Invoke("GlobalInjectHandler",0.2f);
        }
        void GlobalInjectHandler()
        {
            foreach(ChatManager cm in UnityEngine.Object.FindObjectsOfType<ChatManager>())
            {
                if (cm.enabled){InjectHandler(cm);}
            }
        }
        public static void Log(LogLevel level,object log)
        {
            mevar.Logger.Log(level,log);
        }
        public static void Log(int level,object log)
        {
            mevar.Logger.Log((level==0 ? LogLevel.Info:(level==1 ? LogLevel.Warning:LogLevel.Error)),log);
        }
        void LateUpdate()
        {
            Plugin.dontProcessNextTalk=false;
        }
        public static string TranslateBrokenRussianWord(string source)
        {
            string result=source;
            result=result.Replace("bI","ы");
            result=result.ToLower();
            result=result.Replace("uy","й");
            result=result.Replace("iy","й");
            result=result.Replace("sh","ш");
            result=result.Replace("ya","я");
            result=result.Replace("ch","ч");
            result=result.Replace("zel","зел");
            result=result.Replace("zes","жёс");
            result=result.Replace("ze","ж");
            result=result.Replace("pycc","русс");
            result=result.Replace("y","у");
            result=result.Replace("u","у");
            result=result.Replace("i","и");
            result=result.Replace("j","ж");
            result=result.Replace("e","е");
            result=result.Replace("o","о");
            result=result.Replace("b","б");
            result=result.Replace("a","а");
            result=result.Replace("c","ц");
            result=result.Replace("v","в");
            result=result.Replace("k","к");
            result=result.Replace("p","п");
            result=result.Replace("r","р");
            result=result.Replace("s","с");
            result=result.Replace("d","д");
            result=result.Replace("f","ф");
            result=result.Replace("g","г");
            result=result.Replace("h","х");
            result=result.Replace("l","л");
            result=result.Replace("z","з");
            result=result.Replace("n","н");
            result=result.Replace("m","м");
            result=result.Replace("t","т");
            result=result.Replace("x","х");
            return result;
        }
        public static string BreakRussianText(string source)
        {
            string result=source.ToLower();
            result=result.Replace("й","iy");
            result=result.Replace("ц","c");
            result=result.Replace("у","u");
            result=result.Replace("к","k");
            result=result.Replace("е","e");
            result=result.Replace("н","n");
            result=result.Replace("г","g");
            result=result.Replace("ш","sh");
            result=result.Replace("щ","sh");
            result=result.Replace("з","z");
            result=result.Replace("х","h");
            result=result.Replace("ъ","");
            result=result.Replace("ф","f");
            result=result.Replace("ы","bI");
            result=result.Replace("в","v");
            result=result.Replace("а","a");
            result=result.Replace("п","p");
            result=result.Replace("р","r");
            result=result.Replace("о","o");
            result=result.Replace("л","l");
            result=result.Replace("д","d");
            result=result.Replace("ж","j");
            result=result.Replace("э","e");
            result=result.Replace("я","ya");
            result=result.Replace("ч","ch");
            result=result.Replace("с","s");
            result=result.Replace("м","m");
            result=result.Replace("и","i");
            result=result.Replace("т","t");
            result=result.Replace("ь","");
            result=result.Replace("б","b");
            result=result.Replace("ю","iu");
            return result;
        }
        public static string TranslateBrokenRussianSentence(string source)
        {
            string[] words=source.Split(' ');
            string result="";
            bool anythingTranslated=false;
            foreach(string word in words)
            {
                if(!englishWords.Contains(word.ToLower()))
                {
                    result+=TranslateBrokenRussianWord(word);
                    anythingTranslated=true;
                }
                else
                {
                    result+=word;
                }
                result+=" ";
            }
            if(!anythingTranslated){return source;}
            string fResult="";
            bool isFirst=true;
            foreach(char ch in result.ToCharArray())
            {
                if(isFirst)
                {
                    fResult+=ch.ToString().ToUpper();
                    isFirst=false;
                    continue;
                }
                fResult+=ch.ToString();
            }
            return fResult;
        }
    }
}
