using UnityEngine;
using System;
using System.Collections.Generic;

namespace PotatoLegends.Utils
{
    public static class JsonHelper
    {
        public static T[] FromJsonArray<T>(string json)
        {
            // Unity's JsonUtility doesn't support arrays directly, so we need a wrapper
            string wrappedJson = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
            return wrapper.items;
        }

        public static string ToJsonArray<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.items = array;
            string json = JsonUtility.ToJson(wrapper);
            // Remove the wrapper part
            return json.Substring(9, json.Length - 10); // Remove {"items": and }
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }

        public static string ToJson(object obj)
        {
            return JsonUtility.ToJson(obj);
        }

        public static T FromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }
    }
}