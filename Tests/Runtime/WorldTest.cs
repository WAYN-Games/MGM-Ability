
using NUnit.Framework;

using Unity.Collections;
using Unity.Entities;
using Unity.PerformanceTesting;

using WaynGroup.Mgm.Skill;
using WaynGroup.Mgm.Skill.Demo;
using WaynGroup.Mgm.Skill.Tests;

public class EffectSystemTest : DotsTest
{
    private const bool TEST_PERF_ENABLED = true;


    private NativeArray<SkillBuffer> AddSkill(int count, DynamicBuffer<SkillBuffer> buffer)
    {
        for (int i = 0; i < count; i++)
        {
            Skill Skill = new Skill(0, 0, new Range() { Min = float.MinValue, Max = float.MaxValue });
            Skill.IsInRange = true;
            buffer.Add(new SkillBuffer()
            {
                Skill = Skill
            });
        }
        return buffer.AsNativeArray();
    }


    private void AddEffect2ToSkill(DynamicBuffer<Effect2Buffer> buff, int skillIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            buff.Add(new Effect2Buffer()
            {
                Effect = new Effect2()
                {
                    Value = 1
                },
                SkillIndex = skillIndex
            });
        }
    }

    private void AddEffect1ToSkill(DynamicBuffer<Effect1Buffer> buff, int skillIndex, int count)
    {
        for (int i = 0; i < count; i++)
        {
            buff.Add(new Effect1Buffer()
            {
                Effect = new Effect1()
                {
                    Value = 1
                },
                SkillIndex = skillIndex
            });
        }
    }


    [Test, Performance]
    public void Test_Skill_System([Values(1, 10, 100, 1000, 10000, 100000)] int EntityCount)
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
            .WithSystem<SkillUpdateTimingsSystem>()
            .WithSystem<Effect1TriggerSystem>()
            .WithSystem<Effect2TriggerSystem>()
            .WithSystem<SkillDeactivationSystem>()
            .WithSystem<Effect1ConsumerSystem>()
            .WithSystem<Effect2ConsumerSystem>();


        // Act
        _world.GetReference().SetTime(new Unity.Core.TimeData(0, 1));


        AssertSkillActivationState(entity, SkillState.CoolingDown);
        AssertSkillActivationState(entity2, SkillState.CoolingDown);

        _world.UpdateSystem<SkillUpdateTimingsSystem>();
        _world.CompleteAllJobs();

        AssertSkillActivationState(entity, SkillState.CooledDown);
        AssertSkillActivationState(entity2, SkillState.CooledDown);

        _world.UpdateSystem<UserInputSimulationSystem>();
        _world.CompleteAllJobs();

        AssertSkillActivationState(entity, SkillState.Casting);
        AssertSkillActivationState(entity2, SkillState.Casting);

        _world.UpdateSystem<SkillUpdateTimingsSystem>();
        _world.CompleteAllJobs();

        // Assert
        AssertSkillActivationState(entity, SkillState.Active);
        AssertSkillActivationState(entity2, SkillState.Active);

        // Act
        _world.UpdateSystem<Effect1TriggerSystem>();
        _world.UpdateSystem<Effect2TriggerSystem>();
        _world.UpdateSystem<SkillDeactivationSystem>();
        _world.CompleteAllJobs();

        // Assert

        AssertSkillActivationState(entity, SkillState.CoolingDown);
        AssertSkillActivationState(entity2, SkillState.CoolingDown);

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
                _world.UpdateSystem<SkillDeactivationSystem>();
                _world.UpdateSystem<Effect1ConsumerSystem>();
                _world.UpdateSystem<Effect2ConsumerSystem>();
                _world.CompleteAllJobs();
            }).Run();
        }


    }

    private void AssertSkillActivationState(Entity entity, SkillState expectedState)
    {
        Assert.True(_entityManager.HasComponent<SkillBuffer>(entity));
        NativeArray<SkillBuffer> sba = _entityManager.GetBuffer<SkillBuffer>(entity).AsNativeArray();
        Assert.AreEqual(2, sba.Length);
        foreach (SkillBuffer sb in sba)
        {
            Assert.AreEqual(expectedState, sb.Skill.State);
        }
    }

    private void PrepareAttacker(Entity target, Entity entity)
    {
        // 2 skills per attacker
        NativeArray<SkillBuffer> skillBuffer = AddSkill(2, _entityManager.AddBuffer<SkillBuffer>(entity));

        // 2 attacker per target
        // 2 skills per attacker
        // 5 effects of each type per skill
        // 2 types of effect each dealing 1 damage
        // 2 x 5 x 2 x 1 = 20 effect per entity per tick.
        for (int i = 0; i < skillBuffer.Length; i++)
        {
            AddEffect1ToSkill(_entityManager.AddBuffer<Effect1Buffer>(entity), i, 5);
            AddEffect2ToSkill(_entityManager.AddBuffer<Effect2Buffer>(entity), i, 5);
        }
        _entityManager.AddComponentData(entity, new Target() { Value = target });
        _entityManager.AddComponentData(entity, new AttackPower() { Value = 1 });
    }
}
