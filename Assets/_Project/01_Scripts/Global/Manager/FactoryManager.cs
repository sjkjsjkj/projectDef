using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 객체를 생성해줄 팩토리 매니저
/// </summary>
public class FactoryManager  : Singleton<FactoryManager> 
{
    #region ─────────────────────────▶ 인스펙터 ◀─────────────────────────
    //[Header("주제")]
    //[SerializeField] private Class _class;
    #endregion

    #region ─────────────────────────▶ 내부 변수 ◀─────────────────────────
    private bool _isInitialized = false;
    private Dictionary<string, Func<CharacterBaseMono>> _register = new();
    #endregion

    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    public void RegisterCharacter<T>(string key, Func<T> createFunc) where T : CharacterBaseMono
    {
        if(_register.ContainsKey(key))
        {
            UDebug.Print("이미 등록된 키 입니다.", LogType.Warning);
            return;
        }

        _register[key] = createFunc;
    }

    public T CreateCharacter<T>(string key) where T : CharacterBaseMono
    {
        if(!_register.TryGetValue(key, out var createFunc))
        {
            UDebug.Print("등록되지 않은 키 입니다.", LogType.Warning);
            return null;
        }
        return createFunc() as T;
    }

    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    public override void Initialize() {
        if (_isInitialized)
        {
            return;
        }

        // ↑ 필요한 초기화 로직 / 부모 클래스에서 자동 실행
        _isInitialized = true;
    }
    #endregion
}
