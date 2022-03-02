using UnityEngine;

public enum Direction {
    North, East, South, West
}

public static class DirectionExtensions {
    static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 90f, 0f),
        Quaternion.Euler(0f, 180f, 0f),
        Quaternion.Euler(0f, 270f, 0f),
    };

    public static Quaternion GetRotation(this Direction direction) {
        return rotations[(int)direction];
    }
}
