using UnityEngine;

/// <summary>
/// Helper script that automatically sets up escape quests.
/// Scenario: Hidden room key → Open door → Collect elevator parts → Escape!
/// </summary>
public class EscapeQuestSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [SerializeField] private bool setupOnStart = true;
    
    [Header("Quest Configuration")]
    [SerializeField] private bool createHiddenRoomKeyObjective = true;
    [SerializeField] private bool createUnlockDoorObjective = true; // New: Unlock door objective
    [SerializeField] private bool createFindElevatorObjective = true;
    [SerializeField] private bool createNailsObjective = true;
    [SerializeField] private bool createKeycardObjective = true;
    [SerializeField] private bool createScrewdriverObjective = true;
    [SerializeField] private bool createElevatorButtonObjective = true;
    [SerializeField] private bool createElevatorCallButtonObjective = true;
    [SerializeField] private bool createActivateElevatorObjective = true;
    [SerializeField] private bool createEscapeObjective = true;
    
    [Header("Objective Descriptions")]
    [SerializeField] private string hiddenRoomKeyTitle = "Find the Hidden Room Key";
    [SerializeField] private string hiddenRoomKeyDesc = "Search for the key to unlock the hidden room";
    
    [SerializeField] private string unlockDoorTitle = "Unlock the Hidden Room";
    [SerializeField] private string unlockDoorDesc = "Use the key to unlock the hidden room door";
    
    [SerializeField] private string findElevatorTitle = "Find the Escape Elevator";
    [SerializeField] private string findElevatorDesc = "Locate the elevator in the hidden room";
    
    [SerializeField] private string nailsTitle = "Find Nails";
    [SerializeField] private string nailsDesc = "Find nails to secure the elevator panels";
    
    [SerializeField] private string keycardTitle = "Find Keycard";
    [SerializeField] private string keycardDesc = "Find the keycard to authorize elevator access";
    
    [SerializeField] private string screwdriverTitle = "Find Screwdriver";
    [SerializeField] private string screwdriverDesc = "Find a screwdriver to open the elevator panel";
    
    [SerializeField] private string elevatorButtonTitle = "Find Elevator Button";
    [SerializeField] private string elevatorButtonDesc = "Find the replacement button for inside the elevator";
    
    [SerializeField] private string elevatorCallButtonTitle = "Find Elevator Call Button";
    [SerializeField] private string elevatorCallButtonDesc = "Find the call button to activate the elevator";
    
    [SerializeField] private string activateElevatorTitle = "Repair the Elevator";
    [SerializeField] private string activateElevatorDesc = "Collect all parts to repair and activate the elevator";
    
    [SerializeField] private string escapeTitle = "Enter the Elevator!";
    [SerializeField] private string escapeDesc = "Get into the elevator and escape!";
    
    [Header("References")]
    [SerializeField] private ObjectiveSystem objectiveSystem;
    
    void Start()
    {
        if (setupOnStart)
        {
            SetupQuests();
        }
    }
    
    public void SetupQuests()
    {
        if (objectiveSystem == null)
        {
            objectiveSystem = ObjectiveSystem.Instance;
        }
        
        if (objectiveSystem == null)
        {
            Debug.LogError("[EscapeQuestSetup] ObjectiveSystem not found!");
            return;
        }
        
        Debug.Log("[EscapeQuestSetup] Setting up elevator escape quest...");
        
        // 1. Find hidden room key (INITIALLY INACTIVE - activated when door is found)
        if (createHiddenRoomKeyObjective)
        {
            Objective keyObj = new Objective(
                "collect_hidden_room_key",
                hiddenRoomKeyTitle,
                hiddenRoomKeyDesc,
                ObjectiveType.CollectItem
            );
            keyObj.isActive = false; // INITIALLY INACTIVE
            objectiveSystem.AddObjectiveManual(keyObj);
        }
        
        // 2. Unlock the door (INITIALLY INACTIVE - activated when key is collected)
        if (createUnlockDoorObjective)
        {
            Objective unlockObj = new Objective(
                "unlock_door",
                unlockDoorTitle,
                unlockDoorDesc,
                ObjectiveType.UnlockDoor
            );
            unlockObj.isActive = false;
            objectiveSystem.AddObjectiveManual(unlockObj);
        }
        
        // 3. Find elevator (INITIALLY INACTIVE - activated when door is opened)
        if (createFindElevatorObjective)
        {
            Objective elevatorObj = new Objective(
                "find_elevator",
                findElevatorTitle,
                findElevatorDesc,
                ObjectiveType.ReachLocation
            );
            elevatorObj.isActive = false;
            objectiveSystem.AddObjectiveManual(elevatorObj);
        }
        
        // 4-8. Find elevator repair parts (INITIALLY INACTIVE - activated when door is opened)
        if (createNailsObjective)
        {
            Objective nailsObj = new Objective(
                "collect_nails",
                nailsTitle,
                nailsDesc,
                ObjectiveType.CollectItem
            );
            nailsObj.isActive = false; // INITIALLY INACTIVE
            objectiveSystem.AddObjectiveManual(nailsObj);
        }
        
        if (createKeycardObjective)
        {
            Objective keycardObj = new Objective(
                "collect_keycard",
                keycardTitle,
                keycardDesc,
                ObjectiveType.CollectItem
            );
            keycardObj.isActive = false;
            objectiveSystem.AddObjectiveManual(keycardObj);
        }
        
        if (createScrewdriverObjective)
        {
            Objective screwdriverObj = new Objective(
                "collect_screwdriver",
                screwdriverTitle,
                screwdriverDesc,
                ObjectiveType.CollectItem
            );
            screwdriverObj.isActive = false;
            objectiveSystem.AddObjectiveManual(screwdriverObj);
        }
        
        if (createElevatorButtonObjective)
        {
            Objective buttonObj = new Objective(
                "collect_elevator_button",
                elevatorButtonTitle,
                elevatorButtonDesc,
                ObjectiveType.CollectItem
            );
            buttonObj.isActive = false;
            objectiveSystem.AddObjectiveManual(buttonObj);
        }
        
        if (createElevatorCallButtonObjective)
        {
            Objective callButtonObj = new Objective(
                "collect_elevator_call_button",
                elevatorCallButtonTitle,
                elevatorCallButtonDesc,
                ObjectiveType.CollectItem
            );
            callButtonObj.isActive = false;
            objectiveSystem.AddObjectiveManual(callButtonObj);
        }
        
        // 8. Activate elevator (INITIALLY INACTIVE - Elevator script activates when parts are collected)
        if (createActivateElevatorObjective)
        {
            Objective activateObj = new Objective(
                "activate_elevator",
                activateElevatorTitle,
                activateElevatorDesc,
                ObjectiveType.Custom
            );
            activateObj.isActive = false;
            objectiveSystem.AddObjectiveManual(activateObj);
        }
        
        // 9. Escape (INITIALLY INACTIVE - activated when elevator becomes active)
        if (createEscapeObjective)
        {
            Objective escapeObj = new Objective(
                "escape",
                escapeTitle,
                escapeDesc,
                ObjectiveType.Escape
            );
            escapeObj.isActive = false;
            objectiveSystem.AddObjectiveManual(escapeObj);
        }
        
        Debug.Log("[EscapeQuestSetup] Elevator escape quest setup complete!");
    }
    
    /// <summary>
    /// For testing in Unity Editor
    /// </summary>
    [ContextMenu("Setup Quests Now")]
    public void SetupQuestsEditor()
    {
        SetupQuests();
    }
}
