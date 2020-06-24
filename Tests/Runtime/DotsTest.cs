using NUnit.Framework;

using Unity.Entities;

namespace WaynGroup.Mgm.Ability.Tests
{
    public abstract class DotsTest
    {

        protected TestWorld _world;
        protected EntityManager _entityManager;

        [SetUp]
        public void SetUp()
        {
            _world = new TestWorld();
            _entityManager = _world.GetEntityManager();
        }


        [TearDown]
        public void TearDown()
        {
            _world.CompleteAllJobs();
            _world.Dispose();
        }

    }

}
