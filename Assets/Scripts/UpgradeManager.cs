using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
	[System.Serializable]
	public class Upgrade
	{
		public string name;
		public string description;
		public int currentTier;
		public float basePrice;
		public float baseValue;
		public Button upgradeButton;
		public TMP_Text upgradeText;
	}

	public List<Upgrade> upgrades;
	public GameManager gameManager;
	public RocketController rocketController;

	[Header("UI References")]
	public Button engineUpgradeButton;
	public Button fuelTankUpgradeButton;
	public Button aerodynamicsUpgradeButton;
	public Button hullUpgradeButton;
	public TMP_Text engineUpgradeText;
	public TMP_Text fuelTankUpgradeText;
	public TMP_Text aerodynamicsUpgradeText;
	public TMP_Text hullUpgradeText;
	public TMP_Text currentMoneyText;

	[Header("Tooltip")]
	public GameObject tooltipPanel;
	public TMP_Text tooltipText;

	[Header("Stats Display")]
	public GameObject statsPanel;
	public TMP_Text thrustStatsText;
	public TMP_Text fuelStatsText;
	public TMP_Text rotationStatsText;
	public TMP_Text armorStatsText;

	private string currentlyHoveredUpgrade = "";

	private void Awake()
	{
		InitializeUpgrades();
		SetupTooltips();
	}

	private void Start()
	{
		UpdateCurrentMoneyText();
		if (tooltipPanel != null)
		{
			tooltipPanel.SetActive(false);
		}
		UpdateStatsDisplay();
	}

	private void InitializeUpgrades()
	{
		upgrades = new List<Upgrade>
		{
			new Upgrade {
				name = "Engines",
				description = "Increases thrust power.\nAllows you to reach higher altitudes faster.",
				currentTier = 0,
				basePrice = 100,
				baseValue = 250,
				upgradeButton = engineUpgradeButton,
				upgradeText = engineUpgradeText
			},
			new Upgrade {
				name = "Fuel Tanks",
				description = "Increases maximum fuel amount.\nAllows you to fly for much longer.",
				currentTier = 0,
				basePrice = 150,
				baseValue = 10,
				upgradeButton = fuelTankUpgradeButton,
				upgradeText = fuelTankUpgradeText
			},
			new Upgrade {
				name = "Aerodynamics",
				description = "Improves steering speed.\nAllows you to control the ship better during flight.",
				currentTier = 0,
				basePrice = 200,
				baseValue = 0.1f,
				upgradeButton = aerodynamicsUpgradeButton,
				upgradeText = aerodynamicsUpgradeText
			},
			new Upgrade {
				name = "Hull",
				description = "Increases armor protection.\nHelps you survive againts impacts and lighting strikes.",
				currentTier = 0,
				basePrice = 250,
				baseValue = 10,
				upgradeButton = hullUpgradeButton,
				upgradeText = hullUpgradeText
			}
		};

		foreach (var upgrade in upgrades)
		{
			if (upgrade.upgradeButton != null)
			{
				upgrade.upgradeButton.onClick.AddListener(() => PurchaseUpgrade(upgrade));
			}

			UpdateUpgradeText(upgrade);
		}
	}

	private void SetupTooltips()
	{
		foreach (var upgrade in upgrades)
		{
			if (upgrade.upgradeButton != null)
			{
				EventTrigger eventTrigger = upgrade.upgradeButton.gameObject.GetComponent<EventTrigger>();
				if (eventTrigger == null)
				{
					eventTrigger = upgrade.upgradeButton.gameObject.AddComponent<EventTrigger>();
				}

				EventTrigger.Entry enterEntry = new EventTrigger.Entry();
				enterEntry.eventID = EventTriggerType.PointerEnter;
				enterEntry.callback.AddListener((eventData) => {
					ShowTooltip(upgrade);
					currentlyHoveredUpgrade = upgrade.name;
					UpdateStatsDisplay();
				});
				eventTrigger.triggers.Add(enterEntry);

				EventTrigger.Entry exitEntry = new EventTrigger.Entry();
				exitEntry.eventID = EventTriggerType.PointerExit;
				exitEntry.callback.AddListener((eventData) => {
					HideTooltip();
					currentlyHoveredUpgrade = "";
					UpdateStatsDisplay();
				});
				eventTrigger.triggers.Add(exitEntry);
			}
		}
	}

	private void UpdateStatsDisplay()
	{
		if (statsPanel == null) return;

		string thrustLine = FormatStatLine("Thrust", rocketController.thrust,
			currentlyHoveredUpgrade == "Engines" ? GetUpgradeByName("Engines").baseValue : 0);

		string fuelLine = FormatStatLine("Fuel Capacity", rocketController.maxFuel,
			currentlyHoveredUpgrade == "Fuel Tanks" ? GetUpgradeByName("Fuel Tanks").baseValue : 0);

		string rotationLine = FormatStatLine("Rotation Speed", rocketController.rotationSpeed,
			currentlyHoveredUpgrade == "Aerodynamics" ? GetUpgradeByName("Aerodynamics").baseValue : 0);

		string armorLine = FormatStatLine("Armor", rocketController.armor,
			currentlyHoveredUpgrade == "Hull" ? GetUpgradeByName("Hull").baseValue : 0);

		if (thrustStatsText != null) thrustStatsText.text = thrustLine;
		if (fuelStatsText != null) fuelStatsText.text = fuelLine;
		if (rotationStatsText != null) rotationStatsText.text = rotationLine;
		if (armorStatsText != null) armorStatsText.text = armorLine;
	}

	private string FormatStatLine(string statName, float currentValue, float increase)
	{
		if (increase > 0)
		{
			float newValue = currentValue + increase;
			return $"{statName}: \n{currentValue:F1} <color=green>-> {newValue:F1}</color>";
		}
		return $"{statName}: {currentValue:F1}";
	}

	private Upgrade GetUpgradeByName(string name)
	{
		return upgrades.Find(u => u.name == name);
	}

	private void ShowTooltip(Upgrade upgrade)
	{
		if (tooltipPanel != null && tooltipText != null)
		{
			string currentValue = GetCurrentUpgradeValue(upgrade);
			string nextValue = GetNextUpgradeValue(upgrade);

			tooltipText.text = $"{upgrade.description}\n\nCurrent Value: {currentValue}";

			if (upgrade.currentTier < 10)
			{
				tooltipText.text += $"\nNext Level: {nextValue}";
			}

			tooltipPanel.SetActive(true);
		}
	}

	private string GetCurrentUpgradeValue(Upgrade upgrade)
	{
		switch (upgrade.name)
		{
			case "Engines":
				return $"{rocketController.thrust:F0} thrust";
			case "Fuel Tanks":
				return $"{rocketController.maxFuel:F0} fuel capacity";
			case "Aerodynamics":
				return $"{rocketController.rotationSpeed:F1} rotation speed";
			case "Hull":
				return $"{rocketController.armor:F0} armor";
			default:
				return "N/A";
		}
	}

	private string GetNextUpgradeValue(Upgrade upgrade)
	{
		if (upgrade.currentTier >= 10) return "MAX";

		switch (upgrade.name)
		{
			case "Engines":
				return $"{rocketController.thrust + upgrade.baseValue:F0} thrust";
			case "Fuel Tanks":
				return $"{rocketController.maxFuel + upgrade.baseValue:F0} fuel capacity";
			case "Aerodynamics":
				return $"{rocketController.rotationSpeed + upgrade.baseValue:F1} rotation speed";
			case "Hull":
				return $"{rocketController.armor + upgrade.baseValue:F0} armor";
			default:
				return "N/A";
		}
	}

	private void HideTooltip()
	{
		if (tooltipPanel != null)
		{
			tooltipPanel.SetActive(false);
		}
	}

	public void PurchaseUpgrade(Upgrade upgrade)
	{
		float cost = CalculateUpgradeCost(upgrade);
		if (gameManager.GetTotalMoneyEarned() >= cost && upgrade.currentTier < 10)
		{
			gameManager.SpendMoney(cost);
			upgrade.currentTier++;
			ApplyUpgrade(upgrade);
			UpdateUpgradeText(upgrade);
			UpdateCurrentMoneyText();
			UpdateStatsDisplay();
		}
	}

	private float CalculateUpgradeCost(Upgrade upgrade)
	{
		return upgrade.basePrice * Mathf.Pow(2, upgrade.currentTier);
	}

	private void ApplyUpgrade(Upgrade upgrade)
	{
		switch (upgrade.name)
		{
			case "Engines":
				rocketController.thrust += upgrade.baseValue;
				break;
			case "Fuel Tanks":
				rocketController.maxFuel += upgrade.baseValue;
				break;
			case "Aerodynamics":
				rocketController.rotationSpeed += upgrade.baseValue;
				break;
			case "Hull":
				rocketController.armor += upgrade.baseValue;
				break;
		}
	}

	private void UpdateUpgradeText(Upgrade upgrade)
	{
		if (upgrade.upgradeText != null)
		{
			if (upgrade.currentTier >= 10)
			{
				upgrade.upgradeText.text = $"{upgrade.name}\n<color=green>PATH COMPLETE!</color>";
				upgrade.upgradeButton.interactable = false;
			}
			else
			{
				float nextCost = CalculateUpgradeCost(upgrade);
				string costColor = gameManager.GetTotalMoneyEarned() >= nextCost ? "green" : "red";
				upgrade.upgradeText.text = $"{upgrade.name} (Tier {upgrade.currentTier}/10)\nCost: <color={costColor}>${nextCost:F2}</color>";
			}
		}
	}

	public void UpdateCurrentMoneyText()
	{
		if (currentMoneyText != null)
		{
			currentMoneyText.text = $"Current Money: ${gameManager.GetTotalMoneyEarned():F2}";
		}

		foreach (var upgrade in upgrades)
		{
			UpdateUpgradeText(upgrade);
		}
	}

	public void ResetUpgrades()
	{
		foreach (var upgrade in upgrades)
		{
			upgrade.currentTier = 0;
			upgrade.upgradeButton.interactable = true;
			UpdateUpgradeText(upgrade);
		}

		rocketController.thrust = 2500f;
		rocketController.maxFuel = 100f;
		rocketController.rotationSpeed = 1000f;
		rocketController.armor = 0f;

		UpdateCurrentMoneyText();
		UpdateStatsDisplay();
	}
}
