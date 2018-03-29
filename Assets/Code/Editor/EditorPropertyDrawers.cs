using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Data;
using System;

[CustomPropertyDrawer(typeof(LevelAttribute))]
public class LevelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<string> levels = new List<string>();
        int selectedIndex = LevelManager.Order.Count;
        for (int i = 0; i < LevelManager.Order.Count; i++)
        {
            levels.Add(LevelManager.Order[i].levelName);
            if (LevelManager.Order[i].levelName == property.stringValue)
            {
                selectedIndex = i;
            }
        }

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, levels.ToArray());
        if (newIndex == LevelManager.Order.Count) property.stringValue = LevelManager.Order[0].levelName;
        else property.stringValue = LevelManager.Order[newIndex].levelName;

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(ConfigSettingAttribute))]
public class ConfigSettingDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.LabelField(position, label);
        if (!property.stringValue.Contains("+"))
        {
            property.stringValue = "+";
        }

        string configName = property.stringValue.Split('+')[0];
        string configVar = property.stringValue.Split('+')[1];

        List<Type> types = ConfigManager.ConfigTypes;

        List<string> options = new List<string>();
        int selectedIndex = 0;
        for (int i = 0; i < types.Count; i++)
        {
            if (types[i].Name == configName)
            {
                selectedIndex = options.Count;
            }

            options.Add(types[i].Name);
        }

        int newIndex = EditorGUI.Popup(new Rect(position.x, position.y + 15, position.width, 15), "    Config", selectedIndex, options.ToArray());
        configName = options[newIndex];

        Type configType = ConfigManager.GetConfig(configName);
        if (configType != null)
        {
            System.Reflection.FieldInfo[] fields = configType.GetFields();
            options.Clear();
            selectedIndex = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                if(fields[i].Name != "name")
                {
                    if (fields[i].Name == configVar)
                    {
                        selectedIndex = options.Count;
                    }

                    options.Add(fields[i].Name);
                }
            }

            newIndex = EditorGUI.Popup(new Rect(position.x, position.y + 30, position.width, 15), "    Variable", selectedIndex, options.ToArray());
            configVar = options[newIndex];
        }

        property.stringValue = configType + "+" + configVar;
        property.serializedObject.ApplyModifiedProperties();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + 30;
    }
}

[CustomPropertyDrawer(typeof(EnemyAttribute))]
public class EnemyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnemyManager em = GameObject.FindObjectOfType<EnemyManager>();
        List<string> enemyNames = new List<string>();
        int selectedIndex = em.enemies.Count;
        for (int i = 0; i < em.enemies.Count; i++)
        {
            enemyNames.Add(em.enemies[i].enemyName);
            if (em.enemies[i].enemyName == property.stringValue)
            {
                selectedIndex = i;
            }
        }

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, enemyNames.ToArray());
        if (newIndex == em.enemies.Count) property.stringValue = em.enemies[0].enemyName;
        else property.stringValue = em.enemies[newIndex].enemyName;

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(AmmoTypeAttribute))]
public class AmmoTypeAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ItemManager im = GameObject.FindObjectOfType<ItemManager>();
        List<string> ammoTypeNames = new List<string>();
        int selectedIndex = 0;
        for (int i = 0; i < im.ammo.Count; i++)
        {
            ammoTypeNames.Add(im.ammo[i].name);
            if (im.ammo[i].name == property.stringValue)
            {
                selectedIndex = i;
            }
        }

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, ammoTypeNames.ToArray());
        property.stringValue = im.ammo[newIndex].name;

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumNames);

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(Armor))]
public class ArmorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ItemManager im = GameObject.FindObjectOfType<ItemManager>();
        List<string> armorNames = new List<string>();
        int selectedIndex = im.armor.Count;
        for (int i = 0; i < im.armor.Count; i++)
        {
            armorNames.Add(im.armor[i].name);
            if (im.armor[i] == property.objectReferenceValue)
            {
                selectedIndex = i;
            }
        }
        armorNames.Add("None");

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, armorNames.ToArray());
        if (newIndex == im.armor.Count) property.objectReferenceValue = null;
        else property.objectReferenceValue = im.armor[newIndex];

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(Consumable))]
public class ConsumableDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ItemManager im = GameObject.FindObjectOfType<ItemManager>();
        List<string> itemNames = new List<string>();
        int selectedIndex = im.consumables.Count;
        for (int i = 0; i < im.consumables.Count; i++)
        {
            if (im.consumables[i])
            {
                itemNames.Add(im.consumables[i].name);
                if (im.consumables[i] == property.objectReferenceValue)
                {
                    selectedIndex = i;
                }
            }
        }
        itemNames.Add("None");

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, itemNames.ToArray());
        if (newIndex == im.consumables.Count) property.objectReferenceValue = null;
        else property.objectReferenceValue = im.consumables[newIndex];

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(Item))]
public class ItemDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ItemManager im = GameObject.FindObjectOfType<ItemManager>();
        List<string> itemNames = new List<string>();
        int selectedIndex = im.items.Count;
        for (int i = 0; i < im.items.Count; i++)
        {
            if(im.items[i])
            {
                itemNames.Add(im.items[i].name);
                if (im.items[i] == property.objectReferenceValue)
                {
                    selectedIndex = i;
                }
            }
        }
        itemNames.Add("None");

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, itemNames.ToArray());
        if (newIndex == im.items.Count) property.objectReferenceValue = null;
        else property.objectReferenceValue = im.items[newIndex];

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(Gun))]
public class GunDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ItemManager im = GameObject.FindObjectOfType<ItemManager>();
        List<string> gunNames = new List<string>();
        int selectedIndex = im.guns.Count;
        for (int i = 0; i < im.guns.Count; i++)
        {
            gunNames.Add(im.guns[i].name);
            if (im.guns[i] == property.objectReferenceValue as Gun)
            {
                selectedIndex = gunNames.Count-1;
            }
        }
        gunNames.Add("None");

        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, gunNames.ToArray());
        if (newIndex == im.guns.Count) property.objectReferenceValue = null;
        else property.objectReferenceValue = im.guns[newIndex];

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(AlternateTilesetAttribute))]
public class AlternateTilesetAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GeneratorManager gm = GameObject.FindObjectOfType<GeneratorManager>();
        List<string> tilesets = new List<string>();
        int selectedIndex = -1;
        for (int i = 0; i < gm.tilesets.Count; i++)
        {
            if (gm.tilesets[i].sprites.Length == 9)
            {
                if (gm.tilesets[i].name == property.stringValue)
                {
                    selectedIndex = tilesets.Count;
                }
                tilesets.Add(gm.tilesets[i].name);
            }
        }
        if(selectedIndex == -1)
        {
            selectedIndex = tilesets.Count;
        }
        tilesets.Add("None");
        if (selectedIndex > tilesets.Count) selectedIndex = 0;

        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, tilesets.ToArray());
        
        property.stringValue = tilesets[selectedIndex];

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(AnyTilesetAttribute))]
public class AnyTilesetAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GeneratorManager gm = GameObject.FindObjectOfType<GeneratorManager>();
        List<string> tilesets = new List<string>();
        int selectedIndex = 0;
        for (int i = 0; i < gm.tilesets.Count; i++)
        {
            if (gm.tilesets[i].name == property.stringValue)
            {
                selectedIndex = tilesets.Count;
            }
            tilesets.Add(gm.tilesets[i].name);
        }
        if (selectedIndex >= tilesets.Count) selectedIndex = 0;

        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, tilesets.ToArray());
        property.stringValue = tilesets[selectedIndex];

        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(TilesetAttribute))]
