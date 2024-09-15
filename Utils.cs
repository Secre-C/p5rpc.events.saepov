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

    //Chat GPT Copypasta
    public static Quaternion GetRotationBetweenPoints(Vector3* fromPoint, Vector3* toPoint)
    {
        // Calculate the rotation from 'fromPoint' to 'toPoint'
        var direction = Vector3.Normalize(*toPoint - *fromPoint);
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
    public static Matrix4x4 glmCreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector) // Because the Microsoft one doesn't produce the correct results
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
