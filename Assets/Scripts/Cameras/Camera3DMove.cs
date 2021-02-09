using ErgoShop.Managers;
using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.Cameras
{
    /// <summary>
    ///     Camera used in the 3D View
    /// </summary>
    public class Camera3DMove : MonoBehaviour
    {
        public float Speed = 200.0f;
        public int yMinLimit = -80;
        public int yMaxLimit = 80;
        public int zoomRate = 100;
        public float zoomDampening = 5.0f;

        public float moveSpeed;

        public float Timer;

        private bool canMove;
        private bool canRotate;

        private Quaternion currentRotation;
        private Quaternion desiredRotation;
        private readonly Vector3 maxBound = new Vector3(50, 20, 50);

        private readonly Vector3 minBound = new Vector3(-50, 0, -50);
        private Vector3 mousePos;
        private Vector3 position;

        private Vector3 previousAngle;

        private Vector3 prevMousePos;
        private Quaternion rotation;
        private bool startMoving;

        private float xDeg;
        private float yDeg;

        private void Start()
        {
            Init();
        }

        private void LateUpdate()
        {
            canRotate = !GetComponent<Camera>().orthographic;
            canMove = GlobalManager.Instance.CanCameraMove(this);
            if (canMove)
            {
                if (Input.GetMouseButtonDown(1)) BeginMoveCam();

                if (Input.GetMouseButton(1))
                {
                    if (!startMoving) BeginMoveCam();
                    prevMousePos = mousePos;
                    mousePos = Input.mousePosition;
                }

                if (Input.GetMouseButtonUp(1)) EndMoveCam();
                MoveWithArrows();
            }
            else
            {
                EndMoveCam();
            }

            if (Input.GetMouseButton(2) && canRotate && canMove)
            {
                xDeg += Input.GetAxis("Mouse X") * Speed * 0.02f;
                yDeg -= Input.GetAxis("Mouse Y") * Speed * 0.02f;

                yDeg = VectorFunctions.ClampAngle(yDeg, yMinLimit, yMaxLimit);

                desiredRotation = Quaternion.Euler(yDeg, xDeg, 0.0f);
                currentRotation = transform.rotation;

                rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
                transform.rotation = rotation;
            }

            if (canMove)
            {
                position += transform.forward * Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * zoomRate *
                            moveSpeed;
                if (mousePos - prevMousePos != Vector3.zero)
                    position += transform.rotation * (mousePos - prevMousePos) * -1 * Time.deltaTime * moveSpeed;
            }


            var x = Mathf.Clamp(position.x, minBound.x, maxBound.x);
            var y = Mathf.Clamp(position.y, minBound.y, maxBound.y);
            var z = Mathf.Clamp(position.z, minBound.z, maxBound.z);

            if (float.IsNaN(x)) x = 0;
            if (float.IsNaN(y)) y = 0;
            if (float.IsNaN(z)) z = 0;

            position = new Vector3(x, y, z);

            transform.position = new Vector3(x, y, z);
        }

        private void OnEnable()
        {
            Init();
        }

        public void Init()
        {
            position = transform.position;
            rotation = transform.rotation;
            currentRotation = transform.rotation;
            desiredRotation = transform.rotation;

            xDeg = Vector3.Angle(Vector3.right, transform.right);
            yDeg = Vector3.Angle(Vector3.up, transform.up);
        }

        public void SetTopView()
        {
            previousAngle = transform.localEulerAngles;
            GetComponent<Camera>().orthographic = true;
            transform.localEulerAngles = Vector3.right * 90f;
        }

        public void SetNormalView()
        {
            GetComponent<Camera>().orthographic = false;
            if (previousAngle != Vector3.zero) transform.localEulerAngles = previousAngle;
        }

        public void SetFaceView(GameObject wall)
        {
            previousAngle = transform.localEulerAngles;
            GetComponent<Camera>().orthographic = true;
            transform.LookAt(wall.transform);
        }

        private void MoveWithArrows()
        {
            var speed = 3f;
            if (Input.GetKey(KeyCode.LeftArrow)) position -= transform.right * (Time.deltaTime * speed);
            if (Input.GetKey(KeyCode.RightArrow)) position -= transform.right * (-1f * Time.deltaTime * speed);
            if (Input.GetKey(KeyCode.UpArrow)) position -= transform.forward * (-1f * Time.deltaTime * speed);
            if (Input.GetKey(KeyCode.DownArrow)) position -= transform.forward * (Time.deltaTime * speed);
        }

        private Vector3 GetMousePos()
        {
            return GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        }

        private void BeginMoveCam()
        {
            prevMousePos = mousePos = Input.mousePosition;
            startMoving = true;
        }

        private void EndMoveCam()
        {
            prevMousePos = mousePos = Vector3.zero;
            startMoving = false;
        }

        public void SetPosition(Vector3 p)
        {
            position = p;
        }

        public void SetRotation(Vector3 euler)
        {
            currentRotation = Quaternion.Euler(euler);
            desiredRotation = Quaternion.Euler(euler);
            rotation = Quaternion.Euler(euler);
            xDeg = euler.y;
            yDeg = euler.x;
            previousAngle = euler;
            transform.rotation = rotation;
        }

        public void SetMoveSpeed(float s)
        {
            moveSpeed = s;
        }
    }
}