using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

/// <summary>
/// Скрипт для исправления проблем WebGL сборки
/// </summary>
public class WebGLBuildFixer : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
        {
            Debug.Log("=== WebGL Build Preprocessing ===");
            
            // Настраиваем параметры сборки для WebGL
            PlayerSettings.SetManagedStrippingLevel(BuildTarget.WebGL, ManagedStrippingLevel.Disabled);
            PlayerSettings.SetScriptingBackend(BuildTarget.WebGL, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTarget.WebGL, Il2CppCompilerConfiguration.Release);
            
            // Отключаем оптимизации, которые могут вызывать проблемы
            PlayerSettings.SetWebGLMemorySize(BuildTarget.WebGL, 512);
            PlayerSettings.SetWebGLExceptionSupport(BuildTarget.WebGL, WebGLExceptionSupport.ExplicitlyThrow);
            PlayerSettings.SetWebGLNameFilesAsHashes(BuildTarget.WebGL, false);
            PlayerSettings.SetWebGLDataCaching(BuildTarget.WebGL, true);
            
            Debug.Log("✓ WebGL build settings configured");
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
public static class WebGLBuildSettings
{
    static WebGLBuildSettings()
    {
        // Автоматически настраиваем параметры при загрузке проекта
        EditorApplication.delayCall += () =>
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.WebGL)
            {
                ConfigureWebGLSettings();
            }
        };
    }
    
    [MenuItem("Tools/WebGL/Fix Build Settings")]
    public static void ConfigureWebGLSettings()
    {
        Debug.Log("=== Configuring WebGL Build Settings ===");
        
        // Настройки для WebGL
        PlayerSettings.SetManagedStrippingLevel(BuildTarget.WebGL, ManagedStrippingLevel.Disabled);
        PlayerSettings.SetScriptingBackend(BuildTarget.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTarget.WebGL, Il2CppCompilerConfiguration.Release);
        
        // Настройки памяти
        PlayerSettings.SetWebGLMemorySize(BuildTarget.WebGL, 512);
        PlayerSettings.SetWebGLExceptionSupport(BuildTarget.WebGL, WebGLExceptionSupport.ExplicitlyThrow);
        PlayerSettings.SetWebGLNameFilesAsHashes(BuildTarget.WebGL, false);
        PlayerSettings.SetWebGLDataCaching(BuildTarget.WebGL, true);
        
        // Настройки IL2CPP
        PlayerSettings.SetAdditionalIl2CppArgs("--enable-generic-sharing");
        
        Debug.Log("✓ WebGL settings configured successfully");
        Debug.Log("  - Managed Stripping: Disabled");
        Debug.Log("  - Scripting Backend: IL2CPP");
        Debug.Log("  - Memory Size: 512MB");
        Debug.Log("  - Exception Support: Explicitly Throw");
    }
    
    [MenuItem("Tools/WebGL/Clean Build Cache")]
    public static void CleanBuildCache()
    {
        Debug.Log("=== Cleaning WebGL Build Cache ===");
        
        // Очищаем кэш сборки
        System.IO.Directory.Delete("Library/Bee", true);
        System.IO.Directory.Delete("Library/Artifacts", true);
        System.IO.Directory.Delete("Library/ScriptAssemblies", true);
        
        // Очищаем кэш IL2CPP
        System.IO.Directory.Delete("Library/il2cpp_cache", true);
        
        Debug.Log("✓ Build cache cleaned");
        Debug.Log("Please restart Unity Editor for changes to take effect");
    }
}
#endif
