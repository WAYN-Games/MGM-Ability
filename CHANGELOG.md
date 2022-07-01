# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.4.0] - Unreleased 

### Informations

- This version requires UNITY 2020.2.0a15 or higher, for the UI to work (in editor)
- Bug have been reported to unity for the zero sized UI panel in 2020.1 and non working cutom UI Element in 2020.1 and 2020.2 alpha

### Added

- Simple ability UI support (this feature is highly experimental and uses the unity runtime package for UI Elements (UI Toolkit)) it does not work in builds yet (see informations).

### Changed 

- Updated package dependency
- **License** (see [LICENSE.md](./LICENSE.md))

## [0.3.0] - 05/07/2020

### Added

- Support for cost constraint and consumtion on skill
- Support for simple target selection from skill (self or target)
- System groups to organize systems update order

### Changed 

- ![](https://upload.wikimedia.org/wikipedia/commons/thumb/f/f6/OOjs_UI_icon_alert-destructive.svg/20px-OOjs_UI_icon_alert-destructive.svg.png) BREAKING CHANGE - Rename all occurences of "Skill" to "Ability" and "Skills" to "Abilities".![](https://upload.wikimedia.org/wikipedia/commons/thumb/f/f6/OOjs_UI_icon_alert-destructive.svg/20px-OOjs_UI_icon_alert-destructive.svg.png)
- Trigger system won't do anything if it's consumer counter part won't run. This avoid creating the native stream, which in turn avoid any risk a memory leak so there is no need for the consumer to always update to do the clean up.

### Code Test Coverage

- Started monitoring the code coverage of the package and switched to system behavior test strategy instead of a world test strategy.

| Name                         | Covered | Uncovered | Coverable | Total | Line coverage |
|------------------------------|---------|-----------|-----------|-------|---------------|
| WaynGroup.Mgm.Ability        | 233     | 72        | 305       | 951   | 76.3%         |
| WaynGroup.Mgm.Ability.Editor | 0       | 137       | 137       | 546   | 0%            |


- CRAP score is currenlty wrong, it show 2 false positive CRAP method.

## [0.2.0] - 09/06/2020

### Added

- Support for Min and Max range constraint on skill

### Changed 

- Update to com.unity.entities 0.11.0-preview.7 (no impact)

## [0.1.2] - 08/06/2020

### Fixed

- [#3 Possible memory leak](https://github.com/WAYNGROUP/MGM-Skill/issues/3)
- [#4 Error with JobsDebugger enabled](https://github.com/WAYNGROUP/MGM-Skill/issues/4)
- [#5 Trigger context entity description is a OR, should be an AND](https://github.com/WAYNGROUP/MGM-Skill/issues/5)


## [0.1.1] - 07/06/2020

### Fixed

- [#1 unused loop variable](https://github.com/WAYNGROUP/MGM-Skill/pull/1)

## [0.1.0] - 31/05/2020

### This is the first release of *\<MGM Skill\>*.

This first realease provides a simple skill system that can be authored by Scriptable Object.
For now, it allows the definition of single target skill that are subject to a cast time and cooldown time.
Its allows to define any custom type of custom direct effect and comes with a Simple Skill sample that demontrate how to define a simple direct damage skill.
