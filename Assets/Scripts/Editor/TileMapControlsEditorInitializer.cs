using System.Linq;
using UnityEditor;
using UnityEngine;




[InitializeOnLoad]
public static class TileMapControlsEditorInitializer
{   

    private static bool initialized = false;
    static TileMapControlsEditorInitializer ()
    {
        // Code that runs as the application starts
        //EditorApplication.delayCall += UpdateAllViewMode;
        EditorApplication.update += WaitForFocusToDoUpdate;
    }

    // Update All modes
    public static void WaitForFocusToDoUpdate()
    {
        if (initialized) return;
        initialized = true;

        
    }
    

}
