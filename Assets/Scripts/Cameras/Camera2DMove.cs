using ErgoShop.Managers;
using UnityEngine;

namespace ErgoShop.Cameras
{
    /// <summary>
    ///     Camera used in the 2D View
    /// </summary>
    public class Camera2DMove : MonoBehaviour
    {
        /// <summary>
        ///     Target used to make the camera move
        /// </summary>
        public Transform target;

        public Vector3 targetOffset;

        public float moveSpeed;

        public float distance = 10.0f;
        public float maxDistance = 40.0f;
        public float minDistance = 0.6f;
        public int zoomRate = 40;
        public float zoomDampening = 5.0f;

        /// <summary>
        ///     Updated by GlobalManager
        /// </summary>
        private bool canMove;

        private float currentDistance;

        private float desiredDistance = 10;
        private readonly Vector3 maxBound = new Vector3(40, 40, 1);

        private readonly Vector3 minBound = new Vector3(-40, -40, -30);
        private Vector3 mouseMovement;

        private Vector3 position;

        //Quaternion currentRotation;
        //Quaternion desiredRotation;
        //Quaternion rotation;

        private Vector3 prevMousePos;

        private void Start()
        {
            Init();
        }


        private void LateUpdate()
        {
            canMove = GlobalManager.Instance.CanCameraMove(this);
            if (canMove)
            {
                if (Input.GetMouseButtonDown(1)) BeginMoveCam();

                if (Input.GetMouseButton(1))
                {
                    mouseMovement = Input.mousePosition - prevMousePos;
                    prevMousePos = Input.mousePosition;

                    if (mouseMovement.magnitude > 200)
                    {
                        mouseMovement.Normalize();
                        mouseMovement *= 20;
                    }

                    mouseMovement *= moveSpeed;

                    targetOffset += mouseMovement * Time.deltaTime;
                }

                if (Input.GetMouseButtonUp(1)) EndMoveCam();
                desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate *
                                   Mathf.Abs(desiredDistance) * moveSpeed;
            }

            desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);

            currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);
            position = target.position - (Vector3.forward * currentDistance + targetOffset);

            var x = Mathf.Clamp(position.x, minBound.x, maxBound.x);
            var y = Mathf.Clamp(position.y, minBound.y, maxBound.y);
            var z = Mathf.Clamp(position.z, minBound.z, maxBound.z);

            if (float.IsNaN(x)) x = 0;
            if (float.IsNaN(y)) y = 0;
            if (float.IsNaN(z)) z = 0;

            position = new Vector3(x, y, z);

            transform.position = position;
        }

        private void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            prevMousePos = mouseMovement = Vector3.zero;
        }

        private void BeginMoveCam()
        {
            prevMousePos = Input.mousePosition;
        }

        private void EndMoveCam()
        {
            prevMousePos = mouseMovement = Vector3.zero;
        }

        private float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360)
                angle += 360;
            if (angle > 360)
                angle -= 360;
            return Mathf.Clamp(angle, min, max);
        }

        public void ForceCamPos(Vector3 newPos)
        {
            SetPosition(newPos);
            desiredDistance = maxDistance;
        }

        public void SetPosition(Vector3 pos3D)
        {
            targetOffset = new Vector3(-pos3D.x, -pos3D.z, 0);
        }

        public void SetMoveSpeed(float s)
        {
            moveSpeed = s;
        }
    }
}