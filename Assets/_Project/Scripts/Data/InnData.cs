using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>객잔의 명예별 등급 가중치입니다. 판매 후보는 유닛 테이블에서 전달받습니다.</summary>
[CreateAssetMenu(fileName = "InnData_", menuName = "ScriptableObjects/Inn/InnData")]
public class InnData : ScriptableObject
{
    [Tooltip("배열의 0~9번이 명예 1~10입니다. 수치는 확정 밸런스가 아닌 임시 가중치입니다.")]
    [SerializeField] private HonorGradeOdds[] honorOdds =
    {
        new HonorGradeOdds(100, 0, 0, 0, 0),
        new HonorGradeOdds(80, 20, 0, 0, 0),
        new HonorGradeOdds(65, 30, 5, 0, 0),
        new HonorGradeOdds(50, 35, 15, 0, 0),
        new HonorGradeOdds(40, 35, 23, 2, 0),
        new HonorGradeOdds(30, 35, 30, 5, 0),
        new HonorGradeOdds(20, 30, 35, 14, 1),
        new HonorGradeOdds(15, 25, 35, 22, 3),
        new HonorGradeOdds(10, 20, 30, 32, 8),
        new HonorGradeOdds(5, 10, 25, 40, 20)
    };

    /// <summary>먼저 등급을 추첨하고, 해당 등급의 장수 중 동일 확률로 선택합니다.</summary>
    public bool TryRollOffers(IReadOnlyList<UnitData> candidates, int honorLevel, System.Random random, out UnitData[] offers)
    {
        offers = null;
        if (random == null || honorLevel < 1 || honorLevel > 10 ||
            honorOdds == null || honorOdds.Length != 10 || candidates == null)
            return false;

        HonorGradeOdds odds = honorOdds[honorLevel - 1];
        if (odds == null || odds.TotalWeight <= 0)
            return false;

        var pools = new List<UnitData>[5];
        for (int i = 0; i < pools.Length; i++)
            pools[i] = new List<UnitData>();

        var registered = new HashSet<string>();
        foreach (UnitData data in candidates)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Id) || !registered.Add(data.Id))
                continue;
            pools[data.Grade - 1].Add(data);
        }

        // 등장 가능한 등급의 데이터가 빠졌을 때 확률을 임의로 바꾸지 않습니다.
        for (int i = 0; i < pools.Length; i++)
            if (odds.GetWeight(i + 1) > 0 && pools[i].Count == 0)
                return false;

        offers = new UnitData[InnManager.OfferCount];
        for (int i = 0; i < offers.Length; i++)
        {
            int grade = odds.SelectGrade(random.Next(odds.TotalWeight));
            List<UnitData> pool = pools[grade - 1];
            offers[i] = pool[random.Next(pool.Count)];
        }
        return true;
    }
}

[Serializable]
public class HonorGradeOdds
{
    [SerializeField, Range(0, 100)] private int grade1;
    [SerializeField, Range(0, 100)] private int grade2;
    [SerializeField, Range(0, 100)] private int grade3;
    [SerializeField, Range(0, 100)] private int grade4;
    [SerializeField, Range(0, 100)] private int grade5;

    public HonorGradeOdds(int grade1, int grade2, int grade3, int grade4, int grade5)
    {
        this.grade1 = grade1;
        this.grade2 = grade2;
        this.grade3 = grade3;
        this.grade4 = grade4;
        this.grade5 = grade5;
    }

    public int TotalWeight => GetWeight(1) + GetWeight(2) + GetWeight(3) + GetWeight(4) + GetWeight(5);

    public int GetWeight(int grade)
    {
        int weight = grade switch { 1 => grade1, 2 => grade2, 3 => grade3, 4 => grade4, 5 => grade5, _ => 0 };
        return Math.Max(0, Math.Min(100, weight));
    }

    /// <summary>0 이상 TotalWeight 미만의 정수 표본을 등급으로 변환합니다. 0 가중치는 절대 선택하지 않습니다.</summary>
    public int SelectGrade(int roll)
    {
        if (roll < 0 || roll >= TotalWeight)
            throw new ArgumentOutOfRangeException(nameof(roll));
        for (int grade = 1; grade <= 5; grade++)
        {
            roll -= GetWeight(grade);
            if (roll < 0)
                return grade;
        }
        throw new InvalidOperationException("등급 가중치가 올바르지 않습니다.");
    }
}
