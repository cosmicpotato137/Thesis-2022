using UnityEngine;

public static class MatrixExtensions
{
    public static Quaternion GetRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 GetPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 GetScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }

    public static Matrix4x4 Translate(this Matrix4x4 matrix, Vector3 translation)
    {
        return matrix * Matrix4x4.TRS(translation, Quaternion.identity, Vector3.one);
    }

    public static Matrix4x4 Rotate(this Matrix4x4 matrix, Vector3 eulerRot)
    {
        return matrix * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(eulerRot), Vector3.one);
    }

    public static Matrix4x4 Rotate(this Matrix4x4 matrix, Quaternion quat)
    {
        return matrix * Matrix4x4.TRS(Vector3.zero, quat, Vector3.one);
    }

    public static Matrix4x4 Scale(this Matrix4x4 matrix, Vector3 scale)
    {
        return matrix * Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
    }

    public static Matrix4x4 SetScale(this Matrix4x4 matrix, Vector3 scale)
    {
        return Matrix4x4.TRS(matrix.GetPosition(), matrix.GetRotation(), scale);
    }
}

public static class TransformExtensions
{
    public static void FromMatrix(this Transform transform, Matrix4x4 matrix)
    {
        transform.localScale = matrix.GetScale();
        transform.rotation = matrix.GetRotation();
        transform.position = matrix.GetPosition();
    }
}