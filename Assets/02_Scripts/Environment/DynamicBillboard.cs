using UnityEngine;

namespace FindTheLover.Environment
{
    public enum BillboardMode
    {
        FaceCamera, // Xoay mặt song song với Camera (Chuẩn 2.5D đẹp nhất)
        FacePlayer  // Dõi theo vị trí của người chơi khi lại gần
    }

    /// <summary>
    /// Giúp các Prefab 2D (Cây, Đá, Bụi rậm) luôn xoay mặt về phía Camera hoặc Player,
    /// triệt tiêu hoàn toàn hiện tượng bị mỏng dẹt như tờ giấy khi người chơi di chuyển.
    /// </summary>
    [ExecuteAlways]
    public class DynamicBillboard : MonoBehaviour
    {
        [Header("Chế Độ Xoay")]
        [Tooltip("Chọn cách xoay theo Camera hoặc xoay theo Player")]
        [SerializeField] private BillboardMode mode = BillboardMode.FaceCamera;

        [Header("Tối Ưu & Phạm Vi")]
        [Tooltip("Khoảng cách tối đa để kích hoạt xoay (tiết kiệm CPU cho cây ở xa)")]
        [SerializeField] private float activeDistance = 35f;
        [Tooltip("Tốc độ xoay mượt mà (số càng lớn xoay càng nhanh)")]
        [SerializeField] private float smoothRotationSpeed = 15f;

        private Camera _mainCam;
        private Transform _playerTransform;

        private void Start()
        {
            _mainCam = Camera.main;
            FindPlayer();
        }

        private void LateUpdate()
        {
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam == null) return;

            // 1. Kiểm tra cự ly: Nếu ở quá xa tầm mắt thì không cần tính toán để tiết kiệm CPU
            float distToCam = Vector3.Distance(transform.position, _mainCam.transform.position);
            if (distToCam > activeDistance) return;

            Vector3 targetDirection = Vector3.zero;

            // 2. Tính toán vector hướng xoay
            if (mode == BillboardMode.FaceCamera)
            {
                // Lấy hướng nhìn của Camera
                targetDirection = _mainCam.transform.forward;
            }
            else if (mode == BillboardMode.FacePlayer)
            {
                if (_playerTransform == null) FindPlayer();
                if (_playerTransform != null)
                {
                    // Lấy hướng từ vật thể trỏ thẳng vào Player
                    targetDirection = (_playerTransform.position - transform.position).normalized;
                }
            }

            // 3. KHÓA TRỤC Y: Cực kỳ quan trọng để cây không bị ngửa mặt lên trời
            targetDirection.y = 0f;

            if (targetDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

                // 4. Xoay mượt mà bằng Slerp
                if (Application.isPlaying)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothRotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = targetRotation; // Cập nhật tức thì trong Scene View khi chưa ấn Play
                }
            }
        }

        private void FindPlayer()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) player = GameObject.Find("Quene_Player");
            if (player != null) _playerTransform = player.transform;
        }

        private void OnDrawGizmosSelected()
        {
            // Vẽ vòng tròn bán kính nhận diện trong Scene View để bạn dễ căn chỉnh
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, activeDistance);
        }
    }
}