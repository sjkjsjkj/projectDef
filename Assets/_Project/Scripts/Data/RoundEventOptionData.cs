using UnityEngine;

[CreateAssetMenu(fileName = "RoundEventOption_", menuName = "ScriptableObjects/Round Event/Option")]
public class RoundEventOptionData : BaseSO
{
    [SerializeField] private string title;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private RoundEventEffect effect;

    public string Title => title;
    public string Description => description;
    public Sprite Icon => icon;
    public RoundEventEffect Effect => effect;

    public override bool IsValid() => base.IsValid() && !string.IsNullOrWhiteSpace(title) &&
        effect != null && effect.IsValid();
}
