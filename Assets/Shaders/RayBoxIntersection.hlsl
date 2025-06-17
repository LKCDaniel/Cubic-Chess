//
// This function calculates the length of a ray's path inside a box (AABB).
//
void RayBoxIntersection_float(
    float3 RayOrigin,       // The starting point of the ray (e.g., Camera Position)
    float3 RayDir,          // The direction of the ray (normalized)
    float3 BoxCenter,       // The center of the box (object's pivot)
    float3 BoxSize,         // The size of the box (object's scale)
    out float Out)          // The output: distance traveled inside the box
{
    // Prepare box bounds
    float3 boxMin = BoxCenter - BoxSize / 2.0;
    float3 boxMax = BoxCenter + BoxSize / 2.0;

    // Ray-AABB intersection test (Slab method)
    float3 invDir = 1.0 / RayDir;
    float3 t0s = (boxMin - RayOrigin) * invDir;
    float3 t1s = (boxMax - RayOrigin) * invDir;

    float3 tsmaller = min(t0s, t1s);
    float3 tbigger = max(t0s, t1s);

    float tmin = max(tsmaller.x, max(tsmaller.y, tsmaller.z));
    float tmax = min(tbigger.x, min(tbigger.y, tbigger.z));

    // If tmax < tmin, ray does not intersect the box. Distance is 0.
    // If tmax < 0, box is behind the ray. Distance is 0.
    if (tmax < tmin || tmax < 0)
    {
        Out = 0.0;
        return;
    }

    // If camera is inside the box, the entry distance is 0.
    float entryDistance = max(tmin, 0);

    // The length of the ray inside the box is the difference between exit and entry.
    Out = tmax - entryDistance;
}