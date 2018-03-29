using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using DescriptionAttribute = System.ComponentModel.DescriptionAttribute;
using Color = UnityEngine.Color;

public enum MultiplierType
{
    Additive,
    Multiply,
    Set
}

public enum ItemType
{
    None,
    [Description("Player movement speed")]
    PlayerSpeed,
    [Description("Player acceleration")]
    PlayerAcceleration,
    [Description("Fire rate")]
    GunFireRate,
    [Description("Bullets to shoot")]
    GunShots,
    [Description("Gun damage")]
    GunDamage,
    [Description("Bullet size")]
    BulletSize,
    [Description("Bullet bounces")]
    BulletBounces,
    [Description("Global chance")]
    Chance,
    [Description("Pickups value")]
    PickupValue,
    [Description("Ammo value")]
    AmmoValue,
    [Description("Health value")]
    HealthValue,
    [Description("Currency value")]
    CurrencyValue,
    [Description("Visibility range")]
    VisibilityRange,
    [Description("Look range")]
    ViewRange,
    [Description("Cursor light")]
    CursorVisibility,
    [Description("Explosive range")]
    ExplosionRange,
    [Description("Health drop chance")]
    HealthDropChance,
    [Description("Ammo drop chance")]
    AmmoDropChance,
    [Description("Currency drop chance")]
    CurrencyDropChance,
    [Description("Global drop amount")]
    PickupDropAmount,
    [Description("Health to drop")]
    HealthDropAmount,
    [Description("Ammo to drop")]
    AmmoDropAmount,
    [Description("Currency to drop")]
    CurrencyDropAmount,
    [Description("Scroll effect")]
    ScrollEffect,
    [Description("Healing")]
    HealthHeal,
    [Description("Max health increase")]
    HealthIncrease,
    [Description("Recoil")]
    GunRecoil,
    [Description("Enemy count multiplier")]
    EnemyCount,
    [Description("Contact damage")]
    PlayerContactDamage,
    [Description("Gem drop chance")]
    GemDropChance,
    [Description("Gems to drop")]
    GemDropAmount,
    [Description("Gem value")]
    GemValue,
    [Description("Ammo usage")]
    GunAmmoUsage,
    [Description("Fire bullet chance")]
    BulletFlameChance,
    [Description("Crit chance")]
    CritChance,
    [Description("Crit damage multiplier")]
    CritDamage,
    [Description("Damage receieved multiplier")]
    DamageMultiplier,
    [Description("Bullet fly speed multiplier")]
    BulletSpeed,
    [Description("Lasers size multiplier")]
    LaserSize,
    [Description("Lasers damage multiplier")]
    LaserDamage,
    [Description("Lasers fire rate multiplier")]
    LaserFireRate
}

public class QualityAttribute : PropertyAttribute
{
    public QualityAttribute() { }
}

public class LevelAttribute : PropertyAttribute
{
    public LevelAttribute() { }
}

public class ConfigSettingAttribute : PropertyAttribute
{
    public ConfigSettingAttribute() { }
}

public class EnemyAttribute : PropertyAttribute
{
    public EnemyAttribute() { }
}

public class BossAttribute : PropertyAttribute
{
    public BossAttribute() { }
}

public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}

public class TilesetAttribute : PropertyAttribute
{
    public TilesetAttribute() { }
}

public class AlternateTilesetAttribute : PropertyAttribute
{
    public AlternateTilesetAttribute() { }
}

public class AnyTilesetAttribute : PropertyAttribute
{
    public AnyTilesetAttribute() { }
}

public class AmmoTypeAttribute : PropertyAttribute
{
    public AmmoTypeAttribute() { }
}

public interface IScriptReloadable
{
    void ScriptReload();
}

[Serializable]
public class IntRange
{
    public int min = 0;
    public int max = 0;

    public IntRange(int max)
    {
        this.min = 0;
        this.max = max;
    }
    public IntRange(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
    public int Random()
    {
        return UnityEngine.Random.Range(min, max);
    }
    public bool Contains(int value)
    {
        return value <= max && value >= min;
    }
}

[Serializable]
public struct Vector2Int
{
    public int x;
    public int y;

    public static Vector2Int Zero
    {
        get { return new Vector2Int(0, 0); }
    }
    public static Vector2Int Right
    {
        get { return new Vector2Int(1, 0); }
    }

