using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GetPositionInfo
{
    
    public static class PositionExtensions
    { 
        public enum MoveDirection{
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }
        public static string GetUnitName(this Vector3 position)
        {
            Logger.Info("Click: " + position);
            Vector3 startPosition = new Vector3(position.x, 10f, position.z);
            Vector3 direction = Vector3.down;
            float distance = 20f; // Total ray length from y=10 to y=-10
            
            int unitLayer = LayerMask.NameToLayer("DeployedUnit");
            int unitLayerMask = 1 << unitLayer;
            RaycastHit unitHit;
            if(Physics.Raycast(startPosition, direction, out unitHit, distance, unitLayerMask))
            {
                if(unitHit.collider.gameObject.GetComponent<DeployedUnit>() != null)
                {
                    return unitHit.collider.gameObject.GetComponent<DeployedUnit>().GetDeployedUnitName();
                }
            }
            return null;
        }

        public static Vector3 GetNeighbourTile(this Vector3 position, MoveDirection direction){
            // Get the direction vector from our dictionary
            Dictionary<MoveDirection, Vector3> directions = new Dictionary<MoveDirection, Vector3> {
                { MoveDirection.Up, Vector3.forward },
                { MoveDirection.Down, Vector3.back },
                { MoveDirection.Left, Vector3.left },
                { MoveDirection.Right, Vector3.right },
                { MoveDirection.UpRight, (Vector3.forward + Vector3.right).normalized },
                { MoveDirection.UpLeft, (Vector3.forward + Vector3.left).normalized },
                { MoveDirection.DownRight, (Vector3.back + Vector3.right).normalized },
                { MoveDirection.DownLeft, (Vector3.back + Vector3.left).normalized }
            };

            Vector3 directionVector = directions[direction];
            
            // Calculate the expected neighbor position (tiles are assumed to be 1 unit apart)
            Vector3 expectedPosition = position + directionVector;
            // Verify the tile exists at that position
            Vector3 startPosition = new Vector3(expectedPosition.x, 10f, expectedPosition.z);
            int tileLayer = LayerMask.NameToLayer("BattleTile");
            int tileLayerMask = 1 << tileLayer;

            if (Physics.Raycast(startPosition, Vector3.down, out RaycastHit hit, 20f, tileLayerMask)) 
            {
                GameObject hitObject = hit.collider.gameObject;
                Vector3 tempPosition = hitObject.transform.position;
                return new Vector3(tempPosition.x, 1f, tempPosition.z);
            }
            return position;
        }

        public static MoveDirection? FlipDirection(MoveDirection direction){
            switch(direction){
                case MoveDirection.Up:
                    return MoveDirection.Down;
                case MoveDirection.Down:
                    return MoveDirection.Up;
                case MoveDirection.Left:
                    return MoveDirection.Right;
                case MoveDirection.Right:
                    return MoveDirection.Left;
                case MoveDirection.UpLeft:
                    return MoveDirection.DownRight;
                case MoveDirection.UpRight:
                    return MoveDirection.DownLeft;
                case MoveDirection.DownLeft:
                    return MoveDirection.UpRight;
                case MoveDirection.DownRight:
                    return MoveDirection.UpLeft;
                default:
                    return null;
            }
        }

        public static GameObject GetGameObject(this Vector3 position, string layerName){
            Vector3 startPosition = new Vector3(position.x, 10f, position.z);
            Vector3 direction = Vector3.down;
            float distance = 20f; // Total ray length from y=10 to y=-10
            
            int layer = LayerMask.NameToLayer(layerName);
            int layerMask = 1 << layer;
            RaycastHit unitHit;
            if(Physics.Raycast(startPosition, direction, out unitHit, distance, layerMask))
            {
                Debug.DrawRay(unitHit.point, Vector3.up * 10f, Color.green, 10f);
                return unitHit.collider.gameObject;
            }
            return null;

        }

        public static MoveDirection? GetActionDirection(Vector3 start, Vector3 end){
            // Calculate direction vector and normalize it
            Vector3 direction = (end - start).normalized;
            
            // Use dot product to determine the closest direction
            float bestAlignment = float.MinValue;
            MoveDirection? bestDirection = null;
            
            // Define all possible direction vectors
            Dictionary<MoveDirection, Vector3> directions = new Dictionary<MoveDirection, Vector3> {
                { MoveDirection.Up, Vector3.forward },
                { MoveDirection.Down, Vector3.back },
                { MoveDirection.Left, Vector3.left },
                { MoveDirection.Right, Vector3.right },
                { MoveDirection.UpRight, (Vector3.forward + Vector3.right).normalized },
                { MoveDirection.UpLeft, (Vector3.forward + Vector3.left).normalized },
                { MoveDirection.DownRight, (Vector3.back + Vector3.right).normalized },
                { MoveDirection.DownLeft, (Vector3.back + Vector3.left).normalized }
            };
            
            // Find the best matching direction
            foreach (var dir in directions) {
                float alignment = Vector3.Dot(direction, dir.Value);
                if (alignment > bestAlignment) {
                    bestAlignment = alignment;
                    bestDirection = dir.Key;
                }
            }
            
            // Only return a direction if the alignment is good enough
            return bestAlignment > 0.7f ? bestDirection : null;
        }

        public static float GetActionCost(Vector3 start, Vector3 end){
            MoveDirection? direction = GetActionDirection(start, end);
            Vector3 currentStep = start;
            float cost = 0;
            while(Vector3.Distance(currentStep, end) > 1.5f){
                Tile tileCurrent = GetGameObject(currentStep, "BattleTile").GetComponent<Tile>();
                currentStep = GetNeighbourTile(currentStep, direction.Value);
                Tile tileNext = GetGameObject(currentStep, "BattleTile").GetComponent<Tile>();
                float currentActionCost = 1 + (tileNext.tileData.elevation - tileCurrent.tileData.elevation);
                cost += currentActionCost > 0.5f ? currentActionCost : 0.5f;
            }
            Logger.Info("Action cost: " + cost);
            return cost;
        }

        public static GameObject GetUnitGameObject(string unitName){
            GameObject unit = GameObject.FindObjectsByType<DeployedUnit>(FindObjectsSortMode.None)
                .FirstOrDefault(du => du.deployedUnitName == unitName)?.gameObject;
            return unit != null ? unit : null;
        }
    }
}
