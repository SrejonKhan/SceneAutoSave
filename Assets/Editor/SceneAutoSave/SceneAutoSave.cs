using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneAutoSave : EditorWindow
{
    public int saveInterval = 60;
    public int nextSave = 0;

    private int timeToSave;
    private bool canSave = true;

    [MenuItem("Window/SceneAutoSave")]
    static void ShowWindow()
    {
        SceneAutoSave window = (SceneAutoSave)EditorWindow.GetWindowWithRect(typeof(SceneAutoSave), new Rect(0, 0, 400, 200));

        var showWithMode = typeof(EditorWindow).GetMethod("ShowPopupWithMode", BindingFlags.Instance | BindingFlags.NonPublic);
        var menuType = 0;
        showWithMode.Invoke(window, new object[] { menuType, true });
    }

    void OnGUI()
    {
        GUIStyle headerStyle = new GUIStyle
        {
            fontSize = 25,
            alignment = TextAnchor.UpperCenter
        };

        //Adjust with selected editor theme
        if (EditorGUIUtility.isProSkin)
        {
            headerStyle.normal.textColor = Color.white;
        }

        //Header
        GUILayout.BeginVertical();
        GUI.Label(new Rect(position.width / 4, 0, (position.width / 2), 30), "Scene Auto Save", headerStyle);
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical();
        saveInterval = EditorGUI.IntField(new Rect(10, 40, (position.width - 20), 20) , "Save Interval (sec)", saveInterval);
        EditorGUI.LabelField(new Rect(10, 60, (position.width - 20), 20), "Next Save", timeToSave.ToString() + " sec");
        if (GUI.Button(new Rect(10, 90, (position.width - 20), 30), "Open Save Location")) OpenSaveLocation();
        if (GUI.Button(new Rect(10, 125, (position.width - 20), 30), canSave ? "STOP" : "START")) ToggleSaving(); 
        GUILayout.EndHorizontal();

        //focus loss
        if (GUI.Button(new Rect(0,0, 1000,1000), "", GUIStyle.none))
        {
            GUI.FocusControl(null);
        }
        Repaint();
    }

    void OnInspectorUpdate()
    {
        if (!canSave) return;
        timeToSave = nextSave - (int)EditorApplication.timeSinceStartup;
        if (EditorApplication.timeSinceStartup > nextSave)
            Save();
    }

    void Save()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        nextSave = (int)EditorApplication.timeSinceStartup + saveInterval;

        if (EditorApplication.isCompiling || EditorApplication.isPlaying || !activeScene.isDirty) 
            return;
        
        string sceneName = DateTime.Now.ToString("hh-mm-ss tt d_M_yyyy") + "_" + activeScene.name;
        string backupFolderPath = Path.Combine(Application.dataPath, $"../Temp/Autosave/{activeScene.name}");
        //create dir if not exists
        if (!Directory.Exists(backupFolderPath))
            Directory.CreateDirectory(backupFolderPath);

        //save scene
        EditorSceneManager.SaveScene(activeScene, backupFolderPath + $"/{sceneName}.unity", true);
        //update timer                                    
    }

    void ToggleSaving()
    {
        canSave = !canSave;
        nextSave = (int)EditorApplication.timeSinceStartup + saveInterval;
    }

    void OpenSaveLocation()
    {
        string backupFolderPath = Path.Combine(Application.dataPath, $"../Temp/Autosave/{EditorSceneManager.GetActiveScene().name}");

        if (!Directory.Exists(backupFolderPath))
            Directory.CreateDirectory(backupFolderPath);

        EditorUtility.RevealInFinder(backupFolderPath);
    }
}