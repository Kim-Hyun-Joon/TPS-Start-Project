using UnityEngine;

public struct DamageMessage {
    //공격자가 공격을 당하는 객체에게 보내는 구조체
    //클래스로 안 보내는 이유는 공격을 당하는 객체가
    //구조체를 수정해야 하는 경우도 있기 때문(구조체는 value 타입)
    //클래스로 보내면 이 경우에서 문제가 발생

    public GameObject damager;
    public float amount;

    public Vector3 hitPoint;
    public Vector3 hitNormal;
}