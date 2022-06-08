using System;
using System.Collections.Generic;
using System.Text;

namespace WorldReader
{
    public class LogicDatabase
    {
        [Flags]
        public enum SeedRepresentation : long
        {
            None = 0x0L,
            ItemIceaxe = 0x1L, // Axe
            ApocalypseAmashilama = 0x2L, // No swim + 3 tile high jump
            ItemGishru = 0x4L, // Boomerang
            ApocalypseSheshkala = 0x8L, // Ledgegrab
            ApocalypseLiru = 0x10L, // Underwater Movement
            ApocalypseGudAnna = 0x20L, // Explosion
            ApocalypseMushhus = 0x40L, // Hack
            ApocalypseUman = 0x80L, // Drone
            ItemBronzeAxe = 0x100L, // Upgrade Axe
            ApocalypseNinhursag = 0x200L, // Wallclimb
            ItemCarnelianRing = 0x400L, // Axe charged projectile
            ApocalypseEskiri = 0x800L, // Hook
            ItemDoubleaxe = 0x1000L, // Big Axe
            ApocalypseUsanRah = 0x2000L, // Drone charge attack
            ApocalypseDun = 0x4000L, // Teleport
            ApocalypseImdugBir = 0x8000L, // Drone hook spin
            ItemUdusan = 0x10000L, // Drone attack upgrade
            ApocalypseAnuman = 0x20000L, // Body swap
            ItemRoyalBracelet = 0x40000L, // DoubleAxe Upgrade
            ApocalypseKi = 0x80000L, // Breach Vision
            ApocalypseBarash = 0x100000L, // Drone Hover
            ItemCopperDagger = 0x200000L, // Dagger
            ApocalypseHalam = 0x400000L, // Breach exit
            ItemBreachAttractor = 0x800000L, // Breach attractor
            ItemHalusan = 0x1000000L, // Drone charge projectile
            ItemAnGishru = 0x2000000L, // Remote boomerang
            ItemUlGishru = 0x4000000L, // Boomerang upgrade
            ApocalypseTilhar = 0x8000000L, // Nanocloud
            ItemSickleSword = 0x10000000L, // Sicklesword
            ApocalypseZaltu = 0x20000000L, // Instant hook spin
            ItemRoyalRing = 0x40000000L, // Dagger upgrade projectile at full health
            ApocalypseRah = 0x80000000L, // Axe charge attack
            ItemCompass = 0x100000000L, // Compass
            ItemCompassGem = 0x200000000L, // Compass Gem
            ItemEyeRing = 0x400000000L, // Visibility in the dark
            ItemNanolattice1 = 0x800000000L,
            ItemNanolattice2 = 0x1000000000L,
            ItemNanolattice3 = 0x2000000000L,
            ItemPowerMatrix1 = 0x4000000000L,
            ItemPowerMatrix2 = 0x8000000000L,
            ItemPowerMatrix3 = 0x10000000000L,
            ItemPowerMatrix4 = 0x20000000000L,
            ItemIsBreakable = 0x40000000000L,
        }

        public static Dictionary<string, Dictionary<uint, List<SeedRepresentation>>> logicDatabase = new Dictionary<string, Dictionary<uint, List<SeedRepresentation>>>
        {
            { "Outside", new Dictionary<uint, List<SeedRepresentation>>
                {
                    { 2744, new List<SeedRepresentation> // HealthNode1 // glacier // FirstSavePointRoom
                        {
                            SeedRepresentation.ItemIceaxe | SeedRepresentation.ItemCarnelianRing | SeedRepresentation.ItemIsBreakable, // maybe you need amashilama
                            SeedRepresentation.ApocalypseAmashilama | SeedRepresentation.ApocalypseSheshkala,
                            SeedRepresentation.ItemGishru | SeedRepresentation.ItemIsBreakable,
                            SeedRepresentation.ApocalypseGudAnna,
                            SeedRepresentation.ApocalypseUman,
                            SeedRepresentation.ItemBronzeAxe | SeedRepresentation.ItemCarnelianRing | SeedRepresentation.ItemIsBreakable, // Maybe you need amashilama
                            SeedRepresentation.ApocalypseNinhursag,
                            SeedRepresentation.ApocalypseAnuman,
                            SeedRepresentation.ItemCopperDagger | SeedRepresentation.ItemRoyalRing,
                            SeedRepresentation.ItemIsBreakable | SeedRepresentation.ItemAnGishru,
                            SeedRepresentation.ItemIsBreakable | SeedRepresentation.ItemUlGishru,
                            SeedRepresentation.ApocalypseTilhar,
                        }
                    },
                }
            },
            { "Inside", new Dictionary<uint, List<SeedRepresentation>>
                {
                    { 393, new List<SeedRepresentation> // IceAxe // intro-region // FirstRoom
                        {
                            SeedRepresentation.None
                        }
                    },
                }
            },
            { "Breach", new Dictionary<uint, List<SeedRepresentation>>
                {
                    { 3566, new List<SeedRepresentation> // PowerMatrix1 // mountain // FirstBreachRoom
                        {
                            SeedRepresentation.ApocalypseUman | SeedRepresentation.ApocalypseMushhus,
                            SeedRepresentation.ApocalypseUman | SeedRepresentation.ItemBreachAttractor,
                            SeedRepresentation.ApocalypseUman | SeedRepresentation.ApocalypseTilhar | SeedRepresentation.ApocalypseKi | SeedRepresentation.ApocalypseEskiri,
                            SeedRepresentation.ApocalypseUman | SeedRepresentation.ApocalypseTilhar | SeedRepresentation.ApocalypseKi | SeedRepresentation.ApocalypseImdugBir,
                            SeedRepresentation.ApocalypseUman | SeedRepresentation.ApocalypseTilhar | SeedRepresentation.ApocalypseKi | SeedRepresentation.ApocalypseZaltu,
                            SeedRepresentation.ApocalypseAnuman | SeedRepresentation.ApocalypseMushhus,
                            SeedRepresentation.ApocalypseAnuman | SeedRepresentation.ItemBreachAttractor,
                            SeedRepresentation.ApocalypseAnuman | SeedRepresentation.ApocalypseTilhar | SeedRepresentation.ApocalypseKi | SeedRepresentation.ApocalypseEskiri,
                            SeedRepresentation.ApocalypseAnuman | SeedRepresentation.ApocalypseTilhar | SeedRepresentation.ApocalypseKi | SeedRepresentation.ApocalypseImdugBir,
                            SeedRepresentation.ApocalypseAnuman | SeedRepresentation.ApocalypseTilhar | SeedRepresentation.ApocalypseKi | SeedRepresentation.ApocalypseZaltu,
                        }
                    },
                }
            }
        };

    }
}
