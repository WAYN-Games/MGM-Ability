using System.Reflection;

using Unity.Entities;
using Unity.Jobs;

using UnityEngine;

public class TestWorld
{

    private World world;
    private MethodInfo CompleteDependacy;

    private PropertyInfo SystemDependacy;

    public TestWorld()
    {
        world = new World("TestWorld");
        World.DefaultGameObjectInjectionWorld = world;
        CompleteDependacy = typeof(SystemBase).GetMethod("CompleteDependency", BindingFlags.NonPublic | BindingFlags.Instance);
        SystemDependacy = typeof(SystemBase).GetProperty("Dependency", BindingFlags.NonPublic | BindingFlags.Instance);
        if (SystemDependacy == null)
        {
            Debug.LogError("Did not get jobHandle propperty reference");
        }
    }

    public World GetReference()
    {
        return world;
    }

    public EntityManager GetEntityManager()
    {
        return world.EntityManager;
    }

    public TestWorld WithSystem<SYSTEM>() where SYSTEM : ComponentSystemBase
    {
        world.GetOrCreateSystem<SYSTEM>();
        return this;
    }

    public SYSTEM UpdateSystem<SYSTEM>(SYSTEM system = null) where SYSTEM : ComponentSystemBase
    {
        system = system ?? world.GetExistingSystem<SYSTEM>();
        system.Update();
        return system;
    }

    public SYSTEM CompleteSystem<SYSTEM>(SYSTEM system = null) where SYSTEM : ComponentSystemBase
    {
        system = system ?? world.GetExistingSystem<SYSTEM>();
        if (typeof(SystemBase).IsAssignableFrom(typeof(SYSTEM)))
        {
            CompleteDependacy.Invoke(system, null);
        }
        if (typeof(JobComponentSystem).IsAssignableFrom(typeof(SYSTEM)))
        {
            PropertyInfo prop = typeof(JobComponentSystem).GetProperty("m_PreviousFrameDependency", BindingFlags.NonPublic | BindingFlags.Instance);
            ((JobHandle)prop.GetValue(system)).Complete();
        }
        return system;
    }



    public void CompleteAllSystems()
    {
        world.EntityManager.CompleteAllJobs();
    }

    public void WaitForAllSystemsToComplete()
    {
        bool AllCompleted = false;
        while (!AllCompleted)
        {
            bool gate = true;
            foreach (ComponentSystemBase system in world.Systems)
            {
                if (typeof(SystemBase).IsAssignableFrom(system.GetType()))
                {
                    gate &= ((JobHandle)SystemDependacy.GetValue(system)).IsCompleted;
                }
                if (typeof(JobComponentSystem).IsAssignableFrom(system.GetType()))
                {
                    PropertyInfo prop = typeof(JobComponentSystem).GetProperty("m_PreviousFrameDependency", BindingFlags.NonPublic | BindingFlags.Instance);
                    gate &= ((JobHandle)prop.GetValue(system)).IsCompleted;
                }

            }
            AllCompleted = gate;
        }
    }

    public SYSTEM UpdateAndCompleteSystem<SYSTEM>() where SYSTEM : ComponentSystemBase
    {
        UpdateSystem<SYSTEM>();
        return CompleteSystem<SYSTEM>();
    }

    public void Dispose()
    {
        world.Dispose();
    }
}
