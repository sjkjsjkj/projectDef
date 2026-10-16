/// <summary>월드의 유닛을 클릭했을 때 발행합니다. 드래그 완료는 선택으로 처리하지 않습니다.</summary>
public readonly struct OnUnitSelected
{
    public readonly Unit Unit;
    public OnUnitSelected(Unit unit) => Unit = unit;
}
