# Lootbound Isles — Unity MCP Workflow

This file contains only Lootbound Isles-specific MCP rules.

The generic technical operating procedure for MCP for Unity is provided by the installed Codex skill:

`unity-mcp-orchestrator`

Codex must load/use that skill for Unity Editor tasks. Do not duplicate or replace its current tool schemas here.

## Project-specific rule

If Unity MCP can safely perform an Editor operation, Codex should perform it itself instead of telling the user to do it manually.

For example, do not ask the user to manually:
- create a GameObject;
- add a Component;
- set a serialized Inspector value;
- drag a reference;
- create a ScriptableObject;
- configure a Prefab;
- save a Scene;

when the connected MCP exposes a safe operation for it.

## Inspect before modifying

Lootbound Isles already contains working prototype systems.

Before creating or replacing anything:
1. inspect the actual scene/assets;
2. search for an existing equivalent;
3. inspect components and references;
4. extend/reuse the existing implementation where practical.

Do not create duplicate Player, Camera, Canvas, spawn systems, enemy prefabs, ScriptableObjects or managers.

## Existing architecture constraints

- Player uses `CharacterController`.
- Enemies use `NavMeshAgent`.
- Shared definitions/configuration are data-driven.
- Runtime instance state must not be stored in shared ScriptableObject definitions.
- Gameplay and presentation are separate.
- Gameplay cannot depend on primitive color, scale or concrete visual model.
- No giant `GameManager`.
- No unnecessary DI/EventBus/ECS/pooling architecture.
- Extend working systems rather than rewriting them without need.

## Critical final-design constraints

Do not restore obsolete design:

`Boss Death -> WorldLevel++ -> endless global scaling`

Final progression is authored zones:

`Zone -> Main Boss first kill -> permanently unlock next Zone`

Also preserve these final rules when modifying existing prototype code:

- ordinary Player death respawns in Safe Zone with configurable partial HP, baseline about 10%, not full HP;
- Level Up does not pause `Time.timeScale`;
- during Level Up, Player movement is blocked but Auto Attack/world continue;
- Player Level cap is 100;
- current version supports Rebirth I-III only;
- equipment has no Player Level requirement;
- no Fast Travel / Return Stone;
- main loot goes directly to inventory/currency rather than physical pickups.

See the GDD for full requirements.

## Manual fallback

Manual user steps are acceptable only when:
- MCP does not expose the required operation;
- a required tool group cannot be made available;
- Unity requires a user decision/interaction that cannot be automated safely;
- the operation is destructive or ambiguous and needs approval;
- MCP failed and the cause was diagnosed.

When falling back, Codex must state:
1. what it attempted;
2. why MCP cannot safely complete it;
3. the minimum manual action required.

## Updating PROJECT_STATE.md

`Docs/PROJECT_STATE.md` is not design documentation.

After a verified implementation stage:
- update what is actually implemented;
- record known partial/conflicting behaviour;
- remove stale claims;
- never mark a feature complete just because code was drafted.

## End-of-stage project checklist

In addition to the `unity-mcp-orchestrator` skill's verification workflow:

- [ ] Relevant GDD requirement is satisfied.
- [ ] Existing Lootbound Isles architecture was preserved unless change was necessary.
- [ ] No duplicate project objects/assets were introduced.
- [ ] Required Inspector references are actually assigned.
- [ ] Scene/Prefab changes are saved.
- [ ] `PROJECT_STATE.md` reflects the verified result.
- [ ] User receives a short concrete Play Mode test scenario.
- [ ] Codex stops before starting the next major stage.
