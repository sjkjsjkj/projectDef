using System.IO;

/// <summary>씬의 논리 ID는 Build Settings의 순서와 무관합니다.</summary>
public static class SceneCatalog
{
    public const string MainScene = "MainScene";
    public const string StageScene = "StageScene";

    public static EScene Identify(string nameOrPath)
    {
        if (string.IsNullOrWhiteSpace(nameOrPath)) return EScene.Unknown;
        switch (Path.GetFileNameWithoutExtension(nameOrPath.Replace('\\', '/')))
        {
            case MainScene: return EScene.Main;
            case StageScene: return EScene.Stage;
            case "BootScene": return EScene.Boot;
            case "TitleScene": return EScene.Title;
            case "ForestScene": return EScene.Forest;
            case "CaveScene": return EScene.Cave;
            default: return EScene.Unknown;
        }
    }
}
