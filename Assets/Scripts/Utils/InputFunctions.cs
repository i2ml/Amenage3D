using System.Collections.Generic;
using System.Linq;
using ErgoShop.Managers;
using UnityEngine;

namespace ErgoShop.Utils
{
    /// <summary>
    ///     Keyboard and mouse input functions
    /// </summary>
    public static class InputFunctions
    {
        public static bool CTRL()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        public static bool CTRLC()
        {
            return CTRL()
                   && Input.GetKeyDown(KeyCode.C);
        }

        public static bool CTRLV()
        {
            return CTRL()
                   && Input.GetKeyDown(KeyCode.V);
        }

        /// <summary>
        ///     true if mouse points on scene and not on an ui element
        /// </summary>
        /// <returns></returns>
        public static bool IsMouseOutsideUI()
        {
            return !GlobalManager.Instance.eventSystem.IsPointerOverGameObject();
        }

        /// <summary>
        ///     true if mouse points on scene and not on an ui element and user is clicking
        /// </summary>
        /// <returns></returns>
        public static bool IsClickingOutsideUI()
        {
            return Input.GetMouseButtonDown(0) && !GlobalManager.Instance.eventSystem.IsPointerOverGameObject();
        }

        public static Vector3 GetWorldPoint2D(Camera cam2D)
        {
            var mousePos = Input.mousePosition;
            mousePos.z = cam2D.transform.position.z * -1;
            var screenPos = cam2D.ScreenToWorldPoint(mousePos);
            var hit = Physics2D.Raycast(screenPos, Vector2.zero, Mathf.Infinity, 1 << cam2D.gameObject.layer);

            if (hit) return hit.point;
            return Vector2.zero;
        }

        public static Vector3 GetWorldPoint(Camera cam)
        {
            if (cam.gameObject.layer != (int) ErgoLayers.ThreeD) return GetWorldPoint2D(cam);

            var ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << cam.gameObject.layer)) return hit.point;
            return Vector3.positiveInfinity;
        }

        public static Vector3 GetWorldPoint(Camera cam, GameObject avoid, string[] allowedTags)
        {
            avoid.SetActive(false);
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;

            hits = Physics.RaycastAll(ray, Mathf.Infinity, 1 << cam.gameObject.layer);
            foreach (var hit in hits)
                if (allowedTags.Contains(hit.collider.tag))
                {
                    avoid.SetActive(true);
                    return hit.point;
                }

            avoid.SetActive(true);
            return Vector3.positiveInfinity;
        }

        public static Vector3 GetWorldPoint(out GameObject go, Camera cam, GameObject avoid, string[] allowedTags)
        {
            avoid.SetActive(false);
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;

            hits = Physics.RaycastAll(ray, Mathf.Infinity, 1 << cam.gameObject.layer);
            var goodHits = new List<RaycastHit>();
            foreach (var hit in hits)
                if (allowedTags.Contains(hit.collider.tag))
                    goodHits.Add(hit);
            RaycastHit best;
            var bestPoint = Vector3.positiveInfinity;
            go = null;
            foreach (var h in goodHits)
            {
                var d = Vector3.Distance(cam.transform.position, h.point);
                if (d < Vector3.Distance(cam.transform.position, bestPoint))
                {
                    best = h;
                    bestPoint = h.point;
                    go = best.collider.gameObject;
                }
            }

            avoid.SetActive(true);
            return bestPoint;
        }

        public static Vector3 GetWorldPoint3D(Camera cam, bool isOnWall)
        {
            var ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits;

            hits = Physics.RaycastAll(ray, Mathf.Infinity, 1 << cam.gameObject.layer);

            foreach (var hit in hits)
                if (isOnWall)
                {
                    if (hit.collider.gameObject.name.Contains("Wall") 
                        || hit.collider.gameObject.name.Contains("Door")) return hit.point;
                    if (hit.collider.gameObject.CompareTag("Wall") 
                        || hit.collider.gameObject.CompareTag("WallOpening")) return hit.point; return hit.point;
                }
                else
                {
                    if (hit.collider.gameObject.name.Contains("Ground")) return hit.point;
                }

            return Vector3.positiveInfinity;
        }

        public static GameObject GetHoveredObject2D(Camera cam2D, string avoidName = "", bool onlyFurniture = false)
        {
            if (!IsMouseOutsideUI()) return null;
            var es = GlobalManager.Instance.eventSystem;
            var mousePos = Input.mousePosition;
            mousePos.z = cam2D.transform.position.z * -1;
            var screenPos = cam2D.ScreenToWorldPoint(mousePos);
            var hits = Physics2D.RaycastAll(screenPos, Vector2.zero, Mathf.Infinity, 1 << cam2D.gameObject.layer);

            var goodObjects = new List<GameObject>();

            if (hits.Length > 0)
            {
                // Prio element arrow
                foreach (var hit in hits)
                    if (hit.collider.gameObject.tag == "ElementArrow")
                        return hit.collider.gameObject;
                foreach (var hit in hits)
                {
                    var avoidOk = string.IsNullOrEmpty(avoidName) || !hit.collider.gameObject.name.Contains(avoidName);
                    if (onlyFurniture) avoidOk = avoidOk && hit.collider.gameObject.tag == "Furniture";
                    if (!hit.collider.gameObject.name.Contains("2D - Top")
                        && avoidOk)
                        goodObjects.Add(hit.collider.gameObject);
                }
            }

            if (goodObjects.Count == 0) return null;

            var bestGo = goodObjects[0];
            // ALWAYS RETURN OBJECT ON TOP
            foreach (var go in goodObjects)
                if (go.transform.position.z < bestGo.transform.position.z)
                    bestGo = go;

            return bestGo;
        }

        public static GameObject GetHoveredObject(Camera cam)
        {
            if (cam.gameObject.layer == (int) ErgoLayers.Top) return GetHoveredObject2D(cam);
            if (!IsMouseOutsideUI()) return null;
            var es = GlobalManager.Instance.eventSystem;
            var ray = cam.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) return hit.collider.gameObject;

            return null;
        }
    }
}