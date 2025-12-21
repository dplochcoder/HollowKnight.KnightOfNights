using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace KnightOfNights.Scripts.Lib
{
    public class SceneDataPack
    {
        private const string PATH = "Assets/Resources/Data/scene_data.json";

        private SortedDictionary<string, List<object>> data = new SortedDictionary<string, List<object>>();

        public SceneDataPack() { Dirty = true;  }

        public static SceneDataPack Load()
        {
            var pack = new SceneDataPack();

            var fr = System.IO.File.OpenRead(PATH);
            try
            {
                var sr = new System.IO.StreamReader(fr);
                pack.data = Deserialize<SortedDictionary<string, List<object>>>(sr.ReadToEnd());
            }
            finally { fr.Close(); }

            pack.Dirty = false;
            return pack;
        }

        public bool Save()
        {
            if (!Dirty) return false;

            var fw = System.IO.File.Open(PATH, System.IO.FileMode.Truncate);
            try
            {
                var sw = new System.IO.StreamWriter(fw);
                sw.Write(Serialize(data));
                sw.Flush();
            }
            finally { fw.Close(); }

            Dirty = false;
            return true;
        }

        public bool Dirty { get; private set; }

        public void Clear()
        {
            Dirty |= data.Count > 0;
            data.Clear();
        }

        public bool Update(string scene, List<object> objects)
        {
            if (data.TryGetValue(scene, out var prev))
            {
                if (objects.Count == 0)
                {
                    data.Remove(scene);
                    Dirty = true;
                    return true;
                }
                else
                {
                    var prevStr = Serialize(prev);
                    var newStr = Serialize(objects);
                    if (prevStr == newStr) return false;

                    data[scene] = objects;
                    Dirty = true;
                    return true;
                }
            }

            if (objects.Count == 0) return false;

            data[scene] = objects;
            Dirty = true;
            return true;
        }

        private static string FixType(string typeString, bool forMod)
        {
            var parts = typeString.Split(new string[] { ", " }, System.StringSplitOptions.None);
            var type = parts[0];
            var isSharedLib = type.StartsWith("KnightOfNights") && type.Contains("SharedLib.Data");

            if (forMod) return isSharedLib ? $"{type}, KnightOfNights" : typeString;
            else return isSharedLib ? $"{type}, Assembly-CSharp" : typeString;
        }

        private static void FixTypes(JToken token, bool forMod)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    {
                        foreach (var prop in ((JObject)token).Properties())
                        {
                            if (prop.Name == "$type")
                            {
                                var stringToken = (JValue)prop.Value;
                                stringToken.Value = FixType((string)stringToken.Value, forMod);
                            }
                            else FixTypes(prop.Value, forMod);
                        }
                        break;
                    }
                case JTokenType.Array:
                    {
                        foreach (var item in (JArray)token) FixTypes(item, forMod);
                        break;
                    }
                default:
                    break;
            }
        }

        private static string Serialize<T>(T obj)
        {
            JsonSerializer serializer = new JsonSerializer
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var stringWriter = new System.IO.StringWriter();
            serializer.Serialize(stringWriter, obj, typeof(T));

            var token = JToken.Parse(stringWriter.GetStringBuilder().ToString());
            FixTypes(token, true);

            return token.ToString(Formatting.Indented);
        }

        private static T Deserialize<T>(string json)
        {
            var token = JToken.Parse(json);
            FixTypes(token, false);

            return JsonConvert.DeserializeObject<T>(token.ToString(Formatting.None), new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }
    }
}
