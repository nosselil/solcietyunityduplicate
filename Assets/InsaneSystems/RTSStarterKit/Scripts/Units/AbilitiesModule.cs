using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Abilities;

namespace InsaneSystems.RTSStarterKit
{
	public class AbilitiesModule : Module
	{
		public List<Ability> abilities { get; protected set; }

		void Start()
		{
			abilities = new List<Ability>();
			for (var i = 0; i < selfUnit.data.unitAbilities.Count; i++)
				GetOrAddAbility(selfUnit.data.unitAbilities[i]);
		}

		void Update()
		{
			for (var i = 0; i < abilities.Count; i++)
				abilities[i].Update();
		}

		/// <summary> Gets ability of unit by ability data template. </summary>
		public Ability GetAbility(Ability abilityTemplate)
		{
			if (!abilityTemplate)
				return null;
			
			for (int i = 0; i < abilities.Count; i++)
			{
				if (abilities[i].abilityName == abilityTemplate.abilityName)
					return abilities[i];
			}

			return null;
		}
		
		/// <summary> Gets or adds ability of unit by ability data template. </summary>
		public Ability GetOrAddAbility(Ability template)
		{
			var gettedAbility = GetAbility(template);

			if (!gettedAbility)
			{
				var abilityInstance = Instantiate(template);
				abilityInstance.Init(selfUnit);
				abilities.Add(abilityInstance);

				gettedAbility = abilityInstance;
			}

			return gettedAbility;
		}
		
		public Ability GetAbilityById(int id)
		{
			if (abilities.Count > id)
				return abilities[id];
			
			return null;
		}
	}
}