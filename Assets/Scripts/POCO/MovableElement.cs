using System.Collections.Generic;
using ErgoShop.Managers;
using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     A movable element is like an element, but movable (thx captain)
    ///     The main feature is the Move method, which will be used by most of elements
    /// </summary>
    public abstract class MovableElement : Element
    {
        /// <summary>
        ///     If true, prevents the object from moving
        /// </summary>
        public bool IsLocked { get; set; }

        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public Vector3 Size { get; set; }
        public Vector3 EulerAngles { get; set; }

        public Element AssociatedElement { get; set; }

        // for furnitures
        public bool IsOnWall { get; set; }

        public bool CanBePutOnFurniture { get; set; }

        /// <summary>
        ///     adjust 2d and 3d objects transform according to data
        /// </summary>
        /// <param name="pos3D"></param>
        /// <param name="go"></param>
        /// <param name="startingPos"></param>
        private void AdjustCurrentFurnitureWallPos3D(Vector3 pos3D, GameObject go, Vector3 startingPos)
        {
            if (IsOnWall)
            {
                //GetComponent<Rigidbody>().MovePosition
                associated3DObject.transform.position = new Vector3(
                    pos3D.x,
                    Mathf.Clamp(pos3D.y, 0.1f, 3),
                    pos3D.z
                );
                associated2DObject.transform.position = VectorFunctions.GetExactPositionFrom3DObject(associated2DObject,
                    associated3DObject, associated3DObject.transform.position);
                //VectorFunctions.Switch3D2D(associated3DObject.transform.position);

                var w = WallsCreator.Instance.GetWallFromGameObject(go);

                var perp = w.GetPerpendicularFromPos(pos3D);

                associated3DObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, perp);
                associated3DObject.transform.localEulerAngles = new Vector3(0,
                    associated3DObject.transform.localEulerAngles.y, EulerAngles.z);

                associated2DObject.transform.localEulerAngles = VectorFunctions.Switch2D3D(new Vector3(0,
                    -associated3DObject.transform.localEulerAngles.y,
                    associated3DObject.transform.localEulerAngles.z));
                EulerAngles = associated3DObject.transform.localEulerAngles;
                AssociatedElement = w;
            }
            else
            {
                if (pos3D.magnitude != float.PositiveInfinity)
                    associated3DObject.transform.position =
                        pos3D; //.GetComponent<Rigidbody>().MovePosition(associated3DObject.transform.position + (pos3D - startingPos));
                associated2DObject.transform.position = this is HelperElement
                    ? VectorFunctions.Switch3D2D(associated3DObject.transform.position)
                    : VectorFunctions.GetExactPositionFrom3DObject(associated2DObject, associated3DObject,
                        associated3DObject.transform.position);
            }
        }

        /// <summary>
        ///     Set element size, updates 2d and 3d objects
        /// </summary>
        /// <param name="newSize">Wanted size</param>
        public virtual void SetSize(Vector3 newSize)
        {
            Size = newSize;
            var s = Size;
            var ms = MeshSize;
            var sOnMs = new Vector3(s.x / ms.x, s.y / ms.y, s.z / ms.z);
            associated3DObject.transform.localScale = sOnMs;
            associated2DObject.transform.localScale = VectorFunctions.Switch3D2D(sOnMs);
        }

        /// <summary>
        ///     Update position from wanted one
        /// </summary>
        /// <param name="newPosition"></param>
        public virtual void SetPosition(Vector3 newPosition)
        {
            Position = newPosition;
            associated2DObject.transform.position = VectorFunctions.Switch3D2D(newPosition);
            if (associated3DObject) associated3DObject.transform.position = newPosition;
        }

        /// <summary>
        ///     Move function. The starting pos is where the users starts clicking,
        ///     so its a kind of offset so the element does not "teleport" its position into mouse but moves smoothly
        /// </summary>
        /// <param name="startingPos">where the users starts clicking</param>
        public virtual void Move(Vector3 startingPos)
        {
            Camera cam = GlobalManager.Instance.GetActiveCamera();

            List<string> tags = new List<string> { "" };

            if (IsOnWall)
            {
                tags.Add("Wall");
            }
            else
            {
                tags.Add("Ground");
            }

            if (CanBePutOnFurniture) tags.Add("Furniture");

            tags.Add("WorkPlane");

            switch (cam.gameObject.layer)
            {
                case (int)ErgoLayers.Top:
                    var pos2D = InputFunctions.GetWorldPoint(cam);
                    var closestProj = pos2D;

                    // If on wall then stick to wall
                    if (IsOnWall)
                    {
                        //Debug.Log("Sticking...");
                        Wall closestWall = null;
                        // Seek closest projection for a wall
                        foreach (var wd in WallsCreator.Instance.GetWalls())
                            if (closestWall == null)
                            {
                                closestWall = wd;
                                closestProj = Math3d.ProjectPointOnLineSegment(wd.P1, wd.P2, pos2D);
                            }
                            else
                            {
                                var proj = Math3d.ProjectPointOnLineSegment(wd.P1, wd.P2, pos2D);
                                if (Vector2.Distance(pos2D, proj)
                                    < Vector2.Distance(pos2D, closestProj))
                                {
                                    closestWall = wd;
                                    closestProj = proj;
                                }
                            }

                        AssociatedElement = closestWall;
                        Vector3 pos33D = VectorFunctions.Switch2D3D(closestProj - closestWall.Thickness * (closestProj - pos2D).normalized);

                        pos33D = pos33D + Position.y * Vector3.up;

                        // Repeat the 3D algo and readapt 2D                        
                        AdjustCurrentFurnitureWallPos3D(pos33D, closestWall.associated3DObject, startingPos);
                    }
                    else
                    {
                        associated2DObject.transform.position += pos2D - startingPos;

                        if (associated3DObject)
                        {
                            float y = 0;
                            if (CanBePutOnFurniture)
                            {
                                var potentialFurniture = InputFunctions.GetHoveredObject2D(
                                    GlobalManager.Instance.cam2DTop.GetComponent<Camera>(), associated2DObject.name,
                                    true);
                                if (potentialFurniture)
                                {
                                    var f = FurnitureCreator.Instance.GetFurnitureFromGameObject(potentialFurniture);
                                    y = f.Position.y + f.Size.y;
                                }
                            }

                            associated2DObject.transform.position = new Vector3(associated2DObject.transform.position.x,
                                associated2DObject.transform.position.y, -y / 5f);
                            associated3DObject.transform.position =
                                VectorFunctions.Switch2D3D(associated2DObject.transform.position, y);
                        }
                    }

                    if (text2D) text2D.transform.position = associated2DObject.transform.position;

                    break;
                case (int)ErgoLayers.ThreeD:
                    Vector3 pos3D = InputFunctions.GetWorldPoint(cam);
                    Vector3 closestProj3D = pos3D;
                    GameObject go;

                    if (IsOnWall)
                    {
                        Vector3 pos33D = InputFunctions.GetWorldPoint(out go, cam, associated3DObject, tags.ToArray());
                        AdjustCurrentFurnitureWallPos3D(pos33D, go, startingPos);
                    }
                    else
                    {
                        //Fix a l'etage l'objet
                        float tmpY = associated3DObject.transform.position.y;

                        associated3DObject.transform.position += pos3D - startingPos;

                        if (associated3DObject)
                        {
                            float y = 0;
                            if (CanBePutOnFurniture)
                            {
                                var potentialFurniture = InputFunctions.GetHoveredObject2D(
                                    GlobalManager.Instance.cam2DTop.GetComponent<Camera>(), associated2DObject.name,
                                    true);
                                if (potentialFurniture)
                                {
                                    var f = FurnitureCreator.Instance.GetFurnitureFromGameObject(potentialFurniture);
                                    y = f.Position.y + f.Size.y;
                                }
                            }

                           
                            associated3DObject.transform.position = associated3DObject.transform.position;
                            associated3DObject.transform.position = new Vector3(associated3DObject.transform.position.x, tmpY, associated3DObject.transform.position.z);

                            associated2DObject.transform.position = new Vector3(associated3DObject.transform.position.x, associated3DObject.transform.position.z, associated2DObject.transform.position.z);
                        }
                    }

                    break;
            }

            if (associated3DObject)
                Position = associated3DObject.transform.position;
            else
                Position = VectorFunctions.Switch2D3D(associated2DObject.transform.position);
        }
    }
}