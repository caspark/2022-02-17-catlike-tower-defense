using System;
using UnityEngine;

public class Enemy : GameBehavior {

    [SerializeField] Transform model = default;

    [SerializeField] EnemyAnimationConfig animationConfig = default;

    private EnemyFactory originFactory;

    public EnemyFactory OriginFactory {
        get => originFactory;
        set {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public float Scale { get; private set; }

    private GameTile tileFrom, tileTo;
    private Vector3 positionFrom, positionTo;
    private float progress, progressFactor;
    private Direction direction;
    private DirectionChange directionChange;
    private float directionAngleFrom, directionAngleTo;
    private float speed;
    private float pathOffset;

    private float Health;
    private ParticleSystem deathEffectPrefab;

    private Collider targetPointCollider;

    [SerializeField] bool useCustomAnimator = false;
    private EnemyAnimator customAnimator;

    private Animator builtinAnimator;

    private static int builtinAnimParamSpeed = Animator.StringToHash("Speed");
    private static int builtinAnimParamDoLanding = Animator.StringToHash("DoLanding");
    private static int builtinAnimParamDoFloat = Animator.StringToHash("DoFloat");
    private static int builtinAnimParamDoDeath = Animator.StringToHash("DoDeath");

    public Collider TargetPointCollider {
        set {
            Debug.Assert(targetPointCollider == null, "Redefined target point collider!");
            targetPointCollider = value;
        }
    }

    public bool IsValidTarget => targetPointCollider.enabled;

    private void Awake() {
        if (useCustomAnimator) {
            customAnimator.Configure(
                model.GetChild(0).gameObject.AddComponent<Animator>(),
                animationConfig
            );
        }
        else {
            builtinAnimator = model.GetChild(0).GetComponent<Animator>();
        }
    }

    public void Initialize(float scale, float speed, float pathOffset, float health, ParticleSystem deathEffectPrefab) {
        model.localScale = new Vector3(scale, scale, scale);
        this.Scale = scale;
        this.speed = speed;
        this.pathOffset = pathOffset;
        this.Health = health;
        this.deathEffectPrefab = deathEffectPrefab;
        if (useCustomAnimator) {
            customAnimator.PlayIntro();
        }
        else {
            // builtinAnimator.StartPlayback();
            builtinAnimator.SetTrigger(builtinAnimParamDoLanding);
        }
        targetPointCollider.enabled = false;
    }

    private void OnDestroy() {
        if (useCustomAnimator) {
            customAnimator.Destroy();
        }
    }

    public void ApplyDamage(float damage) {
        Debug.Assert(damage >= 0f, "Negative damage applied!");
        Health -= damage;
    }

    public override bool GameUpdate() {
        if (useCustomAnimator) {
            customAnimator.GameUpdate();
            if (customAnimator.CurrentClip == EnemyAnimator.Clip.Intro) {
                if (!customAnimator.IsDone) {
                    return true;
                }
                customAnimator.PlayMove(speed / Scale);
                targetPointCollider.enabled = true;
            }
            else if (customAnimator.CurrentClip == EnemyAnimator.Clip.Outro
                    || customAnimator.CurrentClip == EnemyAnimator.Clip.Dying) {
                if (customAnimator.IsDone) {
                    Recycle();
                    return false;
                }
                return true;
            }
            if (Health <= 0f) {
                Game.EnemyDied(this);
                SpawnDeathParticleSystem();
                customAnimator.PlayDying();
                targetPointCollider.enabled = false;
                return true;
            }
        }
        else {
            AnimatorStateInfo currentAnim = builtinAnimator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo nextAnim = builtinAnimator.GetNextAnimatorStateInfo(0);
            if (currentAnim.IsTag("Intro")) {
                // nothing to do since intro will transition to idle automatically
                return true;
            }
            else if (currentAnim.IsTag("Idle") && !nextAnim.IsTag("Move")) {
                // get the enemy moving
                builtinAnimator.SetFloat(builtinAnimParamSpeed, speed / 2.0f - 0.1f);
                targetPointCollider.enabled = true;
            }
            else if (currentAnim.IsTag("Exited") || currentAnim.IsTag("Dead")) {
                Recycle();
                return false;
            }
            else if (currentAnim.IsTag("Dying") || nextAnim.IsTag("Dying") || currentAnim.IsTag("Outro") || nextAnim.IsTag("Outro")) {
                return true;
            }

            if (Health <= 0f && !currentAnim.IsTag("Dying") && !nextAnim.IsTag("Dying")) {
                Game.EnemyDied(this);
                SpawnDeathParticleSystem();
                builtinAnimator.SetTrigger(builtinAnimParamDoDeath);
                targetPointCollider.enabled = false;
                return true;
            }
        }

        progress += Time.deltaTime * progressFactor;
        while (progress >= 1f) {
            if (tileTo == null) {
                Game.EnemyReachedDestination();
                if (useCustomAnimator) {
                    customAnimator.PlayOutro();
                }
                else {
                    builtinAnimator.SetTrigger(builtinAnimParamDoFloat);
                }
                targetPointCollider.enabled = false;
                return true;
            }
            progress = (progress - 1f) / progressFactor;
            PrepareNextState();
            progress *= progressFactor;
        }
        if (directionChange == DirectionChange.None) {
            transform.localPosition = Vector3.Lerp(positionFrom, positionTo, progress);
        }
        else {
            float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        return true;
    }

    private void SpawnDeathParticleSystem() {
        ParticleSystem deathSystem = Instantiate(deathEffectPrefab, transform.localPosition + model.localPosition, Quaternion.identity);
        deathSystem.transform.localScale = this.Scale * Vector3.one;
        ParticleSystem.EmissionModule emission = deathSystem.emission;
        emission.rateOverTime = this.Scale * emission.rateOverTimeMultiplier;
        deathSystem.GetComponent<ParticleSystem>();
        deathSystem.GetComponent<ParticleSystemRenderer>().material.color = model.gameObject.GetComponentsInChildren<Renderer>()[0].material.color;
    }

    public override void Recycle() {
        OriginFactory.Reclaim(this);
        if (useCustomAnimator) {
            customAnimator.Stop();
        }
    }

    internal void spawnOn(GameTile tile) {
        Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.NextTileOnPath;
        progress = 0f;
        PrepareIntro();
    }

    void PrepareIntro() {
        positionFrom = tileFrom.transform.localPosition;
        transform.localPosition = positionFrom;
        positionTo = tileFrom.ExitPoint;
        direction = tileFrom.PathDirection;
        directionChange = DirectionChange.None;
        directionAngleFrom = direction.GetAngle();
        directionAngleTo = direction.GetAngle();
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
        PrepareForward();
    }

    void PrepareNextState() {
        tileFrom = tileTo;
        tileTo = tileTo.NextTileOnPath;
        positionFrom = positionTo;
        if (tileTo == null) {
            PrepareOutro();
            return;
        }
        positionTo = tileFrom.ExitPoint;
        directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
        direction = tileFrom.PathDirection;
        directionAngleFrom = directionAngleTo;
        switch (directionChange) {
            case DirectionChange.None: PrepareForward(); break;
            case DirectionChange.TurnRight: PrepareTurnRight(); break;
            case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
            default: PrepareTurnAround(); break;
        }
    }

    void PrepareForward() {
        transform.localRotation = direction.GetRotation();
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        progressFactor = speed;
    }

    void PrepareTurnRight() {
        directionAngleTo = directionAngleFrom + 90f;
        model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
    }

    void PrepareTurnLeft() {
        directionAngleTo = directionAngleFrom - 90f;
        model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
        transform.localPosition = positionFrom + direction.GetHalfVector();
        progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
    }

    void PrepareTurnAround() {
        directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localPosition = positionFrom;
        progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
    }

    void PrepareOutro() {
        positionTo = tileFrom.transform.localPosition;
        directionChange = DirectionChange.None;
        directionAngleTo = direction.GetAngle();
        model.localPosition = new Vector3(pathOffset, 0f);
        transform.localRotation = direction.GetRotation();
        progressFactor = 2f * speed;
    }
}
