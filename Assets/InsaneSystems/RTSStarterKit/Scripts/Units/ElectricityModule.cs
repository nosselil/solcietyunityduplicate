namespace InsaneSystems.RTSStarterKit
{
	/// <summary> This module allows unit to produce on consume electricity. </summary>
	public class ElectricityModule : Module
	{
		int addsElectricity, neededElectricity;

		protected override void AwakeAction()
		{
			addsElectricity = selfUnit.data.addsElectricity;
			neededElectricity = selfUnit.data.usesElectricity;

			Unit.unitSpawnedEvent += OnBuildingComplete;
		}

		void Start()
		{
			selfUnit.GetModule<Damageable>().damageableDiedEvent += OnDie;
		}

		void OnBuildingComplete(Unit unit)
		{
			if (unit != selfUnit)
				return;

			Player.GetPlayerById(selfUnit.OwnerPlayerId).AddElectricity(addsElectricity);
			Player.GetPlayerById(selfUnit.OwnerPlayerId).AddUsedElectricity(neededElectricity);
		}

		void OnDie(Unit unit)
		{
			if (unit != selfUnit)
				return;

			Player.GetPlayerById(selfUnit.OwnerPlayerId).RemoveElectricity(addsElectricity);
			Player.GetPlayerById(selfUnit.OwnerPlayerId).RemoveUsedElectricity(neededElectricity);
		}

		public void IncreaseAddingElectricity(int addToAdding)
		{
			addsElectricity += addToAdding;
			Player.GetPlayerById(selfUnit.OwnerPlayerId).AddElectricity(addToAdding);
		}

		void OnDestroy()
		{
			Unit.unitSpawnedEvent -= OnBuildingComplete;
		}
	}
}