# Setting up a new Ability


## Creating the scriptable asset

To create a new ability you need to create a new scriptable ability asset using the Create > MGM > Ability menu.

![Create > MGM > Ability](https://github.com/WAYNGROUP/MGM-Ability/blob/master/Documentation~/images/Create%20new%20Ability.png)

## Authoring the ability

By clicking on the scriptable ability asset you just created you should be able to see it's content in the inspector.

![Ability inspector](https://github.com/WAYNGROUP/MGM-Ability/blob/master/Documentation~/images/NewAbilityInspector.PNG)

You can edit the name of the ability, it's cooldown (time needed to recharge the ability) and it's cast time (time needed between the castign of the ability and it's activation).
You will also see a list of all the effect type declared in your porject.

![Effect list](https://github.com/WAYNGROUP/MGM-Ability/blob/master/Documentation~/images/EffectTypeDropDown.png)

Selecting an effect in the list and clicking the 'Add' button will add it to the ability's effect.

![Effect on ability](https://github.com/WAYNGROUP/MGM-Ability/blob/master/Documentation~/images/EffectListOnAbility.png)

## Add the ability to an entity

On a game object in the hierary, add the Ability Auhtoring component and populate the list of ability with the scriptable ability assets.

![Ability on entity](https://github.com/WAYNGROUP/MGM-Ability/blob/master/Documentation~/images/Add%20abilities%20to%20entity.PNG)


# Known issue

Removing or renaming an effect will cause an error in the deserialization porcess of the scriptable ability.
To restore the scriptable ability asset you can edit the asset with a text editor and change / remove the concerned effect.

