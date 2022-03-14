using UnityEngine;

public class LaserTower : Tower {

    [SerializeField]
    float damagePerSecond = 10f;

    [SerializeField]
    Transform turret = default, laserBeam = default;

    [SerializeField]
    AudioClip laserStart;

    public override TowerType TowerType => TowerType.Laser;

    TargetPoint target;

    Vector3 laserBeamScale;
    private AudioSource audioSource;

    private void Awake() {
        laserBeamScale = laserBeam.localScale;

        audioSource = GetComponent<AudioSource>();
        Debug.Assert(audioSource != null, "No audio source found!");
        Debug.Assert(audioSource.clip != null, "Audio source clip should be set to laser sustain sound!");
    }

    public override void GameUpdate() {
        bool hadTarget = target != null;
        if (TrackTarget(ref target) || AcquireTarget(out target)) {
            Shoot(!hadTarget);
        }
        else {
            laserBeam.localScale = Vector3.zero;
            audioSource.Stop();
        }
    }

    private void Shoot(bool isNewTarget) {
        Vector3 point = target.Position;
        turret.LookAt(point);
        laserBeam.localRotation = turret.localRotation;

        float d = Vector3.Distance(turret.position, point);
        laserBeamScale.z = d;
        laserBeam.localScale = laserBeamScale;
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;

        if (isNewTarget) {
            audioSource.PlayOneShot(laserStart);
            audioSource.PlayDelayed(laserStart.length);
            Debug.Log("Playing new target sound");
        }

        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
    }
}
