# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.2.0] - In progress

### Added

- Support for Min and Max range constraint on skill

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