    public static Vector2Int Left
    {
        get { return new Vector2Int(-1, 0); }
    }

    public static Vector2Int Up
    {
        get { return new Vector2Int(0, 1); }
    }

    public static Vector2Int Down
    {
        get { return new Vector2Int(0, -1); }
    }

    public Vector2 Normalized
    {
        get
        {
            return new Vector2(x, y).normalized;
        }
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public static float Distance(Vector2Int a, Vector2Int b)
    {
        return Vector2.Distance(a.ToVector(), b.ToVector());
    }

    public override bool Equals(object obj)
    {
        if(obj is Vector2Int)
        {
            return ((Vector2Int)obj) == this;
        }
        else
        {
            return false;
        }
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode() << 2;
    }

    public Vector2Int(Vector2 v)
    {
        this.x = v.x.RoundToInt();
        this.y = v.y.RoundToInt();
    }

    public Vector2Int(float x, float y)
    {
        this.x = x.RoundToInt();
        this.y = y.RoundToInt();
    }

    public static bool operator ==(Vector2Int a, Vector2Int b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Vector2Int a, Vector2Int b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public static Vector2Int operator +(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x + b.x, a.y + b.y);
    }

    public static Vector2Int operator -(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.x - b.x, a.y - b.y);
    }

    public static Vector2Int operator *(Vector2Int a, float b)
    {
        return new Vector2Int(a.x * b, a.y * b);
    }

    public static Vector2Int operator /(Vector2Int a, float b)
    {
        return new Vector2Int(a.x / b, a.y / b);
    }
}

public static class GizmosExtra
{
    public static void DrawCube(Vector2 position, UnityEngine.Color color, float duration = 1f)
    {
        Debug.DrawLine(position + new Vector2(-0.5f, -0.5f), position + new Vector2(-0.5f, 0.5f), color, duration);
        Debug.DrawLine(position + new Vector2(0.5f, -0.5f), position + new Vector2(0.5f, 0.5f), color, duration);

        Debug.DrawLine(position + new Vector2(-0.5f, -0.5f), position + new Vector2(0.5f, -0.5f), color, duration);
        Debug.DrawLine(position + new Vector2(-0.5f, 0.5f), position + new Vector2(0.5f, 0.5f), color, duration);
    }

    public static void DrawArrow(Vector2 pos, Vector2 direction, int length = 8)
    {
        Gizmos.DrawRay(pos, direction * length);
        Gizmos.DrawCube(pos + direction * length, Vector3.one * 2);
    }

    public static void DrawArrow(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        if (direction == Vector3.zero) return;

        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }
}

public static class HelperExt
{
    public static string ToHex(this Color color)
    {
        int r = (color.r * 255f).RoundToInt();
        int g = (color.g * 255f).RoundToInt();
        int b = (color.b * 255f).RoundToInt();

        return "#" + r.ToString("X2") + g.ToString("X2") + b.ToString("X2");
    }

    public static Color HexToColor(this string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }

    public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        if (items == null) throw new ArgumentNullException("items");
        if (predicate == null) throw new ArgumentNullException("predicate");

        int retVal = 0;
        foreach (var item in items)
        {
            if (predicate(item)) return retVal;
            retVal++;
        }
        return -1;
    }
    
