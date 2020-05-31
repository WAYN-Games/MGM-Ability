using NUnit.Framework;

using Unity.Entities;

namespace WaynGroup.Mgm.Skill.Tests
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
            _world.CompleteAllSystems();
            _world.Dispose();
        }

    }

}
