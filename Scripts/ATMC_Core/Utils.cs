using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyPerfect.City;

namespace ATMC
{
    public delegate void ATMC_Event();

    public class Utils
    {
        static public float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
        {
            Vector3 perp = Vector3.Cross(fwd, targetDir);
            float dir = Vector3.Dot(perp, up);

            if (dir > 0.5f)
            {
                return 1f;
            }
            else if (dir < -0.5f)
            {
                return -1f;
            }
            else
            {
                return 0.0f;
            }
        }

        static public int FindClosestPathPositionIndexInUnitFront(Transform objTransform, Path path)
        {
            int currentClosestIndex = 0;
            Transform ClosestPathPosition = null;
            for (int i = 0; i < path.pathPositions.Count; i++)
            {
                Transform CurrentIndexPosition = path.pathPositions[i];
                if (Vector3.Dot(objTransform.forward, (CurrentIndexPosition.position - objTransform.position).normalized) > 0)
                {
                    if (ClosestPathPosition == null)
                    {
                        currentClosestIndex = i;
                        ClosestPathPosition = CurrentIndexPosition;
                    }

                    if ((CurrentIndexPosition.position - objTransform.position).sqrMagnitude < (ClosestPathPosition.position - objTransform.position).sqrMagnitude)
                    {
                        currentClosestIndex = i;
                        ClosestPathPosition = CurrentIndexPosition;
                    }
                }
            }

            return currentClosestIndex;
        }

        static public bool IsInFrontSight(Transform objTransform, Transform other)
        {

            float carDirection = Vector3.Angle(objTransform.right, (other.transform.position - objTransform.position).normalized);
            float frontOrRear = Utils.AngleDir(objTransform.right, (objTransform.position - other.transform.position), Vector3.up);

            return (carDirection < 145 && carDirection > 55 && frontOrRear > 0);
        }

        static public bool IsInRearSight(Transform objTransform, Transform other)
        {

            float carDirection = Vector3.Angle(objTransform.right, (other.transform.position - objTransform.position).normalized);
            float frontOrRear = Utils.AngleDir(objTransform.right, (objTransform.position - other.transform.position), Vector3.up);

            return (carDirection < 145 && carDirection > 55 && frontOrRear < 0);
        }

        static public bool IsInSameDirection(Transform objTransform, Vector3 otherDirection)
        {
            float direction = Vector3.Angle(objTransform.forward, otherDirection);

            return direction < 45;
        }

        static public bool IsTargetVisible(Camera c, GameObject go)
        {
            var planes = GeometryUtility.CalculateFrustumPlanes(c);
            var point = go.transform.position;
            foreach (var plane in planes)
            {
                if (plane.GetDistanceToPoint(point) < 0)
                    return false;
            }
            return true;
        }
    }
}