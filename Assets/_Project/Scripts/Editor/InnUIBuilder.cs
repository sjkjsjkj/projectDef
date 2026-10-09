using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>선택한 객잔 매니저에 연결된 기본 UI를 생성합니다. 씬 저장은 사용자가 수행합니다.</summary>
public static class InnUIBuilder
{
    [MenuItem("GameObject/삼국지 디펜스/객잔 UI 생성", false, 10)]
    private static void Create()
    {
        InnManager manager = Selection.activeGameObject != null
            ? Selection.activeGameObject.GetComponent<InnManager>() : null;
        if (manager == null)
        {
            Debug.LogWarning("InnManager가 부착된 씬 오브젝트를 선택한 뒤 실행하세요.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("객잔 UI 생성");
        var canvasObject = new GameObject("InnCanvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "객잔 UI 생성");
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform content = Rect("InnContent", canvas.transform, -560, 20, 1120, 320);
        content.anchorMin = content.anchorMax = new Vector2(0.5f, 0);
        content.gameObject.AddComponent<Image>().color = new Color(0.1f, 0.09f, 0.08f, 0.97f);
        TMP_Text resources = Label("Resources", content, 20, 270, 840, 35, "객잔", 22);
        Button minimize = Button("Minimize", content, 980, 270, 120, 35, "최소화");
        Button refresh = Button("Refresh", content, 850, 15, 250, 38, "목록 초기화 · 1장");
        TMP_Text message = Label("Message", content, 20, 15, 800, 38, "", 18);
        Button open = Button("OpenInn", canvas.transform, -150, 20, 130, 45, "객잔 열기");
        var openRect = (RectTransform)open.transform;
        openRect.anchorMin = openRect.anchorMax = new Vector2(1, 0);

        InnPanel panel = canvasObject.AddComponent<InnPanel>();
        Assign(panel, "innManager", manager);
        Assign(panel, "contentRoot", content.gameObject);
        Assign(panel, "openButton", open);
        Assign(panel, "minimizeButton", minimize);
        Assign(panel, "refreshButton", refresh);
        Assign(panel, "resourcesText", resources);
        Assign(panel, "messageText", message);
        var views = new InnOfferView[InnManager.OfferCount];
        for (int i = 0; i < views.Length; i++)
        {
            RectTransform card = Rect($"Offer_{i + 1}", content, 20 + i * 220, 65, 200, 195);
            card.gameObject.AddComponent<Image>().color = new Color(0.24f, 0.2f, 0.16f);
            Image portrait = Rect("Portrait", card, 58, 83, 84, 84).gameObject.AddComponent<Image>();
            portrait.preserveAspect = true;
            portrait.raycastTarget = false;
            TMP_Text name = Label("Name", card, 10, 160, 180, 30, "장수", 20);
            TMP_Text grade = Label("Grade", card, 10, 48, 180, 30, "1등급", 18);
            Button buy = Button("Purchase", card, 10, 10, 180, 34, "고용");
            views[i] = card.gameObject.AddComponent<InnOfferView>();
            Assign(views[i], "portrait", portrait);
            Assign(views[i], "nameText", name);
            Assign(views[i], "gradeText", grade);
            Assign(views[i], "purchaseButton", buy);
            Assign(views[i], "priceText", buy.GetComponentInChildren<TMP_Text>());
        }
        var serializedPanel = new SerializedObject(panel);
        SerializedProperty array = serializedPanel.FindProperty("offerViews");
        array.arraySize = views.Length;
        for (int i = 0; i < views.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = views[i];
        serializedPanel.ApplyModifiedPropertiesWithoutUndo();
        open.gameObject.SetActive(false);

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            var events = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(events, "객잔 EventSystem 생성");
            events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        Undo.CollapseUndoOperations(undoGroup);
        Selection.activeGameObject = canvasObject;
        Debug.Log("객잔 UI를 생성했습니다. 한글을 지원하는 TMP 폰트를 지정하고 씬을 저장하세요.", canvasObject);
    }

    [MenuItem("GameObject/삼국지 디펜스/객잔 UI 생성", true)]
    private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;

    private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    private static TMP_Text Label(string name, Transform parent, float x, float y, float width, float height, string text, int size)
    {
        var label = Rect(name, parent, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1, 0.93f, 0.8f);
        label.raycastTarget = false;
        return label;
    }

    private static Button Button(string name, Transform parent, float x, float y, float width, float height, string text)
    {
        RectTransform rect = Rect(name, parent, x, y, width, height);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.4f, 0.28f, 0.14f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        Label("Label", rect, 0, 0, width, height, text, 18);
        return button;
    }

    private static void Assign(Object target, string property, Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(property).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }
}
