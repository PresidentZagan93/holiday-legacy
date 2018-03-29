using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EasySpriteSheetCopy{
	
	private class CopySpriteClipboard
    {
		public bool isSpriteType = false;
		public bool clipboardSet = false;
		public string copyType;

		public TextureImporter spriteImporter;
		public TextureImporterSettings spriteSettings;
		public List<SpriteMetaData> spriteData = new List<SpriteMetaData>();
        public List<TextureImporterPlatformSettings> settings = new List<TextureImporterPlatformSettings>();
    }
	
	#region Copy Method
	private static CopySpriteClipboard Clipboard
    {
        get
        {
            if (internalClipboard == null) internalClipboard = new CopySpriteClipboard();
            return internalClipboard;
        }
        set
        {
            internalClipboard = value;
        }
    }

	private static CopySpriteClipboard internalClipboard = new CopySpriteClipboard();
    private static string[] platforms = new string[] { "Windows", "Linux", "macOS", "PS4", "XBox One", "WebGL", "Android", "iOS", "tvOS", "Tizen", "SamsungTV" };

	[MenuItem("CONTEXT/TextureImporter/Copy All Sprite Sheet Settings", false, 150)]
	private static void CopySpriteTextureSettings(MenuCommand command)
    {
		//Grab current Texture Importer
		Clipboard.spriteImporter = command.context as TextureImporter;

		//Copy sprite meta data
		foreach (SpriteMetaData metaData in Clipboard.spriteImporter.spritesheet)
        {
            SpriteMetaData tempMeta = new SpriteMetaData()
            {
                name = metaData.name,
                rect = metaData.rect,
                pivot = metaData.pivot,
                alignment = metaData.alignment,
                border = metaData.border
            };
            Clipboard.spriteData.Add(tempMeta);
		}

        GetPlatformSettings();

		//Initiate our Settings grabber
		TextureImporterSettings tempSpriteSettings = new TextureImporterSettings();

        //Grab Settings
        Clipboard.spriteImporter.ReadTextureSettings(tempSpriteSettings);

        //Assign settings to public vars
        Clipboard.spriteSettings = tempSpriteSettings;

        //Let validator know we have data
        Clipboard.clipboardSet = true;
        Clipboard.copyType = "AllData";
	}

	[MenuItem("CONTEXT/TextureImporter/Copy All Except Overrides", false,151)]
	private static void CopySpriteExceptOverride(MenuCommand command)
    {
        //Grab current Texture Importer
        Clipboard.spriteImporter = command.context as TextureImporter;

		//Copy sprite meta data
		foreach (SpriteMetaData metaData in Clipboard.spriteImporter.spritesheet)
        {
            SpriteMetaData tempMeta = new SpriteMetaData()
            {
                name = metaData.name,
                rect = metaData.rect,
                pivot = metaData.pivot,
                alignment = metaData.alignment,
                border = metaData.border
            };
            Clipboard.spriteData.Add(tempMeta);
		}
		
		//Initiate our Settings grabber
		TextureImporterSettings tempSpriteSettings = new TextureImporterSettings();

        //Grab Settings
        Clipboard.spriteImporter.ReadTextureSettings(tempSpriteSettings);

        //Assign settings to public vars
        Clipboard.spriteSettings = tempSpriteSettings;

        //Let validator know we have data
        Clipboard.clipboardSet = true;
        Clipboard.copyType = "AllDataNoOverride";
	}

	[MenuItem("CONTEXT/TextureImporter/Copy Only Overrides", false,151)]
	private static void CopyOverride(MenuCommand command)
    {
        //Grab current Texture Importer
        Clipboard.spriteImporter = command.context as TextureImporter;

        GetPlatformSettings();

        //Let validator know we have data
        Clipboard.clipboardSet = true;
        Clipboard.copyType = "OnlyOverride";
	}
    
    private static void GetPlatformSettings()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            TextureImporterPlatformSettings setting = Clipboard.spriteImporter.GetPlatformTextureSettings(platforms[i]);
            if (setting != null)
            {
                Clipboard.settings.Add(setting);
            }
        }
    }

    #endregion

    #region Paste Method
    [MenuItem("CONTEXT/TextureImporter/Paste Copied Settings", false,200)]
	private static void PasteSpriteTextureSettings(MenuCommand command)
    {
		TextureImporter currentTexture = command.context as TextureImporter;
        
        if (Clipboard.copyType == "AllDataNoOverride" || Clipboard.copyType == "AllData")
        {
            currentTexture.spritesheet = Clipboard.spriteData.ToArray();
            currentTexture.SetTextureSettings(Clipboard.spriteSettings);
        }
        if (Clipboard.copyType == "OnlyOverride" || Clipboard.copyType == "AllData")
        {
            for (int i = 0; i < Clipboard.settings.Count; i++)
            {
                currentTexture.SetPlatformTextureSettings(Clipboard.settings[i]);
            }
        }


        Clipboard = null;
        AssetDatabase.ImportAsset(currentTexture.assetPath, ImportAssetOptions.ForceUpdate);
	}
	#endregion

	#region Validation
	[MenuItem("CONTEXT/TextureImporter/Copy All Sprite Sheet Settings", true)]
	[MenuItem("CONTEXT/TextureImporter/Copy All Except Overrides", true)]
	[MenuItem("CONTEXT/TextureImporter/Copy Only Overrides", true)]
	[MenuItem("CONTEXT/TextureImporter/Reset Overrides", true)]
	[MenuItem("CONTEXT/TextureImporter/Reset Sprite Settings (Excludes Overrides)", true)]
	static bool ValidateTextureType(MenuCommand command)
    {
		CopySpriteClipboard tempClipboard = new CopySpriteClipboard();
		tempClipboard.spriteImporter = command.context as TextureImporter;
        if (tempClipboard.spriteImporter.textureType == TextureImporterType.Sprite || tempClipboard.spriteImporter.textureType == TextureImporterType.Default)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

	[MenuItem("CONTEXT/TextureImporter/Paste Copied Settings", true)]
	static bool ValidateClipboard(MenuCommand command)
    {
		return Clipboard.clipboardSet;
	}
	#endregion
}