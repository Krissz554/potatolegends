using UnityEngine;
using UnityEditor;

namespace PotatoLegends.Editor
{
    public class TestMenu
    {
        [MenuItem("Potato Legends/Test Menu")]
        public static void TestMenu()
        {
            Debug.Log("✅ Test menu working! Only 2 options should be visible.");
        }
    }
}