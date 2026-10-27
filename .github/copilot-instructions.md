# GitHub Copilot Instructions for SaveModlistAsCollection Mod

## Mod Overview and Purpose

The SaveModlistAsCollection mod for RimWorld allows players to save their current mod list as a Steam collection. This makes it easier for players to create and manage mod collections, especially when they want to keep track of their "Current play-through" mods. The mod requires an active Steam login and only adds mods that are published on Steam to the collection. The maximum number of mods in a collection is 1000.

## Key Features and Systems

The mod provides a settings window where players can save their current mod list to a Steam collection. The mods are added to the collection in the current load order, and any existing mods on the list are removed. The mod skips itself when adding mods to the collection.

The mod uses the Steam Web API to fetch the user's collections and update the collection with the current mod list. The mod also includes a status window that shows the progress of the collection update.

## Coding Patterns and Conventions

The mod is written in C# and follows the following coding patterns and conventions:

- The code is organized into classes and methods, with each class having a single responsibility.
- The code uses descriptive variable and method names to make it easier to understand.
- The code uses error handling to ensure that any exceptions are caught and handled gracefully.
- The code uses comments and documentation to explain complex logic and provide guidance on how to use the code.

## XML Integration

The mod does not use any XML files or definitions. All of the code is written in C#.

## Harmony Patching

The mod does not use Harmony patching.

## Suggestions for Copilot

Here are some suggestions for Copilot when working on the SaveModlistAsCollection mod:

- When working with C# code, Copilot should follow the coding patterns and conventions used in the mod.
- When working with the Steam Web API, Copilot should be familiar with the API documentation and use it to fetch and update collections.
- When working with the mod's settings window, Copilot should be familiar with the RimWorld API for creating and managing settings windows.
- When working with the mod's status window, Copilot should be familiar with the RimWorld API for creating and managing status windows.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

