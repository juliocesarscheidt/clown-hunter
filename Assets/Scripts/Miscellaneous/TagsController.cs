using UnityEngine;

public class TagsController : MonoBehaviour
{
    public enum Tags {
        Player,
        Enemy,
        EnemyHead,
        Ground,
        Ceiling,
        Wall,
        Paper,
        GunAmmo,
        FirstAid,
        EnemyArea,
    }

    public static string GetTag(Tags tag) {
        return tag.ToString();
    }

    public static string Player {
        get { return GetTag(Tags.Player); }
    }

    public static string Enemy {
        get { return GetTag(Tags.Enemy); }
    }

    public static string EnemyHead {
        get { return GetTag(Tags.EnemyHead); }
    }

    public static string Ground {
        get { return GetTag(Tags.Ground); }
    }

    public static string Ceiling {
        get { return GetTag(Tags.Ceiling); }
    }

    public static string Wall {
        get { return GetTag(Tags.Wall); }
    }

    public static string Paper {
        get { return GetTag(Tags.Paper); }
    }

    public static string GunAmmo {
        get { return GetTag(Tags.GunAmmo); }
    }

    public static string FirstAid {
        get { return GetTag(Tags.FirstAid); }
    }

    public static string EnemyArea {
        get { return GetTag(Tags.EnemyArea); }
    }
}
