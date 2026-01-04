# V1.1.0 Release Notes

## Action Changes

- The documentation paths that are sent to Neuro now send all possible docs rather than just obtained from upgrades..
- The action for buying upgrades now has an additional configurable option that allows for Neuro to buy an upgrade when not in the menu. The upgrade config has also been changed..

### Query Actions
There are now many actions for querying different parts of the game.

- Current resources.
- Information about the drones.
- Current information about the world state.
- Query the built in features about the ingame language.
- Query the upgrades, This is only enabled when Neuro can buy an upgrade at any time.


### Diff Patches

- Added using multiple search and replace in a single file search.
- Allow for multiple files in a single patch.
- Errors should be thrown more often for inaccurate patches.

### Action Registering
How actions are registered has been rewrittern, this should allow for easier development and for less bugs.

## Context
While alot of the context has been improved the two main changes are.

- Context is now sent when the game is stared describing the purpose of the game and some things Neuro will need to know.
- Error messages from the code windows are now sent, this includes their reason and the position of the error. This information can be inaccurate but it should always be close enough to the correct place.