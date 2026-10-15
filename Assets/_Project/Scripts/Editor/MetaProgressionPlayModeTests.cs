using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class MetaProgressionPlayModeTests
{
    [UnityTest]
    public IEnumerator MainPurchaseStageBonusesAndResultReturnWorkTogether()
    {
        yield return new EnterPlayMode();
        yield return WaitForFlow();
        Assert.That(SceneFlowManager.Ins.CurrentScene, Is.EqualTo(EScene.Main));
        MetaProgressionManager meta = MetaProgressionManager.Ins;
        // 실제 사용자 저장에는 쓰지 않고 이 실행에만 메모리 저장 함수를 주입합니다.
        var settings = Resources.Load<MetaProgressionSettings>("MetaProgressionSettings");
        var progression = new MetaProgression(settings, new MetaProgression.SaveData { points = 2000 }, _ => { });
        typeof(MetaProgressionManager).GetProperty("Progression").SetValue(meta, progression);
        typeof(MetaProgressionManager).GetProperty("Bonuses").SetValue(meta, MetaUpgradeBonuses.From(progression));

        Button upgrade = GameObject.Find("MainMenuCanvas/Upgrades").GetComponent<Button>();
        Assert.That(upgrade.interactable, Is.True);
        upgrade.onClick.Invoke();
        MetaUpgradePanel panel = Object.FindAnyObjectByType<MetaUpgradePanel>();
        Assert.That(panel, Is.Not.Null);
        Assert.That(panel.GetComponentInChildren<TMPro.TMP_Text>().font.HasCharacter('강', true, true), Is.True,
            "강화 화면에 한글을 표시할 수 있어야 합니다.");
        Button attack = panel.transform.Find("Panel/Viewport/Cards/attack/Buy").GetComponent<Button>();
        attack.onClick.Invoke();
        Assert.That(progression.Level("attack"), Is.EqualTo(1));
        Assert.That(progression.Points, Is.EqualTo(1990));
        foreach (string id in new[] { "enemy_health", "enemy_capacity", "starting_gold", "starting_honor", "starting_refresh", "starting_recruitment" })
            Assert.That(meta.TryPurchase(id, out string reason), Is.True, reason);
        yield return null;
        Canvas.ForceUpdateCanvases();
        CapturePanel(panel);
        panel.gameObject.SetActive(false);
        Assert.That(SceneFlowManager.Ins.StartStage(), Is.True);
        yield return WaitForFlow();
        yield return null;
        Assert.That(GameManager.Ins.State, Is.EqualTo(GameState.Playing));
        Assert.That(meta.TryPurchase("attack", out _), Is.False);

        var resourcesObject = new GameObject("MetaTestResources");
        ResourceManager resources = resourcesObject.AddComponent<ResourceManager>();
        Assert.That(resources.Gold, Is.EqualTo(5));
        Assert.That(resources.HonorLevel, Is.EqualTo(2));
        Assert.That(resources.GetUnitSelectionTickets(5), Is.EqualTo(1));
        Assert.That(resources.InnRefreshTickets, Is.EqualTo(1));
        resources.EnsureMetaInitialized();
        Assert.That(resources.Gold, Is.EqualTo(5));

        WaveManager wave = GameManager.Ins.WaveManager;
        int baseLimit = (int)typeof(WaveManager).GetField("maxEnemyCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(wave);
        Assert.That(wave.MaxEnemyCount, Is.EqualTo(baseLimit + 5));
        var enemyObject = new GameObject("MetaTestEnemy");
        enemyObject.SetActive(false);
        EnemyHealth health = enemyObject.AddComponent<EnemyHealth>();
        health.InitializeForSpawn(1000);
        Assert.That(health.MaxHealth, Is.EqualTo(980).Within(.01f));
        health.InitializeForSpawn(1000);
        Assert.That(health.MaxHealth, Is.EqualTo(980).Within(.01f));
        Object.Destroy(enemyObject);
        Object.Destroy(resourcesObject);

        int before = progression.Points;
        GameManager.Ins.EndGame(true);
        Assert.That(meta.LastReward, Is.EqualTo(settings.victoryBonus));
        Assert.That(progression.Points, Is.EqualTo(before + settings.victoryBonus));
        GameManager.Ins.EndGame(true);
        Assert.That(progression.Points, Is.EqualTo(before + settings.victoryBonus));
        MetaRunResultView result = Object.FindAnyObjectByType<MetaRunResultView>();
        Assert.That(result, Is.Not.Null);
        result.transform.Find("Panel/Return").GetComponent<Button>().onClick.Invoke();
        yield return WaitForFlow();
        Assert.That(SceneFlowManager.Ins.CurrentScene, Is.EqualTo(EScene.Main));
        Assert.That(Object.FindAnyObjectByType<MainMenuView>(), Is.Not.Null);
        Assert.That(MetaProgressionManager.Ins.Progression, Is.SameAs(progression));
        yield return new ExitPlayMode();
    }

    private static IEnumerator WaitForFlow()
    {
        float deadline = Time.realtimeSinceStartup + 30;
        while (SceneFlowManager.Ins.IsTransitioning && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.That(SceneFlowManager.Ins.IsTransitioning, Is.False, "씬 전환 시간 초과");
    }

    // 에디터 창이 가려져 있어도 실제 Canvas와 TMP를 렌더 텍스처로 검증합니다.
    private static void CapturePanel(MetaUpgradePanel panel)
    {
        Canvas canvas = panel.GetComponent<Canvas>();
        var cameraObject = new GameObject("MetaUICaptureCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.cullingMask = 1 << 5;
        camera.orthographic = true;
        camera.orthographicSize = 360;
        camera.transform.position = new Vector3(0, 0, -100);
        var texture = new RenderTexture(1280, 720, 24);
        camera.targetTexture = texture;
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 10;
        foreach (Transform child in panel.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 5;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = texture;
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        image.Apply();
        File.WriteAllBytes(Path.Combine(Path.GetTempPath(), "monoeye-meta-upgrades.png"), image.EncodeToPNG());
        RenderTexture.active = previous;
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.worldCamera = null;
        camera.targetTexture = null;
        Object.Destroy(image);
        Object.Destroy(texture);
        Object.Destroy(cameraObject);
    }
}
