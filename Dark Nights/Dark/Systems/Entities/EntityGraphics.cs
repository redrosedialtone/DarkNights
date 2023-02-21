using Dark.World;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dark
{
    public interface IEntityGraphics
    {
        string MaterialID { get; }
        int DrawPriority { get; }
        EntityLayer Layer { get; }

        EntityTextureSettings GetTexture(ITileData TileData);
        void AddTextureRules(ITextureRule[] textureAdjacentSelectionRules);
    }

    public interface ITextureRule
    {
        bool Match(ITileData Data, AdjacentTileData adjacencyData, TextureAdjacencyData neighbourData, out EntityTextureSettings TextureSettings);
    }

    public class EntityTextureSettings
    {
        public string TextureID;
        public float Angle;
        public bool[] ReadFromNeighbours;
        public bool MirrorX;

        public override string ToString()
        {
            return $"TextureSettings::ID[{TextureID}] Angle[{Angle}]";
        }
    }

    public struct TextureAdjacencyData
    {
        public int ExactNeighbours;
        public int LayerNeighbours;
    }
}

namespace Dark.Entities { 

    public class EntityGraphics : IEntityGraphics
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[ENTITY]");

        public string TextureID { get; protected set; }
        public string MaterialID { get; protected set; } = "tilables";
        public int DrawPriority { get; protected set; } = 0;
        public EntityLayer Layer { get; protected set; }

        public bool Instanced = true;
        private ITextureRule[] TextureRules;
        private readonly string DefName;

        public EntityGraphics(string TextureID, string MaterialID, string DefName, EntityLayer Layer = EntityLayer.BASE)
        { this.TextureID = TextureID; this.MaterialID = MaterialID; this.Layer = Layer; this.DefName = DefName; }

        public EntityTextureSettings GetTexture(ITileData TileData)
        {
            AdjacentTileData adjacentTileData = TileManager.AdjacencyData(TileData);
            if (TextureRules != null)
            {
                int exactNeighbours = 0;
                int layerNeighbours = 0;

                foreach (var neighbour in adjacentTileData)
                {
                    if (neighbour != null)
                    {
                        foreach (var entity in neighbour.Container.TileEntities())
                        {
                            if (entity.DefName == DefName)
                            {
                                exactNeighbours++;
                            }
                            if (entity.EntityGraphicsDef != null && entity.EntityGraphicsDef.Layer == this.Layer)
                            {
                                layerNeighbours++;
                            }

                        }
                    }
                }

                TextureAdjacencyData textureAdjacencyData = new TextureAdjacencyData()
                {
                    ExactNeighbours = exactNeighbours,
                    LayerNeighbours = layerNeighbours
                };

                for (int i = 0; i < TextureRules.Length; i++)
                {
                    var textureRule = TextureRules[i];
                   
                    if (textureRule.Match(TileData, adjacentTileData, textureAdjacencyData, out EntityTextureSettings Settings))
                    {
                        return Settings;
                    }
                }
            }
            else
            {
                return new EntityTextureSettings()
                {
                    TextureID = TextureID,
                    Angle = 0,
                    ReadFromNeighbours = null
                };
            }

            return new EntityTextureSettings()
            {
                TextureID = TextureID,
                Angle = 0,
                ReadFromNeighbours = Enumerable.Repeat(true, 8).ToArray()
            };

        }

