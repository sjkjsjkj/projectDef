using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitAttackAnimation))]
public class UnitAttackAnimationEditor : Editor
{
    private Transform _previewPivot;
    private Quaternion _restRotation;
    private Vector3 _restPosition;
    private double _startedAt;
    private AnimationClip _previewClip;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var animation = (UnitAttackAnimation)target;
        EditorGUILayout.HelpBox("프리팹 모드 또는 씬 인스턴스에서 공격을 미리 볼 수 있습니다. 피해는 발생하지 않습니다.", MessageType.Info);
        using (new EditorGUI.DisabledScope(animation.Visual == null || EditorUtility.IsPersistent(animation) ||
            (_previewPivot == null && AnimationMode.InAnimationMode())))
        {
            if (GUILayout.Button("공격 미리보기"))
            {
                StopPreview();
                if (Application.isPlaying) animation.PlayAttack(animation.transform.position + Vector3.right, 1f);
                else
                {
                    _previewPivot = animation.Visual;
                    _restRotation = _previewPivot.localRotation;
                    _restPosition = _previewPivot.localPosition;
                    _startedAt = EditorApplication.timeSinceStartup;
                    _previewClip = new AnimationClip { hideFlags = HideFlags.HideAndDontSave };
                    string path = AnimationUtility.CalculateTransformPath(_previewPivot, animation.transform);
                    var curves = new[] { new AnimationCurve(), new AnimationCurve(), new AnimationCurve(), new AnimationCurve() };
                    var positions = new[] { new AnimationCurve(), new AnimationCurve(), new AnimationCurve() };
                    for (int frame = 0; frame <= 60; frame++)
                    {
                        float t = frame / 60f;
                        Quaternion rotation = _restRotation * Quaternion.Euler(0f, 0f, animation.EvaluateAngle(t, Vector3.right));
                        Vector3 position = _restPosition + animation.EvaluateOffset(t, Vector3.right);
                        for (int axis = 0; axis < 4; axis++) curves[axis].AddKey(t * animation.Duration, rotation[axis]);
                        for (int axis = 0; axis < 3; axis++) positions[axis].AddKey(t * animation.Duration, position[axis]);
                    }
                    for (int axis = 0; axis < 4; axis++)
                        _previewClip.SetCurve(path, typeof(Transform), "m_LocalRotation." + "xyzw"[axis], curves[axis]);
                    for (int axis = 0; axis < 3; axis++)
                        _previewClip.SetCurve(path, typeof(Transform), "m_LocalPosition." + "xyz"[axis], positions[axis]);
                    _previewClip.EnsureQuaternionContinuity();
                    AnimationMode.StartAnimationMode();
                    EditorApplication.update += UpdatePreview;
                }
            }
        }
    }

    private void UpdatePreview()
    {
        if (target == null || _previewPivot == null || EditorApplication.isPlayingOrWillChangePlaymode)
        { StopPreview(); return; }
        var animation = (UnitAttackAnimation)target;
        float t = (float)(EditorApplication.timeSinceStartup - _startedAt) / animation.Duration;
        if (t >= 1f) { StopPreview(); return; }
        AnimationMode.BeginSampling();
        AnimationMode.SampleAnimationClip(animation.gameObject, _previewClip, t * animation.Duration);
        AnimationMode.EndSampling();
        SceneView.RepaintAll();
    }

    private void StopPreview()
    {
        EditorApplication.update -= UpdatePreview;
        if (_previewClip != null)
        {
            AnimationMode.StopAnimationMode();
            DestroyImmediate(_previewClip);
            _previewClip = null;
        }
        _previewPivot = null;
        SceneView.RepaintAll();
    }

    private void OnDisable() => StopPreview();
}
