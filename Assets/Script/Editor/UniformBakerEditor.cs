using UnityEngine;
using UnityEngine.VFX;

namespace UnityEditor.VFX
{
    [CustomEditor(typeof(UniformBaker))]
    public class UniformBakerEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Bake"))
            {
                var uniformBaker = target as UniformBaker;
                uniformBaker.Bake();
            }
        }
    }

    [CustomEditor(typeof(UniformBakerCustomOrder))]
    public class UniformBakerCustomOrderEditor : UniformBakerEditor
    {
        override public void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}