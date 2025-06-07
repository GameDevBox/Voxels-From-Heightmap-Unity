/*
 * HeightmapGeneratorEditor.cs
 * 
 * Created by Arian - GameDevBox
 * YouTube Channel: https://www.youtube.com/@GameDevBox
 *
 * 🎮 Want more Unity tips, tools, and advanced systems?
 * 🧠 Learn from practical examples and well-explained logic.
 * 📦 Subscribe to GameDevBox for more game dev content!
 *
 * Custom Inspector for the HeightmapGenerator ScriptableObject.
 */


#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HeightmapGenerator))]
public class HeightmapGeneratorEditor : Editor
{
    SerializedProperty width, height, format;
    SerializedProperty noiseScale, octaves, persistence, lacunarity, offset, heightMultiplier;
    SerializedProperty enableSharpness, sharpnessIterations, sharpnessIntensity;
    SerializedProperty randomizeSeed, seed, randomOffsetRange, randomScaleRange;
    SerializedProperty heightmapType;

    bool showNoiseSettings = true;
    bool showSharpnessSettings = false;
    bool showRandomSettings = false;

    private void OnEnable()
    {
        width = serializedObject.FindProperty("width");
        height = serializedObject.FindProperty("height");
        format = serializedObject.FindProperty("format");

        noiseScale = serializedObject.FindProperty("noiseScale");
        octaves = serializedObject.FindProperty("octaves");
        persistence = serializedObject.FindProperty("persistence");
        lacunarity = serializedObject.FindProperty("lacunarity");
        offset = serializedObject.FindProperty("offset");
        heightMultiplier = serializedObject.FindProperty("heightMultiplier");

        enableSharpness = serializedObject.FindProperty("enableSharpness");
        sharpnessIterations = serializedObject.FindProperty("sharpnessIterations");
        sharpnessIntensity = serializedObject.FindProperty("sharpnessIntensity");

        randomizeSeed = serializedObject.FindProperty("randomizeSeed");
        seed = serializedObject.FindProperty("seed");
        randomOffsetRange = serializedObject.FindProperty("randomOffsetRange");
        randomScaleRange = serializedObject.FindProperty("randomScaleRange");

        heightmapType = serializedObject.FindProperty("heightmapType");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Heightmap Basic Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(height);
        EditorGUILayout.PropertyField(format);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(heightmapType);

        EditorGUILayout.Space();

        showNoiseSettings = EditorGUILayout.Foldout(showNoiseSettings, "Noise Settings");
        if (showNoiseSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(noiseScale, new GUIContent("Noise Scale"));
            EditorGUILayout.PropertyField(octaves);
            EditorGUILayout.PropertyField(persistence);
            EditorGUILayout.PropertyField(lacunarity);
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(heightMultiplier);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        showSharpnessSettings = EditorGUILayout.Foldout(showSharpnessSettings, "Sharpness Settings");
        if (showSharpnessSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(enableSharpness);
            if (enableSharpness.boolValue)
            {
                EditorGUILayout.PropertyField(sharpnessIterations);
                EditorGUILayout.PropertyField(sharpnessIntensity);
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        showRandomSettings = EditorGUILayout.Foldout(showRandomSettings, "Randomization Settings");
        if (showRandomSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(randomizeSeed);
            if (!randomizeSeed.boolValue)
            {
                EditorGUILayout.PropertyField(seed);
            }
            EditorGUILayout.PropertyField(randomOffsetRange);
            EditorGUILayout.PropertyField(randomScaleRange);
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
