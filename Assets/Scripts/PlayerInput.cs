using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public string fireButtonName = "Fire1";
    public string jumpButtonName = "Jump";
    public string moveHorizontalAxisName = "Horizontal";
    public string moveVerticalAxisName = "Vertical";
    public string reloadButtonName = "Reload";

    public Vector2 moveInput { get; private set; }
    public bool fire { get; private set; }
    public bool reload { get; private set; }
    public bool jump { get; private set; }
    
    private void Update()
    {
        if (GameManager.Instance != null
            && GameManager.Instance.isGameover)
        {
            moveInput = Vector2.zero;
            fire = false;
            reload = false;
            jump = false;
            return;
        }

        //사이퍼즈에서 대각선으로 이동하는 스킬들이 더 빠른 이유
        //대각선 이동하는 속도를 normalize하는 과정을 해주지 않아서
        moveInput = new Vector2(Input.GetAxis(moveHorizontalAxisName), Input.GetAxis(moveVerticalAxisName));
        if (moveInput.sqrMagnitude > 1) moveInput = moveInput.normalized;

        jump = Input.GetButtonDown(jumpButtonName);
        fire = Input.GetButton(fireButtonName);
        reload = Input.GetButtonDown(reloadButtonName);
    }
}