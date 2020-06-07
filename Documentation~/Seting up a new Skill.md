# Setting up a new Skill


## Creating the scriptable asset

To create a new skill you need to create a new scriptable skill asset using the Create > MGM > Skill menu.

![Create > MGM > Skill](https://github.com/WAYNGROUP/MGM-Skill/blob/master/Documentation~/images/Create%20new%20Skill.png)

## Authoring the skill

By clicking on the scriptable skill asset you just created you should be able to see it's content in the inspector.

![Skill inspector](https://github.com/WAYNGROUP/MGM-Skill/blob/master/Documentation~/images/NewSkillInspector.PNG)

You can edit the name of the skill, it's cooldown (time needed to recharge the skill) and it's cast time (time needed between the castign of the skill and it's activation).
You will also see a list of all the effect type declared in your porject.

![Effect list](https://github.com/WAYNGROUP/MGM-Skill/blob/master/Documentation~/images/EffectTypeDropDown.png)

Selecting an effect in the list and clicking the 'Add' button will add it to the skill's effect.

![Effect on skill](https://github.com/WAYNGROUP/MGM-Skill/blob/master/Documentation~/images/EffectListOnSkill.png)

## Add the skill to an entity

On a game object in the hierary, add the Skill Auhtoring component and populate the list of skill with the scriptable skill assets.

![Skill on entity](https://github.com/WAYNGROUP/MGM-Skill/blob/master/Documentation~/images/Add%20skills%20to%20entity.png)


# Known issue

Removing or renaming an effect will cause an error in the deserialization porcess of the scriptable skill.
To restore the scriptable skill asset you can edit the asset with a text editor and change / remove the concerned effect.

