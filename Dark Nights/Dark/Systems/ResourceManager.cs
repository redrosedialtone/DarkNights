using Microsoft.Xna.Framework.Graphics;
using Nebula.Systems;
using System.Collections;
using System.Collections.Generic;

namespace Dark
{
    public class ResourceManager : IManager
    {
        #region Static
        private static ResourceManager instance;
        public static ResourceManager Get => instance;

        public bool Initialized => throw new System.NotImplementedException();

        private static readonly NLog.Logger log = NLog.LogManager.GetLogger("[RESOURCES]");
        #endregion

        private const string EntityResourcePath = "Entities/";
        private const string EntityMaterialPath = EntityResourcePath + "Materials";
        private const string EntityTexturePath = EntityResourcePath + "Textures";
        private const string EntityShaderPath = EntityResourcePath + "Shaders";

        private const string CreatureResourcePath = "Creatures/";

        private const string CharacterResourcePath = CreatureResourcePath + "Characters/";
        private const string CharacterTexturePath = CharacterResourcePath + "Textures";

        private const string UtilityResourcePath = "Utility/";
        private const string UtilityTexturePath = UtilityResourcePath + "Textures";

        public void Init()
        {
            log.Info("> Resource Manager Init..");
            instance = this;
        }

        public static Texture2D LoadEntityTexture(string textureName)
        {
            return instance.Instance_LoadEntityTexture(textureName);
        }

        private Texture2D Instance_LoadEntityTexture(string textureName)
        {
            log.Info($"Retrieving Tile Texture: {textureName}");
            string _path = $"{EntityTexturePath}/{textureName}";
            Texture2D tileTexture = null;// Resources.Load(_path) as Texture2D;
            if (tileTexture != null)
            {
                return tileTexture;
            }
            log.Warn($"Failed to locate Texture: {textureName} at path: {_path}");
            return null;
        }

        /*public static Material LoadEntityMaterial(string materialName)
        {
            return instance.Instance_LoadEntityMaterial(materialName);
        }

        private Material Instance_LoadEntityMaterial(string textureName)
        {
            log.Info($"Retrieving Material: {textureName}");
            string _path = $"{EntityMaterialPath}/{textureName}";
            Material mat = Resources.Load(_path) as Material;
            if (mat != null)
            {
                return mat;
            }
            log.Warn($"Failed to locate Material: {textureName} at path: {_path}");
            return null;
        }

        public static Shader LoadEntityShader(string shaderName)
        {
            return instance.Instance_LoadEntityShader(shaderName);
        }

        private Shader Instance_LoadEntityShader(string shaderName)
        {
            log.Info($"Retrieving Shader: {shaderName}");
            string _path = $"{EntityShaderPath}/{shaderName}";
            Shader shader = Resources.Load(_path) as Shader;
            if (shader != null)
            {
                return shader;
            }
            log.Warn($"Failed to locate Material: {shaderName} at path: {_path}");
            return null;
        }

        public static Texture2D LoadCharacterTexture(string textureName)
        {
            return instance.Instance_LoadCharacterTexture(textureName);
        }

        private Texture2D Instance_LoadCharacterTexture(string textureName)
        {
            log.Info($"Retrieving Character Texture: {textureName}");
            string _path = $"{CharacterTexturePath}/{textureName}";
            Texture2D tileTexture = Resources.Load(_path) as Texture2D;
            if (tileTexture != null)
            {
                return tileTexture;
            }
            log.Warn($"Failed to locate Texture: {textureName} at path: {_path}");
            return null;
        }

        public static Texture2D LoadUtilityTexture(string textureName)
        {
            return instance.Instance_LoadUtilityTexture(textureName);
        }

        private Texture2D Instance_LoadUtilityTexture(string textureName)
        {
            log.Info($"Retrieving Character Texture: {textureName}");
            string _path = $"{UtilityTexturePath}/{textureName}";
            Texture2D tileTexture = Resources.Load(_path) as Texture2D;
            if (tileTexture != null)
            {
                return tileTexture;
            }
            log.Warn($"Failed to locate Texture: {textureName} at path: {_path}");
            return null;
        }*/

        public void OnInitialized()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }
    }
}