        public void AddTextureRules(ITextureRule[] Rules)
        {
            TextureRules = Rules;
        }
    }



    public enum NeighbourRule { DontCare = 0, Exists = 1, Not = -1 }

    public class TextureAdjacentSelectionRule : ITextureRule
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[TEXADJRULE]");

        public IEntity Entity;
        public string TextureID { get; protected set; }
        public enum TransformRule { Fixed, Rotated, MirrorX, MirrorY, RMirrorX, RMirrorY }
        public TransformRule Transform;
        //[0] [1] [2] 
        //[3] [-] [4]
        //[5] [6] [7]
        public NeighbourRule[] NeighbourRules;

        public enum MatchRule { ExactType, Layer }
        public MatchRule TypeMatch;

        private AdjacentTileData adjacentTileData;
        private readonly EntityLayer renderLayer;

        public TextureAdjacentSelectionRule(IEntity Entity, string TextureID, TransformRule Transform, int[] Rules, MatchRule TypeMatch = MatchRule.ExactType)
        {
            this.Entity = Entity;
            this.TextureID = TextureID;
            this.Transform = Transform;
            this.renderLayer = Entity.EntityGraphicsDef.Layer;
            this.TypeMatch = TypeMatch;
            NeighbourRules = new NeighbourRule[8];
            for (int i = 0; i < 8; i++)
            {
                NeighbourRules[i] = (NeighbourRule)Rules[i];
            }
        }

        public bool Match(ITileData Data, AdjacentTileData adjacencyData, TextureAdjacencyData neighbourData, out EntityTextureSettings Settings)
        {
            string _transformLog = Transform == TransformRule.Fixed ? "FIXED" : "ROTATED";
            adjacentTileData = adjacencyData;          
            Settings = new EntityTextureSettings()
            {
                TextureID = this.TextureID,
                Angle = 0,
                ReadFromNeighbours = Enumerable.Repeat(true, 8).ToArray()
            };

            log.Trace($"{adjacentTileData.Origin}::CHECKING::{this.TextureID}::{_transformLog}::{neighbourData.LayerNeighbours}::{neighbourData.ExactNeighbours}");

            int maxNeighbourCount = 8;
            int minNeighbourCount = 0;

            for (int i = 0; i < 8; i++)
            {
                if (NeighbourRules[i] == NeighbourRule.Not)
                {
                    maxNeighbourCount--;
                }
                else if (NeighbourRules[i] == NeighbourRule.Exists)
                {
                    minNeighbourCount++;
                }
            }
            int val = TypeMatch == MatchRule.ExactType ? neighbourData.ExactNeighbours : neighbourData.LayerNeighbours;
            if (val > maxNeighbourCount || val < minNeighbourCount)
            {
                log.Trace($"{this.TextureID}::NEIGHBOURCHECK::<color=red>FAIL</color>");
                return false;
            }

            bool allRulesPass = true;
            switch (Transform)
            {
                case TransformRule.Fixed:
                    for (int i = 0; i < 8; i++)
                    {
                        switch (NeighbourRules[i])
                        {
                            case NeighbourRule.Exists:
                                allRulesPass = allRulesPass && MatchingNeighbour(adjacentTileData.AdjacentTiles[i]);
                                break;
                            case NeighbourRule.Not:
                                allRulesPass = allRulesPass && !MatchingNeighbour(adjacentTileData.AdjacentTiles[i]);
                                break;
                            default:
                                break;
                        }
                    }
                    if (allRulesPass)
                    {
                        log.Trace($"{this.TextureID}::<color=green>PASS</color>");
                    }
                    else
                    {
                        log.Trace($"{this.TextureID}::<color=red>FAIL</color>");
                    }
                    
                    return allRulesPass;
                case TransformRule.Rotated:
                    if (RotationMatch(out float Angle))
                    {
                        log.Trace($"{this.TextureID}::<color=green>PASS</color>");
                        Settings = new EntityTextureSettings()
                        {
                            TextureID = this.TextureID,
                            Angle = Angle,
                            ReadFromNeighbours = Enumerable.Repeat(true, 8).ToArray()
                        };
                        return true;
                    }
                    log.Trace($"{this.TextureID}::<color=red>FAIL</color>");
                    return false;
                case TransformRule.RMirrorX:
                    if (RotationMatch(out float _Angle))
                    {
                        log.Trace($"{this.TextureID}::<color=green>PASS</color>");
                        Settings = new EntityTextureSettings()
                        {
                            TextureID = this.TextureID,
                            Angle = _Angle,
                            ReadFromNeighbours = Enumerable.Repeat(true, 8).ToArray()
                        };
                        return true;
                    }
                    if (RotatedMirrorMatch(out float mirroredAngle))
                    {
                        log.Trace($"{this.TextureID}::<color=green>MIRROR PASS</color>");
                        Settings = new EntityTextureSettings()
                        {
                            TextureID = this.TextureID,
                            Angle = mirroredAngle,
                            ReadFromNeighbours = Enumerable.Repeat(true, 8).ToArray(),
                            MirrorX = true
                        };
                        return true;
                    }
                    log.Trace($"{this.TextureID}::<color=red>FAIL</color>");
                    return false;
                default:
                    return false;
            }
        }

        private bool MatchingNeighbour(ITileData Neighbour)
        {
            if (Neighbour == null) return false;
            switch (TypeMatch)
            {
                case MatchRule.ExactType:
                    IEntity Match = Neighbour.Container.GetEntity(Entity.DefName);
                    return Match != null;
                case MatchRule.Layer:
                    IEntity[] Matches = Neighbour.Container.EntitiesInLayer(renderLayer);
                    if (Matches != null && Matches.Length > 0)
                    {
                        return true;
                    }
                    return false;
                default:
                    break;
            }
            return false;
            
        }

        private bool RotationMatch(out float Angle)
        {
            // Check every 90 degree angle
            for (int angle = 0; angle <= 270; angle += 90)
            {
                log.Trace($"@{angle}::");
                NeighbourRule[] RotatedRules = new NeighbourRule[8];
                for (int i = 0; i < 8; i++)
                {
                    NeighbourRule rule = NeighbourRules[GetRotatedIndex(i, angle)];
                    RotatedRules[i] = rule;
                    //this.Debug($"RotatedRules[{i}:{AdjacentTileData.IndexToString[i]}] = {rule}", LoggingPriority.Low);
                }

                bool angleRulesPass = true;
                // Check each neighbour
                for (int i = 0; i < 8; i++)
                {
                    switch (RotatedRules[i])
                    {
                        case NeighbourRule.Exists:
                            angleRulesPass = angleRulesPass && MatchingNeighbour(adjacentTileData.AdjacentTiles[i]);
                            string passLog = angleRulesPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>";
                            log.Trace($"{AdjacentTileData.IndexToString[i]} - Exists? = {passLog}");
                            break;
                        case NeighbourRule.Not:
                            angleRulesPass = angleRulesPass && !MatchingNeighbour(adjacentTileData.AdjacentTiles[i]);
                            string _passLog = angleRulesPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>";
                            log.Trace($"{AdjacentTileData.IndexToString[i]} - Not? = {_passLog}");
                            break;
                        default:
                            break;
                    }
                    if (!angleRulesPass) break;
                }
                if (angleRulesPass)
                {
                    Angle = angle;
                    return true;
                }
            }
            Angle = 0;
            return false;
        }

        private bool RotatedMirrorMatch(out float Angle)
        {
            // Check every 90 degree angle
            for (int angle = 0; angle <= 270; angle += 90)
            {
                log.Trace($"@{angle}::");
                NeighbourRule[] RotatedRules = new NeighbourRule[8];
                for (int i = 0; i < 8; i++)
                {
                    NeighbourRule rule = NeighbourRules[GetRotatedMirroredIndex(i, angle)];
                    RotatedRules[i] = rule;
                    log.Trace($"RotatedMirrorRules[{i}:{AdjacentTileData.IndexToString[i]}] = {rule}");
                }

                bool angleRulesPass = true;
                // Check each neighbour
                for (int i = 0; i < 8; i++)
                {
                    switch (RotatedRules[i])
                    {
                        case NeighbourRule.Exists:
                            angleRulesPass = angleRulesPass && MatchingNeighbour(adjacentTileData.AdjacentTiles[i]);
                            string passLog = angleRulesPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>";
                            log.Trace($"{AdjacentTileData.IndexToString[i]} - Exists? = {passLog}");
                            break;
                        case NeighbourRule.Not:
                            angleRulesPass = angleRulesPass && !MatchingNeighbour(adjacentTileData.AdjacentTiles[i]);
                            string _passLog = angleRulesPass ? "<color=green>PASS</color>" : "<color=red>FAIL</color>";
                            log.Trace($"{AdjacentTileData.IndexToString[i]} - Not? = {_passLog}");
                            break;
                        default:
                            break;
                    }
                    if (!angleRulesPass) break;
                }
                if (angleRulesPass)
                {
                    Angle = angle;
                    return true;
                }
            }
            Angle = 0;
            return false;
        }

        private static readonly int[,] RotatedOrMirroredIndexes =
        {
            {5, 3, 0, 6, 1, 7, 4, 2}, // 90
            {7, 6, 5, 4, 3, 2, 1, 0}, // 180, XY
            {2, 4, 7, 1, 6, 0, 3, 5}, // 270
            {2, 1, 0, 4, 3, 7, 6, 5}, // X
            {5, 6, 7, 3, 4, 0, 1, 2}, // Y
        };

        private int GetRotatedIndex(int original, int rotation)
        {
            switch (rotation)
            {
                case 0:
                    return original;
                case 90:
                    return RotatedOrMirroredIndexes[0, original];
                case 180:
                    return RotatedOrMirroredIndexes[1, original];
                case 270:
                    return RotatedOrMirroredIndexes[2, original];
                default:
                    break;
            }
            return original;
        }

        private int GetRotatedMirroredIndex(int original, int rotation)
        {
            return RotatedOrMirroredIndexes[3, GetRotatedIndex(original, rotation)];

        }
    }

    public class TextureRandomSelectionRule : ITextureRule
    {
        private readonly string TextureID;
        private readonly int Selection;

        public TextureRandomSelectionRule(string TextureID, int Selection)
        {
            this.TextureID = TextureID;
            this.Selection = Selection;
        }

        public bool Match(ITileData Data, AdjacentTileData adjacencyData, TextureAdjacencyData neighbourData, out EntityTextureSettings TextureSettings)
        {
            TextureSettings = new EntityTextureSettings()
            {
                TextureID = this.TextureID + Selection.ToString(),
                Angle = 0,
                ReadFromNeighbours = null
            };
            return true;
        }
    }
}