    public static int IndexOf<T>(this IEnumerable<T> items, T item) { return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i)); }

    public static T DeepCopy<T>(T other)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, other);
            ms.Position = 0;
            return (T)formatter.Deserialize(ms);
        }
    }

    public static void SetLayer(this GameObject gameObject, string layer)
    {
        gameObject.layer = LayerMask.NameToLayer(layer);

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform child = gameObject.transform.GetChild(i);

            SetLayer(child.gameObject, layer);
        }
    }

    public static int GeometryLength(this string message, Font font, int size)
    {
        int totalLength = 0;
        
        CharacterInfo characterInfo = new CharacterInfo();

        char[] arr = message.ToCharArray();
        for(int i = 0; i < arr.Length;i++)
        {
            font.GetCharacterInfo(arr[i], out characterInfo, size);

            totalLength += characterInfo.advance;
        }

        return totalLength;
    }

    public static void SetLayer(this Transform transform, string layer)
    {
        transform.gameObject.layer = LayerMask.NameToLayer(layer);

        for(int i = 0; i < transform.childCount;i++)
        {
            SetLayer(transform.GetChild(i), layer);
        }
    }

    public static void FixImageRenderer(this UnityEngine.UI.Image imageRenderer)
    {
        if (!imageRenderer || !imageRenderer.sprite) return;

        if (imageRenderer.sprite.rect.width % 2 != 0 || imageRenderer.sprite.rect.height % 2 != 0)
        {
            if (imageRenderer.sprite.rect.width % 2 != 0 && imageRenderer.transform.localPosition.x % 2 == 0)
            {
                imageRenderer.transform.localPosition = new Vector3(((int)imageRenderer.transform.localPosition.x) + 0.5f, imageRenderer.transform.localPosition.y, imageRenderer.transform.localPosition.z);
            }
            if (imageRenderer.sprite.rect.height % 2 != 0 && imageRenderer.transform.localPosition.y % 2 == 0)
            {
                imageRenderer.transform.localPosition = new Vector3(imageRenderer.transform.localPosition.x, ((int)imageRenderer.transform.localPosition.y) + 0.5f, imageRenderer.transform.localPosition.z);
            }
        }
        else
        {
            imageRenderer.transform.localPosition = new Vector3((int)imageRenderer.transform.localPosition.x, (int)imageRenderer.transform.localPosition.y, imageRenderer.transform.localPosition.z);
        }
    }

    public static void FixSpriteRenderer(this SpriteRenderer spriteRenderer)
    {
        if (!spriteRenderer || !spriteRenderer.sprite) return;

        if (spriteRenderer.sprite.rect.width % 2 != 0 || spriteRenderer.sprite.rect.height % 2 != 0)
        {
            if (spriteRenderer.sprite.rect.width % 2 != 0 && spriteRenderer.transform.localPosition.x % 2 == 0)
            {
                spriteRenderer.transform.localPosition = new Vector3(((int)spriteRenderer.transform.localPosition.x) + 0.5f, spriteRenderer.transform.localPosition.y, spriteRenderer.transform.localPosition.z);
            }
            if (spriteRenderer.sprite.rect.height % 2 != 0 && spriteRenderer.transform.localPosition.y % 2 == 0)
            {
                spriteRenderer.transform.localPosition = new Vector3(spriteRenderer.transform.localPosition.x, ((int)spriteRenderer.transform.localPosition.y) + 0.5f, spriteRenderer.transform.localPosition.z);
            }
        }
        else
        {
            spriteRenderer.transform.localPosition = new Vector3((int)spriteRenderer.transform.localPosition.x, (int)spriteRenderer.transform.localPosition.y, spriteRenderer.transform.localPosition.z);
        }
    }

    public static void FixSpriteRenderers(SpriteRenderer[] spriteRenderers)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            FixSpriteRenderer(spriteRenderers[i]);
        }
    }

    public static void FixSpriteRenderers(this Transform transform)
    {
        SpriteRenderer[] spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
        FixSpriteRenderers(spriteRenderers);
    }

    public static bool Has<T>(this Enum type, T value)
    {
        try
        {
            return (((int)(object)type & (int)(object)value) == (int)(object)value);
        }
        catch
        {
            return false;
        }
    }

    public static bool Is<T>(this Enum type, T value)
    {
        try
        {
            return (int)(object)type == (int)(object)value;
        }
        catch
        {
            return false;
        }
    }


    public static T Add<T>(this Enum type, T value)
    {
        try
        {
            return (T)(object)(((int)(object)type | (int)(object)value));
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                string.Format(
                    "Could not append value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
        }
    }


    public static T Remove<T>(this System.Enum type, T value)
    {
        try
        {
            return (T)(object)(((int)(object)type & ~(int)(object)value));
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                string.Format(
                    "Could not remove value from enumerated type '{0}'.",
                    typeof(T).Name
                    ), ex);
        }
    }

    public static Vector2Int ToVector2Int(this Vector2 v)
    {
        return new Vector2Int(v);
    }

    public static Vector2Int ToVector2Int(this Vector3 v)
    {
        return new Vector2Int(v);
    }

    public static Vector2 ToWorldPos(this Vector2 v)
    {
        return v * GeneratorManager.TileDimension;
    }

    public static Vector2 ToWorldPos(this Vector3 v)
    {
        return v * GeneratorManager.TileDimension;
    }

    public static Vector2 ToBlockPos(this Vector2 v)
    {
        Vector2 pos = (v + new Vector2(-16f, -16f)) / GeneratorManager.TileDimension;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);

        return pos;
    }

    public static Vector3 ToBlockPos(this Vector3 v)
    {
        Vector3 pos = (v + new Vector3(-16f, -16f)) / GeneratorManager.TileDimension;
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);
        pos.z = v.z;

        return pos;
    }

    public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    public static string GetDescription<T>(this T enumerationValue) where T : struct
    {
        Type type = enumerationValue.GetType();
        if (!type.IsEnum)
        {
            throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
        }

        //Tries to find a DescriptionAttribute for a potential friendly name
        //for the enum
        MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                //Pull out the description value
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }
        //If we have no description attribute, just return the ToString of the enum
        return enumerationValue.ToString();
    }

    public static List<T> Randomize<T>(this List<T> list)
    {
        return list.OrderBy(item => UnityEngine.Random.Range(0, list.Count)).ToList();
    }

    public static int ToInt(this string s)
    {
        return int.Parse(s);
    }

    public static float ToFloat(this string s)
    {
        return float.Parse(s);
    }

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        return Quaternion.Euler(0, 0, degrees) * v;
    }

    public static int RoundToInt(this float i)
    {
        i = Mathf.RoundToInt(i);
        return (int)i;
    }

    public static void LookAt2D(this Transform t, Vector2 point, float offset = 0f)
    {
        Vector2 diff = point - (Vector2)t.position;
        diff.Normalize();

        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        t.rotation = Quaternion.Euler(0f, 0f, rot_z - 90 + offset);
    }

    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static int Remap(this int value, float from1, float to1, float from2, float to2)
    {
        return Mathf.RoundToInt((value - from1) / (to1 - from1) * (to2 - from2) + from2);
    }

    public static T CopyFrom<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().CopyFrom(toAdd) as T;
    }
}

