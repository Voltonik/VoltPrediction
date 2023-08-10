using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LagCompensationManager {
    public static List<BaseSyncedTransform> controllers = new List<BaseSyncedTransform>();

    public static void SetWorldState(long tick) {
        foreach (BaseSyncedTransform c in controllers) {
            foreach (BaseNetworkResultState r in c.ClientResults) {
                if (r.timestamp == tick) {
                    c.transform.position = r.position;
                    c.transform.rotation = r.rotation;
                    break;
                }
            }
        }
    }

    public static bool Raycast(long tick, Transform rootTransform, Vector3 origin, bool rootDirection, Vector3 direction, out RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        SetWorldState(tick);
        if (rootTransform == null)
            return Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        else if (rootDirection)
            return Physics.Raycast(rootTransform.TransformPoint(origin), rootTransform.TransformDirection(direction), out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        else
            return Physics.Raycast(rootTransform.TransformPoint(origin), direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
    }

    public static bool Linecast(long tick, Vector3 start, Vector3 end, out RaycastHit hitInfo, int layerMask = -5, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal) {
        Vector3 direction = end - start;
        return Raycast(tick, null, start, false, direction, out hitInfo, direction.magnitude, layerMask, queryTriggerInteraction);
    }

    public static void RegisterController(BaseSyncedTransform controller) {
        if (controllers == null)
            return;

        if (!controllers.Contains(controller))
            controllers.Add(controller);
    }
}
