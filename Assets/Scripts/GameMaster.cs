using System;
using UnityEngine;
using UINamespace;
using OverworldNamespace;
using PaletteNamespace;
using System.Collections.Generic;
using System.Collections;
using DiscoJockeyNamespace;
using VertexVanguard.Utility;

public class StrictQueue<T> 
{
    private List<T> internalList;

    public StrictQueue(List<T> list = null)
    {
        if (list == null)
        {
            internalList = new List<T>(); 
            return;
        }
        internalList = list;
    }
    public bool IsEmpty() { return internalList.Count == 0; }

    /// <summary>
    /// Retriev the first enemy in the list
    /// </summary>
    /// <returns>First element</returns>
    public T Pop()
    {
        if (internalList.Count == 0) { throw new IndexOutOfRangeException(); }
        T firstElem = internalList[0];
        internalList.RemoveAt(0);
        return firstElem;
    }
    
    public int Length { get { return internalList.Count; } }
    
    public void Push(T elem) { internalList.Add(elem); }
    
    public void Clear() { internalList.Clear(); }
} 

public class GameMaster : MonoBehaviour
{
    [SerializeField] private OverworldData overworldTemplate;

    public GameState CurrentState { get; private set; }
    public GameState PreviousState = null;
    public StrictQueue<Action> actionQueue = new StrictQueue<Action>(); 
    public DataManager dataManager;
    
    private void Awake()
    {
        if(Settings.Instance != null){
            Settings.Instance.LoadSettings();
        }

        dataManager = new DataManager(overworldTemplate);
    }

    private void Start()
    {
        ChangeState(new OverworldState(this));
        UINamespace.UIGuy.Initialize();
        Palette.Instance.Initialize();
        
    }

    private void Update()
    {
        CurrentState?.Update();
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState != null)
        {
            Logger.Info($"Changing state from {CurrentState.GetType().Name} to {newState.GetType().Name}");
            CurrentState.Exit();
        }

        PreviousState = CurrentState;
        CurrentState = newState;
        CurrentState.Enter();
        
        if (DJ.Instance != null)
            DJ.Instance.PlayTrack(newState.GetType().Name);

        string name = newState.GetType().Name;
        Logger.Success($"Game State changed to: {name}");
    } 

    public void CleanQueue(){
        actionQueue.Clear();
    }

    public DataManager GetDataManager() => dataManager; 
}
