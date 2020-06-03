
using Unity.Entities;

public class TestWorld
{

    private World world;

    public TestWorld()
    {
        world = new World("TestWorld");
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

    public void CompleteAllJobs()
    {
        world.EntityManager.CompleteAllJobs();
    }


    public void Dispose()
    {
        world.Dispose();
    }
}
