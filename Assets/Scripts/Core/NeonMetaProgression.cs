using UnityEngine;

namespace NeonArena
{
    public enum ShipType
    {
        Vex09 = 0,
        Phantom7 = 1,
        TitanMk4 = 2
    }

    public enum ShopUpgradeType
    {
        Hull,
        Damage,
        Speed,
        Magnet,
        Greed
    }

    public struct ShipDefinition
    {
        public ShipType Type;
        public string Name;
        public string Role;
        public string Perk;
        public int Cost;
        public Color AccentColor;
    }

    /// <summary>
    /// Persistent meta-progression saved via PlayerPrefs: Neon Credits, 3 unlockable Ships,
    /// and 5 permanent Hangar Shop upgrades.
    /// </summary>
    public static class NeonMetaProgression
    {
        private const string CreditsKey = "NeonArena_Credits";
        private const string SelectedShipKey = "NeonArena_SelectedShip";
        public const int MaxUpgradeLevel = 5;

        public static readonly ShipDefinition[] Ships =
        {
            new ShipDefinition
            {
                Type = ShipType.Vex09,
                Name = "VEX-09",
                Role = "DENGELİ SİBER AVCI",
                Perk = "Dengeli hız, zırh ve çift namlulu lazer çekirdeği",
                Cost = 0,
                AccentColor = new Color(0f, 0.95f, 1f)
            },
            new ShipDefinition
            {
                Type = ShipType.Phantom7,
                Name = "PHANTOM-7",
                Role = "SUİKASTÇI & KRİTİK",
                Perk = "+%20 Hız • +%18 Kritik Şansı • -%30 Dash Süresi",
                Cost = 1800,
                AccentColor = new Color(0.88f, 0.25f, 1f)
            },
            new ShipDefinition
            {
                Type = ShipType.TitanMk4,
                Name = "TITAN-MK4",
                Role = "AĞIR SAVAŞ GEMİSİ",
                Perk = "+55 Maks Can • +%22 Hasar • 1 Yörünge Bıçağı ile başlar",
                Cost = 4500,
                AccentColor = new Color(1f, 0.72f, 0.15f)
            }
        };

        public static int Credits
        {
            get => PlayerPrefs.GetInt(CreditsKey, 0);
            private set
            {
                PlayerPrefs.SetInt(CreditsKey, Mathf.Max(0, value));
                PlayerPrefs.Save();
            }
        }

        public static ShipType SelectedShip
        {
            get => (ShipType)PlayerPrefs.GetInt(SelectedShipKey, 0);
            private set
            {
                PlayerPrefs.SetInt(SelectedShipKey, (int)value);
                PlayerPrefs.Save();
            }
        }

        public static void AddCredits(int amount)
        {
            if (amount <= 0) return;
            Credits += amount;
        }

        public static bool IsShipUnlocked(ShipType type)
        {
            if (type == ShipType.Vex09) return true;
            return PlayerPrefs.GetInt($"NeonArena_Ship_{type}", 0) == 1;
        }

        public static bool TrySelectOrBuyShip(ShipType type)
        {
            if (IsShipUnlocked(type))
            {
                SelectedShip = type;
                return true;
            }

            int cost = GetShipDef(type).Cost;
            if (Credits < cost) return false;

            Credits -= cost;
            PlayerPrefs.SetInt($"NeonArena_Ship_{type}", 1);
            SelectedShip = type;
            PlayerPrefs.Save();
            return true;
        }

        public static ShipDefinition GetShipDef(ShipType type)
        {
            for (int i = 0; i < Ships.Length; i++)
                if (Ships[i].Type == type) return Ships[i];
            return Ships[0];
        }

        public static int GetUpgradeLevel(ShopUpgradeType type)
        {
            return GetUpgradeLevel(SelectedShip, type);
        }

        public static int GetUpgradeLevel(ShipType ship, ShopUpgradeType type)
        {
            return PlayerPrefs.GetInt($"NeonArena_Shop_{ship}_{type}", 0);
        }

        public static int GetUpgradeCost(ShopUpgradeType type)
        {
            int lv = GetUpgradeLevel(SelectedShip, type);
            float shipMult = SelectedShip == ShipType.TitanMk4 ? 1.5f : (SelectedShip == ShipType.Phantom7 ? 1.25f : 1f);
            int baseCost = 200 + lv * 260 + lv * lv * 140;
            return Mathf.RoundToInt(baseCost * shipMult);
        }

        public static bool TryBuyUpgrade(ShopUpgradeType type)
        {
            var ship = SelectedShip;
            int lv = GetUpgradeLevel(ship, type);
            if (lv >= MaxUpgradeLevel) return false;

            int cost = GetUpgradeCost(type);
            if (Credits < cost) return false;

            Credits -= cost;
            PlayerPrefs.SetInt($"NeonArena_Shop_{ship}_{type}", lv + 1);
            PlayerPrefs.Save();
            return true;
        }

        // Permanent stat bonuses
        public static float BonusMaxHealth => GetUpgradeLevel(ShopUpgradeType.Hull) * 18f;
        public static float DamageMultiplier => 1f + GetUpgradeLevel(ShopUpgradeType.Damage) * 0.12f;
        public static float SpeedMultiplier => 1f + GetUpgradeLevel(ShopUpgradeType.Speed) * 0.06f;
        public static float MagnetMultiplier => 1f + GetUpgradeLevel(ShopUpgradeType.Magnet) * 0.16f;
        public static float CreditMultiplier => 1f + GetUpgradeLevel(ShopUpgradeType.Greed) * 0.18f;
    }
}
