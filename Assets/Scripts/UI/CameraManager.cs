using UnityEngine;
using UINamespace;

namespace CameraNamespace {
    public enum Direction {
        Left,
        Right
    }
    public enum CameraVariant {
        Main,
        Battle,
        BattleInfo
    }
    public class CameraManager : MonoBehaviour
    {
        public Camera mainCamera;
        public Camera battleCamera;
        public Camera battleInfoCamera;
        private CameraVariant activeCamera = CameraVariant.Main;

        public GameObject cameraViewingPoint;

        public Transform[] cameraViewingPoints;
        public Transform[] battleInfoViewingPoints;

        private int currentCameraRotationIndex = 0;
        private int currentBattleInfoPositionIndex = 0;
        private bool isTransitioning = false;
        private Vector3 targetPosition;
        private Vector3 infoTargetPosition;
        private Quaternion targetRotation;
        private Quaternion infoTargetRotation;
        public CameraRotationButtons cameraRotationButtons;

        // Start is called once before the first execution of Update after the MonoBehaviour is created



        public void SetActiveCamera(CameraVariant cameraType){
            if(isTransitioning) return;
            switch(cameraType){
                case CameraVariant.Main:
                    mainCamera.enabled = true;
                    battleCamera.enabled = false;
                    battleInfoCamera.enabled = false;
                    cameraRotationButtons.Hide();
                    activeCamera = CameraVariant.Main;
                    break;
                case CameraVariant.Battle:
                    mainCamera.enabled = false;
                    battleCamera.enabled = true;
                    battleInfoCamera.enabled = false;
                    cameraRotationButtons.Show();
                    activeCamera = CameraVariant.Battle;
                    break;
                case CameraVariant.BattleInfo:
                    mainCamera.enabled = false;
                    battleCamera.enabled = false;
                    battleInfoCamera.enabled = true;
                    cameraRotationButtons.Hide();
                    activeCamera = CameraVariant.BattleInfo;
                    break;
                default:
                    break;
            }
        }

        public void RotateCamera(Direction direction){
            if(activeCamera == CameraVariant.Battle){
                int increment = direction == Direction.Left ? -1 : 1;
                currentCameraRotationIndex += increment;
                if(currentCameraRotationIndex < 0){
                    currentCameraRotationIndex = cameraViewingPoints.Length - 1;
                }
                else if(currentCameraRotationIndex >= cameraViewingPoints.Length){
                    currentCameraRotationIndex = 0;
                }
                
                targetPosition = cameraViewingPoints[currentCameraRotationIndex].position;
                targetRotation = cameraViewingPoints[currentCameraRotationIndex].rotation;
            } else if(activeCamera == CameraVariant.BattleInfo){
                int increment = direction == Direction.Left ? -1 : 1;
                currentBattleInfoPositionIndex += increment;
                if(currentBattleInfoPositionIndex < 0){
                    currentBattleInfoPositionIndex = battleInfoViewingPoints.Length - 1;
                }
                else if(currentBattleInfoPositionIndex >= battleInfoViewingPoints.Length){
                    currentBattleInfoPositionIndex = 0;
                }

                infoTargetPosition = battleInfoViewingPoints[currentBattleInfoPositionIndex].position;
                infoTargetRotation = battleInfoViewingPoints[currentBattleInfoPositionIndex].rotation;
            }
            isTransitioning = true;
        }

        private void Update(){
            if(isTransitioning){
                SmoothCameraTransition();
            }
        }

        private void SmoothCameraTransition(){
            Camera currentCamera = activeCamera == CameraVariant.Battle ? battleCamera : battleInfoCamera;
            Vector3 actualTargetPosition = activeCamera == CameraVariant.Battle ? this.targetPosition : this.infoTargetPosition;
            Quaternion actualTargetRotation = activeCamera == CameraVariant.Battle ? this.targetRotation : this.infoTargetRotation;
            float speed = 5f;
            currentCamera.transform.position = Vector3.Lerp(currentCamera.transform.position, actualTargetPosition, Time.deltaTime * speed);
            currentCamera.transform.rotation = Quaternion.Lerp(currentCamera.transform.rotation, actualTargetRotation, Time.deltaTime * speed);
            
            // Check if close enough to stop transitioning
            if(Vector3.Distance(currentCamera.transform.position, actualTargetPosition) < 0.01f &&
               Quaternion.Angle(currentCamera.transform.rotation, actualTargetRotation) < 0.1f){
                currentCamera.transform.position = actualTargetPosition;
                currentCamera.transform.rotation = actualTargetRotation;
                isTransitioning = false;
            }
        }
    }
}