public class TilesetAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GeneratorManager gm = GameObject.FindObjectOfType<GeneratorManager>();
        List<string> tilesets = new List<string>();
        int selectedIndex = 0;
        for (int i = 0; i < gm.tilesets.Count; i++)
        {
            if (gm.tilesets[i].sprites.Length > 9)
            {
                if(gm.tilesets[i].name == property.stringValue)
                {
                    selectedIndex = tilesets.Count;
                }
                tilesets.Add(gm.tilesets[i].name);
            }
        }
        if (selectedIndex >= tilesets.Count) selectedIndex = 0;

        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, tilesets.ToArray());
        property.stringValue = tilesets[selectedIndex];
        
        property.serializedObject.ApplyModifiedProperties();
    }
}

[CustomPropertyDrawer(typeof(IntRange))]
public class IntRangeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty min = property.FindPropertyRelative("min");
        SerializedProperty max = property.FindPropertyRelative("max");
        Rect contentPosition = EditorGUI.PrefixLabel(position, label);

        float half = contentPosition.width / 2;
        GUI.skin.label.padding = new RectOffset(3, 3, 6, 6);

        //show the X and Y from the point
        EditorGUIUtility.labelWidth = 28f;
        contentPosition.width *= 0.5f;
        EditorGUI.indentLevel = 0;

        // Begin/end property & change check make each field
        // behave correctly when multi-object editing.
        EditorGUI.BeginProperty(contentPosition, label, min);
        {
            EditorGUI.BeginChangeCheck();
            int newVal = EditorGUI.IntField(contentPosition, new GUIContent("Min"), min.intValue);
            if (EditorGUI.EndChangeCheck())
                min.intValue = newVal;
        }
        EditorGUI.EndProperty();

        contentPosition.x += half;

        EditorGUI.BeginProperty(contentPosition, label, max);
        {
            EditorGUI.BeginChangeCheck();
            int newVal = EditorGUI.IntField(contentPosition, new GUIContent("Max"), max.intValue);
            if (EditorGUI.EndChangeCheck())
                max.intValue = newVal;
        }
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 16f;
    }
}

[CustomPropertyDrawer(typeof(GeneratorPreset.GeneratorRoomTypePointer))]
public class GeneratorRoomTypePointerDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        List<string> typeNames = new List<string>();
        var types = EditorHelper.RoomTypes;
        int currentIndex = 0;
        for (int i = 0; i < types.Length; i++)
        {
            //if (types[i].name != "ObjectRoomBoss")
            {
                if (property.FindPropertyRelative("typeClassName").stringValue == types[i].name)
                {
                    currentIndex = typeNames.Count;
                }
                typeNames.Add(types[i].name);
            }
        }
        if(currentIndex >= typeNames.Count)
        {
            currentIndex = typeNames.Count - 1;
        }

        currentIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames.ToArray());

        property.FindPropertyRelative("typeClassName").stringValue = typeNames[currentIndex];

        property.serializedObject.ApplyModifiedProperties();
    }
}