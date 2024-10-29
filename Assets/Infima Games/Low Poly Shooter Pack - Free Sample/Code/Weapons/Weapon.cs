using UnityEngine;
using TMPro;

namespace InfimaGames.LowPolyShooterPack
{
    public class Weapon : WeaponBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Firing")]
        [Tooltip("Is this weapon automatic? If yes, then holding down the firing button will continuously fire.")]
        [SerializeField]
        private bool automatic;

        [Tooltip("How fast the projectiles are.")]
        [SerializeField]
        private float projectileImpulse = 400.0f;

        [Tooltip("Amount of shots this weapon can shoot in a minute. It determines how fast the weapon shoots.")]
        [SerializeField]
        private int roundsPerMinutes = 200;

        [Tooltip("Mask of things recognized when firing.")]
        [SerializeField]
        private LayerMask mask;

        [Tooltip("Maximum distance at which this weapon can fire accurately.")]
        [SerializeField]
        private float maximumDistance = 500.0f;

        [Header("Animation")]
        [Tooltip("Transform that represents the weapon's ejection port.")]
        [SerializeField]
        private Transform socketEjection;

        [Header("Resources")]
        [Tooltip("Casing Prefab.")]
        [SerializeField]
        private GameObject prefabCasing;

        [Tooltip("Projectile Prefab.")]
        [SerializeField]
        private GameObject prefabProjectile;

        [Tooltip("The AnimatorController a player character needs to use while wielding this weapon.")]
        [SerializeField]
        public RuntimeAnimatorController controller;

        [Tooltip("Weapon Body Texture.")]
        [SerializeField]
        private Sprite spriteBody;

        [Header("Audio Clips Holster")]
        [Tooltip("Holster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipHolster;

        [Tooltip("Unholster Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipUnholster;

        [Header("Audio Clips Reloads")]
        [Tooltip("Reload Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReload;

        [Tooltip("Reload Empty Audio Clip.")]
        [SerializeField]
        private AudioClip audioClipReloadEmpty;

        [Header("Audio Clips Other")]
        [Tooltip("AudioClip played when this weapon is fired without any ammunition.")]
        [SerializeField]
        private AudioClip audioClipFireEmpty;

        [Header("UI References")]
        [Tooltip("TextMeshPro for displaying current ammunition.")]
        [SerializeField]
        private TMP_Text textMeshCurrentAmmo;

        [Tooltip("TextMeshPro for displaying total ammunition.")]
        [SerializeField]
        private TMP_Text textMeshTotalAmmo;

        #endregion

        #region FIELDS

        private Animator animator;
        private WeaponAttachmentManagerBehaviour attachmentManager;
        private int ammunitionCurrent;
        private int totalBullets = 20;

        private MagazineBehaviour magazineBehaviour;
        private MuzzleBehaviour muzzleBehaviour;

        private IGameModeService gameModeService;
        private CharacterBehaviour characterBehaviour;
        private Transform playerCamera;

        #endregion

        #region UNITY

        protected override void Awake()
        {
            animator = GetComponent<Animator>();
            attachmentManager = GetComponent<WeaponAttachmentManagerBehaviour>();

            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            characterBehaviour = gameModeService.GetPlayerCharacter();
            playerCamera = characterBehaviour.GetCameraWorld().transform;
        }

        protected override void Start()
        {
            magazineBehaviour = attachmentManager.GetEquippedMagazine();
            muzzleBehaviour = attachmentManager.GetEquippedMuzzle();
            ammunitionCurrent = magazineBehaviour.GetAmmunitionTotal();

            UpdateAmmoUI(); // Update UI on start
        }

        #endregion

        #region GETTERS

        public override Animator GetAnimator() => animator;
        public override Sprite GetSpriteBody() => spriteBody;
        public override AudioClip GetAudioClipHolster() => audioClipHolster;
        public override AudioClip GetAudioClipUnholster() => audioClipUnholster;
        public override AudioClip GetAudioClipReload() => audioClipReload;
        public override AudioClip GetAudioClipReloadEmpty() => audioClipReloadEmpty;
        public override AudioClip GetAudioClipFireEmpty() => audioClipFireEmpty;
        public override AudioClip GetAudioClipFire() => muzzleBehaviour.GetAudioClipFire();
        public override int GetAmmunitionCurrent() => ammunitionCurrent;
        public override int GetAmmunitionTotal() => magazineBehaviour.GetAmmunitionTotal();
        public override bool IsAutomatic() => automatic;
        public override float GetRateOfFire() => roundsPerMinutes;
        public override bool IsFull() => ammunitionCurrent == magazineBehaviour.GetAmmunitionTotal();
        public override bool HasAmmunition() => ammunitionCurrent > 0;
        public override RuntimeAnimatorController GetAnimatorController() => controller;
        public override WeaponAttachmentManagerBehaviour GetAttachmentManager() => attachmentManager;

        #endregion

        #region METHODS

        private void UpdateAmmoUI()
        {
            if (textMeshCurrentAmmo != null)
                textMeshCurrentAmmo.text = ammunitionCurrent.ToString();

            if (textMeshTotalAmmo != null)
                textMeshTotalAmmo.text = totalBullets.ToString();
        }

        public override void Reload()
        {
            Debug.Log($"Reloading started. Current Ammo: {ammunitionCurrent}, Total Bullets: {totalBullets}");

            if (totalBullets > 0 && !IsFull())
            {
                int bulletsNeeded = magazineBehaviour.GetAmmunitionTotal() - ammunitionCurrent;
                int reloadAmount = Mathf.Min(bulletsNeeded, totalBullets);

                totalBullets -= reloadAmount;
                ammunitionCurrent += reloadAmount;

                Debug.Log($"Reloaded {reloadAmount} bullets. New Ammo: {ammunitionCurrent}, Total Bullets Left: {totalBullets}");

                animator.Play(HasAmmunition() ? "Reload" : "Reload Empty", 0, 0.0f);
                UpdateAmmoUI();
            }
            else if (totalBullets == 0)
            {
                Debug.Log("No bullets left to reload.");
            }
        }

        public override void Fire(float spreadMultiplier = 1.0f)
        {
            if (muzzleBehaviour == null || playerCamera == null || !HasAmmunition())
            {
                AudioSource.PlayClipAtPoint(audioClipFireEmpty, transform.position);
                return;
            }

            Transform muzzleSocket = muzzleBehaviour.GetSocket();
            const string stateName = "Fire";
            animator.Play(stateName, 0, 0.0f);
            ammunitionCurrent = Mathf.Clamp(ammunitionCurrent - 1, 0, magazineBehaviour.GetAmmunitionTotal());

            muzzleBehaviour.Effect();

            Quaternion rotation = Quaternion.LookRotation(playerCamera.forward * 1000.0f - muzzleSocket.position);
            if (Physics.Raycast(new Ray(playerCamera.position, playerCamera.forward), out RaycastHit hit, maximumDistance, mask))
                rotation = Quaternion.LookRotation(hit.point - muzzleSocket.position);

            GameObject projectile = Instantiate(prefabProjectile, muzzleSocket.position, rotation);
            projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * projectileImpulse;

            UpdateAmmoUI();
        }

        // public override void FillAmmunition(int amount)
        // {
        //     totalBullets = amount;
        //     UpdateAmmoUI();
        // }

        public override void EjectCasing()
        {
            if (prefabCasing != null && socketEjection != null)
                Instantiate(prefabCasing, socketEjection.position, socketEjection.rotation);
        }

        #endregion
    }
}
