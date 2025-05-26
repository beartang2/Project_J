#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class MissingScriptFinder : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts In Scene")]
    public static void ShowWindow()
    {
        GetWindow(typeof(MissingScriptFinder), false, "Missing Script Finder");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Find Missing Scripts"))
        {
            FindMissingScripts();
        }
    }

    private void FindMissingScripts()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();

            foreach (Component c in components)
            {
                if (c == null)
                {
                    Debug.LogWarning($"Missing script found in GameObject: {GetFullPath(go)}", go);
                    count++;
                }
            }
        }

        Debug.Log($"Total missing scripts found: {count}");
    }

    private string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform.parent;

        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
#endif
