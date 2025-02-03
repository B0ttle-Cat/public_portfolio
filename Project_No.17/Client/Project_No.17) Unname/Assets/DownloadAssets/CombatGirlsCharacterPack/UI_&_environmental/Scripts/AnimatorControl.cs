using UnityEngine;
using UnityEngine.UI;

public class AnimatorControl : MonoBehaviour
{
    private Animator animator;
    public Toggle rootMotionToggle; // Toggle UI ������Ʈ�� �����մϴ�.

    private void Start()
    {
        // ĳ���� �������� Animator ������Ʈ�� �����մϴ�.
        animator = GetComponent<Animator>();

        // ��� UI�� ���¸� ������ ������ �Լ��� ȣ���մϴ�.
        rootMotionToggle.onValueChanged.AddListener(ToggleRootMotion);
    }

    public void ToggleRootMotion(bool enableRootMotion)
    {
        // ���ö��� ��Ʈ��� �ɼ��� �����մϴ�.
        animator.applyRootMotion = enableRootMotion;
    }
}
