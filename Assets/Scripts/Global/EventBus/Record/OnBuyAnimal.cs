/// <summary>
/// 플레이어가 동물을 구매했을 때
/// </summary>
public readonly struct OnBuyAnimal
{
    public readonly string animalId;

    public OnBuyAnimal(string animalId)
    {
        this.animalId = animalId;
    }

    /// <param name="animalId">숫자</param>
    public static void Publish(string animalId)
    {
        EventBus<OnBuyAnimal>.Publish(new OnBuyAnimal(animalId));
    }
}
