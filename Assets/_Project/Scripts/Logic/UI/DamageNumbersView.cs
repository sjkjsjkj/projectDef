using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>피해 이벤트를 화면 숫자로 표시합니다. 최대 64개를 재사용해 광역 공격에도 생성량을 제한합니다.</summary>
public class DamageNumbersView : MonoBehaviour
{
    private sealed class Entry { public TMP_Text Text; public Vector3 World; public float Age; }
    private readonly List<Entry> _entries = new();
    private DataManager _data;
    private int _next;

    private void OnEnable()
    {
        _data = DataManager.Ins;
        EnemyHealth.OnDamageTaken += Show;
        _data.OnOptionsChanged += RefreshVisibility;
    }

    private void OnDisable()
    {
        EnemyHealth.OnDamageTaken -= Show;
        if (_data != null) _data.OnOptionsChanged -= RefreshVisibility;
        foreach (Entry entry in _entries) entry.Text.gameObject.SetActive(false);
    }

    private void Show(Vector3 world, float damage)
    {
        if (!_data.Option.ShowDamage || Camera.main == null) return;
        Entry entry = _entries.Find(item => !item.Text.gameObject.activeSelf);
        if (entry == null && _entries.Count < 64)
        {
            var text = InnUIFactory.Label("Damage", transform, 0, 0, 160, 42, "", 24, TMP_Settings.defaultFontAsset);
            text.rectTransform.anchorMin = text.rectTransform.anchorMax = text.rectTransform.pivot = new Vector2(.5f, .5f);
            entry = new Entry { Text = text };
            _entries.Add(entry);
        }
        if (entry == null) { entry = _entries[_next]; _next = (_next + 1) % _entries.Count; }
        entry.World = world;
        entry.Age = 0;
        entry.Text.text = damage.ToString("0.#");
        entry.Text.gameObject.SetActive(true);
        Position(entry);
    }

    private void RefreshVisibility()
    {
        if (_data.Option.ShowDamage) return;
        foreach (Entry entry in _entries) entry.Text.gameObject.SetActive(false);
    }

    private void Update()
    {
        foreach (Entry entry in _entries)
        {
            if (!entry.Text.gameObject.activeSelf) continue;
            entry.Age += Time.deltaTime;
            if (entry.Age >= .8f) { entry.Text.gameObject.SetActive(false); continue; }
            Position(entry);
        }
    }

    private void Position(Entry entry)
    {
        Camera camera = Camera.main;
        if (camera == null) { entry.Text.gameObject.SetActive(false); return; }
        Vector3 screen = camera.WorldToScreenPoint(entry.World);
        if (screen.z <= 0) { entry.Text.gameObject.SetActive(false); return; }
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, screen, null, out Vector2 point);
        entry.Text.rectTransform.anchoredPosition = point + new Vector2(0, 24 + entry.Age * 48);
        entry.Text.color = new Color(1f, .8f, .35f, 1 - entry.Age / .8f);
    }
}
