using System.Diagnostics;
using System.Numerics;

namespace p5rpc.events.saepov;

internal unsafe class Utils
{
    internal static long BaseAddress = Process.GetCurrentProcess().MainModule.BaseAddress.ToInt64();

    /// <summary>
    /// Gets a global variable address from an LEA or MOV instruction
    /// </summary>
    /// <param name="instructionAdr">The address of the instruction</param>
    /// <param name="length">The length of the instruction</param>
    /// <returns>The address of the global variable</returns>
    public static long GetAddressFromGlobalRef(long instructionAdr, byte length)
    {
        var opd = *(int*)(instructionAdr + length - 4);
        return instructionAdr + opd + length;
    }

    /// <summary>
    /// Ebic chatgpt copypasta
    /// </summary>
    /// <param name="eulerRotation"></param>
    /// <returns></returns>
    public static Quaternion ToQuaternion(Vector3 v)
    {
        v.X = ToRadians(v.X);
        v.Y = ToRadians(v.Y);
        v.Z = ToRadians(v.Z);

        var cy = (float)Math.Cos(v.Z * 0.5);
        var sy = (float)Math.Sin(v.Z * 0.5);
        var cp = (float)Math.Cos(v.Y * 0.5);
        var sp = (float)Math.Sin(v.Y * 0.5);
        var cr = (float)Math.Cos(v.X * 0.5);
        var sr = (float)Math.Sin(v.X * 0.5);

        return new Quaternion
        {
            W = (cr * cp * cy) + (sr * sp * sy),
            X = (sr * cp * cy) - (cr * sp * sy),
            Y = (cr * sp * cy) + (sr * cp * sy),
            Z = (cr * cp * sy) - (sr * sp * cy)
        };

    }

    public static Vector3 ToEulerAngles(Quaternion q)
    {
        Vector3 angles = new();

        // roll / x
        double sinr_cosp = 2 * ((q.W * q.X) + (q.Y * q.Z));
        double cosr_cosp = 1 - (2 * ((q.X * q.X) + (q.Y * q.Y)));
        angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

        // pitch / y
        double sinp = 2 * ((q.W * q.Y) - (q.Z * q.X));
        if (Math.Abs(sinp) >= 1)
        {
            angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
        }
        else
        {
            angles.Y = (float)Math.Asin(sinp);
        }

        // yaw / z
        double siny_cosp = 2 * ((q.W * q.Z) + (q.X * q.Y));
        double cosy_cosp = 1 - (2 * ((q.Y * q.Y) + (q.Z * q.Z)));
        angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

        angles.X = ToDegrees(angles.X);
        angles.Y = ToDegrees(angles.Y);
        angles.Z = ToDegrees(angles.Z);

        return angles;
    }

    /// <summary>
    /// Convert degrees to radians.
    /// </summary>
    public static float ToRadians(float degrees)
    {
        return degrees * (float)Math.PI / 180;
    }

    /// <summary>
    /// Convert radians to degrees.
    /// </summary>
    public static float ToDegrees(float radians)
    {
        return radians * 180f / (float)Math.PI;
    }

    //Chat GPT Copypasta
    public static Quaternion GetRotationBetweenPoints(Vector3 fromPoint, Vector3 toPoint)
    {
        // Calculate the rotation from 'fromPoint' to 'toPoint'
        var direction = Vector3.Normalize(toPoint - fromPoint);
        var rotation = Quaternion.CreateFromRotationMatrix(glmCreateLookAt(Vector3.Zero, direction, Vector3.UnitY));
        return rotation;
    }

    /// <summary>
    /// Left handed glm Lookat function port, taken from this github issue https://github.com/dotnet/runtime/issues/34859 opened by sunkin351
    /// </summary>
    /// <param name="cameraPosition"></param>
    /// <param name="cameraTarget"></param>
    /// <param name="cameraUpVector"></param>
    /// <returns></returns>
    public static Matrix4x4 glmCreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
    {
        var f = Vector3.Normalize(cameraTarget - cameraPosition);
        var s = Vector3.Normalize(Vector3.Cross(f, cameraUpVector));
        var u = Vector3.Cross(s, f);

        var result = Matrix4x4.Identity;

        result.M11 = s.X;
        result.M12 = s.Y;
        result.M13 = s.Z;

        result.M21 = u.X;
        result.M22 = u.Y;
        result.M23 = u.Z;

        result.M31 = -f.X;
        result.M32 = -f.Y;
        result.M33 = -f.Z;

        result.M41 = -Vector3.Dot(s, cameraPosition);
        result.M42 = -Vector3.Dot(u, cameraPosition);
        result.M43 = Vector3.Dot(f, cameraPosition);

        return result;
    }

    /// <summary>
    /// Chat GPT Copypasta
    /// </summary>
    /// <param name="cameraPosition"></param>
    /// <param name="targetPoint"></param>
    /// <param name="distance"></param>
    internal static void MoveCameraTowardsPoint(ref Vector3 cameraPosition, Vector3 targetPoint, float distance)
    {
        // Calculate the direction from the camera position to the target point
        var direction = Vector3.Normalize(targetPoint - cameraPosition);

        // Move the camera along the direction by the specified distance
        cameraPosition += direction * distance;
    }
}
