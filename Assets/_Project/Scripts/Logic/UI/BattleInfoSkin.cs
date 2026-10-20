using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>전투정보 패널과 동적으로 생성되는 목록이 공유하는 스프라이트입니다.</summary>
[CreateAssetMenu(menuName = "삼국지 디펜스/UI/Battle Info Skin")]
public class BattleInfoSkin : ScriptableObject
{
    public Sprite panel;
    public Sprite title;
    public Sprite tabNormal;
    public Sprite tabSelected;
    public Sprite countStrip;
    public Sprite unitRow;
    public Sprite synergyRow;
    public Sprite emptyFrame;
    public Sprite gradeBadge;

    public static BattleInfoSkin Load() => Resources.Load<BattleInfoSkin>("UI/BattleInfoSkin");

    public void ApplyPanel(BattleInfoPanel view)
    {
        RectTransform root = view.transform.Find("BattleInfo") as RectTransform;
        if (root == null) return;
        root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 260);
        SetImage(root.GetComponent<Image>(), panel);
        Image plaque = ImageChild("TitlePlaque", root, title);
        Place(plaque.rectTransform, 12, 12, 236, 32);
        plaque.transform.SetAsFirstSibling();
        TMP_Text label = root.Find("Title")?.GetComponent<TMP_Text>();
        if (label != null)
        {
            Place(label.rectTransform, 24, 14, 212, 28);
            label.text = "전투정보";
            label.fontSize = 20;
            if (view.UIFont != null) label.font = view.UIFont;
            label.alignment = TextAlignmentOptions.Midline;
        }
        Transform hint = root.Find("ShortcutHint");
        if (hint != null) hint.gameObject.SetActive(false);
        RectTransform body = root.Find("Body") as RectTransform;
        if (body != null)
        {
            Stretch(body, 16, 16, 92, 18);
            if (body.TryGetComponent(out Image fill)) fill.color = Color.clear;
            foreach (RectTransform content in body)
                Stretch(content, 0, 0, 0, 0);
            Transform dfs = body.Find("DFSContent");
            if (dfs != null) dfs.gameObject.SetActive(false);
        }
        string[] names = { "Units", "Synergies" };
        string[] labels = { "출전장수", "시너지" };
        TMP_FontAsset font = view.UIFont;
        for (int i = 0; i < names.Length; i++)
        {
            RectTransform tab = root.Find(names[i] + "Tab") as RectTransform;
            if (tab == null) continue;
            Place(tab, 16 + i * 117, 52, 111, 32);
            Transform icon = tab.Find("Icon");
            if (icon != null) icon.gameObject.SetActive(false);
            Transform oldLabel = tab.Find("Label");
            if (oldLabel != null) oldLabel.gameObject.SetActive(false);
            TMP_Text tabLabel = TextChild("TabLabel", tab, font, 15);
            Stretch(tabLabel.rectTransform, 6, 6, 2, 2);
            tabLabel.text = labels[i];
            tabLabel.alignment = TextAlignmentOptions.Midline;
            SetImage(tab.GetComponent<Image>(), i == 0 ? tabSelected : tabNormal);
            if (tab.TryGetComponent(out Button button))
                button.transition = Selectable.Transition.None;
        }
        Transform dfsTab = root.Find("DFSTab");
        if (dfsTab != null) dfsTab.gameObject.SetActive(false);
    }

    public static void SetImage(Image image, Sprite sprite)
    {
        if (image == null || sprite == null) return;
        image.sprite = sprite;
        image.color = Color.white;
        image.type = Image.Type.Simple;
    }

    public static Image ImageChild(string name, Transform parent, Sprite sprite)
    {
        RectTransform rect = Child(name, parent);
        Image image = rect.GetComponent<Image>();
        if (image == null) image = rect.gameObject.AddComponent<Image>();
        SetImage(image, sprite);
        image.raycastTarget = false;
        return image;
    }

    public static TMP_Text TextChild(string name, Transform parent, TMP_FontAsset font, int size)
    {
        RectTransform rect = Child(name, parent);
        TMP_Text text = rect.GetComponent<TMP_Text>();
        if (text == null) text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.fontSize = size;
        text.color = new Color32(236, 231, 215, 255);
        text.raycastTarget = false;
        text.richText = false;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        return text;
    }

    public static RectTransform Child(string name, Transform parent)
    {
        RectTransform rect = parent.Find(name) as RectTransform;
        if (rect != null) return rect;
        rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    public static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    public static void Stretch(RectTransform rect, float left, float right, float top, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
