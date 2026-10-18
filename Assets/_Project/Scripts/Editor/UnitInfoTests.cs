using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class UnitInfoTests
{
    private readonly List<Object> _objects = new List<Object>();
    private Mouse _mouse, _previousMouse;
    private Unit _selected;

    [TearDown]
    public void TearDown()
    {
        EventBus<OnUnitSelected>.Unsubscribe(OnSelected);
        if (_mouse != null) InputSystem.RemoveDevice(_mouse);
        if (_previousMouse != null && _previousMouse.added) _previousMouse.MakeCurrent();
        for (int i = _objects.Count - 1; i >= 0; i--)
            if (_objects[i] != null) Object.DestroyImmediate(_objects[i]);
        _objects.Clear();
        _mouse = null;
        _selected = null;
    }

    [Test]
    public void ClickSelectsWithoutMovingOrChangingSlot()
    {
        DragDropController controller = CreateInput(out Unit unit);
        Vector3 original = unit.transform.position;
        IDropTarget slot = unit.CurrentTarget;
        MoveMouse(new Vector2(640, 360), true);
        Invoke(controller, "TryBeginDrag");
        Assert.That(unit.IsBeingDragged, Is.False, "누르기만 한 상태에서는 공격 시간을 멈추지 않습니다.");
        MoveMouse(new Vector2(643, 360), true);
        Invoke(controller, "UpdatePointerGesture");
        Assert.That(unit.transform.position, Is.EqualTo(original));
        MoveMouse(new Vector2(643, 360), false);
        Invoke(controller, "UpdatePointerGesture");
        Assert.That(_selected, Is.SameAs(unit));
        Assert.That(unit.CurrentTarget, Is.SameAs(slot));
        Assert.That(slot.Occupant, Is.SameAs(unit));
        Assert.That(unit.transform.position, Is.EqualTo(original));
    }

    [Test]
    public void DragDoesNotSelectAndInvalidDropReturnsToSource()
    {
        DragDropController controller = CreateInput(out Unit unit);
        Vector3 original = unit.transform.position;
        IDropTarget slot = unit.CurrentTarget;
        MoveMouse(new Vector2(640, 360), true);
        Invoke(controller, "TryBeginDrag");
        MoveMouse(new Vector2(700, 360), true);
        Invoke(controller, "UpdatePointerGesture");
        Assert.That(unit.transform.position, Is.Not.EqualTo(original));
        Assert.That(unit.IsBeingDragged, Is.True);
        MoveMouse(new Vector2(700, 360), false);
        Invoke(controller, "UpdatePointerGesture");
        Assert.That(_selected, Is.Null);
        Assert.That(unit.IsBeingDragged, Is.False);
        Assert.That(unit.transform.position, Is.EqualTo(original));
        Assert.That(unit.CurrentTarget, Is.SameAs(slot));
        Assert.That(slot.Occupant, Is.SameAs(unit));
    }

    [Test]
    public void DisablingDragControllerRestoresPositionAndUnfreezesUnit()
    {
        DragDropController controller = CreateInput(out Unit unit);
        Vector3 original = unit.transform.position;
        MoveMouse(new Vector2(640, 360), true);
        Invoke(controller, "TryBeginDrag");
        MoveMouse(new Vector2(700, 360), true);
        Invoke(controller, "UpdatePointerGesture");
        Assert.That(unit.IsBeingDragged, Is.True);
        Invoke(controller, "OnDisable");
        Assert.That(unit.IsBeingDragged, Is.False);
        Assert.That(unit.transform.position, Is.EqualTo(original));
    }

    [Test]
    public void DetailTracksCurrentStatsSkillUpgradeAndSwitchingUnits()
    {
        Unit first = CreateUnit("관우");
        AbilityData initial = Track(ScriptableObject.CreateInstance<AbilityData>());
        AbilityData upgraded = Track(ScriptableObject.CreateInstance<AbilityData>());
        Set(initial, "skillName", "청룡참");
        Set(initial, "skillDescription", "첫 번째 설명");
        Set(upgraded, "skillName", "강화 청룡참");
        Set(upgraded, "skillDescription", "승급한 설명");
        Set(first.Data, "skillLevel1", initial);
        Set(first.Data, "skillLevel2", upgraded);
        UnitInfoPanel panel = CreatePanel(first);
        panel.Select(first);
        Assert.That(Text(panel, "Skill/Header/Name"), Does.Contain("청룡참"));
        Assert.That(Text(panel, "Skill/Description"), Is.EqualTo("첫 번째 설명"));
        string before = Text(panel, "Stats");
        for (int i = 0; i < 3; i++) first.TryLevelUp();
        Set(panel, "_nextRefresh", float.NegativeInfinity);
        Invoke(panel, "LateUpdate");
        Assert.That(Text(panel, "Stats"), Is.Not.EqualTo(before));
        Assert.That(Text(panel, "Skill/Header/Name"), Does.Contain("Lv.2").And.Contain("강화 청룡참"));
        Assert.That(Text(panel, "Skill/Description"), Is.EqualTo("승급한 설명"));
        Unit second = CreateUnit("장비");
        panel.Select(second);
        Assert.That(panel.SelectedUnit, Is.SameAs(second));
        Assert.That(Text(panel, "Identity/Name"), Is.EqualTo("장비"));
        Assert.That(Text(panel, "Skill/Description"), Is.EqualTo("등록된 스킬이 없습니다."));
        Assert.That(panel.transform.Find("UnitInfo/Viewport/Content/Identity/Portrait/Image").GetComponent<Image>().enabled, Is.False);
        second.gameObject.SetActive(false);
        Invoke(panel, "LateUpdate");
        Assert.That(panel.IsOpen, Is.False);
        Assert.That(panel.SelectedUnit, Is.Null);
    }

    [Test]
    public void DetailCountsOnlyBattleUnitsAndClosesWhenUnitDestroyed()
    {
        Unit first = CreateUnit("관우");
        Unit second = CreateUnit("장비");
        Set(first.Data, "region", "촉");
        Set(second.Data, "region", "촉");
        Slot battle = CreateObject("Battle").AddComponent<Slot>();
        battle.OnDrop(first);
        Slot waiting = CreateObject("Waiting").AddComponent<Slot>();
        Set(waiting, "placementArea", Slot.PlacementArea.Waiting);
        waiting.OnDrop(second);
        UnitInfoPanel panel = CreatePanel(first);
        panel.Select(second);
        Assert.That(Text(panel, "Synergies"), Does.Contain("촉 · 출전 1 · 대기"));
        Object.DestroyImmediate(second.gameObject);
        Invoke(panel, "LateUpdate");
        Assert.That(panel.IsOpen, Is.False);
    }

    private DragDropController CreateInput(out Unit unit)
    {
        _previousMouse = Mouse.current;
        _mouse = InputSystem.AddDevice<Mouse>();
        _mouse.MakeCurrent();
        EventBus<OnUnitSelected>.Subscribe(OnSelected);
        Camera camera = CreateObject("Camera").AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.pixelRect = new Rect(0, 0, 1280, 720);
        camera.transform.position = new Vector3(10000, 10000, -10);
        unit = CreateUnit("관우");
        unit.gameObject.AddComponent<BoxCollider2D>();
        Slot slot = CreateObject("Slot").AddComponent<Slot>();
        slot.transform.position = new Vector3(10000, 10000, 0);
        slot.OnDrop(unit);
        Physics2D.SyncTransforms();
        DragDropController controller = CreateObject("Controller").AddComponent<DragDropController>();
        Set(controller, "targetCamera", camera);
        Set(controller, "draggableLayer", (LayerMask)(-1));
        Set(controller, "dropTargetLayer", (LayerMask)0);
        return controller;
    }

    private UnitInfoPanel CreatePanel(Unit unit)
    {
        UnitInfoPanel panel = UnitInfoUIFactory.Ensure(unit.gameObject.scene);
        Track(panel.gameObject);
        Assert.That(panel.IsOpen, Is.False);
        Assert.That(UnitInfoUIFactory.Ensure(unit.gameObject.scene), Is.SameAs(panel));
        return panel;
    }

    private Unit CreateUnit(string name)
    {
        UnitData data = Track(ScriptableObject.CreateInstance<UnitData>());
        Set(data, "unitName", name);
        Unit unit = CreateObject(name).AddComponent<Unit>();
        Set(unit, "unitData", data);
        return unit;
    }

    private void OnSelected(OnUnitSelected selection) => _selected = selection.Unit;
    private void MoveMouse(Vector2 position, bool pressed)
    {
        InputSystem.QueueStateEvent(_mouse, new MouseState { position = position }.WithButton(MouseButton.Left, pressed));
        InputSystem.Update();
        // 에디터의 실제 마우스 이벤트가 current 장치를 바꿔도 가상 입력을 읽도록 합니다.
        _mouse.MakeCurrent();
        Assert.That(_mouse.leftButton.isPressed, Is.EqualTo(pressed));
    }

    private static string Text(UnitInfoPanel panel, string path) =>
        panel.transform.Find("UnitInfo/Viewport/Content/" + path).GetComponent<TMP_Text>().text;
    private GameObject CreateObject(string name) => Track(new GameObject(name));
    private T Track<T>(T value) where T : Object { _objects.Add(value); return value; }
    private static void Set(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static void Invoke(object target, string name) =>
        target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
}
