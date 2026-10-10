using System;
using UnityEngine;

/// <summary>적 한 마리의 보상 항목입니다. 지급과 획득량 보정은 ResourceManager가 담당합니다.</summary>
[Serializable]
public abstract class Bounty
{
    [SerializeField, Min(1)] private int amount = 1;

    public int Amount => amount;
    public virtual bool IsValid => amount > 0;

    protected Bounty(int amount) { this.amount = amount; }

    // 웨이브 원본 및 다른 적과 실행 중 보상 데이터를 공유하지 않습니다.
    public abstract Bounty Copy();
}

[Serializable]
public sealed class GoldBounty : Bounty
{
    public GoldBounty(int amount = 1) : base(amount) { }
    public override Bounty Copy() => new GoldBounty(Amount);
}

public enum BountyItemType
{
    InnRefreshTicket = 0,
    UnitSelectionTicket = 1
}

[Serializable]
public sealed class ItemBounty : Bounty
{
    [SerializeField] private BountyItemType itemType;
    [Tooltip("유닛 선택권에만 사용합니다.")]
    [SerializeField, Range(1, 5)] private int unitGrade = 1;

    public BountyItemType ItemType => itemType;
    public int UnitGrade => unitGrade;
    public override bool IsValid => base.IsValid &&
        (itemType == BountyItemType.InnRefreshTicket ||
         (itemType == BountyItemType.UnitSelectionTicket && unitGrade >= 1 && unitGrade <= 5));

    public ItemBounty(BountyItemType itemType = BountyItemType.InnRefreshTicket,
        int amount = 1, int unitGrade = 1) : base(amount)
    {
        this.itemType = itemType;
        this.unitGrade = unitGrade;
    }

    public override Bounty Copy() => new ItemBounty(itemType, Amount, unitGrade);
}