public class Helper {

    public static int RandomID
    {
        get
        {
            return UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }
    }

    public static Health FindClosestHealth(Transform transform, float distance, GameTeam team, LayerMask layerMask, bool ignoreRaycast = false)
    {
        var allHealths = ObjectManager.GetAllOfType<Health>();
        float threshold = float.MaxValue;
        int index = -1;

        for (int i = 0; i < allHealths.Count; i++)
        {
            if (!allHealths[i] || allHealths[i].IsDead) continue;
            if (allHealths[i].transform == transform) continue;
            
            if (allHealths[i].team == team)
            {
                float dist = Vector2.Distance(allHealths[i].transform.position, transform.position);

                if (dist < distance)
                {
                    if (ignoreRaycast)
                    {
                        if (dist < threshold)
                        {
                            threshold = dist;
                            index = i;
                        }
                    }
                    else
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, (allHealths[i].transform.position - transform.position).normalized, Mathf.Infinity, layerMask.value);

                        if (hit.collider != null)
                        {
                            if (hit.collider.transform == allHealths[i].transform)
                            {
                                if (dist < threshold)
                                {
                                    threshold = dist;
                                    index = i;
                                }
                            }
                        }
                    }
                }
            }
        }

        if (index != -1)
        {
            return allHealths[index];
        }
        else
        {
            return null;
        }
    }

    public static List<Health> FindClosestHealths(Transform transform, float distance, GameTeam team, LayerMask layerMask, bool ignoreRaycast = false)
    {
        var allHealths = ObjectManager.GetAllOfType<Health>();
        List<Health> inRange = new List<Health>();

        for (int i = 0; i < allHealths.Count; i++)
        {
            if (!allHealths[i] || allHealths[i].IsDead) continue;
            if (allHealths[i].transform == transform) continue;

            if (allHealths[i].team == team)
            {
                float dist = Vector2.Distance(allHealths[i].transform.position, transform.position);
                if (dist < distance)
                {
                    if(ignoreRaycast)
                    {
                        inRange.Add(allHealths[i]);
                    }
                    else
                    {
                        RaycastHit2D hit = Physics2D.Raycast(transform.position, (allHealths[i].transform.position - transform.position).normalized, Mathf.Infinity, layerMask.value);

                        if (hit.collider != null)
                        {
                            if (hit.collider.transform == allHealths[i].transform)
                            {
                                inRange.Add(allHealths[i]);
                            }
                        }
                    }
                }
            }
        }

        return inRange;
    }

    public static KeyCode ToKeyCode(string key)
    {
        key = key.ToUpper();

        if(key.Length == 1 && char.IsLetterOrDigit(key[0]))
        {
            return (KeyCode)Enum.Parse(typeof(KeyCode), key, true);
        }

        return KeyCode.None;
    }

    public static Vector2 Bezier(Vector2 s, Vector2 st, Vector2 et, Vector2 e, float t)
    {
        return (((-s + 3 * (st - et) + e) * t + (3 * (s + et) - 6 * st)) * t + 3 * (st - s)) * t + s;
    }

    public static Vector3 Hermite(Vector3 t1, Vector3 v1, Vector3 v2, Vector3 t2, float s)
    {
        float s2 = s * s;
        float s3 = s2 * s;
        float p1 = ((2f * s3) - (3f * s2)) + 1f;
        float p2 = (-2f * s3) + (3f * s2);
        float p3 = (s3 - (2f * s2)) + s;
        float p4 = s3 - s2;
        Vector3 res = (v1 * p1) + (v2 * p2) + (t1 * p3) + (t2 * p4);
        return res;
    }

    public static float Width
    {
        get
        {
            float w = Screen.width;
            if(Screen.height < Screen.width / Ratio)
            {
                w = Screen.height * Ratio;
            }
            return w;
        }
    }
    public static float Height
    {
        get
        {
            float h = Screen.height;
            if(Screen.width < Screen.height * Ratio)
            {
                h = Screen.width / Ratio;
            }
            return h;
        }
    }
    public static float Ratio
    {
        get
        {
            return (float)GameManager.GameWidth / (float)GameManager.GameHeight;
        }
    }

    public static string RandomName
    {
        get
        {
            string[] names = new string[] {"Banana", "Pink", "Mango", "Lime", "Sky", "Fudge"};
            return names[UnityEngine.Random.Range(0,names.Length)];
        }
    }

    public static Color RandomColor
    {
        get
        {
            Color[] colors = new Color[]
            {
                new Color(66, 134, 244,255),
                new Color(116, 66, 244,255),
                new Color(244, 66, 220,255),
                new Color(244, 89, 66,255),
                new Color(244, 203, 66,255),
                new Color(164, 244, 66,255),
                new Color(66, 244, 137,255)
            };

            return colors[UnityEngine.Random.Range(0, colors.Length)]/255f;
        }
    }

    public static Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
    {
        List<Vector3> points;
        List<Vector3> curvedPoints;
        int pointsLength = 0;
        int curvedLength = 0;

        if (smoothness < 1.0f) smoothness = 1.0f;

        pointsLength = arrayToCurve.Length;

        curvedLength = (pointsLength * Mathf.RoundToInt(smoothness)) - 1;
        curvedPoints = new List<Vector3>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Vector3>(arrayToCurve);

            for (int j = pointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }

            curvedPoints.Add(points[0]);
        }

        return (curvedPoints.ToArray());
    }

    public static void DrawCrosshair(Vector2 position, float size, float duration = 0.02f)
    {
        size *= 0.5f;

        Debug.DrawLine(new Vector3(position.x + size, position.y + size, 0f), new Vector3(position.x - size, position.y - size, 0f), Color.red, duration);
        Debug.DrawLine(new Vector3(position.x - size, position.y + size, 0f), new Vector3(position.x + size, position.y - size, 0f), Color.red, duration);
    }

    public static void DrawSquare(Vector2 position, float size, float duration = 0.02f)
    {
        size *= 0.5f;

        Debug.DrawLine(new Vector3(position.x + size, position.y + size, 0f), new Vector3(position.x + size, position.y - size, 0f), Color.red, duration);
        Debug.DrawLine(new Vector3(position.x + size, position.y - size, 0f), new Vector3(position.x - size, position.y - size, 0f), Color.red, duration);
        Debug.DrawLine(new Vector3(position.x - size, position.y - size, 0f), new Vector3(position.x - size, position.y + size, 0f), Color.red, duration);
        Debug.DrawLine(new Vector3(position.x - size, position.y + size, 0f), new Vector3(position.x + size, position.y + size, 0f), Color.red, duration);
    }

    public static void SetColliders(Transform transform, bool state)
    {
        Collider2D[] cols = transform.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = state;
        }
    }

    public static object DeepClone(object obj)
    {
        object objResult = null;
        using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
        {
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            bf.Serialize(ms, obj);

            ms.Position = 0;
            objResult = bf.Deserialize(ms);
        }
        return objResult;
    }

    public static void ChangeColor(GameObject particleSystem, UnityEngine.Color newColor)
    {
        if(particleSystem)
        {
            ParticleSystem[] systems = particleSystem.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < systems.Length; i++)
            {
            	ParticleSystem.MainModule main = systems[i].main;
                main.startColor = newColor;
            }
        }
    }
}

// Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable 
public class TextureScale
{
    public class ThreadData
    {
        public int start;
        public int end;
        public ThreadData(int s, int e)
        {
            start = s;
            end = e;
        }
    }

    private static UnityEngine.Color[] texColors;
    private static UnityEngine.Color[] newColors;
    private static int w;
    private static float ratioX;
    private static float ratioY;
    private static int w2;
    private static int finishCount;
    private static Mutex mutex;

    public static void Point(Texture2D tex, int newWidth, int newHeight)
    {
        ThreadedScale(tex, newWidth, newHeight, false);
    }

    public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
    {
        ThreadedScale(tex, newWidth, newHeight, true);
    }

    private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
    {
        if(newHeight <= 0)
        {
            newHeight = 1;
        }
        if(newWidth <= 0)
        {
            newWidth = 1;
        }

        texColors = tex.GetPixels();
        newColors = new UnityEngine.Color[newWidth * newHeight];
        if (useBilinear)
        {
            ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
            ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
        }
        else
        {
            ratioX = ((float)tex.width) / newWidth;
            ratioY = ((float)tex.height) / newHeight;
        }
        w = tex.width;
        w2 = newWidth;
        var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
        var slice = newHeight / cores;

        finishCount = 0;
        if (mutex == null)
        {
            mutex = new Mutex(false);
        }
        if (cores > 1)
        {
            int i = 0;
            ThreadData threadData;
            for (i = 0; i < cores - 1; i++)
            {
                threadData = new ThreadData(slice * i, slice * (i + 1));
                ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                Thread thread = new Thread(ts);
                thread.Start(threadData);
            }
            threadData = new ThreadData(slice * i, newHeight);
            if (useBilinear)
            {
                BilinearScale(threadData);
            }
            else
            {
                PointScale(threadData);
            }
            while (finishCount < cores)
            {
                Thread.Sleep(1);
            }
        }
        else
        {
            ThreadData threadData = new ThreadData(0, newHeight);
            if (useBilinear)
            {
                BilinearScale(threadData);
            }
            else
            {
                PointScale(threadData);
            }
        }

        tex.Resize(newWidth, newHeight);
        tex.SetPixels(newColors);
        tex.Apply();
    }

