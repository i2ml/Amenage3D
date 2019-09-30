using ErgoShop.Utils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ErgoShop.Utils
{
    /// <summary>
    /// Functions to manipulate vectors
    /// </summary>
    public static class VectorFunctions
    {
        /// <summary>
        /// Invert y and z
        /// </summary>
        /// <param name="v2D"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Vector3 Switch2D3D(Vector3 v2D, float y = 0)
        {
            return new Vector3(v2D.x, y, v2D.y);
        }

        /// <summary>
        /// Invert z and y
        /// </summary>
        /// <param name="v3D"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Vector3 Switch3D2D(Vector3 v3D, float z = 0.01f)
        {
            return new Vector3(v3D.x, v3D.z, z);
        }

        public static Vector3 InvertXZ(Vector3 v)
        {
            return new Vector3(v.z, v.y, v.x);
        }

        /// <summary>
        /// Clamp angle between 0 and 360
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v3"></param>
        /// <param name="snapAngle"></param>
        /// <returns></returns>
        public static Vector3 SnapToAngle(Vector3 v3, float snapAngle)
        {
            float angle = Vector3.Angle(v3, Vector3.up);
            if (angle < snapAngle / 2.0f)          // Cannot do cross product 
                return Vector3.up * v3.magnitude;  //   with angles 0 & 180
            if (angle > 180.0f - snapAngle / 2.0f)
                return Vector3.down * v3.magnitude;

            float t = Mathf.Round(angle / snapAngle);
            float deltaAngle = (t * snapAngle) - angle;
            Vector3 axis = Vector3.Cross(Vector3.up, v3);
            Quaternion q = Quaternion.AngleAxis(deltaAngle, axis);
            return q * v3;
        }

        public static Vector3 GetCenter(List<Vector3> vertices)
        {
            Vector3 center = Vector3.zero;

            foreach (var v in vertices)
            {
                center += v;
            }

            center = new Vector3(center.x / vertices.Count, center.y / vertices.Count, center.z / vertices.Count);

            return center;
        }

        /// <summary>
        /// Return point Vector2 at intersection of two lines
        /// </summary>
        /// <param name="dir1"></param>
        /// <param name="dir2"></param>
        /// <returns></returns>
        public static Vector2 GetIntersection(Vector2 dir1, Vector2 dir2)
        {
            float A1 = dir1.y - dir1.y;
            float B1 = dir1.x - dir1.x;
            float C1 = A1 * dir1.x + B1 * dir1.y;

            float A2 = dir2.y - dir2.y;
            float B2 = dir2.x - dir2.x;
            float C2 = A2 * dir2.x + B2 * dir2.y;

            float det = A1 * B2 - A2 * B1;

            float x = (B2 * C1 - B1 * C2) / det;
            float y = (A1 * C2 - A2 * C1) / det;

            return new Vector2(x, y);
        }

        /// <summary>
        /// Check bounds and update position of 2D Object from 3D
        /// </summary>
        /// <param name="associated2DObject"></param>
        /// <param name="associated3DObject"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector3 GetExactPositionFrom3DObject(GameObject associated2DObject, GameObject associated3DObject, Vector3 position)
        {
            Vector3 finalPosition = Vector3.zero;

            Bounds b = associated3DObject.GetMeshBounds();
            Bounds sb = associated2DObject.GetSpriteBounds();

            //Debug.Log(Switch3D2D(b.center-position) + " versus " + sb.center + " versus " + (sb.center-Switch3D2D(position)));
            Vector3 offset = Switch3D2D(b.center - position);
            float newZ = Mathf.Max(-position.y / 5f, -0.5f);
            // Rustine à changer ?
            if (associated2DObject.name.Contains("Carpet")) { newZ = 0.05f; }
            return Switch3D2D(position,newZ)+offset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="normal"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static List<Vector3> SortVerticesConvex(Vector3 normal, List<Vector3> nodes)
        {
            Vector3 first = nodes[0];

            //Sort by distance from random point to get 2 adjacent points.
            List<Vector3> temp = nodes.OrderBy(n => Vector3.Distance(n, first)).ToList();

            //Create a vector from the 2 adjacent points,
            //this will be used to sort all points, except the first, by the angle to this vector.
            //Since the shape is convex, angle will not exceed 180 degrees, resulting in a proper sort.
            Vector3 refrenceVec = (temp[1] - first);

            //Sort by angle to reference, but we are still missing the first one.
            List<Vector3> results = temp.Skip(1).OrderBy(n => Vector3.Angle(refrenceVec, n - first)).ToList();

            //insert the first one, at index 0.
            results.Insert(0, nodes[0]);

            //Now that it is sorted, we check if we got the direction right, if we didn't we reverse the list.
            //We compare the given normal and the cross product of the first 3 point.
            //If the magnitude of the sum of the normal and cross product is less than Sqrt(2) then then there is more than 90 between them.
            if ((Vector3.Cross(results[1] - results[0], results[2] - results[0]).normalized + normal.normalized).magnitude < 1.414f)
            {
                results.Reverse();
            }
            return results;
        }

        /// <summary>
        /// CONVEX TRIANGULATION
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static List<Vector3> TriangulateConvex(List<Vector3> vertices)
        {
            List<Vector3> triangles = new List<Vector3>();

            for (int i = 2; i < vertices.Count; i++)
            {
                Vector3 a = vertices[0];
                Vector3 b = vertices[i - 1];
                Vector3 c = vertices[i];
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);
            }

            return triangles;
        }


        /// <summary>
        /// Are 2 vectors parallel?
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool IsParallel(Vector2 v1, Vector2 v2)
        {
            //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
            if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Are 2 vectors orthogonal?
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool IsOrthogonal(Vector2 v1, Vector2 v2)
        {
            //2 vectors are orthogonal is the dot product is 0
            //We have to check if close to 0 because of floating numbers
            if (Mathf.Abs(Vector2.Dot(v1, v2)) < 0.000001f)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Is a point c between 2 other points a and b?
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsBetween(Vector2 a, Vector2 b, Vector2 c)
        {
            bool isBetween = false;

            //Entire line segment
            Vector2 ab = b - a;
            //The intersection and the first point
            Vector2 ac = c - a;

            //Need to check 2 things: 
            //1. If the vectors are pointing in the same direction = if the dot product is positive
            //2. If the length of the vector between the intersection and the first point is smaller than the entire line
            if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
            {
                isBetween = true;
            }

            return isBetween;
        }

        /// <summary>
        /// NOT USED YET (replaced by another one)
        /// ESSAIS TRIANGULATIONS CONCAVE
        /// </summary>
        /// <param name="pointsSorted"></param>
        /// <returns></returns>
        public static List<Vector3> Triangulate(List<Vector3> pointsSorted)
        {
            List<Vector3> points = new List<Vector3>();

            for (int i = pointsSorted.Count - 1; i >= 0; i--)
            {
                points.Add(pointsSorted[i]);
            }

            //The list with triangles the method returns
            List<Triangle> triangles = new List<Triangle>();

            //If we just have three points, then we dont have to do all calculations
            if (points.Count == 3)
            {
                triangles.Add(new Triangle(points[0], points[1], points[2]));

                List<Vector3> ress = new List<Vector3>();
                foreach (var t in triangles)
                {
                    ress.Add(t.v1.position);
                    ress.Add(t.v2.position);
                    ress.Add(t.v3.position);
                }
                return ress;
            }

            // Step 1.Store the vertices in a list and we also need to know the next and prev vertex
            List<Vertex> vertices = new List<Vertex>();

            for (int i = 0; i < points.Count; i++)
            {
                vertices.Add(new Vertex(points[i]));
            }

            //Find the next and previous vertex
            for (int i = 0; i < vertices.Count; i++)
            {
                int nextPos = ClampListIndex(i + 1, vertices.Count);

                int prevPos = ClampListIndex(i - 1, vertices.Count);

                vertices[i].prevVertex = vertices[prevPos];

                vertices[i].nextVertex = vertices[nextPos];
            }



            //Step 2. Find the reflex (concave) and convex vertices, and ear vertices
            for (int i = 0; i < vertices.Count; i++)
            {
                CheckIfReflexOrConvex(vertices[i]);
            }

            //Have to find the ears after we have found if the vertex is reflex or convex
            List<Vertex> earVertices = new List<Vertex>();

            for (int i = 0; i < vertices.Count; i++)
            {
                IsVertexEar(vertices[i], vertices, earVertices);
            }

            //Step 3. Triangulate!
            while (true)
            {
                //This means we have just one triangle left
                if (vertices.Count == 3)
                {
                    //The final triangle
                    triangles.Add(new Triangle(vertices[0], vertices[0].prevVertex, vertices[0].nextVertex));

                    break;
                }

                //Make a triangle of the first ear
                Vertex earVertex = earVertices[0];

                Vertex earVertexPrev = earVertex.prevVertex;
                Vertex earVertexNext = earVertex.nextVertex;

                Triangle newTriangle = new Triangle(earVertex, earVertexPrev, earVertexNext);

                triangles.Add(newTriangle);

                //Remove the vertex from the lists
                earVertices.Remove(earVertex);

                vertices.Remove(earVertex);

                //Update the previous vertex and next vertex
                earVertexPrev.nextVertex = earVertexNext;
                earVertexNext.prevVertex = earVertexPrev;

                //...see if we have found a new ear by investigating the two vertices that was part of the ear
                CheckIfReflexOrConvex(earVertexPrev);
                CheckIfReflexOrConvex(earVertexNext);

                earVertices.Remove(earVertexPrev);
                earVertices.Remove(earVertexNext);

                IsVertexEar(earVertexPrev, vertices, earVertices);
                IsVertexEar(earVertexNext, vertices, earVertices);
            }

            //Debug.Log(triangles.Count);

            List<Vector3> res = new List<Vector3>();
            foreach (var t in triangles)
            {
                res.Add(t.v1.position);
                res.Add(t.v2.position);
                res.Add(t.v3.position);
            }
            return res;
        }

        /// <summary>
        /// Check if a vertex if reflex or convex, and add to appropriate list
        /// </summary>
        /// <param name="v"></param>
        private static void CheckIfReflexOrConvex(Vertex v)
        {
            v.isReflex = false;
            v.isConvex = false;

            //This is a reflex vertex if its triangle is oriented clockwise
            Vector2 a = v.prevVertex.GetPos2D_XZ();
            Vector2 b = v.GetPos2D_XZ();
            Vector2 c = v.nextVertex.GetPos2D_XZ();

            if (IsTriangleOrientedClockwise(a, b, c))
            {
                v.isReflex = true;
            }
            else
            {
                v.isConvex = true;
            }
        }

        /// <summary>
        /// Check if a vertex is an ear
        /// </summary>
        /// <param name="v"></param>
        /// <param name="vertices"></param>
        /// <param name="earVertices"></param>
        private static void IsVertexEar(Vertex v, List<Vertex> vertices, List<Vertex> earVertices)
        {
            //A reflex vertex cant be an ear!
            if (v.isReflex)
            {
                return;
            }

            //This triangle to check point in triangle
            Vector2 a = v.prevVertex.GetPos2D_XZ();
            Vector2 b = v.GetPos2D_XZ();
            Vector2 c = v.nextVertex.GetPos2D_XZ();

            bool hasPointInside = false;

            for (int i = 0; i < vertices.Count; i++)
            {
                //We only need to check if a reflex vertex is inside of the triangle
                if (vertices[i].isReflex)
                {
                    Vector2 p = vertices[i].GetPos2D_XZ();

                    //This means inside and not on the hull
                    if (IsPointInTriangle(a, b, c, p))
                    {
                        hasPointInside = true;

                        break;
                    }
                }
            }

            if (!hasPointInside)
            {
                earVertices.Add(v);
            }
        }

        //Is a triangle in 2d space oriented clockwise or counter-clockwise
        //https://math.stackexchange.com/questions/1324179/how-to-tell-if-3-connected-points-are-connected-clockwise-or-counter-clockwise
        //https://en.wikipedia.org/wiki/Curve_orientation
        public static bool IsTriangleOrientedClockwise(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            bool isClockWise = true;

            float determinant = p1.x * p2.y + p3.x * p1.y + p2.x * p3.y - p1.x * p3.y - p3.x * p2.y - p2.x * p1.y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }

        //From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
        //p is the testpoint, and the other points are corners in the triangle
        public static bool IsPointInTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p)
        {
            bool isWithinTriangle = false;

            //Based on Barycentric coordinates
            float denominator = ((p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y));

            float a = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / denominator;
            float b = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / denominator;
            float c = 1 - a - b;

            //The point is within the triangle or on the border if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
            //if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
            //{
            //    isWithinTriangle = true;
            //}

            //The point is within the triangle
            if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }

        //Clamp list indices
        //Will even work if index is larger/smaller than listSize, so can loop multiple times
        public static int ClampListIndex(int index, int listSize)
        {
            index = ((index % listSize) + listSize) % listSize;

            return index;
        }

    }

    public class Vertex
    {
        public Vector3 position;

        ////The outgoing halfedge (a halfedge that starts at this vertex)
        //Doesnt matter which edge we connect to it
        public HalfEdge halfEdge;

        //Which triangle is this vertex a part of?
        public Triangle triangle;

        //The previous and next vertex this vertex is attached to
        public Vertex prevVertex;
        public Vertex nextVertex;

        //Properties this vertex may have
        //Reflex is concave
        public bool isReflex;
        public bool isConvex;
        public bool isEar;

        public Vertex(Vector3 position)
        {
            this.position = position;
        }

        //Get 2d pos of this vertex
        public Vector2 GetPos2D_XZ()
        {
            Vector2 pos_2d_xz = new Vector2(position.x, position.z);

            return pos_2d_xz;
        }
    }

    public class HalfEdge
        {
            //The vertex the edge points to
            public Vertex v;

            //The face this edge is a part of
            public Triangle t;

            //The next edge
            public HalfEdge nextEdge;
            //The previous
            public HalfEdge prevEdge;
            //The edge going in the opposite direction
            public HalfEdge oppositeEdge;

            //This structure assumes we have a vertex class with a reference to a half edge going from that vertex
            //and a face (triangle) class with a reference to a half edge which is a part of this face 
            public HalfEdge(Vertex v)
            {
                this.v = v;
            }
        }

    public class Triangle
        {
            //Corners
            public Vertex v1;
            public Vertex v2;
            public Vertex v3;

            //If we are using the half edge mesh structure, we just need one half edge
            public HalfEdge halfEdge;

            public Triangle(Vertex v1, Vertex v2, Vertex v3)
            {
                this.v1 = v1;
                this.v2 = v2;
                this.v3 = v3;
            }

            public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
            {
                this.v1 = new Vertex(v1);
                this.v2 = new Vertex(v2);
                this.v3 = new Vertex(v3);
            }

            public Triangle(HalfEdge halfEdge)
            {
                this.halfEdge = halfEdge;
            }

            //Change orientation of triangle from cw -> ccw or ccw -> cw
            public void ChangeOrientation()
            {
                Vertex temp = this.v1;

                this.v1 = this.v2;

                this.v2 = temp;
            }
        }

    //And edge between two vertices
    public class Edge
        {
            public Vertex v1;
            public Vertex v2;

            //Is this edge intersecting with another edge?
            public bool isIntersecting = false;

            public Edge(Vertex v1, Vertex v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }

            public Edge(Vector3 v1, Vector3 v2)
            {
                this.v1 = new Vertex(v1);
                this.v2 = new Vertex(v2);
            }

            //Get vertex in 2d space (assuming x, z)
            public Vector2 GetVertex2D(Vertex v)
            {
                return new Vector2(v.position.x, v.position.z);
            }

            //Flip edge
            public void FlipEdge()
            {
                Vertex temp = v1;

                v1 = v2;

                v2 = temp;
            }
        }

    public class Plane
        {
            public Vector3 pos;

            public Vector3 normal;

            public Plane(Vector3 pos, Vector3 normal)
            {
                this.pos = pos;

                this.normal = normal;
            }
        }
}

