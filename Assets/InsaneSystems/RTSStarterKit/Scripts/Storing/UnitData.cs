using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Abilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace InsaneSystems.RTSStarterKit
{
	[CreateAssetMenu(fileName = "UnitData", menuName = "RTS Starter Kit/Unit Data")]
	public class UnitData : ScriptableObject
	{
		public enum UsedDamageType
		{
			UseDamageSettingFromShell,
			UseCustomDamageValue
		}

		public enum MoveType { Ground, Flying }
		public enum AttackPossibility { Land, Air, LandAndAir }

		public string textId;
		[Tooltip("This is icon of unit, which will be shown on build panel and when you select this unit ingame.")]
		public Sprite icon;
		public GameObject unitModel;


		[Header("Base parameters")]
		[Tooltip("If true, this unit will be able to be killed, otherwise it will be invulnerable.")]
		public bool canBeDestroyed = true;
		[Range(1f, 10000f)] public float maxHealth = 100;
		[Tooltip("Is this unit harvest resources?")]
		public bool isHarvester;
		[Tooltip("Check this true if your unit is infantry soldier and should have animation module support.")]
		public bool isInfantry;
		[Tooltip("Vision distance in Fog of War. Used ONLY if Fog of War is ON.")]
		[Range(0f, 512f)] public float visionDistance = 10f;

		[Header("Build parameters")]
		[Range(0, 10000)] public int price = 100;
		[Range(0f, 360f)] public float buildTime = 3f;
		public List<ResourceRequirement> ResourceRequirements = new List<ResourceRequirement>();

		[Header("Movement parameters")]
		public bool hasMoveModule = true;
		public MoveType moveType = MoveType.Ground;
		[Range(0f, 100f)] public float moveSpeed = 2;
		[Range(0f, 720f)] public float rotationSpeed = 360;
		[Range(2f, 10f)] public float flyingFlyHeight = 6f;
		public bool canMoveWhileAttackingTarget = true;

		[Header("Attack parameters")]
		public bool hasAttackModule;
		public AttackPossibility attackPossibility = AttackPossibility.Land;
		[Range(0f, 1000f)] public float attackDistance = 5;
		public UsedDamageType usedDamageType = UsedDamageType.UseDamageSettingFromShell;
		[Tooltip("Note that this value will be used only if Used Damage Type set to Use Custom Damage Value setting, otherwise will be used damage from Shell settings.")]
		public float attackDamage = 15f;
		[Tooltip("Here you should place shell/bullet/rocket object, which will be spawned when unit shoot.")]
		public GameObject attackShell;
		[Tooltip("If true, an unit obstacles on fireline will be ignored, otherwise attacker unit will search better shoot position. Good for games with big count of units on battlefield.")]
		public bool allowShootThroughUnitObstacles = true;
		[Tooltip("If true, unit will be able shoot through other units and walls. Previous setting will be ignored.")]
		public bool allowShootThroughAnyObstacles = true;
		public bool needAimToTargetToShoot = true;
		[Tooltip("If you disabled previous toggle, look at this. If it true, units will still try to aim enemies (rotate to them), but of course will be able to shoot without aim. If you NOT need rotation to target, uncheck this toggle.")]
		public bool stillTryRotateToTargetWhenNoAimNeeded = true;
		[Tooltip("Unit reload time in seconds.")]
		[Range(0f, 360f)] public float reloadTime = 1;
		[Space(5)]
		public bool hasTurret;
		[FormerlySerializedAs("towerRotationSpeed")] [Range(0f, 360f)] public float turretRotationSpeed = 1;
		[FormerlySerializedAs("limitTowerRotationAngle")] public bool limitTurretRotationAngle;
		[FormerlySerializedAs("maximumTowerRotationAngle")]
		[Tooltip("This parameter works only if Limit Turret Rotation Angle toggle is on.")]
		[Range(0f, 179f)] public float maximumTurretRotationAngle = 15;

		[Header("Electricity parameters")]
		[Range(0, 40)] public int addsElectricity;
		[Range(0, 40)] public int usesElectricity;

		[Header("Carry parameters")]
		[Tooltip("Is this unit can be carried on board of carrier units?")]
		public bool canBeCarried;
		[Tooltip("How much units can carry this unit on his board? 0 means he can't carry any units.")]
		[Range(0, 40)] public int canCarryUnitsCount;
		
		[Header("Harvester parameters")]
		public int harvestMaxResources = 600;
		public float harvestTime = 5f;
		public float harvestCarryOutTime = 3f;
		
		[Header("Effects")]
		[Tooltip("Object, added to this field, will be spawned after unit death. You can place here explosion effect or any other, what you want. Particles effect should be Play on Awake.")]
		public GameObject explosionEffect;
		[Tooltip("Effect, which will be spawned on unit Shoot Point when unit attack target. Particles effect should be Play on Awake.")]
		public GameObject shootEffect;

		[Tooltip("Check this toggle if your unit have animations like move, attack, etc. It usually used for infantry, soldiers etc. Because it is humanuid unit and always have animations.")]
		public bool useAnimations;
		[Tooltip("If you have ready animator controller for your unit, place it here, and next time automatic prefab generation will add it to the unit prefab Animator settings.")]
		public RuntimeAnimatorController animatorController;
		
		[Header("Audio Settings")]
		[Tooltip("Add in this array all unit shoot sound variations, one of them will be selected randomly every shoot.")]
		[Sound] public AudioClip[] shootSoundVariations;
		[Tooltip("Add in this array all unit selection sound variations (for example, voices or sound effects), one of them will be selected randomly every unit selection.")]
		[Sound] public AudioClip[] selectionSoundVariations;
		[Tooltip("Add in this array all unit order sound variations (for example, voices or sound effects), one of them will be selected randomly and played every order for unit.")]
		[Sound] public AudioClip[] orderSoundVariations;
		[Tooltip("Increasing this value make shoot audio effect sounds little different every time.")]
		[Range(0, 0.5f)] public float shootSoundPitchRandomization = 0.05f;

		[Header("Misc")]
		[Tooltip("Add to this field prefab of Unit, for which you done this settings. Object from this field will be spawned, when player buy this unit ingame.")]
		public GameObject selfPrefab;

		[Header("Building settings")]
		public bool isBuilding;
		[Tooltip("Refinery is building, where your harvesters will bring resources. Check this true, if this building is refinery.")]
		public bool isRefinery;
		[Tooltip("Check this toggle if your building should produce something. For example, if this building allows to build another buildings or units.")]
		public bool isProduction;
		[Tooltip("List of production categories of this unit. Left empty, if it has no production module attached to. Note that usually there will be only one category.")]
		public List<ProductionCategory> productionCategories = new List<ProductionCategory>();
		public GameObject drawerObject;

		[Header("Abilities")]
		[Tooltip("List of unit abilities. Drag here data files of needed abilities and it will appear ingame.")]
		public List<Ability> unitAbilities;

		[Header("Upgrades-New")]
		public int UpgradePrice = 0;
		public UnitData BuildingToUpgrade = null;
        public UnitData NewBuildingData = null;
    }
}

[System.Serializable]
public class ResourceRequirement 
{
	public GameResource gameResource;
	public int ResourceRequired = 300;
}