using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour {
    public enum State {
        //총의 상태

        Ready,      //발사 준비
        Empty,      //총알 없음
        Reloading   //재장전중
    }
    public State state { get; private set; }

    private PlayerShooter gunHolder;                //총의 주인이 누구인지
    private LineRenderer bulletLineRenderer;        //총알 궤적

    private AudioSource gunAudioPlayer;
    public AudioClip shotClip;
    public AudioClip reloadClip;

    public ParticleSystem muzzleFlashEffect;
    public ParticleSystem shellEjectEffect;

    public Transform fireTransform;
    public Transform leftHandMount;

    public float damage = 25;
    public float fireDistance = 100f;

    public int ammoRemain = 100;
    public int magAmmo;
    public int magCapacity = 30;

    public float timeBetFire = 0.12f;
    public float reloadTime = 1.8f;

    [Range(0f, 10f)] public float maxSpread = 3f;
    [Range(1f, 10f)] public float stability = 1f;
    [Range(0.01f, 3f)] public float restoreFromRecoilSpeed = 2f;
    private float currentSpread;
    private float currentSpreadVelocity;

    private float lastFireTime;

    private LayerMask excludeTarget;

    private void Awake() {
        //보통 Awake나 Start에서 필요한 컴포넌트를 가져오는 방식으로 진행
        gunAudioPlayer = GetComponent<AudioSource>();
        bulletLineRenderer = GetComponent<LineRenderer>();

        bulletLineRenderer.positionCount = 2;
        bulletLineRenderer.enabled = false;
    }

    public void Setup(PlayerShooter gunHolder) {
        //총의 주인을 알 수 있도록 초기화를 하는 처리
        this.gunHolder = gunHolder;
        excludeTarget = gunHolder.excludeTarget;
    }

    private void OnEnable() {
        //총이 활성화 될때마다 매번 총의 상태를 초기화
        magAmmo = magCapacity;
        currentSpread = 0f;
        lastFireTime = 0f;
        state = State.Ready;
    }

    private void OnDisable() {
        //비활성화 될때는 총 내부의 모든 코루틴을 종료
        StopAllCoroutines();

    }

    public bool Fire(Vector3 aimTarget) {
        //건 클래스 외부에서 발사를 시도하게 만들어주는 메서드
        //조준 대상을 받아서 해당 방향으로 발사가 가능한 상태에서 Shot 메서드 실행
        //Shot 메서드를 감싼다??
        if (state == State.Ready && Time.time >= lastFireTime + timeBetFire) {
            var fireDirection = aimTarget - fireTransform.position;

            var xError = Utility.GetRandomNormalDistribution(0f, currentSpread);
            var yError = Utility.GetRandomNormalDistribution(0f, currentSpread);

            fireDirection = Quaternion.AngleAxis(yError, Vector3.up) * fireDirection;
            fireDirection = Quaternion.AngleAxis(xError, Vector3.right) * fireDirection;

            currentSpread += 1f / stability;

            lastFireTime = Time.time;
            Shot(fireTransform.position, fireDirection);

            return true;
        }

        return false;
    }

    private void Shot(Vector3 startPoint, Vector3 direction) {
        //총알 발사 처리
        RaycastHit hit;
        Vector3 hitPosition;

        if (Physics.Raycast(startPoint, direction, out hit, fireDistance, ~excludeTarget)) {
            var target = hit.collider.GetComponent<IDamageable>();

            if (target != null) {
                DamageMessage damageMessage;

                damageMessage.damager = gunHolder.gameObject;
                damageMessage.amount = damage;
                damageMessage.hitPoint = hit.point;
                damageMessage.hitNormal = hit.normal;

                target.ApplyDamage(damageMessage);

            } else {
                EffectManager.Instance.PlayHitEffect(hit.point, hit.normal, hit.transform);
            }
            hitPosition = hit.point;
        } else {
            hitPosition = startPoint + direction * fireDistance;

        }
        StartCoroutine(ShotEffect(hitPosition));

        magAmmo--;
        if (magAmmo <= 0) state = State.Empty;
    }

    private IEnumerator ShotEffect(Vector3 hitPosition) {
        //총알이 맞은 지점을 입력으로 받아 총알 발사 관련 이펙트 실행
        //시간 지연 이후 실행하므로 코루틴으로 구현
        muzzleFlashEffect.Play();
        shellEjectEffect.Play();

        gunAudioPlayer.PlayOneShot(shotClip);   //소리를 중첩시키는 경우에는 PlayOneShot을 통해 중첩시킴

        bulletLineRenderer.enabled = true;
        bulletLineRenderer.SetPosition(0, fireTransform.position);
        bulletLineRenderer.SetPosition(1, hitPosition);

        yield return new WaitForSeconds(0.03f);

        bulletLineRenderer.enabled = false;
    }

    public bool Reload() {
        //재장전 시도
        if (state == State.Reloading || ammoRemain <= 0 || magAmmo >= magCapacity) {
            return false;
        }

        StartCoroutine(ReloadRoutine());

        return true;
    }

    private IEnumerator ReloadRoutine() {
        //실제 재장전은 시간 지연 이후 코루틴으로 실행
        state = State.Reloading;
        gunAudioPlayer.PlayOneShot(reloadClip);


        yield return new WaitForSeconds(reloadTime);

        var ammoToFill = Mathf.Clamp(magCapacity - magAmmo, 0, ammoRemain);

        magAmmo += ammoToFill;
        ammoRemain -= ammoToFill;

        state = State.Ready;
    }

    private void Update() {
        //실제 총알 반동값을 상태에 따라 갱신
        currentSpread = Mathf.Clamp(currentSpread, 0f, maxSpread);
        currentSpread = Mathf.SmoothDamp(currentSpread, 0f, ref currentSpreadVelocity, 1f / restoreFromRecoilSpeed);
    }
}