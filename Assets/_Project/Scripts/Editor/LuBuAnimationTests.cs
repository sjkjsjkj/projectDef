using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class LuBuAnimationTests
{
    [Test]
    public void PrefabUsesAssembledImageAndSeparateVisualRoot()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(LuBuAnimationSetup.PrefabPath);
        Assert.That(prefab, Is.Not.Null);
        var data = AssetDatabase.LoadAssetAtPath<UnitData>(LuBuAnimationSetup.DataPath);
        Assert.That(data.Prefab, Is.EqualTo(prefab.GetComponent<Unit>()));
        var animation = prefab.GetComponent<UnitAttackAnimation>();
        Assert.That(animation.Visual, Is.EqualTo(prefab.transform.Find("Visual")));
        Assert.That(animation.Visual.Find("AttackPivot"), Is.Null);
        var image = animation.Visual.Find("Assembled").GetComponent<SpriteRenderer>();
        Assert.That(image.sprite.name, Is.EqualTo("assembled"));
        Assert.That(image.sprite.pixelsPerUnit, Is.EqualTo(1000));
        Assert.That(prefab.GetComponent<BoxCollider2D>(), Is.Not.Null);
    }

    [Test]
    public void RecoilMovesOnlyVisualAndReturnsToRest()
    {
        var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(LuBuAnimationSetup.PrefabPath));
        try
        {
            var animation = instance.GetComponent<UnitAttackAnimation>();
            Vector3 rootPosition = instance.transform.position;
            Vector3 rest = animation.Visual.localPosition;
            animation.PlayAttack(rootPosition + Vector3.right, 1f);
            animation.Tick(0.05f);
            Assert.That(animation.Visual.localPosition.x, Is.GreaterThan(rest.x));
            Assert.That(instance.transform.position, Is.EqualTo(rootPosition));
            animation.Tick(1f);
            Assert.That(animation.IsPlaying, Is.False);
            Assert.That(animation.Visual.localPosition, Is.EqualTo(rest));
            Assert.That(Quaternion.Angle(animation.Visual.localRotation, Quaternion.identity), Is.LessThan(0.001f));
        }
        finally { Object.DestroyImmediate(instance); }
    }
}
