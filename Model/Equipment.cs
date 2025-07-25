namespace RotMGAssetExtractor.Model
{
    public class Equipment : Object
    {
        public string DisplayId { get; set; }
        public string Description { get; set; }
        public string Activate { get; set; }
        public string Rarity { get; set; }
        public string PetFamily { get; set; }
        public string Labels { get; set; }
        public ModelHelpers.AnimatedTexture AnimatedTexture { get; set; }

        public bool Item { get; set; } = false;
        public bool Soulbound { get; set; } = false;
        public bool Consumable { get; set; } = false;
        public bool DropTradable { get; set; } = false;
        public bool Track { get; set; } = false;
        public bool UniqueID { get; set; } = false;

        public int SlotType { get; set; }
        public int BagType { get; set; }
        public int feedPower { get; set; }
        public int Tier { get; set; }
        public int XPBonus { get; set; }
        public int Quantity { get; set; }

        public int CollectionIcon { get; set; }
        public int SetType { get; set; }
        public string SetName { get; set; }
        public bool ExtraTooltipData { get; set; }
        public bool Treasure { get; set; }
        public string ActivateOnEquip { get; set; }
        public string Sound { get; set; }
        public bool EnchantmentSlots { get; set; }
        public ModelHelpers.Projectile Projectile { get; set; }
        public int ItemTier { get; set; }
        public int PowerLevel { get; set; }
        public string Subattack { get; set; } // need class
        public string Shader { get; set; } // needs class?
        public string OldSound { get; set; }
        public bool Usable { get; set; }
        public int MpCost { get; set; }
        public string OnPlayerShootActivate { get; set; }
        public string Flipbook { get; set; }
        public float RateOfFire { get; set; }
        public int NumProjectiles { get; set; }
        public string OnPlayerAbilityActivate { get; set; }
        public bool NoSeasonalToRegularConversion { get; set; }
        public bool Potion { get; set; }
        public int ArcGap { get; set; }
        public string OnPlayerKill { get; set; }
        public string InnerGlow { get; set; }
        public string OnPlayerHitActivate { get; set; }
        public bool Artifact { get; set; }
        public int ScaleValue { get; set; }
        public bool InventoryAnimation { get; set; }
        public string Pulse { get; set; }
        public float Cooldown { get; set; }
        public bool Key { get; set; }
        public string SharedCooldownChannel { get; set; } // class
        public bool PetFood { get; set; }
        public string Chest { get; set; } // class?
        public int ChestType { get; set; }
        public bool SeasonalOnly { get; set; }
        public bool QuickslotAllowed { get; set; }
        public string SuccessorId { get; set; }
        public bool InvUse { get; set; }
        public int MaxProjSpawnDist { get; set; }
        public bool Mark { get; set; }
        public string PetId { get; set; }
        public string ConditionEffect { get; set; }
        public bool AdminOnly { get; set; }
        public string Ability { get; set; } // class
        public int Doses { get; set; }
        public int MpEndCost { get; set; }
        public bool MultiPhase { get; set; }
        public string OnConditionEndActivate { get; set; }
        public int MpCostPerSecond { get; set; }
        public string Hologram { get; set; }
        public string Meter { get; set; } // class
        public string PetSkin { get; set; }
        public string CommonDungeon { get; set; }
        public string StartUse { get; set; }
        public string EndUse { get; set; }
        public string OnMeterFull { get; set; }
        public string Scroller { get; set; }
        public bool ForbidUseOnMaxHP { get; set; }
        public bool SmallSkillPotion { get; set; }
        public bool GuaranteeEnchDrop { get; set; }
        public int BurstCount { get; set; }
        public float BurstDelay { get; set; }
        public float BurstMinDelay { get; set; }
        public bool ForbidUseOnMaxMP { get; set; }
        public float Timer { get; set; }
        public bool Backpack { get; set; }
        public bool NoExchange { get; set; }
        public bool Saddlebag { get; set; }
        public string Credits { get; set; }
        public bool XpBoost { get; set; }
        public bool LTBoosted { get; set; }
        public bool LDBoosted { get; set; }
        public int Rotation { get; set; }
        public bool RemoveShader { get; set; }
        public string DungeonMods { get; set; }
        public int SwapOnSeasonEnd { get; set; }
        public string OnSwitchAbilityActivate { get; set; }
        public bool TrackLoot { get; set; }
        public bool VaultItem { get; set; }
        public bool PetFormStone { get; set; }
        public bool AddsQuickslot { get; set; }

    }
}
