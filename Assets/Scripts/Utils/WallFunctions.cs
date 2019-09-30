using cakeslice;
using Dynagon;
using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ErgoShop.Utils
{
    /// <summary>
    /// Functions used mostly in WallsCreator
    /// </summary>
    public static class WallFunctions
    {
        /// <summary>
        /// angle from w1 to w2
        /// </summary>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <returns></returns>
        public static float GetAngleBetweenWalls(Wall w1, Wall w2)
        {
            Vector3 common = GetCommonPosition(w1, w2);
            if (w1.P1 == common)
            {
                if (w2.P1 == common)
                {
                    return Vector3.SignedAngle(w1.P2 - w1.P1, w2.P2 - w2.P1, Vector3.forward);
                }
                else
                {
                    return Vector3.SignedAngle(w1.P2 - w1.P1, w2.P1 - w2.P2, Vector3.forward);
                }
            }
            else if(w1.P2 == common)
            {
                if (w2.P1 == common)
                {
                    return Vector3.SignedAngle(w1.P1 - w1.P2, w2.P2 - w2.P1, Vector3.forward);
                }
                else
                {
                    return Vector3.SignedAngle(w1.P1 - w1.P2, w2.P1 - w2.P2, Vector3.forward);
                }
            }
            else
            {
                return -999;
            }
        }

        /// <summary>
        /// Check if only one linked wall
        /// </summary>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <returns></returns>
        public static bool IsCommonUnique(Wall w1, Wall w2)
        {
            Vector3 common = GetCommonPosition(w1, w2);

            if(w1.P1 == common)
            {
                if(w2.P1 == common)
                {
                    return w1.linkedP1.Count == 1 && w2.linkedP1.Count == 1;
                }
                else
                {
                    return w1.linkedP1.Count == 1 && w2.linkedP2.Count == 1;
                }
            }
            else
            {
                if(w2.P1 == common)
                {
                    return w1.linkedP2.Count == 1 && w2.linkedP1.Count == 1;
                }
                else
                {
                    return w1.linkedP2.Count == 1 && w2.linkedP2.Count == 1;
                }
            }
        }

        /// <summary>
        /// Get all 4 points 2D to draw the wall
        /// </summary>
        /// <param name="w"></param>
        /// <returns></returns>
        public static List<Vector3> GetFourVerticesFromWall(Wall w)
        {
            if (w.vertices2D.Count < 4) return null;

            Dictionary<Vector3, float> distancesP1 = new Dictionary<Vector3, float>();
            Dictionary<Vector3, float> distancesP2 = new Dictionary<Vector3, float>();
            List<Vector3> res = new List<Vector3>();

            foreach (var v in w.vertices2D)
            {
                if (v != Vector3.positiveInfinity)
                {
                    float val;
                    if (distancesP1.TryGetValue(v, out val))
                    {
                        return null;
                    }
                    if (distancesP2.TryGetValue(v, out val))
                    {
                        return null;
                    }
                    distancesP1.Add(v, Vector3.Distance(v, w.P1));
                    distancesP2.Add(v, Vector3.Distance(v, w.P2));
                }
            }

            if (distancesP1.Count < 2 || distancesP2.Count < 2) return null;

            var d1 = distancesP1.OrderBy(k => k.Value).ToList();
            var d2 = distancesP2.OrderBy(k => k.Value).ToList();

            // Same direction = good side of trapeze
            if((d2[0].Key - d1[0].Key).normalized == w.Direction)
            {
                res.Add(d1[0].Key);
                res.Add(d2[0].Key);
            }
            if ((d2[1].Key - d1[0].Key).normalized == w.Direction)
            {
                res.Add(d1[0].Key);
                res.Add(d2[1].Key);
            }
            if ((d2[0].Key - d1[1].Key).normalized == w.Direction)
            {
                res.Add(d1[1].Key);
                res.Add(d2[0].Key);
            }
            if ((d2[1].Key - d1[1].Key).normalized == w.Direction)
            {
                res.Add(d1[1].Key);
                res.Add(d2[1].Key);
            }

            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <param name="tol"></param>
        /// <returns>The point between the closest extremities of w1 and w2</returns>
        public static Vector3 GetCommonPosition(Wall w1, Wall w2, float tol)
        {
            float bestDistance = float.MaxValue;

            float d1 = Vector3.Distance(w1.P1, w2.P1);
            float d2 = Vector3.Distance(w1.P1, w2.P2);
            float d3 = Vector3.Distance(w1.P2, w2.P1);
            float d4 = Vector3.Distance(w1.P2, w2.P2);


            foreach (float d in new float[]{ d1,d2,d3,d4})
            {
                if (d < bestDistance) bestDistance = d;
            }

            if(bestDistance < tol)
            {
                if (bestDistance == d1) return (w1.P1 + w2.P1) / 2f;
                if (bestDistance == d2) return (w1.P1 + w2.P2) / 2f;
                if (bestDistance == d3) return (w1.P2 + w2.P1) / 2f;
                if (bestDistance == d4) return (w1.P2 + w2.P2) / 2f;
            }
            return Vector3.positiveInfinity;
        }

        /// <summary>
        /// Check if the lines are interesecting in 2d space
        /// </summary>
        /// <param name="intersection"></param>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <returns></returns>
        public static bool IsIntersecting(out Vector3 intersection, Wall w1, Wall w2)
        {
            Vector2 l1_start = w1.P1;
            Vector2 l1_end = w1.P2;

            Vector2 l2_start = w2.P1;
            Vector2 l2_end = w2.P2;

            //Direction of the lines
            Vector2 l1_dir = (l1_end - l1_start).normalized;
            Vector2 l2_dir = (l2_end - l2_start).normalized;

            //If we know the direction we can get the normal vector to each line
            Vector2 l1_normal = new Vector2(-l1_dir.y, l1_dir.x);
            Vector2 l2_normal = new Vector2(-l2_dir.y, l2_dir.x);


            //Step 1: Rewrite the lines to a general form: Ax + By = k1 and Cx + Dy = k2
            //The normal vector is the A, B
            float A = l1_normal.x;
            float B = l1_normal.y;

            float C = l2_normal.x;
            float D = l2_normal.y;

            //To get k we just use one point on the line
            float k1 = (A * l1_start.x) + (B * l1_start.y);
            float k2 = (C * l2_start.x) + (D * l2_start.y);


            //Step 2: are the lines parallel? -> no solutions
            if (VectorFunctions.IsParallel(l1_normal, l2_normal))
            {
                Debug.Log("The lines are parallel so no solutions!");

                intersection = Vector3.zero;
                return false;
            }


            //Step 3: are the lines the same line? -> infinite amount of solutions
            //Pick one point on each line and test if the vector between the points is orthogonal to one of the normals
            if (VectorFunctions.IsOrthogonal(l1_start - l2_start, l1_normal))
            {
                Debug.Log("Same line so infinite amount of solutions!");

                //Return false anyway
                intersection = Vector3.zero;
                return false;
            }


            //Step 4: calculate the intersection point -> one solution
            float x_intersect = (D * k1 - B * k2) / (A * D - B * C);
            float y_intersect = (-C * k1 + A * k2) / (A * D - B * C);

            Vector2 intersectPoint = new Vector2(x_intersect, y_intersect);


            //Step 5: but we have line segments so we have to check if the intersection point is within the segment
            if (VectorFunctions.IsBetween(l1_start, l1_end, intersectPoint)
                && VectorFunctions.IsBetween(l2_start, l2_end, intersectPoint))
            {
                Debug.Log("We have an intersection point!");
                intersection = intersectPoint;
                return true;
            }
            intersection = Vector3.zero;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>true if a wall contain another one</returns>
        public static bool IsWallContainingOther(Wall w1, Wall w2, float dist=1f)
        {
            bool sameExtremities = Vector3.Distance(w1.P1, w2.P1) < dist
                && Vector3.Distance(w1.P2, w2.P2) < dist;

            sameExtremities = sameExtremities || (Vector3.Distance(w1.P1, w2.P2) < dist
                && Vector3.Distance(w1.P2, w2.P1) < dist);
            return sameExtremities;
        }

        /// <summary>
        /// Delete one wall and make the other one the common wall of the two rooms
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        public static void MergeRoomsCommonWall(Room r1, Room r2, Wall w1, Wall w2, float dist=1f)
        {
            if(Mathf.Abs(w1.Length - w2.Length) < dist)
            {
                r2.Walls.Remove(w2);
                r2.Walls.Add(w1);
                
                WallsCreator.Instance.DestroyWall(w2,false);
                ConsolidateRoom(r2, w1, dist);
            }
        }

        /// <summary>
        /// Stick walls to the new common wall
        /// </summary>
        /// <param name="r"></param>
        /// <param name="w">Common wall</param>
        public static void ConsolidateRoom(Room r, Wall w, float dist = 1f)
        {
            foreach(var w1 in r.Walls)
            {
                if (w1 != w)
                {
                    if (Vector3.Distance(w1.P1,w.P2) < dist) w1.P2 = w.P1;
                    if (Vector3.Distance(w1.P2,w.P1) < dist) w1.P1 = w.P2;
                    if (Vector3.Distance(w1.P1,w.P1) < dist) w1.P1 = w.P1;
                    if (Vector3.Distance(w1.P2,w.P2) < dist) w1.P2 = w.P2;                    
                }                
            }            
        }

        /// <summary>
        /// Get the common extremity between two linked walls w1 and w2
        /// </summary>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <returns></returns>
        public static Vector3 GetCommonPosition(Wall w1, Wall w2)
        {
            if(w1==null || w2 == null)
            {
                Debug.Log(w1 + " " + w2);
            }
            if (w1.P1.Equals(w2.P1)
                || w1.P1.Equals(w2.P2))
                return w1.P1;
            if (w1.P2.Equals(w2.P1)
                || w1.P2.Equals(w2.P2)
                )
                return w1.P2;
            return Vector3.positiveInfinity;
        }

        /// <summary>
        /// Get all extremities positions with removing doubles
        /// </summary>
        /// <param name="walls"></param>
        /// <returns></returns>
        public static List<Vector3> GetDistinctPositions(List<Wall> walls)
        {
            List<Vector3> positions = new List<Vector3>();
            foreach(var w in walls)
            {
                if (!positions.Contains(w.P1))
                {
                    positions.Add(w.P1);
                }
                if (!positions.Contains(w.P2))
                {
                    positions.Add(w.P2);
                }
            }

            return positions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walls">An entire room where all the walls are linked (circular)</param>
        /// <returns></returns>
        public static List<Vector3> GetDistinctPositionsSortedForOneRoom(List<Wall> walls)
        {
            List<Vector3> dpositions = GetDistinctPositions(walls);
            List<Vector3> positions = new List<Vector3>();

            Wall first   = walls[0];
            Wall previous= first;
            Wall current = null;

            // pick linkedp1 and go through all the room walls
            foreach(var wl in first.linkedP1)
            {
                if (walls.Contains(wl))
                {
                    current = wl;                    
                    break;
                }
            }

            positions.Add(first.P1);

            //if (current == null)
            //{
            //    foreach (var wl in first.linkedP2)
            //    {
            //        if (walls.Contains(wl))
            //        {
            //            current = wl;
            //            break;
            //        }
            //    }
            //    positions.Add(first.P2);
            //}
            //else
            //{
            //    positions.Add(first.P1);
            //}


            // init state : first is walls[0] and current is the linked contained in walls
            while (positions.Count != dpositions.Count)
            {
                foreach (var dp in dpositions)
                {
                    if (dpositions.Count == positions.Count) break;
                    // seek next
                    Vector3 commonCurrentPrevious = GetCommonPosition(previous, current);
                    Vector3 nextPoint;
                    bool isLinkedP1=false;
                    if(current.P1 == commonCurrentPrevious)
                    {
                        nextPoint = current.P2;
                        isLinkedP1 = false;
                    }
                    else if(current.P2 == commonCurrentPrevious)
                    {
                        nextPoint = current.P1;
                        isLinkedP1 = true;
                    }
                    else
                    {
                        throw new System.Exception("ROOM ERROR");
                    }
                    // Add point
                    positions.Add(nextPoint);

                    // reaffect previous and current
                    Wall newCurrent=null;
                    if (isLinkedP1)
                    {
                        foreach (var linkedP1 in current.linkedP1)
                        {
                            if (linkedP1.P1 == nextPoint && walls.Contains(linkedP1))
                            {
                                newCurrent = linkedP1;
                            }
                            else if (linkedP1.P2 == nextPoint && walls.Contains(linkedP1))
                            {
                                newCurrent = linkedP1;
                            }
                        }
                    }
                    else {                    
                        foreach(var linkedP2 in current.linkedP2)
                        {
                            if (linkedP2.P1 == nextPoint && walls.Contains(linkedP2))
                            {
                                newCurrent = linkedP2;
                            }
                            else if (linkedP2.P2 == nextPoint && walls.Contains(linkedP2))
                            {
                                newCurrent = linkedP2;
                            }
                        }
                    }
                    previous = current;
                    current = newCurrent;
                }
            }

            return positions;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="walls"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static List<Vector3> GetDistinctPositions3D(List<Wall> walls, float y=0)
        {
            List<Vector3> positions = GetDistinctPositions(walls);
            List<Vector3> positions3D = new List<Vector3>();
            foreach (var pos in positions)
            {
                positions3D.Add(VectorFunctions.Switch2D3D(pos, y));
            }
            return positions3D;
        }

        // Get updated ac bd
        private static Vector3[] GetVerticesToStickWalls(Wall cur, Wall linked)
        {
            Vector3 common = GetCommonPosition(cur, linked);

            Vector3 perp = cur.Perpendicular;

            Vector3 decal = perp * cur.Thickness / 2;

            Vector3 a = cur.P1 - decal;
            Vector3 c = cur.P1 + decal;
                                 
            Vector3 b = cur.P2 - decal;
            Vector3 d = cur.P2 + decal;

            Vector3 perp2 = linked.Perpendicular;

            Vector3 decal2 = perp2 * linked.Thickness / 2;

            Vector3 a2 = linked.P1 - decal2;
            Vector3 c2 = linked.P1 + decal2;
                                     
            Vector3 b2 = linked.P2 - decal2;
            Vector3 d2 = linked.P2 + decal2;

            Vector3 f, g;

            // p1 side
            if (cur.P1 == common)
            {
                if (linked.P1 == common)
                {
                    bool okf = Math3d.LineLineIntersection(out f, a, cur.Direction, c2, linked.Direction);
                    bool okg = Math3d.LineLineIntersection(out g, c, cur.Direction, a2, linked.Direction);
                    return new Vector3[] { f, g, b, d };
                }
                else
                {
                    bool okf = Math3d.LineLineIntersection(out f, a, cur.Direction, b2, linked.Direction);
                    bool okg = Math3d.LineLineIntersection(out g, c, cur.Direction, d2, linked.Direction);
                    return new Vector3[] { f, g, b, d };
                }

            }
            else
            {
                if (linked.P1 == common)
                {
                    bool okf = Math3d.LineLineIntersection(out f, b, cur.Direction, b2, linked.Direction);
                    bool okg = Math3d.LineLineIntersection(out g, d, cur.Direction, d2, linked.Direction);
                    return new Vector3[] { a, c, f, g };
                }
                else
                {
                    bool okf = Math3d.LineLineIntersection(out f, b, cur.Direction, d2, linked.Direction);
                    bool okg = Math3d.LineLineIntersection(out g, d, cur.Direction, b2, linked.Direction);
                    return new Vector3[] { a, c, f, g };
                }
            }
        }

        /// <summary>
        /// Draw a wall w with extremities start and end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="w"></param>
        /// <returns></returns>
        public static List<Vector3> MakeGraphicalWall(Vector3 start, Vector3 end, Wall w)
        {
            // min 10 cm wall length
            if (Vector3.Distance(start, end) < 0.1f) return new List<Vector3>();
            bool isCommonP1 = false;
            bool isCommonP2 = false;

            Vector3 center = (start + end) / 2;
            Vector3 wdir = (end - start).normalized;
            Vector3 perp = new Vector3(wdir.y, -wdir.x);

            // rectangle a b d c

            Vector3 a = start - perp * w.Thickness / 2;
            Vector3 c = start + perp * w.Thickness / 2;

            Vector3 b = end - perp * w.Thickness / 2;
            Vector3 d = end + perp * w.Thickness / 2;

            Vector3[] v1 = null;
            Vector3[] v2 = null;


            // TRAPEZES
            if (start == w.P1 && w.linkedP1.Count == 1)
            {
                Vector3 common = WallFunctions.GetCommonPosition(w, w.linkedP1[0]);
                float angle0 = WallFunctions.GetAngleBetweenWalls(w, w.linkedP1[0]);
                isCommonP1 = common == w.P1;
                if (common != Vector3.positiveInfinity && angle0 != 0 && angle0 < 179 && angle0 > -179)
                {
                    //Debug.Log("V1 ANGLE " + angle0);
                    v1 = GetVerticesToStickWalls(w, w.linkedP1[0]);
                    // retour => new Vector3[] { f, g, b, d };
                }
            }
            if (end == w.P2 && w.linkedP2.Count == 1)
            {
                Vector3 common = WallFunctions.GetCommonPosition(w, w.linkedP2[0]);
                float angle0 = WallFunctions.GetAngleBetweenWalls(w, w.linkedP2[0]);
                isCommonP2 = common == w.P2;
                if (common != Vector3.positiveInfinity && angle0 != 0 && angle0 < 179 && angle0 > -179)
                {
                    //Debug.Log("V2 ANGLE " + angle0);
                    v2 = GetVerticesToStickWalls(w, w.linkedP2[0]);
                    // retour => new Vector3[] { a, c, f, g };
                }
            }

            ////////// ADD THICKNESS IF SEVERAL LINKED
            ////////if(start == w.P1 && w.linkedP1.Count > 1)
            ////////{
            ////////    Vector3 common = WallFunctions.GetCommonPosition(w, w.linkedP1[0]);
            ////////    float angle0 = WallFunctions.GetAngleBetweenWalls(w, w.linkedP1[0]);
            ////////    isCommonP1 = common == w.P1;
            ////////    if(common != Vector3.zero && angle0 != 0 && angle0 != 180 && angle0 != -180)
            ////////    {
            ////////        var decal = w.Direction * w.linkedP1[0].Thickness / 2f;
            ////////        v1 = new Vector3[] {a+decal,c+decal,b,d};
            ////////    }
            ////////}
            ////////if (end == w.P2 && w.linkedP2.Count > 1)
            ////////{
            ////////    Vector3 common = WallFunctions.GetCommonPosition(w, w.linkedP2[0]);
            ////////    float angle0 = WallFunctions.GetAngleBetweenWalls(w, w.linkedP2[0]);
            ////////    isCommonP2 = common == w.P2;
            ////////    if (common != Vector3.zero && angle0 != 0 && angle0 != 180 && angle0 != -180)
            ////////    {
            ////////        var decal = w.Direction * w.linkedP2[0].Thickness / 2f;
            ////////        v2 = new Vector3[] { a, c , b + decal, d + decal };
            ////////    }
            ////////}

            if (v1 != null)
            {
                a = v1[0];
                c = v1[1];
                //Debug.Log("f = " + a);
            }
            if (v2 != null)
            {
                b = v2[2];
                d = v2[3];
                //Debug.Log("f = " + b);
            }

            // relative
            a -= center;
            b -= center;
            c -= center;
            d -= center;

            // rotation
            Quaternion rotation;
            Quaternion rotation3D;

            if (wdir == Vector3.left)
            {
                rotation = Quaternion.identity;
                rotation3D = Quaternion.identity;
            }
            else
            {
                rotation = Quaternion.FromToRotation(Vector3.right, wdir);
                rotation3D = Quaternion.FromToRotation(Vector3.up, wdir);
            }

            Quaternion invRot = Quaternion.Inverse(rotation);
            Quaternion invRot3D = Quaternion.Inverse(rotation3D);

            Vector3 a2 = invRot * a;
            Vector3 b2 = invRot * b;
            Vector3 c2 = invRot * c;
            Vector3 d2 = invRot * d;

            // =========== 2D PART ===========
            // 4 vertices to adjust, according to thickness and position
            List<Vector3> new2DVertices = new List<Vector3> { a2, b2, c2, d2 };

            List<Vector3> custTriangles2D = new List<Vector3> { a2, b2, d2, a2, d2, c2 };
            
            Polygon poly = new Polygon2D(new GameObject("Wall2D"), custTriangles2D, w.Color).Build();            
            w.walls2D.Add(poly);
            poly.gameObject.transform.parent = w.associated2DObject.transform;
            poly.gameObject.layer = (int)ErgoLayers.Top;
            poly.gameObject.tag = "Wall";

            var polcol = poly.gameObject.GetComponent<PolygonCollider2D>();

            Vector2[] points = new Vector2[]
            {
                a2,
                b2,
                d2,
                c2,
                a2
            };

            polcol.SetPath(0, points);

            var outline2D = poly.gameObject.AddComponent<Outline>();

            //outline2D.OutlineMode = Outline.Mode.OutlineAll;
            //outline2D.OutlineColor = Color.green;
            //outline2D.OutlineWidth = 10f;

            outline2D.enabled = false;

            // transform
            poly.gameObject.transform.position = new Vector3(center.x, center.y, -0.001f);
            // rotation
            poly.gameObject.transform.rotation = rotation;

            //w.wall2D.transform.localScale = Vector3.one * w.Length;

            // =========== 3D PART ===========
            Vector3 center3D = VectorFunctions.Switch2D3D(center);
            Vector3 a3 = invRot3D * VectorFunctions.Switch2D3D(a);
            Vector3 b3 = invRot3D * VectorFunctions.Switch2D3D(b);
            Vector3 c3 = invRot3D * VectorFunctions.Switch2D3D(c);
            Vector3 d3 = invRot3D * VectorFunctions.Switch2D3D(d);

            Vector3 a3h = invRot3D * (VectorFunctions.Switch2D3D(a) + Vector3.up * w.Height);
            Vector3 b3h = invRot3D * (VectorFunctions.Switch2D3D(b) + Vector3.up * w.Height);
            Vector3 c3h = invRot3D * (VectorFunctions.Switch2D3D(c) + Vector3.up * w.Height);
            Vector3 d3h = invRot3D * (VectorFunctions.Switch2D3D(d) + Vector3.up * w.Height);

            List<Vector3> new3DVertices = new List<Vector3> {
                // Bottom
                a3,b3,c3,d3,
                // Top
                a3h,b3h,c3h,d3h
            };

            List<Vector3> custTriangles3D = new List<Vector3> {
                // up
                a3, b3, d3, a3, d3, c3,
                // down
                a3h, b3h, d3h, a3h, d3h, c3h,
                // face
                c3h,d3h,c3,d3h,d3,c3,
                // back
                a3h,b3h,a3,b3h,b3,a3,
                // right
                b3h,d3h,d3, b3h,d3,b3,
                // left
                a3h,c3h,c3, a3h, c3, a3
            };

            Polygon poly3 = new Polygon3D(new GameObject("Wall3D"), custTriangles3D, w.Color).Build();
            w.walls3D.Add(poly3);
            poly3.gameObject.transform.parent = w.associated3DObject.transform;
            poly3.gameObject.layer = (int) ErgoLayers.ThreeD;
            poly3.gameObject.tag = "Wall";

            var outline = poly3.gameObject.AddComponent<Outline>();

            //outline.OutlineMode = Outline.Mode.OutlineAll;
            //outline.OutlineColor = Color.green;
            //outline.OutlineWidth = 5f;

            outline.enabled = false;

            // transform
            poly3.gameObject.transform.position = center3D;
            // rot
            poly3.gameObject.transform.rotation = rotation3D;
            
            return new List<Vector3> { a+center, b+center, c+center, d+center };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static GameObject SeekAssociatedObjectFromGameObject(GameObject go)
        {
            if (!go || !go.transform || !go.transform.parent || !go.transform.parent.gameObject) return null;
            if(go.transform.parent.gameObject != go)
            {
                if (go.tag == "Associated") return go;
                else return SeekAssociatedObjectFromGameObject(go.transform.parent.gameObject);
            }
            return null;
        }

        /// <summary>
        /// Split a wall into several pieces
        /// </summary>
        /// <param name="wallToCut"></param>
        /// <param name="inter"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        public static Wall[] CutWall(Wall wallToCut, Vector3 inter, float tol = 0.05f)
        {
            Debug.Log("Cutting wall... inter = " + inter + " p1 = " + wallToCut.P1 + " p2 = " + wallToCut.P2);
            // cut w1 ?
            if (Vector3.Distance(inter, wallToCut.P1) > tol
                && Vector3.Distance(inter, wallToCut.P2) > tol)
            {
                Wall newWall1 = new Wall
                {
                    P1 = wallToCut.P1,
                    P2 = inter,
                    Color = wallToCut.Color,
                    Height = wallToCut.Height,
                    IsBearingWall = wallToCut.IsBearingWall,
                    Openings = wallToCut.Openings,
                    Thickness = wallToCut.Thickness,
                    Index = wallToCut.Index * 100
                };
                Wall newWall2 = new Wall
                {
                    P1 = inter,
                    P2 = wallToCut.P2,
                    Color = wallToCut.Color,
                    Height = wallToCut.Height,
                    IsBearingWall = wallToCut.IsBearingWall,
                    Openings = wallToCut.Openings,
                    Thickness = wallToCut.Thickness,
                    Index = wallToCut.Index * 110
                };
                return new Wall[] { newWall1, newWall2 };
            }
            Debug.Log("Got null (too close from p1 or p2)");
            return null;
        }

        /// <summary>
        /// Sort openings by percentage position
        /// </summary>
        /// <param name="opens"></param>
        /// <returns></returns>
        public static List<WallOpening> SortOpenings(List<WallOpening> opens)
        {
            foreach (var o in opens)
            {
                o.SetPercentagePositionFromPosition();
            }
            return opens.OrderBy(o => o.PercentagePosition).ToList();
        }

        /// <summary>
        /// Update wall position by updating P1 and P2, and linked walls
        /// </summary>
        /// <param name="w"></param>
        /// <param name="newP1"></param>
        /// <param name="newP2"></param>
        /// <param name="isRect"></param>
        /// <param name="moveOpenings"></param>
        /// <param name="updateLinked"></param>
        public static void SetNewPointsForWall(Wall w, Vector3 newP1, Vector3 newP2, bool isRect = false, bool moveOpenings = false, bool updateLinked = true)
        {
            // Update linked walls
            if (updateLinked)
            {
                UpdateLinkedWalls(w, w.linkedP1, newP1, isRect);
                UpdateLinkedWalls(w, w.linkedP2, newP2, isRect);
            }

            w.P1 = newP1;
            w.P2 = newP2;

            foreach (var wo in w.Openings)
            {
                wo.SetPositionFromPercentagePosition();
            }
        }

        /// <summary>
        /// NOT USED YET
        /// CHECK IF A WALL IS INTERSECTING WITH ANOTHER ONE
        /// </summary>
        /// <param name="walls"></param>
        /// <param name="tol"></param>
        /// <returns></returns>
        private static List<WallIntersection> CheckWallsIntersections(List<Wall> walls, float tol = 0.05f)
        {
            List<WallIntersection> res = new List<WallIntersection>();
            for (int i = 0; i < walls.Count - 1; i++)
            {
                for (int j = i + 1; j < walls.Count; j++)
                {
                    Wall w1 = walls[i];
                    Wall w2 = walls[j];
                    if (i != j)
                    {
                        // No common pos, then check intersection
                        if (WallFunctions.GetCommonPosition(w1, w2) == Vector3.positiveInfinity)
                        {
                            Debug.Log("Begin check intersection");
                            Vector3 inter;
                            if (WallFunctions.IsIntersecting(out inter, w1, w2))
                            {
                                res.Add(new WallIntersection
                                {
                                    w1 = w1,
                                    w2 = w2,
                                    intersectionPosition = inter
                                });
                            }
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Return list of duplicated walls
        /// todo ? : need to check also walls contained in others ?
        /// </summary>
        /// <param name="walls"></param>
        /// <returns></returns>
        private static List<Wall> GetDoubleWalls(List<Wall> walls)
        {
            List<Wall> res = new List<Wall>();
            foreach (var w1 in walls)
            {
                foreach (var w2 in walls)
                {
                    if (w1 != w2)
                    {
                        if (
                            (w1.P1 == w2.P1 && w1.P2 == w2.P2)
                            ||
                            (w1.P1 == w2.P2 && w1.P2 == w2.P1)
                            )
                        {
                            if (!res.Contains(w1))
                            {
                                res.Add(w1);
                            }
                        }
                    }
                }
            }
            return res;
        }

        /// <summary>
        /// Update linked walls from the common position changing
        /// </summary>
        /// <param name="w"></param>
        /// <param name="linkedWalls"></param>
        /// <param name="newP"></param>
        /// <param name="isRect"></param>
        private static void UpdateLinkedWalls(Wall w, List<Wall> linkedWalls, Vector3 newP, bool isRect)
        {
            foreach (Wall linked in linkedWalls)
            {
                Vector3 common = WallFunctions.GetCommonPosition(w, linked);
                Vector3 move;
                Vector3 newP1, newP2;

                SortOpenings(linked.Openings);

                if (common == linked.P1)
                {
                    move = newP - linked.P1;
                    linked.P1 = newP;
                    if (isRect)
                    {
                        newP2 = linked.P2 + move;
                        UpdateLinkedWalls(linked, linked.linkedP2, newP2, false);
                        linked.P2 = newP2;
                    }
                }
                else
                {
                    move = newP - linked.P2;
                    linked.P2 = newP;
                    if (isRect)
                    {
                        newP1 = linked.P1 + move;
                        UpdateLinkedWalls(linked, linked.linkedP1, newP1, false);
                        linked.P1 = newP1;
                    }
                }
            }
            foreach(Wall linked in linkedWalls)
            {
                foreach(var wo in linked.Openings)
                {
                    wo.SetPositionFromPercentagePosition();
                }
            }
        }        

        /// <summary>
        /// Seek all rooms connected to a wall
        /// </summary>
        /// <param name="w"></param>
        /// <param name="rooms"></param>
        /// <returns></returns>
        public static List<Room> GetRoomsFromWall(Wall w, List<Room> rooms)
        {
            List<Room> result = new List<Room>();
            foreach(var r in rooms)
            {
                foreach(var wa in r.Walls)
                {
                    if (w == wa) {
                        result.Add(r);
                    }                        
                }
            }
            return result;
        }

        /// <summary>
        /// w1 becomes w1+w2 NOT USED YET
        /// </summary>
        /// <param name="w1"></param>
        /// <param name="w2"></param>
        /// <returns>W1 Updated</returns>
        public static Wall MergeWalls(Wall w1, Wall w2)
        {
            Debug.Log("MERGING");
            Vector3 common = GetCommonPosition(w1, w2);
            if(common == w1.P1)
            {
                if(common == w2.P1)
                {
                    w1.P1 = w1.P2;
                    w1.P2 = w2.P2;                                        
                }
                else
                {
                    w1.P1 = w1.P2;
                    w1.P2 = w2.P1;
                }
            }
            else
            {
                if(common == w2.P1)
                {
                    // w1p1 = w1p1
                    w1.P2 = w2.P2;
                }
                else
                {
                    //w1.p1 = w1.p1;
                    w1.P2 = w2.P1;
                }
            }

            w1.Openings.AddRange(w2.Openings);
            return w1;
        }
    }
}
