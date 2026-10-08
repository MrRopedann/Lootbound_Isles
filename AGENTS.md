# Lootbound Isles — Codex Instructions

## Sources of truth

Before making gameplay changes, read:

- `Docs/Lootbound_Isles_GDD.md` — target gameplay/design.
- `Docs/PROJECT_STATE.md` — current verified/known implementation state.
- `Docs/UNITY_MCP_WORKFLOW.md` — Lootbound Isles-specific Unity automation rules.

If implementation conflicts with the GDD, report the conflict. Do not silently change game design.

## Unity MCP Skill

This project uses the installed Codex skill:

`unity-mcp-orchestrator`

For every task that requires interaction with the Unity Editor, use the `unity-mcp-orchestrator` skill.

Before performing Unity MCP operations:

1. Load/read the `unity-mcp-orchestrator` skill instructions.
2. Follow its resource-first workflow and current MCP best practices.
3. Follow `Docs/UNITY_MCP_WORKFLOW.md` for Lootbound Isles-specific rules.
4. Treat examples from the skill as templates, not guaranteed schemas. Inspect live Unity resources/tools and adapt to the current Unity/MCP/package versions.

Priority for Unity work:

1. `Docs/Lootbound_Isles_GDD.md` — WHAT the game must do.
2. `AGENTS.md` — repository/project development rules.
3. `Docs/PROJECT_STATE.md` — current known implementation state.
4. `Docs/UNITY_MCP_WORKFLOW.md` — project-specific Unity automation rules.
5. `unity-mcp-orchestrator` — technical MCP operating procedures and tool usage patterns.

The skill must never override gameplay decisions from the GDD.

## MCP-first rule

When Unity Editor is connected, do not stop after writing C# if the task also requires Editor setup.

When MCP can safely perform an operation, perform it instead of asking the user to do it manually. This includes, when supported:

- inspecting scenes/hierarchy;
- finding/creating/modifying GameObjects;
- adding/removing/configuring Components;
- assigning Inspector references;
- creating/configuring ScriptableObjects;
- creating/updating/instantiating Prefabs;
- changing and saving scenes;
- inspecting Console/compilation state;
- running Unity tests;
- capturing screenshots for verification.

## Mandatory project workflow

For every implementation stage:

1. Read the relevant GDD section.
2. Read `PROJECT_STATE.md`.
3. Load/use `unity-mcp-orchestrator`.
4. Inspect the actual Unity project through MCP before creating anything.
5. Reuse existing objects/systems where practical.
6. Make the smallest coherent code change.
7. Let the skill's MCP workflow handle compilation/editor-state/console verification.
8. Perform required Unity Editor configuration through MCP.
9. Save changed scenes/prefabs/assets.
10. Verify the resulting hierarchy/components/properties.
11. Run tests when practical.
12. Verify visually when relevant.
13. Update `PROJECT_STATE.md` only with facts actually verified.
14. Report the completed stage and STOP before the next major stage.

## Safety

- Never blindly create duplicate Player, Camera, Canvas, systems, managers, spawn points, Prefabs or ScriptableObjects.
- Never delete objects/components/assets merely because they appear unused; inspect dependencies first.
- Do not perform broad refactors unless required by the current stage.
- Do not rename files/classes/namespaces without a concrete reason.
- Do not change Tags, Layers, Input Actions or Project Settings globally without inspecting current configuration.
- Do not claim gameplay was tested unless it was actually run/tested.
- Distinguish static verification, automated Unity tests and manual gameplay verification.

## Project architecture

Keep the project suitable for one developer:

- data-driven;
- separate data, gameplay logic, presentation and UI;
- no giant `GameManager`;
- no giant monolithic MonoBehaviours;
- no enterprise-style DI/service architecture without a concrete need;
- gameplay must not depend on primitive color, model, prefab identity or object scale;
- use ScriptableObject for shared definitions/configuration, not mutable per-instance runtime state.

## Debug convention

Gameplay MonoBehaviours that need routine diagnostics use:

```csharp
[Header("DEBUG")]
[SerializeField] private bool isDebug;
```

Routine `Debug.Log` calls must be guarded by `isDebug`.
Critical configuration errors may use `Debug.LogError` without that guard.

## Development cadence

Small working stage -> verification -> next stage.

Do not implement several major GDD systems in one pass.

## Definition of Done

A Unity stage is complete only when all applicable items are true:

- code is written;
- Unity compilation completed;
- no new compile errors;
- required GameObjects exist;
- required Components are attached;
- Inspector values and references are configured;
- required ScriptableObjects/Prefabs exist;
- scene/prefab changes are saved;
- Console was checked;
- automated tests were run when practical;
- screenshots were checked when visual setup changed;
- `PROJECT_STATE.md` was updated with verified facts.

Manual Unity steps are allowed only if MCP cannot perform the operation safely or a user decision is required.

## Stage report format

At the end of each stage report:

### Done
### Changed files
### Created files
### Unity objects / Inspector changes
### Verification performed
### Manual test scenario
### Remaining issues

Then stop and wait for the next instruction.