    public static void BilinearScale(System.Object obj)
    {
        ThreadData threadData = (ThreadData)obj;
        for (var y = threadData.start; y < threadData.end; y++)
        {
            int yFloor = (int)Mathf.Floor(y * ratioY);
            var y1 = yFloor * w;
            var y2 = (yFloor + 1) * w;
            var yw = y * w2;

            for (var x = 0; x < w2; x++)
            {
                int xFloor = (int)Mathf.Floor(x * ratioX);
                var xLerp = x * ratioX - xFloor;
                newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                       ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                       y * ratioY - yFloor);
            }
        }

        mutex.WaitOne();
        finishCount++;
        mutex.ReleaseMutex();
    }

    public static void PointScale(System.Object obj)
    {
        ThreadData threadData = (ThreadData)obj;
        for (var y = threadData.start; y < threadData.end; y++)
        {
            var thisY = (int)(ratioY * y) * w;
            var yw = y * w2;
            for (var x = 0; x < w2; x++)
            {
                newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
            }
        }

        mutex.WaitOne();
        finishCount++;
        mutex.ReleaseMutex();
    }

    private static UnityEngine.Color ColorLerpUnclamped(UnityEngine.Color c1, UnityEngine.Color c2, float value)
    {
        return new UnityEngine.Color(c1.r + (c2.r - c1.r) * value,
                          c1.g + (c2.g - c1.g) * value,
                          c1.b + (c2.b - c1.b) * value,
                          c1.a + (c2.a - c1.a) * value);
    }
}
