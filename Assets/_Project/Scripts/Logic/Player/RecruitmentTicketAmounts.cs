using System;
using UnityEngine;

/// <summary>초기 지급 및 라운드 보상의 등급별 지명영입권 수량입니다.</summary>
[Serializable]
public class RecruitmentTicketAmounts
{
    [SerializeField, Min(0), InspectorName("1급 지명영입권")] private int grade1;
    [SerializeField, Min(0), InspectorName("2급 지명영입권")] private int grade2;
    [SerializeField, Min(0), InspectorName("3급 지명영입권")] private int grade3;
    [SerializeField, Min(0), InspectorName("4급 지명영입권")] private int grade4;
    [SerializeField, Min(0), InspectorName("5급 지명영입권")] private int grade5;

    public void GrantTo(ResourceManager resources)
    {
        if (resources == null) return;
        resources.AddUnitSelectionTickets(1, grade1);
        resources.AddUnitSelectionTickets(2, grade2);
        resources.AddUnitSelectionTickets(3, grade3);
        resources.AddUnitSelectionTickets(4, grade4);
        resources.AddUnitSelectionTickets(5, grade5);
    }
}
