using UnityEngine;
using System.Collections.Generic;
using VertexVanguard.Commands;

namespace VertexVanguard.UI
{
    public class DamageArcManager : MonoBehaviour
    {
        [Header("Prefab Settings")]
        [SerializeField] private DamageArcDisplay arcPrefab;
        [SerializeField] private int poolSize = 10;
        
        [Header("Display Settings")]
        [SerializeField] private bool defaultPersistentMode = true; // Arcs stay until manually cleared
        
        private Queue<DamageArcDisplay> arcPool = new Queue<DamageArcDisplay>();
        private List<DamageArcDisplay> activeArcs = new List<DamageArcDisplay>();
        
        private static DamageArcManager instance;
        public static DamageArcManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<DamageArcManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("DamageArcManager");
                        instance = go.AddComponent<DamageArcManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Arc visual settings")]
        [SerializeField] private float arcHeight = 1f;
        [SerializeField] private float lineWidth = 0.1f;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        public void Update()
        {
            foreach (var arc in activeArcs)
            {
                arc.SetArcHeight(arcHeight);
                arc.SetLineWidth(lineWidth);
            }
        }

        private void InitializePool()
        {
            if (arcPrefab == null)
            {
                CreateDefaultArcPrefab();
            }
            
            for (int i = 0; i < poolSize; i++)
            {
                DamageArcDisplay arc = Instantiate(arcPrefab, transform);
                arc.Hide();
                arcPool.Enqueue(arc);
            }
        }
        
        private void CreateDefaultArcPrefab()
        {
            GameObject prefabObj = new GameObject("DamageArcPrefab");
            arcPrefab = prefabObj.AddComponent<DamageArcDisplay>();
            
            // Create a basic glow material if none exists
            Material glowMaterial = new Material(Shader.Find("Sprites/Default"));
            glowMaterial.color = Color.cyan;
            
            // Store as prefab reference
            prefabObj.SetActive(false);
        }
        
        public void ShowDamageArc(Transform source, Transform target,  bool attackerIsExhausted = false)
        {
            var command = new ShowDamageArcCommand(this, source, target, attackerIsExhausted);
            command.Execute();
        }
        
        public void ShowDamageArcImmediate(Transform source, Transform target, bool attackerIsExhausted = false)
        {
            DamageArcDisplay arc = GetPooledArc();
            if (arc != null)
            {
                arc.ShowDamageArc(source, target, attackerIsExhausted);
                activeArcs.Add(arc);
            }
        }
        
        public void HideAllArcs()
        {
            var command = new HideAllArcsCommand(this);
            command.Execute();
        }
        
        public void HideAllArcsImmediate()
        {
            foreach (var arc in activeArcs)
            {
                arc.Hide();
                ReturnToPool(arc);
            }
            activeArcs.Clear();
        }
        
        public void HideArc(DamageArcDisplay arc)
        {
            if (arc != null && activeArcs.Contains(arc))
            {
                arc.Hide();
                activeArcs.Remove(arc);
                ReturnToPool(arc);
            }
        }
        
        private DamageArcDisplay GetPooledArc()
        {
            if (arcPool.Count > 0)
            {
                return arcPool.Dequeue();
            }
            
            // If pool is empty, create a new one
            DamageArcDisplay newArc = Instantiate(arcPrefab, transform);
            return newArc;
        }
        
        private void ReturnToPool(DamageArcDisplay arc)
        {
            if (arc != null)
            {
                arc.Hide();
                arcPool.Enqueue(arc);
            }
        }
        
        private System.Collections.IEnumerator ReturnToPoolAfterDelay(DamageArcDisplay arc)
        {
            // Wait for the display duration + a small buffer
            yield return new WaitForSeconds(3f);
            
            if (activeArcs.Contains(arc))
            {
                activeArcs.Remove(arc);
                ReturnToPool(arc);
            }
        }
        
        // Public utility methods
        public void SetDefaultPersistentMode(bool persistent)
        {
            defaultPersistentMode = persistent;
        }
        
        public int GetActiveArcCount()
        {
            return activeArcs.Count;
        }
        
        public List<DamageArcDisplay> GetActiveArcs()
        {
            return new List<DamageArcDisplay>(activeArcs); // Return a copy
        }
        
        // Events for observer pattern
        public System.Action<Transform, Transform, bool> OnDamageArcRequested;
        public System.Action OnHideAllArcsRequested;
        
        private void OnEnable()
        {
            OnDamageArcRequested += ShowDamageArcImmediate;
            OnHideAllArcsRequested += HideAllArcsImmediate;
        }
        
        private void OnDisable()
        {
            OnDamageArcRequested -= ShowDamageArcImmediate;
            OnHideAllArcsRequested -= HideAllArcsImmediate;
        }
    }
}

namespace VertexVanguard.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
    
    public class ShowDamageArcCommand : ICommand
    {
        private UI.DamageArcManager manager;
        private Transform source;
        private Transform target;
        private bool attackerIsExhausted;
        public ShowDamageArcCommand(UI.DamageArcManager manager, Transform source, Transform target, bool attackerIsExhausted = false)
        {
            this.manager = manager;
            this.source = source;
            this.target = target;
            this.attackerIsExhausted = attackerIsExhausted;
        }
        
        public void Execute()
        {
            manager.ShowDamageArcImmediate(source, target, attackerIsExhausted);
        }
        
        public void Undo()
        {
            // For damage arcs, undo might mean hiding all current arcs
            manager.HideAllArcsImmediate();
        }
    }
    
    public class HideAllArcsCommand : ICommand
    {
        private UI.DamageArcManager manager;
        
        public HideAllArcsCommand(UI.DamageArcManager manager)
        {
            this.manager = manager;
        }
        
        public void Execute()
        {
            manager.HideAllArcsImmediate();
        }
        
        public void Undo()
        {
            // Undo for hiding arcs might not be applicable in most cases
        }
    }
}