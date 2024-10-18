using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
	[System.Serializable]
	public class Upgrade
	{
		public string name;
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

	private void Awake()
	{
		InitializeUpgrades();
	}

	private void Start()
	{
		UpdateCurrentMoneyText();
	}

	private void InitializeUpgrades()
	{
		upgrades = new List<Upgrade>
		{
			new Upgrade { name = "Engines", currentTier = 0, basePrice = 100, baseValue = 250, upgradeButton = engineUpgradeButton, upgradeText = engineUpgradeText },
			new Upgrade { name = "Fuel Tanks", currentTier = 0, basePrice = 150, baseValue = 10, upgradeButton = fuelTankUpgradeButton, upgradeText = fuelTankUpgradeText },
			new Upgrade { name = "Aerodynamics", currentTier = 0, basePrice = 200, baseValue = 0.1f, upgradeButton = aerodynamicsUpgradeButton, upgradeText = aerodynamicsUpgradeText },
			new Upgrade { name = "Hull", currentTier = 0, basePrice = 250, baseValue = 10, upgradeButton = hullUpgradeButton, upgradeText = hullUpgradeText }
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
			float nextCost = CalculateUpgradeCost(upgrade);
			upgrade.upgradeText.text = $"{upgrade.name} (Tier {upgrade.currentTier}/10)\nCost: ${nextCost:F2}";
		}
	}

	public void UpdateCurrentMoneyText()
	{
		if (currentMoneyText != null)
		{
			currentMoneyText.text = $"Current Money: ${gameManager.GetTotalMoneyEarned():F2}";
		}
	}

	public void ResetUpgrades()
	{
		foreach (var upgrade in upgrades)
		{
			upgrade.currentTier = 0;
			UpdateUpgradeText(upgrade);
		}

		rocketController.thrust = 2500f;
		rocketController.maxFuel = 100f;
		rocketController.rotationSpeed = 1000f;
		rocketController.armor = 0f;

		UpdateCurrentMoneyText();
	}
}
