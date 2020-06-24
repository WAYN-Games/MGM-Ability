
using NUnit.Framework;

using Unity.Collections;
using Unity.Entities;
using Unity.PerformanceTesting;

using WaynGroup.Mgm.Ability;
using WaynGroup.Mgm.Ability.Demo;
using WaynGroup.Mgm.Ability.Tests;

public class EffectSystemTest : DotsTest
{
    private const bool TEST_PERF_ENABLED = true;


    private NativeArray<AbilityBuffer> AddAbility(int count, DynamicBuffer<AbilityBuffer> buffer)
    {
        for (int i = 0; i < count; i++)
        {
            Ability Ability = new Ability(0, 0, new Range() { Min = float.MinValue, Max = float.MaxValue })
            {
                IsInRange = true
            };
            buffer.Add(new AbilityBuffer()
            {
                Ability = Ability
            });
        }
        return buffer.AsNativeArray();
    }


    private void AddEffect2ToAbility(DynamicBuffer<Effect2Buffer> buff, int abilityIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            buff.Add(new Effect2Buffer()
            {
                Effect = new Effect2()
                {
                    Value = 1
                },
                AbilityIndex = abilityIndex
            });
        }
    }

    private void AddEffect1ToAbility(DynamicBuffer<Effect1Buffer> buff, int abilityIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            buff.Add(new Effect1Buffer()
            {
                Effect = new Effect1()
                {
                    Value = 1
                },
                AbilityIndex = abilityIndex
            });
        }
    }


    [Test, Performance]
    public void Test_Ability_System([Values(1, 10, 100, 1000, 10000, 100000)] int EntityCount)
    {
        // Arrange

        Entity target = _entityManager.CreateEntity();
        _entityManager.AddComponentData(target, new Health() { Value = 100 });
        _entityManager.AddComponentData(target, new Armor() { Value = 1 });

        Entity entity = _entityManager.CreateEntity();
        PrepareAttacker(target, entity);

        Entity entity2 = _entityManager.CreateEntity();
        PrepareAttacker(target, entity2);


        _world
            .WithSystem<UserInputSimulationSystem>()
            .WithSystem<AbilityUpdateTimingsSystem>()
            .WithSystem<Effect1TriggerSystem>()
            .WithSystem<Effect2TriggerSystem>()
            .WithSystem<AbilityDeactivationSystem>()
            .WithSystem<Effect1ConsumerSystem>()
            .WithSystem<Effect2ConsumerSystem>();


        // Act
        _world.GetReference().SetTime(new Unity.Core.TimeData(0, 1));


        AssertAbilityActivationState(entity, AbilityState.CoolingDown);
        AssertAbilityActivationState(entity2, AbilityState.CoolingDown);

        _world.UpdateSystem<AbilityUpdateTimingsSystem>();
        _world.CompleteAllJobs();

        AssertAbilityActivationState(entity, AbilityState.CooledDown);
        AssertAbilityActivationState(entity2, AbilityState.CooledDown);

        _world.UpdateSystem<UserInputSimulationSystem>();
        _world.CompleteAllJobs();

        AssertAbilityActivationState(entity, AbilityState.Casting);
        AssertAbilityActivationState(entity2, AbilityState.Casting);

        _world.UpdateSystem<AbilityUpdateTimingsSystem>();
        _world.CompleteAllJobs();

        // Assert
        AssertAbilityActivationState(entity, AbilityState.Active);
        AssertAbilityActivationState(entity2, AbilityState.Active);

        // Act
        _world.UpdateSystem<Effect1TriggerSystem>();
        _world.UpdateSystem<Effect2TriggerSystem>();
        _world.UpdateSystem<AbilityDeactivationSystem>();
        _world.CompleteAllJobs();

        // Assert

        AssertAbilityActivationState(entity, AbilityState.CoolingDown);
        AssertAbilityActivationState(entity2, AbilityState.CoolingDown);

        // Act
        _world.UpdateSystem<Effect1ConsumerSystem>();
        _world.CompleteAllJobs();
        // Assert
        Assert.AreEqual(80, _entityManager.GetComponentData<Health>(target).Value);
        // Act
        _world.UpdateSystem<Effect2ConsumerSystem>();
        _world.CompleteAllJobs();
        // Assert
        Assert.AreEqual(60, _entityManager.GetComponentData<Health>(target).Value);

        /**************************************************
         * 
         * PERF TEST
         * 
         * **************************************************/
        if (TEST_PERF_ENABLED)
        {
            // Arrange
            for (int i = 0; i < EntityCount - 1; i++)
            {

                Entity t = _entityManager.CreateEntity();
                _entityManager.AddComponentData(t, new Health() { Value = 100 });

                Entity e = _entityManager.CreateEntity();
                PrepareAttacker(t, e);

                Entity e2 = _entityManager.CreateEntity();
                PrepareAttacker(t, e2);
            }

            Measure.Method(() =>
            {
                _world.UpdateSystem<UserInputSimulationSystem>();
                _world.UpdateSystem<Effect1TriggerSystem>();
                _world.UpdateSystem<Effect2TriggerSystem>();
                _world.UpdateSystem<AbilityDeactivationSystem>();
                _world.UpdateSystem<Effect1ConsumerSystem>();
                _world.UpdateSystem<Effect2ConsumerSystem>();
                _world.CompleteAllJobs();
            }).Run();
        }


    }

    private void AssertAbilityActivationState(Entity entity, AbilityState expectedState)
    {
        Assert.True(_entityManager.HasComponent<AbilityBuffer>(entity));
        NativeArray<AbilityBuffer> sba = _entityManager.GetBuffer<AbilityBuffer>(entity).AsNativeArray();
        Assert.AreEqual(2, sba.Length);
        foreach (AbilityBuffer sb in sba)
        {
            Assert.AreEqual(expectedState, sb.Ability.State);
        }
    }

    private void PrepareAttacker(Entity target, Entity entity)
    {
        // 2 abilitys per attacker
        NativeArray<AbilityBuffer> abilityBuffer = AddAbility(2, _entityManager.AddBuffer<AbilityBuffer>(entity));

        // 2 attacker per target
        // 2 abilitys per attacker
        // 5 effects of each type per ability
        // 2 types of effect each dealing 1 damage
        // 2 x 5 x 2 x 1 = 20 effect per entity per tick.
        for (int i = 0; i < abilityBuffer.Length; i++)
        {
            AddEffect1ToAbility(_entityManager.AddBuffer<Effect1Buffer>(entity), i, 5);
            AddEffect2ToAbility(_entityManager.AddBuffer<Effect2Buffer>(entity), i, 5);
        }
        _entityManager.AddComponentData(entity, new Target() { Value = target });
        _entityManager.AddComponentData(entity, new AttackPower() { Value = 1 });
    }
}
