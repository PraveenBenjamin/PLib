The purpose of this System is to allow to user to define routines that persist between scenes, and is called when a new scene is loaded

OnSceneLoadedPersistantRoutineManager :- Maintains a list of persistant routines and attempts to invoke them when a scene is loaded

OnSceneLoadedPersistantRoutineFactory :- Offers an interface to easily create IPersistantRoutine handles that will be fed to the Manager

PersistantRoutineImplementations :- 
	The different types of persistant routines that are part of the scene. 
	Every implementation must implement the IPersistantRoutine interface.
	If a new implementation is added, then the factory must be updated to support it.

AssignPersistantRoutinesOnEnable :- Uses the Factory to convert PersistantRoutineDatums to IPersistantRoutine handles, which it feeds to the Manager 

PersistantRoutineCompiler :- exposes an interface that the PersistantRoutineFactory will use to compile code at runtime

PersistantRoutineDatum :-  
	Class that exposes the various feilds that an IPersistantRoutine may require to the editor. 
	The routine itself is a string field which expects the contents of a function
	The factory will use the utils to compile at runtime to a unique static function that behaves like an Action.
	Take care to only enter independant code here. Like calls to singletons and what now.