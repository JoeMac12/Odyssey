using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
	[System.Serializable]
	public class RocketPart
	{
		public string upgradeName;
		public GameObject[] partObjects;
	}

	[System.Serializable]
	public class Upgrade
	{
		public string name;
		public string description;
		public int currentTier;
		public float basePrice;
		public float baseValue;
		public float basePercentage;
		public Button upgradeButton;
		public TMP_Text upgradeText;
		public int partsChangeThreshold = 2;
	}

	public List<Upgrade> upgrades;
	public List<RocketPart> rocketParts;
	public GameManager gameManager;
	public RocketController rocketController;
	public UISoundSystem uiSoundSystem;

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
		InitializeRocketParts();
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

	private void InitializeRocketParts()
	{
		foreach (var rocketPart in rocketParts)
		{
			if (rocketPart.partObjects != null)
			{
				foreach (var partObject in rocketPart.partObjects)
				{
					if (partObject != null)
					{
						partObject.SetActive(false);
					}
				}
			}
		}

		foreach (var upgrade in upgrades)
		{
			UpdateRocketVisuals(upgrade);
		}
	}

	private void UpdateRocketVisuals(Upgrade upgrade)
	{
		var rocketPart = rocketParts.Find(part => part.upgradeName == upgrade.name);
		if (rocketPart == null || rocketPart.partObjects == null) return;

		int visiblePartIndex = upgrade.currentTier / upgrade.partsChangeThreshold;
		visiblePartIndex = Mathf.Min(visiblePartIndex, rocketPart.partObjects.Length - 1);

		foreach (var partObject in rocketPart.partObjects)
		{
			if (partObject != null)
			{
				partObject.SetActive(false);
			}
		}

		if (upgrade.currentTier > 0 && visiblePartIndex >= 0 && visiblePartIndex < rocketPart.partObjects.Length)
		{
			if (rocketPart.partObjects[visiblePartIndex] != null)
			{
				rocketPart.partObjects[visiblePartIndex].SetActive(true);
			}
		}
	}

	private void InitializeUpgrades()
	{
		upgrades = new List<Upgrade>
		{
			new Upgrade {
				name = "Engines",
				description = "Advanced propulsion systems that dramatically increase thrust power.\n\n" +
							"Powerful engines are crucial for achieving higher altitudes and breaking through dense atmosphere. " +
							"Each upgrade significantly boosts your vertical acceleration and maximum speed potential.",
				currentTier = 0,
				basePrice = 100,
				baseValue = 500,
				upgradeButton = engineUpgradeButton,
				upgradeText = engineUpgradeText
			},
			new Upgrade {
				name = "Fuel Tanks",
				description = "High-capacity fuel containment systems with improved efficiency.\n\n" +
							"Larger fuel reserves allow for extended flight duration and greater altitude potential. " +
							"Essential for reaching the highest points of the atmosphere without running dry.",
				currentTier = 0,
				basePrice = 150,
				baseValue = 50,
				upgradeButton = fuelTankUpgradeButton,
				upgradeText = fuelTankUpgradeText
			},
			new Upgrade {
				name = "Aerodynamics",
				description = "Enhanced control fin surfaces and stabilization systems.\n\n" +
							"Better aerodynamics provide precise maneuvering in all conditions. " +
							"Critical for maintaining stability during high-speed flight and countering strong winds at altitude.",
				currentTier = 0,
				basePrice = 200,
				baseValue = 250,
				upgradeButton = aerodynamicsUpgradeButton,
				upgradeText = aerodynamicsUpgradeText
			},
			new Upgrade {
				name = "Hull",
				description = "Reinforced structural integrity with advanced materials.\n\n" +
							"Strengthened hull plating reduces incoming damage. " +
							"Each upgrade increases your damage resistance, making you more resilient to all types of impacts and weather.",
				currentTier = 0,
				basePrice = 250,
				baseValue = 0,
				basePercentage = 5f,
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

	private void ShowTooltip(Upgrade upgrade)
	{
		if (tooltipPanel != null && tooltipText != null)
		{
			tooltipText.text = upgrade.description;
			tooltipPanel.SetActive(true);
		}
	}

	private void HideTooltip()
	{
		if (tooltipPanel != null)
		{
			tooltipPanel.SetActive(false);
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

		string armorLine = FormatStatLine("Damage Reduction", rocketController.armorPercentage,
			currentlyHoveredUpgrade == "Hull" ? GetUpgradeByName("Hull").basePercentage : 0,
			true);

		if (thrustStatsText != null) thrustStatsText.text = thrustLine;
		if (fuelStatsText != null) fuelStatsText.text = fuelLine;
		if (rotationStatsText != null) rotationStatsText.text = rotationLine;
		if (armorStatsText != null) armorStatsText.text = armorLine;
	}

	private string FormatStatLine(string statName, float currentValue, float increase, bool isPercentage = false)
	{
		if (increase > 0)
		{
			float newValue = currentValue + increase;
			string format = isPercentage ? "{0:F1}%" : "{0:F1}";
			return $"{statName}: \n{string.Format(format, currentValue)} <color=green>-> {string.Format(format, newValue)}</color>";
		}
		return $"{statName}: {(isPercentage ? $"{currentValue:F1}%" : $"{currentValue:F1}")}";
	}

	private Upgrade GetUpgradeByName(string name)
	{
		return upgrades.Find(u => u.name == name);
	}

	public void PurchaseUpgrade(Upgrade upgrade)
	{
		float cost = CalculateUpgradeCost(upgrade);
		if (gameManager.GetTotalMoneyEarned() >= cost && upgrade.currentTier < 10)
		{
			gameManager.SpendMoney(cost);
			uiSoundSystem.PlayUpgradeSuccessSound();
			upgrade.currentTier++;
			ApplyUpgrade(upgrade);
			UpdateUpgradeText(upgrade);
			UpdateCurrentMoneyText();
			UpdateStatsDisplay();
			UpdateRocketVisuals(upgrade);
		}
		else
		{
			uiSoundSystem.PlayUpgradeFailSound();
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
				rocketController.armorPercentage += upgrade.basePercentage;
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
			UpdateRocketVisuals(upgrade);
		}

		rocketController.thrust = 1250f;
		rocketController.maxFuel = 100f;
		rocketController.rotationSpeed = 250f;
		rocketController.armorPercentage = 0f;

		UpdateCurrentMoneyText();
		UpdateStatsDisplay();
	}
}
